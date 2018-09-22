# AmeisenBot - WoW 3.3.5a Bot [![Build Status](https://jenkins.jnns.de/buildStatus/icon?job=AmeisenBot)](https://jenkins.jnns.de/job/AmeisenBot/) [![codecov](https://codecov.io/gh/Jnnshschl/WoW-3.3.5a-Bot/branch/master/graph/badge.svg)](https://codecov.io/gh/Jnnshschl/WoW-3.3.5a-Bot) [![HitCount](http://hits.dwyl.io/jnnshschl/WoW-3.3.5a-Bot.svg)](http://hits.dwyl.io/jnnshschl/WoW-3.3.5a-Bot)


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

💩 **Web Interface:**
To use this piece of turd, junk, trash, don't know how to call this thing, Open the "index.html" thats hidden deep in the shittiest corner of this project...
💡 *Pro Tip: look for a file called: "groessterMuellEUWest.javashit"*

🔪 **How to make a CombatClass:**
Template \*.cs file:
```c#
using AmeisenBotData;
using AmeisenBotLogger;
using AmeisenBotUtilities;
using AmeisenCombatEngine.Interfaces;
using System.Collections.Generic;

namespace AmeisenBotCombat
{
    public class CombatClass : IAmeisenCombatClass
    {
        public AmeisenDataHolder AmeisenDataHolder { get; set; }
        private Me Me
        {
            get { return AmeisenDataHolder.Me; }
            set { AmeisenDataHolder.Me = value; }
        }
        private Unit Target
        {
            get { return AmeisenDataHolder.Target; }
            set { AmeisenDataHolder.Target = value; }
        }

        public void Init()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "CombatClass: In combat now", this);
        }

        public void Exit()
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "CombatClass: Out of combat now", this);
        }

        public void HandleAttacking()
        {
            if (Me != null)
            {
                Me.Update();
            }
            if (Target != null)
            {
                Target.Update();
            }

            Unit unitToAttack = Target;

            // Get a target
            if (Me.TargetGuid == 0)
            {
                unitToAttack = CombatUtils.AssistParty(Me, AmeisenDataHolder.ActiveWoWObjects);
            }

            if (unitToAttack != null)
            {
                // Start autoattack
                if (!Me.InCombat)
                {
                    CombatUtils.FaceTarget(unitToAttack);
                    CombatUtils.AttackTarget();
                }

                DoAttackRoutine();
            }
        }

        private void DoAttackRoutine()
        {
            List<string> targetAuras = CombatUtils.GetAuras(LuaUnit.target);

            Me.Update();
            // Restore Mana
            if (Me.EnergyPercentage < 30 && Me.HealthPercentage > 50)
            {
                CombatUtils.CastSpellByName("Life Tap", true);
            }

            if(targetAuras != null)
                Target.Update();
            // DoT's to apply
            if (!targetAuras.Contains("Curse of Agony"))
            {
                CombatUtils.CastSpellByName("Curse of Agony", false);
            }
            if (!targetAuras.Contains("Corruption"))
            {
                CombatUtils.CastSpellByName("Corruption", false);
            }
            if (!targetAuras.Contains("Unstable Affliction"))
            {
                CombatUtils.CastSpellByName("Unstable Affliction", false);
            }
            if (!targetAuras.Contains("Haunt"))
            {
                CombatUtils.CastSpellByName("Haunt", false);
            }

            if(targetAuras != null) {
                Target.Update();
                // Active-Damage Spell
                if (Target.HealthPercentage < 25)
                {
                    CombatUtils.CastSpellByName("Drain Soul", false);
                }
                else
                {
                    CombatUtils.CastSpellByName("Shadow Bolt", false);
                }
            }
        }

        public void HandleBuffs()
        {
            List<string> myAuras = CombatUtils.GetAuras(LuaUnit.player);

            if (!myAuras.Contains("Demon Armor"))
            {
                CombatUtils.CastSpellByName("Demon Armor", true);
            }
            if (!myAuras.Contains("Blood Pact"))
            {
                CombatUtils.CastSpellByName("Summon Imp", true);
            }
        }

        public void HandleHealing()
        {
        }

        public void HandleTanking()
        {
        }
    }
}

```

---
## Modules
**AmeisenBot.Combat**: CombatClass utilities & template

**AmeisenBot.Core**: Collection of some static object-reading/casting/lua functions

**AmeisenBot.Data**: "DataHolder" to hold things like our playerobject, our target object & active WoWObjects

**AmeisenBot.DB**: Database connection manager, from this thing the map is beeing read/saved

**AmeisenBot.FSM**: StateMachine of the Bot executing all actions

**AmeisenBot.GUI**: WPF GUI

**AmeisenBot.Logger**: Basic logging class

**AmeisenBot.Manager**: Create a new Bot instance here and manage it

**AmeisenBot.Mapping**: Mapping related stuff like loading/saving nodes

**AmeisenBot.Server**: This thing is planned to be a server for the bots to communicate to each other.

**AmeisenBot.Test**: Maybe some tests will appear in this module in the near future

**AmeisenBot.Utilities**: Memory offsets, data structs and a few math related funtions


**AmeisenPathLib**: Pathfinding using A*

**WoWLoginAutomator**: Auto-Login into WoW 3.3.5a

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

### Map

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/map.PNG?raw=true "Map")

### Debug UI

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/debug.PNG?raw=true "Debug GUI")

### Server

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/server.PNG?raw=true "Server")

### Web Interface 💩💩💩

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/webinterface.PNG?raw=true "Web Interface")
