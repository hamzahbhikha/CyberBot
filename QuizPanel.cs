using System;
using System.Drawing;
using System.Windows.Forms;

namespace CyberBot
{
    // The Quiz tab: shows one question at a time with instant feedback
    public class QuizPanel : UserControl
    {
        private const int ContentWidth = 700;

        private readonly ActivityLog log;
        private readonly QuizGame game = new QuizGame();

        private readonly FlowLayoutPanel layout = new FlowLayoutPanel();
        private readonly Label titleLabel = new Label();
        private readonly Label progressLabel = new Label();
        private readonly Label questionLabel = new Label();
        private readonly Button[] optionButtons = new Button[4];
        private readonly Label feedbackLabel = new Label();
        private readonly Button nextButton = new Button();
        private readonly Button startButton = new Button();

        private readonly Color optionColor = Color.FromArgb(55, 55, 55);
        private readonly Color hoverColor = Color.FromArgb(75, 75, 75);
        private readonly Color correctColor = Color.FromArgb(46, 125, 50);
        private readonly Color wrongColor = Color.FromArgb(183, 28, 28);

        private bool answered;

        public QuizPanel(ActivityLog log)
        {
            this.log = log;
            BuildInterface();
            ShowIntro();
        }

        private void BuildInterface()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(28, 28, 28);

            layout.Dock = DockStyle.Fill;
            layout.FlowDirection = FlowDirection.TopDown;
            layout.WrapContents = false;
            layout.AutoScroll = true;
            layout.Padding = new Padding(30, 20, 20, 20);

            StyleLabel(titleLabel, 18, FontStyle.Bold, Color.Cyan, 45);
            titleLabel.Text = "Cybersecurity Quiz";
            StyleLabel(progressLabel, 12, FontStyle.Regular, Color.Silver, 30);
            StyleLabel(questionLabel, 15, FontStyle.Bold, Color.White, 90);

            layout.Controls.Add(titleLabel);
            layout.Controls.Add(progressLabel);
            layout.Controls.Add(questionLabel);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                Button button = new Button();
                button.Size = new Size(ContentWidth, 46);
                button.Margin = new Padding(0, 0, 0, 8);
                button.Padding = new Padding(12, 0, 0, 0);
                button.TextAlign = ContentAlignment.MiddleLeft;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Font = new Font("Segoe UI", 12);
                button.ForeColor = Color.White;
                button.Tag = i;
                button.Click += OptionClicked;
                ResetOption(button);

                optionButtons[i] = button;
                layout.Controls.Add(button);
            }

            StyleLabel(feedbackLabel, 12, FontStyle.Regular, Color.White, 90);
            layout.Controls.Add(feedbackLabel);

            StyleActionButton(nextButton, "Next question");
            nextButton.Click += (sender, args) => GoToNextQuestion();
            layout.Controls.Add(nextButton);

            StyleActionButton(startButton, "Start Quiz");
            startButton.Click += (sender, args) => StartQuiz();
            layout.Controls.Add(startButton);

            Controls.Add(layout);
        }

        private void StyleLabel(Label label, float size, FontStyle style, Color color, int height)
        {
            label.AutoSize = false;
            label.Size = new Size(ContentWidth, height);
            label.Font = new Font("Segoe UI", size, style);
            label.ForeColor = color;
            label.Margin = new Padding(0, 0, 0, 10);
        }

        private void StyleActionButton(Button button, string text)
        {
            button.Text = text;
            button.Size = new Size(200, 42);
            button.Margin = new Padding(0, 5, 0, 5);
            button.BackColor = Color.FromArgb(0, 150, 136);
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void ResetOption(Button button)
        {
            button.BackColor = optionColor;
            button.FlatAppearance.MouseOverBackColor = hoverColor;
        }

        private void ColourOption(Button button, Color color)
        {
            button.BackColor = color;
            button.FlatAppearance.MouseOverBackColor = color;
        }

        // Screen shown before the quiz starts
        private void ShowIntro()
        {
            progressLabel.Text = string.Empty;
            questionLabel.Text = "Test your cybersecurity knowledge! Answer one question at a time and get instant feedback.";
            feedbackLabel.Text = string.Empty;

            foreach (Button button in optionButtons)
                button.Visible = false;

            nextButton.Visible = false;
            startButton.Text = "Start Quiz";
            startButton.Visible = true;
        }

        // Starts (or restarts) the quiz. Called by the Start button and by the chat command.
        public void StartQuiz()
        {
            game.Start();
            log.Add($"Quiz started ({game.TotalQuestions} questions)");
            startButton.Visible = false;
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            QuizQuestion? question = game.CurrentQuestion;

            if (question == null)
            {
                ShowResults();
                return;
            }

            answered = false;
            progressLabel.Text = $"Question {game.QuestionNumber} of {game.TotalQuestions}   |   Score: {game.Score}";
            questionLabel.Text = question.Text;
            feedbackLabel.Text = string.Empty;
            nextButton.Visible = false;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < question.Options.Count)
                {
                    optionButtons[i].Text = (char)('A' + i) + ") " + question.Options[i];
                    ResetOption(optionButtons[i]);
                    optionButtons[i].Visible = true;
                }
                else
                {
                    optionButtons[i].Visible = false;
                }
            }
        }

        private void OptionClicked(object? sender, EventArgs e)
        {
            if (answered) return;
            if (sender is not Button clicked || clicked.Tag is not int chosenIndex) return;

            answered = true;

            QuizQuestion? question = game.CurrentQuestion;
            string feedback = game.SubmitAnswer(chosenIndex, out bool isCorrect);

            if (question != null)
                ColourOption(optionButtons[question.CorrectIndex], correctColor);

            if (!isCorrect)
                ColourOption(clicked, wrongColor);

            feedbackLabel.ForeColor = isCorrect ? Color.LimeGreen : Color.OrangeRed;
            feedbackLabel.Text = feedback;
            progressLabel.Text = $"Question {game.QuestionNumber} of {game.TotalQuestions}   |   Score: {game.Score}";

            nextButton.Text = game.QuestionNumber == game.TotalQuestions ? "See results" : "Next question";
            nextButton.Visible = true;
        }

        private void GoToNextQuestion()
        {
            game.NextQuestion();

            if (game.IsFinished)
                ShowResults();
            else
                ShowQuestion();
        }

        private void ShowResults()
        {
            log.Add($"Quiz completed: scored {game.Score} out of {game.TotalQuestions}");

            foreach (Button button in optionButtons)
                button.Visible = false;

            nextButton.Visible = false;
            questionLabel.Text = "Quiz complete!";
            progressLabel.Text = $"Final score: {game.Score} / {game.TotalQuestions}";
            feedbackLabel.ForeColor = Color.Cyan;
            feedbackLabel.Text = game.GetFinalFeedback();
            startButton.Text = "Play again";
            startButton.Visible = true;
        }
    }
}