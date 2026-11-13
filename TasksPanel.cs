using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CyberBot
{
    // The Tasks tab: add, view, complete and delete tasks (stored in MySQL)
    public class TasksPanel : UserControl
    {
        private readonly TaskRepository repository;
        private readonly ActivityLog log;

        private readonly Panel formPanel = new Panel();
        private readonly Label titleLabel = new Label();
        private readonly TextBox titleBox = new TextBox();
        private readonly Label descriptionLabel = new Label();
        private readonly TextBox descriptionBox = new TextBox();
        private readonly CheckBox reminderCheck = new CheckBox();
        private readonly DateTimePicker reminderPicker = new DateTimePicker();
        private readonly Button addButton = new Button();

        private readonly ListView taskList = new ListView();

        private readonly Panel buttonPanel = new Panel();
        private readonly Button completeButton = new Button();
        private readonly Button deleteButton = new Button();
        private readonly Button refreshButton = new Button();

        public TasksPanel(TaskRepository repository, ActivityLog log)
        {
            this.repository = repository;
            this.log = log;
            BuildInterface();
        }

        private void BuildInterface()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(28, 28, 28);

            // Top form for adding a task
            formPanel.Dock = DockStyle.Top;
            formPanel.Height = 185;
            formPanel.BackColor = Color.FromArgb(28, 28, 28);

            StyleLabel(titleLabel, "Task title", new Point(20, 22));
            titleBox.Location = new Point(150, 18);
            titleBox.Size = new Size(520, 28);
            titleBox.MaxLength = 200;
            StyleTextBox(titleBox);

            StyleLabel(descriptionLabel, "Description", new Point(20, 62));
            descriptionBox.Location = new Point(150, 58);
            descriptionBox.Size = new Size(520, 28);
            descriptionBox.MaxLength = 500;
            StyleTextBox(descriptionBox);

            reminderCheck.Text = "Set a reminder";
            reminderCheck.Location = new Point(150, 100);
            reminderCheck.Size = new Size(130, 28);
            reminderCheck.ForeColor = Color.White;
            reminderCheck.Font = new Font("Segoe UI", 10);
            reminderCheck.CheckedChanged += (sender, args) => reminderPicker.Enabled = reminderCheck.Checked;

            reminderPicker.Location = new Point(290, 98);
            reminderPicker.Size = new Size(260, 28);
            reminderPicker.Format = DateTimePickerFormat.Custom;
            reminderPicker.CustomFormat = "dd MMM yyyy  HH:mm";
            reminderPicker.Font = new Font("Segoe UI", 10);
            reminderPicker.Value = DateTime.Now.AddDays(1);
            reminderPicker.Enabled = false;

            StyleButton(addButton, "Add Task", Color.FromArgb(0, 150, 136), new Point(150, 138), 150);
            addButton.Click += (sender, args) => AddTask();

            formPanel.Controls.Add(titleLabel);
            formPanel.Controls.Add(titleBox);
            formPanel.Controls.Add(descriptionLabel);
            formPanel.Controls.Add(descriptionBox);
            formPanel.Controls.Add(reminderCheck);
            formPanel.Controls.Add(reminderPicker);
            formPanel.Controls.Add(addButton);

            // Task list in the middle
            taskList.Dock = DockStyle.Fill;
            taskList.View = View.Details;
            taskList.FullRowSelect = true;
            taskList.HideSelection = false;
            taskList.MultiSelect = false;
            taskList.GridLines = true;
            taskList.BackColor = Color.FromArgb(38, 38, 38);
            taskList.ForeColor = Color.White;
            taskList.Font = new Font("Segoe UI", 10);
            taskList.BorderStyle = BorderStyle.None;
            taskList.Columns.Add("ID", 50);
            taskList.Columns.Add("Title", 230);
            taskList.Columns.Add("Description", 340);
            taskList.Columns.Add("Reminder", 160);
            taskList.Columns.Add("Status", 100);

            // Buttons at the bottom
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 60;
            buttonPanel.BackColor = Color.FromArgb(10, 10, 10);

            StyleButton(completeButton, "Mark Completed", Color.FromArgb(46, 125, 50), new Point(20, 12), 170);
            StyleButton(deleteButton, "Delete Task", Color.FromArgb(183, 28, 28), new Point(200, 12), 150);
            StyleButton(refreshButton, "Refresh", Color.FromArgb(69, 90, 100), new Point(360, 12), 120);

            completeButton.Click += (sender, args) => CompleteSelectedTask();
            deleteButton.Click += (sender, args) => DeleteSelectedTask();
            refreshButton.Click += (sender, args) => LoadTasks();

            buttonPanel.Controls.Add(completeButton);
            buttonPanel.Controls.Add(deleteButton);
            buttonPanel.Controls.Add(refreshButton);

            // Fill control first, then the docked edges
            Controls.Add(taskList);
            Controls.Add(buttonPanel);
            Controls.Add(formPanel);
        }

        private void StyleLabel(Label label, string text, Point location)
        {
            label.Text = text;
            label.Location = location;
            label.AutoSize = true;
            label.ForeColor = Color.White;
            label.Font = new Font("Segoe UI", 11);
        }

        private void StyleTextBox(TextBox box)
        {
            box.Font = new Font("Segoe UI", 11);
            box.BackColor = Color.FromArgb(45, 45, 45);
            box.ForeColor = Color.White;
            box.BorderStyle = BorderStyle.FixedSingle;
        }

        private void StyleButton(Button button, string text, Color color, Point location, int width)
        {
            button.Text = text;
            button.Location = location;
            button.Size = new Size(width, 38);
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        // Loads all tasks from the database into the list
        public void LoadTasks(bool silent = false)
        {
            try
            {
                List<TaskItem> tasks = repository.GetAllTasks();
                taskList.Items.Clear();

                foreach (TaskItem task in tasks)
                {
                    ListViewItem item = new ListViewItem(task.Id.ToString());
                    item.SubItems.Add(task.Title);
                    item.SubItems.Add(task.Description);
                    item.SubItems.Add(task.ReminderDate.HasValue ? task.ReminderDate.Value.ToString("dd MMM yyyy HH:mm") : "None");
                    item.SubItems.Add(task.IsCompleted ? "Completed" : "Pending");

                    if (task.IsCompleted)
                        item.ForeColor = Color.Gray;

                    taskList.Items.Add(item);
                }
            }
            catch (Exception)
            {
                if (!silent)
                    ShowDatabaseError();
            }
        }

        private void AddTask()
        {
            string title = titleBox.Text.Trim();
            string description = descriptionBox.Text.Trim();

            if (title.Length == 0)
            {
                MessageBox.Show("Please enter a task title.", "Missing title", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (description.Length == 0)
                description = NlpHelper.BuildDescription(title);

            DateTime? reminder = null;
            if (reminderCheck.Checked)
            {
                if (reminderPicker.Value <= DateTime.Now)
                {
                    MessageBox.Show("Please choose a reminder time in the future.", "Invalid reminder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                reminder = reminderPicker.Value;
            }

            try
            {
                repository.AddTask(title, description, reminder);

                if (reminder.HasValue)
                    log.Add($"Task added: '{title}' (reminder set for {reminder.Value:dd MMM yyyy HH:mm})");
                else
                    log.Add($"Task added: '{title}' (no reminder set)");

                titleBox.Clear();
                descriptionBox.Clear();
                reminderCheck.Checked = false;
                LoadTasks();
            }
            catch (Exception)
            {
                ShowDatabaseError();
            }
        }

        // Returns the Id of the selected task, or 0 if nothing is selected
        private int GetSelectedTaskId()
        {
            if (taskList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a task in the list first.", "No task selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 0;
            }

            return int.Parse(taskList.SelectedItems[0].Text);
        }

        private void CompleteSelectedTask()
        {
            int id = GetSelectedTaskId();
            if (id == 0) return;

            string title = taskList.SelectedItems[0].SubItems[1].Text;

            try
            {
                repository.MarkCompleted(id);
                log.Add($"Task completed: '{title}'");
                LoadTasks();
            }
            catch (Exception)
            {
                ShowDatabaseError();
            }
        }

        private void DeleteSelectedTask()
        {
            int id = GetSelectedTaskId();
            if (id == 0) return;

            string title = taskList.SelectedItems[0].SubItems[1].Text;

            DialogResult answer = MessageBox.Show($"Delete the task '{title}'?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes) return;

            try
            {
                repository.DeleteTask(id);
                log.Add($"Task deleted: '{title}'");
                LoadTasks();
            }
            catch (Exception)
            {
                ShowDatabaseError();
            }
        }

        private void ShowDatabaseError()
        {
            MessageBox.Show("Could not reach the database. Please check that MySQL is running and try again.",
                "Database error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}