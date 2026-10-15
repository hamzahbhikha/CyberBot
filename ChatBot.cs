using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberBot
{
    // Delegate: each handler takes the user's lowercase text and returns a reply,
    // or null if it can't handle that message
    public delegate string? ResponseHandler(string input);

    public class ChatBot
    {
        public UserMemory Memory { get; } = new UserMemory();

        private readonly List<ResponseHandler> handlers = new List<ResponseHandler>();
        private string currentTopic = string.Empty;
        private int lastTipIndex = -1;

        public ChatBot()
        {
            // Order matters: the first handler that returns a reply wins
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

            string lower = input.Trim().ToLower();

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
                   "Try 'Tell me about password safety' or 'I'm worried about online scams'.";
        }

        // Handler 1: remembers and recalls the user's favourite topic and name
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
                return $"Great! I'll remember that you're interested in {ResponseBank.Labels[topic]}, {Memory.UserName}. It's a crucial part of staying safe online.";
            }

            return null;
        }

        // Handler 2: basic questions from Part 1
        private string? HandleSmallTalk(string input)
        {
            if (input.Contains("how are you"))
                return $"I'm doing great, {Memory.UserName}, and ready to help you stay safe online!";

            if (input.Contains("purpose") || input.Contains("what do you do"))
                return "My purpose is to teach you about cybersecurity, like spotting phishing, using safe passwords and browsing safely.";

            if (input.Contains("what can i ask") || input.Contains("what can you do") || input == "help")
                return "You can ask me about passwords, phishing, scams, privacy, malware and safe browsing. You can also tell me how you're feeling about online safety.";

            if (input == "hi" || input == "hello" || input == "hey")
                return $"Hello again, {Memory.UserName}! What would you like to learn about?";

            if (input.Contains("thank"))
                return "You're welcome! Stay safe online.";

            if (input == "bye" || input == "goodbye")
                return $"Goodbye, {Memory.UserName}! Stay safe online.";

            return null;
        }

        // Handler 3: follow-ups like "another tip" or "explain more"
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

        // Handler 4: keyword topics and sentiment
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