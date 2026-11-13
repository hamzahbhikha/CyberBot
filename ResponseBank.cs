using System.Collections.Generic;

namespace CyberBot
{
    // All keywords and responses live here, organised in dictionaries and lists
    public static class ResponseBank
    {
        public const string DefaultResponse = "I'm not sure I understand. Can you try rephrasing? You can ask about passwords, phishing, scams, privacy, malware or safe browsing, or say things like 'add a task', 'start the quiz' or 'show activity log'.";

        // Keywords that trigger each topic
        public static readonly Dictionary<string, string[]> Keywords = new Dictionary<string, string[]>
        {
            ["password"] = new[] { "password", "passphrase", "two-factor", "2fa", "authentication" },
            ["phishing"] = new[] { "phish", "suspicious email", "spam" },
            ["scam"] = new[] { "scam", "fraud", "social engineering" },
            ["privacy"] = new[] { "privacy", "personal data", "personal information" },
            ["malware"] = new[] { "malware", "virus", "ransomware" },
            ["browsing"] = new[] { "browsing", "browse", "website", "https", "link" }
        };

        // Friendly names used when the bot talks about a topic
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            ["password"] = "password safety",
            ["phishing"] = "phishing",
            ["scam"] = "scams",
            ["privacy"] = "privacy",
            ["malware"] = "malware protection",
            ["browsing"] = "safe browsing",
            ["general"] = "cybersecurity"
        };

        // Several tips per topic so the bot can pick one at random
        public static readonly Dictionary<string, List<string>> Tips = new Dictionary<string, List<string>>
        {
            ["password"] = new List<string>
            {
                "Use strong, unique passwords for every account. Avoid using personal details like your name or birthday.",
                "Make your password long. A passphrase of four or more random words is stronger than a short, complex password.",
                "Never reuse passwords. If one site is breached, attackers will try the same password everywhere else.",
                "Turn on two-factor authentication (2FA) wherever possible. It protects you even if your password is stolen."
            },
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Check the sender's address carefully. Phishing emails often use addresses that look almost right but have small changes.",
                "Hover over links before clicking to see the real destination. If it looks odd, don't click it.",
                "Urgent language like 'Act now or your account will be closed' is a classic phishing trick. Slow down and verify through the official website."
            },
            ["scam"] = new List<string>
            {
                "If an offer sounds too good to be true, it probably is. Never pay upfront fees to claim a prize or loan.",
                "Never share your PIN, OTP or banking password with anyone, even if they claim to be from your bank.",
                "Scammers create pressure and urgency. Take your time and verify the request using an official phone number.",
                "Social engineering tricks people instead of hacking systems. Always question unexpected calls, messages or requests for information."
            },
            ["privacy"] = new List<string>
            {
                "Review the privacy settings on your social media accounts and limit who can see your personal information.",
                "Only share personal details like your ID number or address when it is necessary and the site is trustworthy.",
                "Think before you post. Photos and updates can reveal your location, routine and personal details.",
                "Check which apps have access to your camera, microphone and location, and remove permissions they don't need."
            },
            ["malware"] = new List<string>
            {
                "Keep your operating system and apps up to date. Updates fix the security holes that malware uses.",
                "Only download software from official sources such as the Microsoft Store or the developer's own website.",
                "Don't open unexpected email attachments, even if they appear to come from someone you know.",
                "Use reputable antivirus software and back up your important files regularly in case of ransomware."
            },
            ["browsing"] = new List<string>
            {
                "Look for 'https' and the padlock icon before entering any personal information on a website.",
                "Avoid clicking suspicious links or pop-ups. If a deal looks too good, type the website address in yourself.",
                "Avoid public Wi-Fi for banking or shopping. Use mobile data or a VPN instead.",
                "Keep your browser updated and be careful with extensions. Only install ones you trust."
            },
            ["general"] = new List<string>
            {
                "Keep your software updated, use strong unique passwords and think before you click. Those three habits stop most attacks.",
                "Enable two-factor authentication on your important accounts like email and banking.",
                "If something feels suspicious, trust your instincts. Stop and verify before you act."
            }
        };
    }
}