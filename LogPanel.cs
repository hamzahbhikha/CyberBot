using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CyberBot
{
    // The Activity Log tab: shows the latest 5 actions with a "Show more" option
    public class LogPanel : UserControl
    {
        private readonly ActivityLog log;

        private readonly Label headingLabel = new Label();
        private readonly RichTextBox logBox = new RichTextBox();
        private readonly Panel buttonPanel = new Panel();
        private readonly Button moreButton = new Button();
        private readonly Button resetButton = new Button();

        private int visibleCount = 5;

        public LogPanel(ActivityLog log)
        {
            this.log = log;
            BuildInterface();

            // Refresh automatically whenever a new action is logged
            log.EntryAdded += RefreshView;
            RefreshView();
        }

        private void BuildInterface()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(28, 28, 28);

            headingLabel.Dock = DockStyle.Top;
            headingLabel.Height = 50;
            headingLabel.Text = "Activity Log (newest first)";
            headingLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            headingLabel.ForeColor = Color.Cyan;
            headingLabel.Padding = new Padding(20, 12, 0, 0);

            logBox.Dock = DockStyle.Fill;
            logBox.ReadOnly = true;
            logBox.BackColor = Color.FromArgb(28, 28, 28);
            logBox.ForeColor = Color.WhiteSmoke;
            logBox.Font = new Font("Segoe UI", 11);
            logBox.BorderStyle = BorderStyle.None;
            logBox.TabStop = false;

            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 60;
            buttonPanel.BackColor = Color.FromArgb(10, 10, 10);

            StyleButton(moreButton, "Show more", new Point(20, 12), 220);
            StyleButton(resetButton, "Show latest 5", new Point(250, 12), 170);

            moreButton.Click += (sender, args) =>
            {
                visibleCount += 5;
                RefreshView();
            };

            resetButton.Click += (sender, args) =>
            {
                visibleCount = 5;
                RefreshView();
            };

            buttonPanel.Controls.Add(moreButton);
            buttonPanel.Controls.Add(resetButton);

            // Fill control first, then the docked edges
            Controls.Add(logBox);
            Controls.Add(buttonPanel);
            Controls.Add(headingLabel);
        }

        private void StyleButton(Button button, string text, Point location, int width)
        {
            button.Text = text;
            button.Location = location;
            button.Size = new Size(width, 38);
            button.BackColor = Color.FromArgb(0, 150, 136);
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void RefreshView()
        {
            logBox.Clear();
            List<ActivityEntry> entries = log.GetRecent(visibleCount);

            if (entries.Count == 0)
            {
                logBox.Text = "No activity yet. Add a task, take the quiz or chat with me and your actions will appear here.";
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    logBox.AppendText($"{i + 1}. [{entries[i].Time:dd MMM yyyy HH:mm:ss}] {entries[i].Description}"
                                      + Environment.NewLine + Environment.NewLine);
                }
            }

            int older = log.Count - entries.Count;
            moreButton.Visible = older > 0;
            moreButton.Text = $"Show more ({older} older)";
            resetButton.Visible = visibleCount > 5;
        }
    }
}