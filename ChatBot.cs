using System;

namespace CyberBot
{
    public class ChatBot
    {
        public string UserName { get; set; } // automatic property (marking requirement)

        public void Start()
        {
            ConsoleUI.Divider();
            ConsoleUI.TypeLine("Hello! Welcome to the Cybersecurity Awareness Bot.");
            ConsoleUI.TypeLine("What is your name?");
            ConsoleUI.Divider();

            Console.Write("You: ");
            UserName = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(UserName))
            {
                Console.Write("I didn't catch that. Please enter your name: ");
                UserName = Console.ReadLine();
            }

            ConsoleUI.TypeLine($"Nice to meet you, {UserName}! I'm here to help you stay safe online.");
            ConsoleUI.TypeLine("You can ask me things like 'How are you?', 'What's your purpose?', or 'What can I ask you about?'");
            ConsoleUI.TypeLine("Type 'exit' to quit.");

            RunConversation();
        }

        private void RunConversation()
        {
            while (true)
            {
                ConsoleUI.Divider();
                Console.Write($"{UserName}: ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    ConsoleUI.TypeLine("I didn't quite understand that. Could you rephrase?");
                    continue;
                }

                input = input.Trim().ToLower();

                if (input == "exit")
                {
                    ConsoleUI.TypeLine($"Goodbye, {UserName}! Stay safe online.");
                    break;
                }

                string response = GetResponse(input);
                ConsoleUI.TypeLine(response);
            }
        }

        private string GetResponse(string input)
        {
            if (input.Contains("how are you"))
                return "I'm functioning perfectly and ready to help you stay safe online!";

            if (input.Contains("purpose"))
                return "My purpose is to teach you about cybersecurity and help you avoid online threats.";

            if (input.Contains("what can i ask"))
                return "You can ask me about password safety, phishing, and safe browsing.";

            if (input.Contains("password"))
                return "Always use strong, unique passwords for each account. Avoid using personal details.";

            if (input.Contains("phishing"))
                return "Be cautious of emails asking for personal information. Scammers often pose as trusted organisations.";

            if (input.Contains("browsing") || input.Contains("browse"))
                return "Stick to secure websites (look for https) and avoid clicking suspicious links.";

            return "I didn't quite understand that. Could you rephrase?";
        }
    }
}