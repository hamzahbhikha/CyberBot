using System;
using System.Collections.Generic;
using MySqlConnector;

namespace CyberBot
{
    // All database actions for tasks (add, read, update, delete)
    public class TaskRepository
    {
        // Adds a task and returns its new Id
        public int AddTask(string title, string description, DateTime? reminderDate)
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            string sql = "INSERT INTO tasks (Title, Description, ReminderDate, IsCompleted, CreatedAt) " +
                         "VALUES (@title, @description, @reminder, 0, @created);";

            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@description", description);
            command.Parameters.AddWithValue("@reminder", (object?)reminderDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@created", DateTime.Now);
            command.ExecuteNonQuery();

            return (int)command.LastInsertedId;
        }

        // Returns every task: unfinished ones first, newest first
        public List<TaskItem> GetAllTasks()
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            string sql = "SELECT Id, Title, Description, ReminderDate, IsCompleted, CreatedAt " +
                         "FROM tasks ORDER BY IsCompleted, Id DESC;";

            using MySqlCommand command = new MySqlCommand(sql, connection);
            return ReadTasks(command);
        }

        // Returns unfinished tasks whose reminder time has passed
        public List<TaskItem> GetDueReminders()
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            string sql = "SELECT Id, Title, Description, ReminderDate, IsCompleted, CreatedAt " +
                         "FROM tasks WHERE IsCompleted = 0 AND ReminderDate IS NOT NULL AND ReminderDate <= @now;";

            using MySqlCommand command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@now", DateTime.Now);
            return ReadTasks(command);
        }

        // Sets or clears the reminder for a task
        public bool UpdateReminder(int id, DateTime? reminderDate)
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("UPDATE tasks SET ReminderDate = @reminder WHERE Id = @id;", connection);
            command.Parameters.AddWithValue("@reminder", (object?)reminderDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", id);
            return command.ExecuteNonQuery() > 0;
        }

        // Marks a task as completed
        public bool MarkCompleted(int id)
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("UPDATE tasks SET IsCompleted = 1 WHERE Id = @id;", connection);
            command.Parameters.AddWithValue("@id", id);
            return command.ExecuteNonQuery() > 0;
        }

        // Deletes a task
        public bool DeleteTask(int id)
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("DELETE FROM tasks WHERE Id = @id;", connection);
            command.Parameters.AddWithValue("@id", id);
            return command.ExecuteNonQuery() > 0;
        }

        // Turns database rows into TaskItem objects
        private List<TaskItem> ReadTasks(MySqlCommand command)
        {
            List<TaskItem> tasks = new List<TaskItem>();

            using MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                TaskItem task = new TaskItem();
                task.Id = reader.GetInt32(0);
                task.Title = reader.GetString(1);
                task.Description = reader.GetString(2);
                task.ReminderDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3);
                task.IsCompleted = reader.GetBoolean(4);
                task.CreatedAt = reader.GetDateTime(5);
                tasks.Add(task);
            }

            return tasks;
        }
    }
}