using System;
using System.Collections.Generic;

namespace CyberBot
{
    // One recorded action
    public class ActivityEntry
    {
        public DateTime Time { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    // Keeps a list of everything the chatbot has done
    public class ActivityLog
    {
        private readonly List<ActivityEntry> entries = new List<ActivityEntry>();

        // Event so the log screen can refresh itself when something new is added
        public event Action? EntryAdded;

        public int Count
        {
            get { return entries.Count; }
        }

        public void Add(string description)
        {
            entries.Add(new ActivityEntry { Time = DateTime.Now, Description = description });
            EntryAdded?.Invoke();
        }

        // Returns the newest entries first (up to 'count' of them)
        public List<ActivityEntry> GetRecent(int count)
        {
            List<ActivityEntry> result = new List<ActivityEntry>();

            for (int i = entries.Count - 1; i >= 0 && result.Count < count; i--)
            {
                result.Add(entries[i]);
            }

            return result;
        }
    }
}