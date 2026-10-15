using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberBot
{
    public class MainForm : Form
    {
        private readonly ChatBot bot = new ChatBot();

        private readonly Panel headerPanel = new Panel();
        private readonly Label logoLabel = new Label();
        private readonly RichTextBox chatBox = new RichTextBox();
        private readonly Panel inputPanel = new Panel();
        private readonly TextBox inputBox = new TextBox();
        private readonly Button sendButton = new Button();

        private readonly Color botColor = Color.LimeGreen;
        private readonly Color userColor = Color.DeepSkyBlue;
        private readonly Color textColor = Color.WhiteSmoke;
        private readonly Color dividerColor = Color.DimGray;

        public MainForm()
        {
            BuildInterface();
            Load += async (sender, args) => await StartAsync();
            sendButton.Click += async (sender, args) => await SendMessageAsync();
        }

        // Creates and styles all the controls
        private void BuildInterface()
        {
            Text = "Cybersecurity Awareness Bot";
            Size = new Size(850, 750);
            MinimumSize = new Size(600, 500);
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

            // Chat window
            chatBox.Dock = DockStyle.Fill;
            chatBox.ReadOnly = true;
            chatBox.BackColor = Color.FromArgb(28, 28, 28);
            chatBox.ForeColor = textColor;
            chatBox.Font = new Font("Segoe UI", 12);
            chatBox.BorderStyle = BorderStyle.None;
            chatBox.TabStop = false;

            // Input area at the bottom
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

            // Fill control is added first, then the docked edges
            Controls.Add(chatBox);
            Controls.Add(inputPanel);
            Controls.Add(headerPanel);

            // Pressing Enter clicks Send
            AcceptButton = sendButton;
        }

        // Runs when the window opens: voice greeting first, then the text greeting
        private async Task StartAsync()
        {
            SetInputEnabled(false);
            await Task.Run(PlayGreetingSound);
            await BotSayAsync(bot.GetWelcomeMessage());
            SetInputEnabled(true);
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

        // Types the bot's message one character at a time, then adds a divider
        private async Task BotSayAsync(string message)
        {
            AppendColored("Bot: ", botColor);

            foreach (char c in message)
            {
                AppendColored(c.ToString(), textColor);
                await Task.Delay(8);
            }

            AppendColored(Environment.NewLine, textColor);
            AppendColored(new string('-', 70) + Environment.NewLine, dividerColor);
        }

        private void AppendColored(string text, Color color)
        {
            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.SelectionLength = 0;
            chatBox.SelectionColor = color;
            chatBox.AppendText(text);
            chatBox.SelectionColor = chatBox.ForeColor;
            chatBox.ScrollToCaret();
        }

        private void SetInputEnabled(bool enabled)
        {
            inputBox.Enabled = enabled;
            sendButton.Enabled = enabled;

            if (enabled)
                inputBox.Focus();
        }
    }
}