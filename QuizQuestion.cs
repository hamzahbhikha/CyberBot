using System.Collections.Generic;

namespace CyberBot
{
    // One quiz question (multiple choice has 4 options, true/false has 2)
    public class QuizQuestion
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}