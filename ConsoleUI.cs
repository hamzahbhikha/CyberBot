using System;
using System.Media;
using System.Threading;

namespace CyberBot
{
    public static class ConsoleUI
    {
        public static void PlayGreetingSound()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("greeting.wav");
                player.PlaySync();
            }
            catch
            {
                // If the file is missing, just continue without sound
            }
        }

        public static void ShowLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   _____      _              _____ 
  / ____|    | |            / ____|
 | |    _   _| |__   ___   | (___  ___  ___
 | |   | | | | '_ \ / _ \   \___ \/ _ \/ __|
 | |___| |_| | |_) |  __/   ____) |  __/ (__
  \_____\__, |_.__/ \___|  |_____/ \___|\___|
         __/ |
        |___/   Cybersecurity Awareness Bot
");
            Console.ResetColor();
        }

        public static void TypeLine(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(15);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void Divider()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("----------------------------------------------------");
            Console.ResetColor();
        }
    }
}