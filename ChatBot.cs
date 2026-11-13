using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CyberBot
{
    // Delegate: each handler takes the user's lowercase text and returns a reply,
    // or null if it can't handle that message
    public delegate string? ResponseHandler(string input);

    public class ChatBot
    {
        private const string DatabaseErrorMessage = "Sorry, I couldn't reach the database right now. Please check that MySQL is running and try again.";

        public UserMemory Memory { get; } = new UserMemory();

        // Events so the GUI can react to chat commands
        public event Action? QuizRequested;
        public event Action? TasksChanged;

        private readonly ActivityLog log;
        private readonly TaskRepository taskRepository;
        private readonly List<ResponseHandler> handlers = new List<ResponseHandler>();

        private string currentTopic = string.Empty;
        private int lastTipIndex = -1;
        private string originalInput = string.Empty; // what the user typed, with original capital letters

        // Task and reminder conversation state
        private int pendingReminderTaskId;
        private string pendingReminderTitle = string.Empty;
        private bool awaitingTaskTitle;
        private bool awaitingIsReminder;
        private DateTime? awaitingReminderDate;

        // Activity log state
        private int logShownCount;
        private bool logJustShown;
        private bool logWasJustShown;

        public ChatBot(ActivityLog log, TaskRepository taskRepository)
        {
            this.log = log;
            this.taskRepository = taskRepository;

            // Order matters: the first handler that returns a reply wins
            handlers.Add(HandlePending);
            handlers.Add(HandleLogCommand);
            handlers.Add(HandleQuizCommand);
            handlers.Add(HandleShowTasks);
            handlers.Add(HandleAddTask);
            handlers.Add(HandleMemory);
            handlers.Add(HandleSmallTalk);
            handlers.Add(HandleFollowUp);
            handlers.Add(HandleTopicOrSentiment);
        }

        public string GetWelcomeMessage()
        {
            return "Hello! Welcome to the Cybersecurity Awareness Bot. I'm here to help you stay safe online. What is your name?";
        }

        // Main entry point: the GUI sends every message here
        public string Process(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ResponseBank.DefaultResponse;

            // The first message from the user is their name
            if (Memory.UserName == string.Empty)
                return SaveName(input);

            originalInput = input.Trim();
            string lower = originalInput.ToLower();

            logWasJustShown = logJustShown;
            logJustShown = false;

            foreach (ResponseHandler handler in handlers)
            {
                string? reply = handler(lower);
                if (reply != null)
                    return reply;
            }

            return ResponseBank.DefaultResponse;
        }

        private string SaveName(string input)
        {
            string name = input.Trim();

            if (name.ToLower().StartsWith("my name is "))
                name = name.Substring(11).Trim();

            if (name.Length == 0)
                return "I didn't catch that. What is your name?";

            Memory.UserName = char.ToUpper(name[0]) + name.Substring(1);

            return $"Nice to meet you, {Memory.UserName}! You can ask me about passwords, phishing, scams, privacy, malware or safe browsing. " +
                   "You can also say 'Add a task to enable 2FA', 'Start the quiz' or 'What have you done for me?'.";
        }

        // ---------- Task and reminder conversation (NLP) ----------

        // Handler 1: answers to questions the bot just asked (task title, reminder yes/no/when)
        private string? HandlePending(string input)
        {
            // The bot asked what the task should be
            if (awaitingTaskTitle)
            {
                awaitingTaskTitle = false;
                DateTime? earlierDate = awaitingReminderDate;
                bool wasReminder = awaitingIsReminder;
                awaitingReminderDate = null;
                awaitingIsReminder = false;

                if (Regex.IsMatch(input, @"^(cancel|never mind|nevermind|stop)\b"))
                    return "No problem, I've cancelled that.";

                TaskRequest request = NlpHelper.ParseTaskRequest("add task " + originalInput);

                if (request.Title == string.Empty)
                    return "I couldn't work out the task. Try something like 'Add task - Enable two-factor authentication'.";

                return CreateTask(request.Title, request.Reminder ?? earlierDate, wasReminder);
            }

            // The bot asked whether the user wants a reminder for a task
            if (pendingReminderTaskId != 0)
            {
                DateTime? date = NlpHelper.FindReminderDate(input, out string phrase);
                bool isYes = Regex.IsMatch(input, @"^(yes|yeah|yep|yup|sure|ok|okay|please)\b") || input.Contains("remind me");
                bool isNo = Regex.IsMatch(input, @"^(no|nope|nah|skip|not now)\b");

                if (date.HasValue)
                {
                    int id = pendingReminderTaskId;
                    string title = pendingReminderTitle;
                    pendingReminderTaskId = 0;

                    try
                    {
                        taskRepository.UpdateReminder(id, date);
                        TasksChanged?.Invoke();
                        log.Add($"Reminder set via chat for '{title}' ({date.Value:dd MMM yyyy HH:mm})");
                        return $"Got it! I'll remind you {DescribeDate(date.Value)} about '{title}'.";
                    }
                    catch (Exception)
                    {
                        return DatabaseErrorMessage;
                    }
                }

                if (isNo)
                {
                    pendingReminderTaskId = 0;
                    return "No problem! Your task is saved without a reminder.";
                }

                if (isYes)
                    return "When should I remind you? You can say things like 'in 3 days', 'tomorrow' or 'next week'.";

                // Something else: forget the question and handle the message normally
                pendingReminderTaskId = 0;
            }

            return null;
        }

        // Handler 2: "show activity log", "what have you done for me?", "show more"
        private string? HandleLogCommand(string input)
        {
            bool wantsMore = logWasJustShown &&
                             (input.Contains("show more") || input.Contains("see more") || input == "more");

            bool wantsLog = (input.Contains("activity") && input.Contains("log")) ||
                            input.Contains("what have you done") || input.Contains("what did you do") ||
                            input.Contains("recent actions") || input.Contains("show log") || input.Contains("view log");

            if (!wantsLog && !wantsMore)
                return null;

            logJustShown = true;

            if (log.Count == 0)
                return "I haven't done anything for you yet. Try adding a task or starting the quiz!";

            logShownCount = wantsMore ? logShownCount + 5 : 5;
            List<ActivityEntry> entries = log.GetRecent(logShownCount);

            List<string> lines = new List<string> { "Here's a summary of recent actions:" };
            for (int i = 0; i < entries.Count; i++)
            {
                lines.Add($"{i + 1}. [{entries[i].Time:dd MMM HH:mm}] {entries[i].Description}");
            }

            if (log.Count > entries.Count)
                lines.Add("Type 'show more' to see older actions.");

            return string.Join(Environment.NewLine, lines);
        }

        // Handler 3: "start the quiz", "test my knowledge", "play a game"
        private string? HandleQuizCommand(string input)
        {
            bool wantsQuiz = input.Contains("quiz") || input.Contains("test my knowledge") || input.Contains("test me") ||
                             input.Contains("mini game") || input.Contains("mini-game") || input.Contains("minigame") ||
                             input.Contains("play a game");

            if (!wantsQuiz)
                return null;

            log.Add("NLP: recognised a quiz request from the chat (keyword: quiz)");
            QuizRequested?.Invoke();

            return $"Let's test your cybersecurity knowledge, {Memory.UserName}! I've opened the Quiz tab for you. Good luck!";
        }

        // Handler 4: "show my tasks", "list tasks"
        private string? HandleShowTasks(string input)
        {
            if (!input.Contains("task"))
                return null;

            bool wantsList = Regex.IsMatch(input, @"\b(show|view|list|see|display|open|what|which|check)\b");
            bool wantsAdd = Regex.IsMatch(input, @"\b(add|create|new|make|set up|setup)\b");

            if (!wantsList || wantsAdd)
                return null;

            try
            {
                List<TaskItem> tasks = taskRepository.GetAllTasks();
                log.Add("NLP: listed tasks from the chat (keyword: task)");

                if (tasks.Count == 0)
                    return "You don't have any tasks yet. Try 'Add a task to enable two-factor authentication'.";

                List<string> lines = new List<string> { "Here are your tasks:" };
                int showCount = Math.Min(tasks.Count, 10);

                for (int i = 0; i < showCount; i++)
                {
                    TaskItem task = tasks[i];
                    string status = task.IsCompleted ? "[Done]" : "[Pending]";
                    string reminder = task.ReminderDate.HasValue
                        ? " - reminder " + task.ReminderDate.Value.ToString("dd MMM yyyy HH:mm")
                        : " - no reminder";
                    lines.Add($"{i + 1}. {status} {task.Title}{reminder}");
                }

                if (tasks.Count > showCount)
                    lines.Add("...and more. Open the Tasks tab to see them all.");

                return string.Join(Environment.NewLine, lines);
            }
            catch (Exception)
            {
                return DatabaseErrorMessage;
            }
        }

        // Handler 5: "add a task to...", "remind me to...", "set a reminder..."
        private string? HandleAddTask(string input)
        {
            bool wantsTask = input.Contains("task") && Regex.IsMatch(input, @"\b(add|create|new|make|set up|setup)\b");
            bool wantsReminder = input.Contains("remind");

            if (!wantsTask && !wantsReminder)
                return null;

            TaskRequest request = NlpHelper.ParseTaskRequest(originalInput);

            // No title found: ask for it and remember any time they already gave
            if (request.Title == string.Empty)
            {
                awaitingTaskTitle = true;
                awaitingIsReminder = wantsReminder;
                awaitingReminderDate = request.Reminder;

                return wantsReminder
                    ? "Sure! What should I remind you about?"
                    : "Sure! What would you like the task to be? For example: 'Enable two-factor authentication'.";
            }

            return CreateTask(request.Title, request.Reminder, request.IsReminderRequest);
        }

        // Saves the task in the database and builds the reply
        private string CreateTask(string title, DateTime? reminder, bool isReminderRequest)
        {
            try
            {
                string description = NlpHelper.BuildDescription(title);
                int id = taskRepository.AddTask(title, description, reminder);
                TasksChanged?.Invoke();

                if (reminder.HasValue)
                {
                    log.Add($"Task added via chat: '{title}' (reminder set for {reminder.Value:dd MMM yyyy HH:mm})");

                    if (isReminderRequest)
                        return $"Reminder set for '{title}' {DescribeDate(reminder.Value)}.";

                    return $"Task added: '{title}'. {description} I'll remind you {DescribeDate(reminder.Value)}.";
                }

                log.Add($"Task added via chat: '{title}' (no reminder set)");

                // Wait for the user's answer about a reminder
                pendingReminderTaskId = id;
                pendingReminderTitle = title;

                if (isReminderRequest)
                    return $"Got it! I've saved '{title}'. When should I remind you? (for example 'in 3 days' or 'tomorrow')";

                return $"Task added with the description \"{description}\" Would you like a reminder?";
            }
            catch (Exception)
            {
                return DatabaseErrorMessage;
            }
        }

        // Turns a date into friendly text, e.g. "tomorrow at 09:00"
        private string DescribeDate(DateTime date)
        {
            if (date.Date == DateTime.Today)
                return $"today at {date:HH:mm}";

            if (date.Date == DateTime.Today.AddDays(1))
                return $"tomorrow at {date:HH:mm}";

            return $"on {date:dd MMM yyyy} at {date:HH:mm}";
        }

        // ---------- Part 2 features: memory, small talk, follow-ups, topics ----------

        // Handler 6: remembers and recalls the user's favourite topic and name
        private string? HandleMemory(string input)
        {
            // Recall questions, e.g. "what am I interested in?"
            if (input.StartsWith("what") &&
                (input.Contains("interested") || input.Contains("favourite") || input.Contains("favorite")))
            {
                if (Memory.FavouriteTopic == string.Empty)
                    return "You haven't told me yet. Try saying 'I'm interested in privacy'.";

                return $"You told me you're interested in {ResponseBank.Labels[Memory.FavouriteTopic]}.";
            }

            if (input.Contains("what") && input.Contains("my name"))
                return $"Your name is {Memory.UserName}.";

            // Storing the topic, e.g. "I'm interested in privacy"
            if (input.Contains("interested in") || input.Contains("favourite topic") || input.Contains("favorite topic"))
            {
                string topic = FindTopic(input);

                if (topic == string.Empty)
                    return "That sounds interesting! Which topic do you mean: passwords, phishing, scams, privacy, malware or safe browsing?";

                Memory.FavouriteTopic = topic;
                currentTopic = topic;
                log.Add($"Remembered favourite topic: {ResponseBank.Labels[topic]}");
                return $"Great! I'll remember that you're interested in {ResponseBank.Labels[topic]}, {Memory.UserName}. It's a crucial part of staying safe online.";
            }

            return null;
        }

        // Handler 7: basic questions from Part 1
        private string? HandleSmallTalk(string input)
        {
            if (input.Contains("how are you"))
                return $"I'm doing great, {Memory.UserName}, and ready to help you stay safe online!";

            if (input.Contains("purpose") || input.Contains("what do you do"))
                return "My purpose is to teach you about cybersecurity, like spotting phishing, using safe passwords and browsing safely.";

            if (input.Contains("what can i ask") || input.Contains("what can you do") || input == "help")
                return "You can ask me about passwords, phishing, scams, privacy, malware and safe browsing. " +
                       "I can also manage your cybersecurity tasks and reminders ('Add a task to enable 2FA'), run a quiz ('Start the quiz') and show what I've done ('Show activity log').";

            if (input == "hi" || input == "hello" || input == "hey")
                return $"Hello again, {Memory.UserName}! What would you like to learn about?";

            if (input.Contains("thank"))
                return "You're welcome! Stay safe online.";

            if (input == "bye" || input == "goodbye")
                return $"Goodbye, {Memory.UserName}! Stay safe online.";

            return null;
        }

        // Handler 8: follow-ups like "another tip" or "explain more"
        private string? HandleFollowUp(string input)
        {
            string[] followUps = { "another tip", "another one", "more", "explain", "confus", "don't understand", "dont understand", "what do you mean", "next tip", "go on" };

            // If the user names a new topic, let the topic handler deal with it
            if (FindTopic(input) != string.Empty)
                return null;

            if (!followUps.Any(phrase => input.Contains(phrase)))
                return null;

            if (currentTopic == string.Empty)
                return "Sure! Which topic would you like to explore: passwords, phishing, scams, privacy, malware or safe browsing?";

            bool needsExplaining = input.Contains("explain") || input.Contains("confus") ||
                                   input.Contains("understand") || input.Contains("what do you mean");

            string prefix = needsExplaining ? "No problem, let me explain it another way. " : "Here's another one. ";
            return prefix + GetRandomTip(currentTopic);
        }

        // Handler 9: keyword topics and sentiment
        private string? HandleTopicOrSentiment(string input)
        {
            string topic = FindTopic(input);
            string sentiment = SentimentDetector.Detect(input);

            if (topic == string.Empty && sentiment == string.Empty)
                return null;

            // Sentiment with no topic: use the current topic, or general advice
            if (topic == string.Empty)
                topic = currentTopic != string.Empty ? currentTopic : "general";

            currentTopic = topic;

            string reply = string.Empty;

            if (sentiment != string.Empty)
                reply += SentimentDetector.GetResponse(sentiment) + " ";

            reply += GetRandomTip(topic);

            // Use memory to personalise the answer
            if (Memory.FavouriteTopic != string.Empty)
                reply += $" As someone interested in {ResponseBank.Labels[Memory.FavouriteTopic]}, remember to review the security settings on your accounts.";

            string logText = $"Gave a tip about {ResponseBank.Labels[topic]}";
            if (sentiment != string.Empty)
                logText += $" (detected mood: {sentiment})";
            log.Add(logText);

            return reply;
        }

        // Looks through the keyword dictionary and returns the matching topic
        private string FindTopic(string input)
        {
            foreach (KeyValuePair<string, string[]> entry in ResponseBank.Keywords)
            {
                foreach (string word in entry.Value)
                {
                    if (input.Contains(word))
                        return entry.Key;
                }
            }
            return string.Empty;
        }

        // Picks a random tip, avoiding the same one twice in a row
        private string GetRandomTip(string topic)
        {
            List<string> tips = ResponseBank.Tips[topic];
            int index = Random.Shared.Next(tips.Count);

            if (tips.Count > 1 && index == lastTipIndex)
                index = (index + 1) % tips.Count;

            lastTipIndex = index;
            return tips[index];
        }
    }
}