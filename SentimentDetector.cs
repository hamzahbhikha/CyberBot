using System;
using System.Collections.Generic;

namespace CyberBot
{
    // Detects simple emotions in the user's message and gives an empathetic reply
    public static class SentimentDetector
    {
        private static readonly Dictionary<string, string[]> Keywords = new Dictionary<string, string[]>
        {
            ["worried"] = new[] { "worried", "scared", "afraid", "anxious", "nervous", "overwhelmed", "stressed" },
            ["frustrated"] = new[] { "frustrated", "annoyed", "angry", "fed up", "irritated" },
            ["curious"] = new[] { "curious", "wondering", "i wonder" },
            ["confused"] = new[] { "confused", "unsure", "lost" }
        };

        private static readonly Dictionary<string, List<string>> Responses = new Dictionary<string, List<string>>
        {
            ["worried"] = new List<string>
            {
                "It's completely understandable to feel that way. Cyber threats can be very convincing, but you can protect yourself. Here's a tip to help you stay safe:",
                "Don't worry, you're not alone. Lots of people feel this way, and a few simple habits make a big difference. Try this:"
            },
            ["frustrated"] = new List<string>
            {
                "I'm sorry this feels frustrating. Cybersecurity can seem complicated, so let's take it one step at a time. Here's a simple tip:",
                "I hear you. Let's keep it simple with one easy step:"
            },
            ["curious"] = new List<string>
            {
                "Great question! Being curious is the best way to stay safe online. Here's something useful:",
                "I love your curiosity! Here's a tip to get you started:"
            },
            ["confused"] = new List<string>
            {
                "No problem at all, it can be confusing at first. Let me break it down simply:",
                "That's okay, let's go through it slowly:"
            }
        };

        // Returns the detected sentiment (e.g. "worried") or an empty string if none found
        public static string Detect(string input)
        {
            foreach (KeyValuePair<string, string[]> entry in Keywords)
            {
                foreach (string word in entry.Value)
                {
                    if (input.Contains(word))
                        return entry.Key;
                }
            }
            return string.Empty;
        }

        // Returns a random empathetic message for the sentiment
        public static string GetResponse(string sentiment)
        {
            List<string> options = Responses[sentiment];
            return options[Random.Shared.Next(options.Count)];
        }
    }
}