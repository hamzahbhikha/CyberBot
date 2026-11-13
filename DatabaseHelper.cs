using MySqlConnector;

namespace CyberBot
{
    // Connects to MySQL and creates the database and table if they don't exist yet
    public static class DatabaseHelper
    {
        // CHANGE THIS to the root password you chose when installing MySQL
        private const string Server = "localhost";
        private const string User = "root";
        private const string Password = "Moonlight#47!";
        private const string DatabaseName = "cyberbot_db";

        // Connection string without a database (used to create the database)
        private static string ServerConnectionString
        {
            get
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
                {
                    Server = Server,
                    Port = 3306,
                    UserID = User,
                    Password = Password
                };
                return builder.ConnectionString;
            }
        }

        // Connection string for the chatbot database
        public static string ConnectionString
        {
            get
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(ServerConnectionString)
                {
                    Database = DatabaseName
                };
                return builder.ConnectionString;
            }
        }

        // Creates the database and tasks table if needed. Throws an exception if MySQL can't be reached.
        public static void Initialize()
        {
            using (MySqlConnection connection = new MySqlConnection(ServerConnectionString))
            {
                connection.Open();
                string createDatabase = $"CREATE DATABASE IF NOT EXISTS {DatabaseName};";
                using MySqlCommand command = new MySqlCommand(createDatabase, connection);
                command.ExecuteNonQuery();
            }

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                string createTable = @"CREATE TABLE IF NOT EXISTS tasks (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(200) NOT NULL,
                    Description VARCHAR(500) NOT NULL,
                    ReminderDate DATETIME NULL,
                    IsCompleted TINYINT(1) NOT NULL DEFAULT 0,
                    CreatedAt DATETIME NOT NULL
                );";
                using MySqlCommand command = new MySqlCommand(createTable, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}