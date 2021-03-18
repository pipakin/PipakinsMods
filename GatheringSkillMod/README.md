# Pipakin's Gathering Skill Mod

Adds the Gathering skill to the game. This skill lets you see estimates for when berry bushes and the like are available to pick again, and increases the drop rate at higher skill levels.

## Notes
NOTE: Version 2.0.1 had a pretty awful bug. It's fixed now but if you played with that version you may have lost the gathering skill entirely. My apologies.

## Optional Dependencies

The following mods will cause extra functionality to be available if this mod is loaded:

* Mod Config Enforcement (https://www.nexusmods.com/valheim/mods/460) - Settings will be driven from the server instead of locally.
* CropReplant (https://www.nexusmods.com/valheim/mods/99) - Adds some patches to fix compatibility

## Default Config:
```
## Settings file was created by plugin GatheringSkillMod v1.3.0
## Plugin GUID: com.pipakin.GatheringSkillMod

[Drops]

## Enable changing drop amounts.
# Setting type: Boolean
# Default value: true
Enabled = true

## Mode for increasing the drop rate. Valid values are Linear, Random, and PartialRandom
# Setting type: DropMode
# Default value: Linear
# Acceptable values: Linear, Random, PartialRandom
Mode = PartialRandom

## Maximum drop multiplier (at level 100). Minimum 1
# Setting type: Int32
# Default value: 4
MaxMultiplier = 4

[Progression]

## Increment to increase skill per interaction
# Setting type: Single
# Default value: 1
LevelingIncrement = 1

## Pickables to ignore. Comma seperated list of object names, e.g. Carrot,CarrotSeeds
# Setting type: String
# Default value:
IgnorePickables =

[TimeEstimate]

## Enable showing estimates. Disable this if you have another mod you want to use estimates from.
# Setting type: Boolean
# Default value: true
Enabled = true

## Level at which to show simple estimates
# Setting type: Int32
# Default value: 1
Simple = 1

## Level at which to show detailed (to the minute) estimates
# Setting type: Int32
# Default value: 10
Detailed = 10
```

The default config should be fine for most cases. The most important settingsare:

MaxMultiplier - The amount to multiply drops by at level 100. This value is scaled by your current level in the skill.
Mode - The multiplier mode to use.

    Linear - Simple linear calculation: ActualDropAmount = Floor(NormalDropAmount * Multiplier * SkillLevel/100).
    Random - Random Drop multiplier between 1 and your current multiplier.  A bonus is calculated based on the same formula as PartialRandom.
    PartialRandom - If your multiplier falls in between two numbers a bonus will be added to your multiplier based on how close to the next number you are. e.g. 1.5 -> 50/50 between 1 and 2. Otherwise same as Linear

IgnorePickables - list (comma separated, no spaces) of pickables to ignore. For example, if you don't want extra drops/gathering xp for carrots:
IgnorePickables = Carrots,CarrotSeeds


## Tested for compatibility with:

* BetterUI﻿
* Berry Planting
* Crop Replant 


Requires my Skill Injector mod. If Mod Config Enforcer is installed, it will be used to sync settings to the client.

Includes the changes from Pickable Time Fix mod but it won't conflict. You can have both, you just don't need both.

Github link: https://github.com/pipakin/PipakinsMods/tree/master/GatheringSkillMod

All my mod code on GitHub: https://github.com/pipakin/PipakinsMods
 
## FAQ

* How do I install this?
  * Extract the zip file and copy the resulting files into the BepInEx plugins folder. BepInEx docs﻿
* Why do I get an error about "com.pipakin.SkillInjectorMod?
  * You need to install my Skill Injector mod.
* Why are the "raiseskill" and "resetskill" cheats weirdly broken?
  * You need the latest version of my Skill Injector mod. I fixed a bug in that.
* I'm getting a "cyclical dependency error" with CropReplant?
  * Update to the latest version of this mod. I fixed that bug (also this isn't a question).

## Installation

After installing BepInEx, copy the `GatheringSkillMod.dll` and `gathering.png` from the zip file
and paste it into BepInEx's plugins directory.

## Configuration

The default config should be fine for most cases. The most important settings
are:

* `MaxMultiplier` - The amount to multiply drops by at level 100. This value
is scaled by your current level in the skill.
* `Mode` - The multiplier mode to use:
  *  `Linear` - Simple linear calculation:
  `ActualDropAmount = Floor(NormalDropAmount * Multiplier * SkillLevel/100)`
  *  `Random` - Random Drop multiplier between 1 and your current multiplier.
  A bonus is calculated based on the same formula as `PartialRandom`
  *  `PartialRandom` - If your multiplier falls in between two numbers a
  bonus will be added to your multiplier based on how close to the next number
  you are. e.g. 1.5 -> 50/50 between 1 and 2. Otherwise same as `Linear`.