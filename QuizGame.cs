using System;
using System.Collections.Generic;

namespace CyberBot
{
    // Runs one quiz: keeps the order, the current question and the score
    public class QuizGame
    {
        private List<QuizQuestion> questions = new List<QuizQuestion>();
        private int currentIndex;

        public int Score { get; private set; }

        public int TotalQuestions
        {
            get { return questions.Count; }
        }

        public int QuestionNumber
        {
            get { return currentIndex + 1; }
        }

        public bool IsFinished
        {
            get { return currentIndex >= questions.Count; }
        }

        public QuizQuestion? CurrentQuestion
        {
            get { return IsFinished ? null : questions[currentIndex]; }
        }

        // Starts a new quiz with the questions in a random order
        public void Start()
        {
            questions = QuizBank.GetQuestions();

            for (int i = questions.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                QuizQuestion temp = questions[i];
                questions[i] = questions[j];
                questions[j] = temp;
            }

            currentIndex = 0;
            Score = 0;
        }

        // Checks the chosen answer, updates the score and returns the feedback
        public string SubmitAnswer(int chosenIndex, out bool isCorrect)
        {
            QuizQuestion? question = CurrentQuestion;

            if (question == null)
            {
                isCorrect = false;
                return string.Empty;
            }

            isCorrect = chosenIndex == question.CorrectIndex;

            if (isCorrect)
            {
                Score++;
                return "Correct! " + question.Explanation;
            }

            return "Not quite. The correct answer was: " + question.Options[question.CorrectIndex] + ". " + question.Explanation;
        }

        public void NextQuestion()
        {
            currentIndex++;
        }

        // Final message based on the percentage scored
        public string GetFinalFeedback()
        {
            int percentage = TotalQuestions == 0 ? 0 : Score * 100 / TotalQuestions;
            string result = $"You scored {Score} out of {TotalQuestions}. ";

            if (percentage >= 80)
                return result + "Great job! You're a cybersecurity pro!";

            if (percentage >= 50)
                return result + "Good effort! Keep practising to stay even safer online.";

            return result + "Keep learning to stay safe online! Ask me for tips and try the quiz again.";
        }
    }
}