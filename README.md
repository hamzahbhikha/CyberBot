# Cybersecurity Awareness Bot — Part 2 (GUI)

A Windows Forms chatbot that teaches users about cybersecurity: passwords, phishing, scams, privacy, malware and safe browsing.

## Features
- GUI with chat window, input box and Send button
- Voice greeting and ASCII logo on launch
- Personalised greeting using the user's name
- Keyword recognition for 6 cybersecurity topics
- Random tips (4 per topic) stored in dictionaries and lists
- Follow-ups such as "another tip" and "explain more"
- Memory: remembers the user's name and favourite topic
- Sentiment detection: worried, frustrated, curious, confused
- Error handling with a default response for unknown input
- Delegate-based handler list in ChatBot

## How to run
1. Open `CyberBot.csproj` in Visual Studio
2. Press F5 to build and run
3. Requires the .NET 10.0 SDK on Windows

## Example inputs
- Tell me about password safety
- Give me a phishing tip, then: another tip
- I'm worried about online scams
- I'm interested in privacy

## CI Status
![CI screenshot](ci-screenshot.png)

## Video Presentation
[Part 2 YouTube link here]

Part 1 (console version) video: https://www.youtube.com/watch?v=LwVLmS66s_w