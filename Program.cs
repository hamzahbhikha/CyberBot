using System;

namespace CyberBot
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleUI.PlayGreetingSound();
            ConsoleUI.ShowLogo();

            ChatBot bot = new ChatBot();
            bot.Start();
        }
    }
}