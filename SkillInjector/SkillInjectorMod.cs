using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pipakin.SkillInjectorMod
{
    [BepInPlugin("com.pipakin.SkillInjectorMod", "SkillInjectorMod", "1.0.2")]
    public class SkillInjector : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.SkillInjectorMod");

        void Awake()
        {
            Config.Bind<int>("General", "NexusID", 341, "Nexus mod ID for updates");

            harmony.PatchAll();
        }
        class SkillInfo
        {
            public Skills.SkillDef m_def;
            public string name;
            public Skills.SkillType m_template;
        }

        private static Dictionary<int, SkillInfo> m_defs = new Dictionary<int, SkillInfo>();

        public static void RegisterNewSkill(int id, string name, string description, float increment, Sprite icon, Skills.SkillType template = 0)
        {
            if (m_defs.ContainsKey(id))
            {
                throw new Exception(Localization.instance.Localize("Id " + id + " already in use by skill $skill_" + id));
            }

            m_defs[id] = new SkillInfo
            {
                m_def = new Skills.SkillDef
                {
                    m_description = description,
                    m_icon = icon,
                    m_increseStep = increment,
                    m_skill = (Skills.SkillType)id
                },
                name = name,
                m_template = template
            };
        }

        public static Skills.SkillDef GetSkillDef(Skills.SkillType type, List<Skills.SkillDef> skills = null)
        {
            int id = (int)type;

            if (!m_defs.ContainsKey(id))
            {
                return null;
            }

            var info = m_defs[id];

            if (info.m_template != 0 && skills != null)
            {
                foreach (Skills.SkillDef skill in skills)
                {
                    if (skill.m_skill == info.m_template)
                    {
                        info.m_def.m_description = info.m_def.m_description ?? skill.m_description;
                        info.m_def.m_icon = info.m_def.m_icon ?? skill.m_icon;
                    }
                }
            }

            Traverse.Create(Localization.instance).Method("AddWord", "skill_" + id, info.name).GetValue("skill_" + id, info.name);
            var newSkill = info.m_def;
            return newSkill;
        }

        [HarmonyPatch(typeof(Skills), "GetSkillDef")]
        public static class SkillInjectionPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Skills.SkillType type, ref Skills.SkillDef __result, List<Skills.SkillDef> ___m_skills)
            {
                if (__result == null)
                {
                    var def = GetSkillDef(type, ___m_skills);

                    if (def != null)
                    {
                        ___m_skills.Add(def);
                        __result = def;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Skills), "IsSkillValid")]
        public static class SkillValidPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Skills.SkillType type, ref bool __result)
            {
                if (__result == false)
                {
                    if (m_defs.ContainsKey((int)type))
                    {
                        __result = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Skills), "CheatRaiseSkill")]
        public static class CheatRaiseSkillPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(string name, float value, Skills __instance, Player ___m_player)
            {
                foreach (int id in m_defs.Keys)
                {
                    SkillInfo value2 = m_defs[id];

                    if (value2.name.ToLower() == name)
                    {
                        Skills.Skill skill = Traverse.Create(__instance).Method("GetSkill", (Skills.SkillType)id).GetValue<Skills.Skill>((Skills.SkillType)id);
                        skill.m_level += value;
                        skill.m_level = Mathf.Clamp(skill.m_level, 0f, 100f);
                        ___m_player.Message(MessageHud.MessageType.TopLeft, "Skill incresed " + value2.name + ": " + (int)skill.m_level, 0, skill.m_info.m_icon);
                        Console.instance.Print("Skill " + value2.name + " = " + skill.m_level);
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Skills), "CheatResetSkill")]
        public static class CheatResetSkillPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(string name, Skills __instance, Player ___m_player)
            {
                foreach (int id in m_defs.Keys)
                {
                    SkillInfo value2 = m_defs[id];

                    if (value2.name.ToLower() == name)
                    {
                        ___m_player.GetSkills().ResetSkill((Skills.SkillType)id);
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
