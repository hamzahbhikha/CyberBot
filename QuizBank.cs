using System.Collections.Generic;

namespace CyberBot
{
    // All the quiz questions (13 in total: 9 multiple choice and 4 true/false)
    public static class QuizBank
    {
        public static List<QuizQuestion> GetQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Text = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams. Real companies never ask for your password by email."
                },
                new QuizQuestion
                {
                    Text = "Which of these is the strongest password?",
                    Options = new List<string> { "password123", "JohnSmith1990", "12345678", "Tr33!House-Blue-Marble" },
                    CorrectIndex = 3,
                    Explanation = "Long passwords that mix random words, numbers and symbols are the hardest to guess."
                },
                new QuizQuestion
                {
                    Text = "What does 'https' at the start of a web address tell you?",
                    Options = new List<string> { "The site is 100% safe", "The site is run by the government", "The connection is encrypted", "The site has no adverts" },
                    CorrectIndex = 2,
                    Explanation = "HTTPS encrypts the data between you and the site, but criminals can use it too, so stay alert."
                },
                new QuizQuestion
                {
                    Text = "What is two-factor authentication (2FA)?",
                    Options = new List<string> { "Using two antivirus programs", "A second step, besides your password, to prove it's you", "Logging in twice", "Using two passwords" },
                    CorrectIndex = 1,
                    Explanation = "2FA protects your account even if someone steals your password."
                },
                new QuizQuestion
                {
                    Text = "A caller says they are from your bank and asks for the OTP sent to your phone. What should you do?",
                    Options = new List<string> { "Give it to them because they sound official", "Ask them to email it instead", "Give them half of it", "Hang up and call your bank on its official number" },
                    CorrectIndex = 3,
                    Explanation = "Banks never ask for your OTP or PIN. Scammers use urgency to rush you."
                },
                new QuizQuestion
                {
                    Text = "Which is a common sign of a phishing email?",
                    Options = new List<string> { "It arrives on a weekday", "It has no attachments", "It threatens to close your account unless you act now", "It is addressed to your email address" },
                    CorrectIndex = 2,
                    Explanation = "Phishing emails create panic and urgency so you click without thinking."
                },
                new QuizQuestion
                {
                    Text = "What is social engineering?",
                    Options = new List<string> { "Tricking people into giving away information or access", "Building social media apps", "Engineering computer networks", "A type of antivirus software" },
                    CorrectIndex = 0,
                    Explanation = "Social engineering targets people instead of computers, so always question unexpected requests."
                },
                new QuizQuestion
                {
                    Text = "What should you do about online banking on public Wi-Fi?",
                    Options = new List<string> { "Go ahead, it is always safe", "Avoid it, or use mobile data or a VPN instead", "Turn the screen brightness down", "Share the Wi-Fi password first" },
                    CorrectIndex = 1,
                    Explanation = "Public Wi-Fi can be monitored by attackers, so keep sensitive activity off it."
                },
                new QuizQuestion
                {
                    Text = "Why are software updates important?",
                    Options = new List<string> { "They fix security holes that attackers use", "They only change how the app looks", "They slow down your device", "They delete old files" },
                    CorrectIndex = 0,
                    Explanation = "Updates patch security weaknesses, so install them as soon as you can."
                },
                new QuizQuestion
                {
                    Text = "True or False: It is safe to use the same password on all your accounts.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "If one site is breached, attackers try the same password everywhere else."
                },
                new QuizQuestion
                {
                    Text = "True or False: You should hover over a link to see where it really goes before you click it.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "Hovering shows the real destination, which often exposes fake links."
                },
                new QuizQuestion
                {
                    Text = "True or False: A website with a padlock icon is always trustworthy.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "The padlock only means the connection is encrypted. Scam sites can have one too."
                },
                new QuizQuestion
                {
                    Text = "True or False: Regular backups and antivirus software help protect you against ransomware.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "Backups let you recover your files without paying criminals, and antivirus helps block the attack."
                }
            };
        }
    }
}