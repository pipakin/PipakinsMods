## Important Announcement

SkillInjector is now part of JotunnLib! You should switch over to that library for your skill injection needs. This library will continue to work as a thin wrapper over JotunnLib so if you're lazy like me you don't need to switch over till you have time.

### FAQ

* Is this going to break my mod?
  * No, as long as players download the new dependency it should still work as written.
* But I don't _want_ to depend on a big library.
  * First, not a question. Second, JotunnLib is a really thin layer over Valheim that provides lots of goodies. Plus our two mods would step all over each other and I don't want that.
* So you're abandoning your mod?
  * Naw, I only created this because no one else had. I'll be keeping up with JotunnLib development and probably contribute some PRs of my own against it.
* Will you be taking down this mod?
  * No. It will remain as a small wrapper as long as even 1 person is dependant on it.
* Ok, you talked me into taking the plunge. how do I switch over fully to JotunnLib?
  * luckily, it's super easy. For example (from my gathering mod):
``` 
using Pipakin.SkillInjectorMod;
...

[BepInPlugin("com.pipakin.GatheringSkillMod", "GatheringSkillMod", "2.0.3")]
[BepInDependency("com.pipakin.SkillInjectorMod")]
public class GatheringSkillMod : BaseUnityPlugin
{
    void Awake()
    {
        ...
        SkillInjector.RegisterNewSkill(
            SKILL_TYPE, 
            "Gathering", 
            "Gathering berries and other items", 
            1.0f, 
            LoadCustomTexture(), 
            Skills.SkillType.Unarmed);
        ...
    }
    ...
}
```
Becomes:
``` 
using JotunnLib.Managers;
...

[BepInPlugin("com.pipakin.GatheringSkillMod", "GatheringSkillMod", "2.0.3")]
[BepInDependency("com.bepinex.plugins.jotunnlib")]
public class GatheringSkillMod : BaseUnityPlugin
{
    void Awake()
    {
        ...
        SkillManager.RegisterSkill(
            new SkillManager.FromSkillInjector(
                "gathering", 
                (Skills.SkillType)SKILL_TYPE,
                "Gathering", 
                "Gathering berries and other items", 
                LoadCustomTexture(), 
                1.0f
            )
        );
        ...
    }
    ...
}
```

## Source (such as it is)

Github link:
https://github.com/pipakin/PipakinsMods/tree/master/SkillInjector


All my mod code on GitHub: https://github.com/pipakin/PipakinsMods