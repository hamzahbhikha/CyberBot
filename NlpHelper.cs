using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CyberBot
{
    // The result of understanding a task or reminder message
    public class TaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public DateTime? Reminder { get; set; }
        public bool IsReminderRequest { get; set; }
    }

    // NLP simulation using string methods and regular expressions
    public static class NlpHelper
    {
        // "remind me to update my password" -> captures "update my password"
        private static readonly Regex RemindMePattern = new Regex(
            @"\bremind\s+me\s*(?:(?:to|about|of|that)\b)?\s*(.+)", RegexOptions.IgnoreCase);

        // "set a reminder to check my privacy settings"
        private static readonly Regex ReminderPattern = new Regex(
            @"\b(?:set|add|create|make|schedule)\s+(?:an?\s+|new\s+|another\s+)*reminder\s*(?:(?:to|for|about|called|named|that)\b|[:\-\u2013\u2014])?\s*(.+)",
            RegexOptions.IgnoreCase);

        // "add a task to enable 2FA" or "add task - review privacy settings"
        private static readonly Regex AddTaskPattern = new Regex(
            @"\b(?:add|create|make|set\s*up|new)\s+(?:an?\s+|new\s+|another\s+)*(?:cyber\s*security\s+)?task\s*(?:(?:to|for|called|named|about)\b|[:\-\u2013\u2014])?\s*(.+)",
            RegexOptions.IgnoreCase);

        // "in 3 days", "in two weeks", "after 5 minutes"
        private static readonly Regex InPattern = new Regex(
            @"\b(?:in|after)\s+(\d+|a|an|one|two|three|four|five|six|seven|eight|nine|ten)\s+(minutes?|mins?|hours?|hrs?|days?|weeks?|months?)\b",
            RegexOptions.IgnoreCase);

        private static readonly Regex TomorrowPattern = new Regex(@"\btomorrow\b", RegexOptions.IgnoreCase);
        private static readonly Regex TodayPattern = new Regex(@"\b(?:today|tonight)\b", RegexOptions.IgnoreCase);
        private static readonly Regex NextPattern = new Regex(@"\bnext\s+(week|month)\b", RegexOptions.IgnoreCase);

        private static readonly Dictionary<string, int> NumberWords = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["an"] = 1,
            ["one"] = 1,
            ["two"] = 2,
            ["three"] = 3,
            ["four"] = 4,
            ["five"] = 5,
            ["six"] = 6,
            ["seven"] = 7,
            ["eight"] = 8,
            ["nine"] = 9,
            ["ten"] = 10
        };

        // Automatic descriptions for tasks created from chat (first matching keyword wins)
        private static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            ["two-factor"] = "Turn on two-factor authentication to add an extra layer of protection to your account.",
            ["2fa"] = "Turn on two-factor authentication to add an extra layer of protection to your account.",
            ["password"] = "Create a strong, unique password to keep your account secure.",
            ["privacy"] = "Review account privacy settings to ensure your data is protected.",
            ["backup"] = "Back up your important files so you can recover them after an attack.",
            ["antivirus"] = "Make sure your antivirus software is installed and up to date.",
            ["phishing"] = "Learn to spot phishing emails and report any suspicious messages.",
            ["update"] = "Keep your software up to date so known security holes are fixed."
        };

        // Works out the task title and reminder time from a message
        public static TaskRequest ParseTaskRequest(string original)
        {
            TaskRequest request = new TaskRequest();
            string text = original.Trim();

            request.IsReminderRequest = text.ToLower().Contains("remind");
            request.Reminder = FindReminderDate(text, out string phrase);

            // Remove the time phrase so it doesn't end up in the title
            if (phrase != string.Empty)
                text = text.Replace(phrase, " ", StringComparison.OrdinalIgnoreCase);

            string title = string.Empty;
            Match match = RemindMePattern.Match(text);

            if (!match.Success)
                match = ReminderPattern.Match(text);

            if (!match.Success)
                match = AddTaskPattern.Match(text);

            if (match.Success)
                title = match.Groups[1].Value;

            request.Title = CleanTitle(title);
            return request;
        }

        // Looks for phrases like "tomorrow", "in 3 days" or "next week".
        // 'phrase' returns the text that matched so it can be removed from the title.
        public static DateTime? FindReminderDate(string text, out string phrase)
        {
            phrase = string.Empty;

            Match inMatch = InPattern.Match(text);
            if (inMatch.Success)
            {
                string amountText = inMatch.Groups[1].Value.ToLower();
                int amount;

                if (!NumberWords.TryGetValue(amountText, out amount) && !int.TryParse(amountText, out amount))
                    amount = 1;

                if (amount < 1) amount = 1;
                if (amount > 365) amount = 365;

                string unit = inMatch.Groups[2].Value.ToLower();
                phrase = inMatch.Value;

                if (unit.StartsWith("min")) return DateTime.Now.AddMinutes(amount);
                if (unit.StartsWith("h")) return DateTime.Now.AddHours(amount);
                if (unit.StartsWith("d")) return DateTime.Now.AddDays(amount);
                if (unit.StartsWith("w")) return DateTime.Now.AddDays(amount * 7);
                return DateTime.Now.AddMonths(amount);
            }

            Match tomorrowMatch = TomorrowPattern.Match(text);
            if (tomorrowMatch.Success)
            {
                phrase = tomorrowMatch.Value;
                return DateTime.Today.AddDays(1).AddHours(9);
            }

            Match nextMatch = NextPattern.Match(text);
            if (nextMatch.Success)
            {
                phrase = nextMatch.Value;
                if (nextMatch.Groups[1].Value.ToLower() == "week")
                    return DateTime.Today.AddDays(7).AddHours(9);
                return DateTime.Today.AddMonths(1).AddHours(9);
            }

            Match todayMatch = TodayPattern.Match(text);
            if (todayMatch.Success)
            {
                phrase = todayMatch.Value;
                DateTime evening = DateTime.Today.AddHours(17);
                return evening > DateTime.Now ? evening : DateTime.Now.AddHours(1);
            }

            return null;
        }

        // Builds a description for tasks created through chat
        public static string BuildDescription(string title)
        {
            string lower = title.ToLower();

            foreach (KeyValuePair<string, string> entry in Descriptions)
            {
                if (lower.Contains(entry.Key))
                    return entry.Value;
            }

            return $"Complete this cybersecurity task to help keep your information safe: {title}.";
        }

        // Tidies up the extracted title
        private static string CleanTitle(string title)
        {
            title = Regex.Replace(title, @"\s+", " ").Trim();
            title = Regex.Replace(title, @"\s*\b(?:please|thanks|thank you)\b\W*$", string.Empty, RegexOptions.IgnoreCase);
            title = Regex.Replace(title, @"^(?:to|that|about)\s+", string.Empty, RegexOptions.IgnoreCase);
            title = title.Trim(' ', '.', ',', '!', '?', '-', ':', ';', '"', '\'');

            if (title.Length == 0)
                return string.Empty;

            if (title.Length > 200)
                title = title.Substring(0, 200);

            return char.ToUpper(title[0]) + title.Substring(1);
        }
    }
}