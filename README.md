# AmeisenBot - WoW 3.3.5a Bot [![Build Status](https://jenkins.jnns.de/buildStatus/icon?job=AmeisenBot)](https://jenkins.jnns.de/job/AmeisenBot/)

A bot written in (at this time) C# only for World of Warcraft WotLK (3.3.5a 12340) (the best WoW :P).
This project will be developed like "Kraut und Rüben" (Herb and beet?) as we say here in Germany, its a synonym for messy, so deal with it 😎.

⚠️ Currently this thing is only a playground for me to try out memory-hacking related stuff, but maybe it turns into something useable in the near future...

---
## Info

❤️ **Libaries used:** Blackmagic (Memory Editing) - https://github.com/acidburn974/Blackmagic

❤️ **Framework used:** UIKit (Web Interface) - https://github.com/uikit/uikit

---
## Usage

Although i don't recommend to run this thing in this stage, **you can do it!**

🕹️ **How to use the Bot:**
Compile it, Start it, profit i guess?

🖥️ **How to use the Server:**
Start the "AmeisenServer.exe", it will open a sketchy HTTP-Api at port 16200...

🌵 **How to enable AutoLogin:**
Place the "WoW-LoginAutomator.exe" in the same folder as the bot, thats all...

💩 **Web Interface**
To use this piece of turd, junk, trash, don't know how to call this thing, Open the "index.html" thats hidden deep in the shittiest corner of this project...
💡 *Pro Tip: look for a file called: "groessterMuellEUWest.javashit"*

---
## Modules

**AmeisenBotGUI**: This is the bot's GUI (C# WPF), what did you expect? :D

**AmeisenManager**: Bot Interaction-Point or something like this...

**AmeisenCore**: This is where the memory magic happens.

**AmeisenUtilities**: Memory offsets and data structs can be foud here.

**AmeisenData**: This thing holds the Me, Target and WoWObjects.

**AmeisenAI**: The bots "Brain" to work on its queue and make decisions etc.

**AmeisenLogging**: Basic logging class

**AmeisenServer**: This thing is planned to be a server for the bots to communicate to each other.


---
## Screenshots

### Character selection without AutoLogin

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/charselect.PNG?raw=true "Character selection")

### Character selection with AutoLogin

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/charselect_auto.PNG?raw=true "Character selection Autologin")

### Main bot screen

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/mainscreen.PNG?raw=true "Mainscreen")

### Settings

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/settings.PNG?raw=true "Settings")

### Debug UI

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/debug.PNG?raw=true "Debug GUI")

### Server

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/server.PNG?raw=true "Server")

### Web Interface 💩💩💩

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/webinterface.PNG?raw=true "Web Interface")
