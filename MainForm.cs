using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberBot
{
    public class MainForm : Form
    {
        private readonly ActivityLog log = new ActivityLog();
        private readonly TaskRepository repository = new TaskRepository();
        private readonly ChatBot bot;

        private readonly TasksPanel tasksPanel;
        private readonly QuizPanel quizPanel;
        private readonly LogPanel logPanel;

        // Header and navigation
        private readonly Panel headerPanel = new Panel();
        private readonly Label logoLabel = new Label();
        private readonly FlowLayoutPanel navPanel = new FlowLayoutPanel();
        private readonly Button chatButton = new Button();
        private readonly Button tasksButton = new Button();
        private readonly Button quizButton = new Button();
        private readonly Button logButton = new Button();
        private readonly Panel contentPanel = new Panel();

        // Chat screen
        private readonly Panel chatView = new Panel();
        private readonly Panel chatPanel = new Panel();
        private readonly RichTextBox chatBox = new RichTextBox();
        private readonly Panel inputPanel = new Panel();
        private readonly TextBox inputBox = new TextBox();
        private readonly Button sendButton = new Button();

        // Reminder checking
        private readonly System.Windows.Forms.Timer reminderTimer = new System.Windows.Forms.Timer();
        private readonly HashSet<int> notifiedReminders = new HashSet<int>();
        private bool botIsTyping;

        private readonly Color botColor = Color.LimeGreen;
        private readonly Color userColor = Color.DeepSkyBlue;
        private readonly Color textColor = Color.WhiteSmoke;
        private readonly Color dividerColor = Color.DimGray;
        private readonly Color activeNavColor = Color.FromArgb(0, 150, 136);
        private readonly Color inactiveNavColor = Color.FromArgb(40, 40, 40);

        // Windows messages used to stop the chat box flickering while the bot types
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_SETREDRAW = 0x000B;
        private const int WM_VSCROLL = 0x0115;
        private const int SB_BOTTOM = 7;

        public MainForm()
        {
            bot = new ChatBot(log, repository);
            tasksPanel = new TasksPanel(repository, log);
            quizPanel = new QuizPanel(log);
            logPanel = new LogPanel(log);

            BuildInterface();

            // Connect chat commands to the other screens
            bot.TasksChanged += () => tasksPanel.LoadTasks();
            bot.QuizRequested += () =>
            {
                ShowView(quizPanel, quizButton);
                quizPanel.StartQuiz();
            };

            reminderTimer.Interval = 20000;
            reminderTimer.Tick += async (sender, args) => await CheckRemindersAsync();

            Load += async (sender, args) => await StartAsync();
            sendButton.Click += async (sender, args) => await SendMessageAsync();
        }

        // Creates and styles all the controls
        private void BuildInterface()
        {
            Text = "Cybersecurity Awareness Bot";
            Size = new Size(950, 850);
            MinimumSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(20, 20, 20);

            // Header with the ASCII art logo
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 170;
            headerPanel.BackColor = Color.FromArgb(10, 10, 10);

            logoLabel.Dock = DockStyle.Fill;
            logoLabel.Text = AsciiArt.Logo;
            logoLabel.Font = new Font("Consolas", 10);
            logoLabel.ForeColor = Color.Cyan;
            logoLabel.Padding = new Padding(20, 10, 0, 0);
            logoLabel.UseMnemonic = false;
            headerPanel.Controls.Add(logoLabel);

            // Navigation bar
            navPanel.Dock = DockStyle.Top;
            navPanel.Height = 40;
            navPanel.BackColor = Color.FromArgb(10, 10, 10);
            navPanel.FlowDirection = FlowDirection.LeftToRight;
            navPanel.WrapContents = false;
            navPanel.Padding = new Padding(0);

            StyleNavButton(chatButton, "Chat");
            StyleNavButton(tasksButton, "Tasks");
            StyleNavButton(quizButton, "Quiz");
            StyleNavButton(logButton, "Activity Log");

            navPanel.Controls.Add(chatButton);
            navPanel.Controls.Add(tasksButton);
            navPanel.Controls.Add(quizButton);
            navPanel.Controls.Add(logButton);

            // Chat window
            chatBox.Dock = DockStyle.Fill;
            chatBox.ReadOnly = true;
            chatBox.BackColor = Color.FromArgb(28, 28, 28);
            chatBox.ForeColor = textColor;
            chatBox.Font = new Font("Segoe UI", 12);
            chatBox.BorderStyle = BorderStyle.None;
            chatBox.TabStop = false;

            chatPanel.Dock = DockStyle.Fill;
            chatPanel.Padding = new Padding(15, 10, 5, 10);
            chatPanel.BackColor = Color.FromArgb(28, 28, 28);
            chatPanel.Controls.Add(chatBox);

            // Input area at the bottom of the chat
            inputPanel.Dock = DockStyle.Bottom;
            inputPanel.Height = 60;
            inputPanel.Padding = new Padding(15, 10, 15, 10);
            inputPanel.BackColor = Color.FromArgb(10, 10, 10);

            inputBox.Dock = DockStyle.Fill;
            inputBox.Multiline = true;
            inputBox.Font = new Font("Segoe UI", 12);
            inputBox.BackColor = Color.FromArgb(45, 45, 45);
            inputBox.ForeColor = Color.White;
            inputBox.BorderStyle = BorderStyle.FixedSingle;

            sendButton.Dock = DockStyle.Right;
            sendButton.Width = 110;
            sendButton.Text = "Send";
            sendButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            sendButton.BackColor = Color.FromArgb(0, 150, 136);
            sendButton.ForeColor = Color.White;
            sendButton.FlatStyle = FlatStyle.Flat;
            sendButton.FlatAppearance.BorderSize = 0;

            inputPanel.Controls.Add(inputBox);
            inputPanel.Controls.Add(sendButton);

            chatView.Dock = DockStyle.Fill;
            chatView.Controls.Add(chatPanel);
            chatView.Controls.Add(inputPanel);

            // Content area holds the four screens (only one is visible at a time)
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(chatView);
            contentPanel.Controls.Add(tasksPanel);
            contentPanel.Controls.Add(quizPanel);
            contentPanel.Controls.Add(logPanel);

            // Fill control first, then the docked edges
            Controls.Add(contentPanel);
            Controls.Add(navPanel);
            Controls.Add(headerPanel);

            chatButton.Click += (sender, args) => ShowView(chatView, chatButton);
            tasksButton.Click += (sender, args) => ShowView(tasksPanel, tasksButton);
            quizButton.Click += (sender, args) => ShowView(quizPanel, quizButton);
            logButton.Click += (sender, args) => ShowView(logPanel, logButton);

            ShowView(chatView, chatButton);
        }

        private void StyleNavButton(Button button, string text)
        {
            button.Text = text;
            button.Size = new Size(160, 40);
            button.Margin = new Padding(0);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            button.ForeColor = Color.White;
            button.BackColor = inactiveNavColor;
        }

        // Shows one screen and hides the others
        private void ShowView(Control view, Button activeButton)
        {
            chatView.Visible = view == chatView;
            tasksPanel.Visible = view == tasksPanel;
            quizPanel.Visible = view == quizPanel;
            logPanel.Visible = view == logPanel;

            foreach (Button button in new[] { chatButton, tasksButton, quizButton, logButton })
            {
                button.BackColor = button == activeButton ? activeNavColor : inactiveNavColor;
            }

            // Enter should only send a chat message while the chat screen is showing
            if (view == chatView)
            {
                AcceptButton = sendButton;
                chatButton.Text = "Chat";
                if (inputBox.Enabled)
                    inputBox.Focus();
            }
            else
            {
                AcceptButton = null;
            }

            if (view == tasksPanel)
                tasksPanel.LoadTasks();
        }

        // Runs when the window opens: voice greeting first, then the text greeting
        private async Task StartAsync()
        {
            log.Add("Chatbot started (voice greeting played)");

            SetInputEnabled(false);
            await Task.Run(PlayGreetingSound);
            await BotSayAsync(bot.GetWelcomeMessage());
            SetInputEnabled(true);

            reminderTimer.Start();
        }

        private void PlayGreetingSound()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "greeting.wav");
                using SoundPlayer player = new SoundPlayer(path);
                player.PlaySync();
            }
            catch
            {
                // If the audio file is missing, carry on without sound
            }
        }

        private async Task SendMessageAsync()
        {
            if (botIsTyping)
                return;

            string text = inputBox.Text.Trim();
            inputBox.Clear();

            if (text.Length > 0)
            {
                string label = bot.Memory.UserName == string.Empty ? "You" : bot.Memory.UserName;
                AppendColored(label + ": ", userColor);
                AppendColored(text + Environment.NewLine, textColor);
            }

            SetInputEnabled(false);

            try
            {
                string reply = bot.Process(text);
                await BotSayAsync(reply);
            }
            catch (Exception)
            {
                await BotSayAsync("Something went wrong on my side, but I'm still here. Please try again.");
            }

            SetInputEnabled(true);
        }

        // Checks the database for reminders that are due and tells the user in the chat
        private async Task CheckRemindersAsync()
        {
            if (botIsTyping || bot.Memory.UserName == string.Empty)
                return;

            List<TaskItem> dueTasks;

            try
            {
                dueTasks = repository.GetDueReminders();
            }
            catch (Exception)
            {
                return;
            }

            foreach (TaskItem task in dueTasks)
            {
                if (notifiedReminders.Contains(task.Id))
                    continue;

                notifiedReminders.Add(task.Id);
                log.Add($"Reminder shown: '{task.Title}'");
                SystemSounds.Asterisk.Play();

                if (!chatView.Visible)
                    chatButton.Text = "Chat (new)";

                SetInputEnabled(false);
                await BotSayAsync($"Reminder: '{task.Title}' is due. {task.Description}");
                SetInputEnabled(true);
            }
        }

        // Types the bot's message a couple of characters at a time, then adds a divider
        private async Task BotSayAsync(string message)
        {
            botIsTyping = true;

            try
            {
                AppendColored("Bot: ", botColor);

                for (int i = 0; i < message.Length; i += 2)
                {
                    int length = Math.Min(2, message.Length - i);
                    AppendColored(message.Substring(i, length), textColor);
                    await Task.Delay(10);
                }

                AppendColored(Environment.NewLine, textColor);
                AppendColored(new string('-', 70) + Environment.NewLine, dividerColor);
            }
            finally
            {
                botIsTyping = false;
            }
        }

        private void AppendColored(string text, Color color)
        {
            // Pause redrawing so the chat box doesn't flicker while it updates
            SendMessage(chatBox.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.SelectionLength = 0;
            chatBox.SelectionColor = color;
            chatBox.AppendText(text);
            chatBox.SelectionColor = chatBox.ForeColor;

            // Jump to the bottom, then turn redrawing back on
            SendMessage(chatBox.Handle, WM_VSCROLL, new IntPtr(SB_BOTTOM), IntPtr.Zero);
            SendMessage(chatBox.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            chatBox.Invalidate();
        }

        private void SetInputEnabled(bool enabled)
        {
            inputBox.Enabled = enabled;
            sendButton.Enabled = enabled;

            if (enabled && chatView.Visible)
                inputBox.Focus();
        }
    }
}