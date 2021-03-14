using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pipakin.GatheringMod
{
    [BepInPlugin("com.pipakin.GatheringSkillMod", "GatheringSkillMod", "1.3.0")]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    [BepInDependency("com.pipakin.PickableTimeFixMod", BepInDependency.DependencyFlags.SoftDependency)]
    public class GatheringSkillMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.GatheringSkillMod");

        enum DropMode
        {
            Linear,
            Random,
            PartialRandom
        }

        private static ConfigEntry<float> configSkillIncrease;
        private static ConfigEntry<string> ignorePickables;

        private static ConfigEntry<bool> estimatesEnabled;
        private static ConfigEntry<int> configSimpleEstimateLevel;
        private static ConfigEntry<int> configDetailedEstimateLevel;


        private static ConfigEntry<bool> dropsEnabled;
        private static ConfigEntry<DropMode> dropMode;
        private static ConfigEntry<int> maxDropMultiplier;

        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        private static Sprite LoadCustomTexture()
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(directoryName, "gathering.png");
            if (File.Exists(filepath))
            {
                Texture2D texture2D = LoadTexture(filepath);
                return Sprite.Create(texture2D, new Rect(0f, 0f, 32f, 32f), Vector2.zero);
            } 
            else
            {
                Debug.LogError("Unable to load skill icon! Make sure you place the gathering.png file in the plugins directory!");
                return null;
            }
        }

        private static Texture2D LoadTexture(string filepath)
        {
            if (cachedTextures.ContainsKey(filepath))
            {
                return cachedTextures[filepath];
            }
            Texture2D texture2D = new Texture2D(0, 0);
            ImageConversion.LoadImage(texture2D, File.ReadAllBytes(filepath));
            return texture2D;
        }


        void Awake()
        {
            harmony.PatchAll(typeof(GardeningSkillIncrease));
            harmony.PatchAll(typeof(GatheringHookRPCs));
            harmony.PatchAll(typeof(GetHoverTextPatch));
            harmony.PatchAll(typeof(FixPickableTime));
            harmony.PatchAll(typeof(AddDropMultiplier));
            harmony.PatchAll(typeof(DropMultiply));

            configSkillIncrease = Config.Bind("Progression",
                                           "LevelingIncrement",
                                           1.0f,
                                           "Increment to increase skill per interaction");

            ignorePickables = Config.Bind("Progression",
                                           "IgnorePickables",
                                           "",
                                           "Pickables to ignore. Comma seperated list of object names, e.g. Carrot,CarrotSeeds");

            estimatesEnabled = Config.Bind("TimeEstimate",
                                           "Enabled",
                                           true,
                                           "Enable showing estimates. Disable this if you have another mod you want to use estimates from.");

            configSimpleEstimateLevel = Config.Bind("TimeEstimate",
                                           "Simple",
                                           1,
                                           "Level at which to show simple estimates");

            configDetailedEstimateLevel = Config.Bind("TimeEstimate",
                                           "Detailed",
                                           10,
                                           "Level at which to show detailed (to the minute) estimates");

            dropsEnabled = Config.Bind("Drops",
                                           "Enabled",
                                           true,
                                           "Enable changing drop amounts.");

            dropMode = Config.Bind("Drops",
                                           "Mode",
                                           DropMode.Linear,
                                           "Mode for increasing the drop rate. Valid values are Linear, Random, and PartialRandom");

            maxDropMultiplier = Config.Bind("Drops",
                                           "MaxMultiplier",
                                           4,
                                           "Maximum drop multiplier (at level 100). Minimum 1");

            SkillInjector.RegisterNewSkill(SKILL_TYPE, "Gathering", "Gathering berries and other items", 1.0f, LoadCustomTexture(), Skills.SkillType.Unarmed);

        }
        static bool IgnorePickable(string name)
        {
            var ignored = ignorePickables.Value?.Split(',');
            if(ignored != null)
            {
                foreach(var i in ignored)
                {
                    if(i.Replace("(Clone)", "") == name.Replace("(Clone)", ""))
                    {
                        return true;
                    }
                }

            }
            return false;
        }
        //hopefully this doesn't conflict.
        public const int SKILL_TYPE = 242;

        public static void IncreaseSkill(Character character, string pickableName)
        {
            try
            {
                if (IgnorePickable(pickableName))
                {
                    Debug.Log("Ignoring pick of: " + pickableName);
                }
                else
                {
                    Debug.Log("Raising skill because of picking: " + pickableName);
                    character.RaiseSkill((Skills.SkillType)SKILL_TYPE, configSkillIncrease.Value);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error raising gathering skill for character " + character.GetHoverName() + ": " + e.ToString());
            } 
        }

        [HarmonyPatch(typeof(Pickable), "Interact")]
        [HarmonyBefore(new string[] { "com.github.johndowson.CropReplant" })]
        public static class GardeningSkillIncrease
        {

            [HarmonyPrefix]
            public static void Prefix(Humanoid character, bool ___m_picked, Pickable __instance)
            {
                //if I'm interacting and it's not picked, I'm gonna pick it!
                if (!___m_picked)
                {
                    //add some skillzz!
                    IncreaseSkill(character, __instance.name);
                }
            }
        }


        [HarmonyPatch(typeof(Pickable), "Awake")]
        public static class GatheringHookRPCs
        {

            [HarmonyPostfix]
            public static void Postfix(ZNetView ___m_nview, Pickable __instance)
            {
                __instance.RegisterSetMultiplierRPC();
            }
        }

        [HarmonyPatch(typeof(Pickable), "GetHoverText")]
        public static class GetHoverTextPatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result, bool ___m_picked, ZNetView ___m_nview, int ___m_respawnTimeMinutes, Pickable __instance)
            {
                if (___m_picked && ___m_nview.GetZDO() != null && estimatesEnabled.Value)
                {
                    if(__instance.name.ToLower().Contains("surt"))
                    {
                        return;
                    }

                    long @long = ___m_nview.GetZDO().GetLong("picked_time", 0L);
                    DateTime dateTime = new DateTime(@long);
                    double mins = (ZNet.instance.GetTime() - dateTime).TotalMinutes;

                    float factor = Player.m_localPlayer.GetSkillFactor((Skills.SkillType)SKILL_TYPE);
                    int level = (int)(factor * 100f);

                    var remainingMinutes = (___m_respawnTimeMinutes - mins);
                    var remainingRatio = remainingMinutes / (float)___m_respawnTimeMinutes;
                    var color = "red";
                    var completeCategory = "in a long time";
                    if (remainingRatio < 0)
                    {
                        color = "blue";
                        completeCategory = "any second now";
                    }
                    else if (remainingRatio < 0.25)
                    {
                        color = "green";
                        completeCategory = "soon";
                    }
                    else if (remainingRatio < 0.5)
                    {
                        color = "yellow";
                        completeCategory = "in a while";
                    }
                    if (level >= configDetailedEstimateLevel.Value)
                    {
                        if (remainingMinutes < 0.0)
                        {
                            __result = Localization.instance.Localize(__instance.GetHoverName() + "\n(<color=" + color + "><b>Ready any second now</b></color>)");
                        }
                        else if (remainingMinutes < 1.0)
                        {
                            __result = Localization.instance.Localize(__instance.GetHoverName() + "\n(<color=" + color + "><b>Ready in less than a minute</b></color>)");
                        }
                        else
                        {
                            __result = Localization.instance.Localize(__instance.GetHoverName() + "\n(<color=" + color + "><b>Ready in " + remainingMinutes.ToString("F0") + " min</b></color>)");
                        }
                    }
                    else if (level >= configSimpleEstimateLevel.Value)
                    {
                        __result = Localization.instance.Localize(__instance.GetHoverName() + "\n(<color=" + color + "><b>Ready " + completeCategory + "</b></color>)");
                    }
                    else if (factor > 0.0)
                    {
                        __result = Localization.instance.Localize(__instance.GetHoverName());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pickable), "SetPicked")]
        public static class FixPickableTime
        {
            public class PickState
            {
                public long picked_time;
                public bool picked;
            }

            [HarmonyPrefix]
            public static void Prefix(ZNetView ___m_nview, bool ___m_picked, ref PickState __state)
            {
                __state = new PickState();
                __state.picked_time = ___m_nview.GetZDO().GetLong("picked_time", 0L);
                __state.picked = ___m_picked;
            }

            [HarmonyPostfix]
            public static void Postfix(ZNetView ___m_nview, bool ___m_picked, ref PickState __state)
            {
                //if we're not changing the state, don't change the pick time.
                //Don't do anything if we're the client. Trust the server in that case.
                if (__state != null && __state.picked == ___m_picked && ___m_nview.IsOwner())
                {
                    ___m_nview.GetZDO().Set("picked_time", __state.picked_time);
                }
            }
        }

        [HarmonyPatch(typeof(Pickable), "Interact")]
        [HarmonyBefore(new string[] { "com.github.johndowson.CropReplant" })]
        public static class AddDropMultiplier
        {
            [HarmonyPrefix]
            public static void Prefix(Humanoid character, bool repeat, ZNetView ___m_nview, bool ___m_picked, Pickable __instance)
            {
                if (!___m_nview.IsValid() || ___m_picked || !dropsEnabled.Value || IgnorePickable(__instance.name))
                {
                    return;
                }

                var mult = 1.0f + character.GetSkillFactor((Skills.SkillType)SKILL_TYPE) * ((float)maxDropMultiplier.Value - 1.0f);
                Debug.Log("Gathering multiplier set to " + mult);

                __instance.SetMultiplier(mult);
            }
        }

        [HarmonyPatch(typeof(Pickable), "Drop")]
        public static class DropMultiply
        {
            [HarmonyPrefix]
            public static void Prefix(GameObject prefab, int offset, ref int stack, ZNetView ___m_nview, bool ___m_picked, Pickable __instance)
            {
                if (!___m_nview.IsValid() || !dropsEnabled.Value || IgnorePickable(__instance.name))
                {
                    return;
                }

                var mult = __instance.GetMultiplier();
                Debug.Log("Gathering multiplier read as " + mult);

                if(dropMode.Value == DropMode.Random)
                {
                    mult = UnityEngine.Random.Range(1.0f, mult);
                    mult += UnityEngine.Random.Range(0.0f, 1.0f) <= (mult - Math.Floor(mult)) ? 1.0f : 0.0f;
                } else if (dropMode.Value == DropMode.PartialRandom)
                {
                    mult += UnityEngine.Random.Range(0.0f, 1.0f) <= (mult - Math.Floor(mult)) ? 1.0f : 0.0f;
                }

                mult = (float)Math.Floor(mult);

                Debug.Log("Actual calculated multiplier " + mult);

                stack = (int)mult * stack;
                Debug.Log("Gathering boosted stack size to " + stack);

                __instance.ClearMultiplier();
            }
        }
    }
}
