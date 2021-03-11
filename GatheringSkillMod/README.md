# Pipakin's Gathering Skill Mod

This mod adds the "Gathering" skill to Valheim.

## Installation

After installing BepInEx, copy the `GatheringSkillMod.dll` from the zip file
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