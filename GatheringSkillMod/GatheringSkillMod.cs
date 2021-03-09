using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pipakin.GatheringMod
{
    [BepInPlugin("com.pipakin.GatheringSkillMod", "GatheringSkillMod", "1.1.0")]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    [BepInDependency("com.pipakin.PickableTimeFixMod", BepInDependency.DependencyFlags.SoftDependency)]
    public class GatheringSkillMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.GatheringSkillMod");

        private static ConfigEntry<float> configSkillIncrease;

        private static ConfigEntry<int> configSimpleEstimateLevel;
        private static ConfigEntry<int> configDetailedEstimateLevel;


        private static ConfigEntry<int> maxDropMultiplier;

        

        void Awake()
        {
            harmony.PatchAll();

            configSkillIncrease = Config.Bind("Progression",
                                           "LevelingIncrement",
                                           1.0f,
                                           "Increment to increase skill per interaction");

            configSimpleEstimateLevel = Config.Bind("TimeEstimate",
                                           "Simple",
                                           1,
                                           "Level at which to show simple estimates");


            configDetailedEstimateLevel = Config.Bind("TimeEstimate",
                                           "Detailed",
                                           10,
                                           "Level at which to show detailed (to the minute) estimates");

            maxDropMultiplier = Config.Bind("Drops",
                                           "MaxMultiplier",
                                           4,
                                           "Maximum drop multiplier (at level 100). Minimum 1");

            SkillInjector.RegisterNewSkill(SKILL_TYPE, "Gathering", "Gathering berries and other items", 1.0f, null, Skills.SkillType.Unarmed);

        }
        //hopefully this doesn't conflict.
        public const int SKILL_TYPE = 242;

        [HarmonyPatch(typeof(Pickable), "Interact")]
        [HarmonyBefore(new string[] { "com.github.johndowson.CropReplant" })]
        public static class GardeningSkillIncrease
        {

            [HarmonyPrefix]
            public static void Prefix(Humanoid character, bool ___m_picked)
            {
                //if I'm interacting and it's not picked, I'm gonna pick it!
                if (!___m_picked)
                {
                    //add some skillzz!
                    character.RaiseSkill((Skills.SkillType)SKILL_TYPE, configSkillIncrease.Value);
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
        public static class PickableTime
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result, bool ___m_picked, ZNetView ___m_nview, int ___m_respawnTimeMinutes, Pickable __instance)
            {
                if (___m_picked && ___m_nview.GetZDO() != null)
                {
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
                if (!___m_nview.IsValid() || ___m_picked)
                {
                    return;
                }

                var mult = 1.0f + character.GetSkillFactor((Skills.SkillType)SKILL_TYPE) * (float)maxDropMultiplier.Value;
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
                if (!___m_nview.IsValid())
                {
                    return;
                }

                var mult = __instance.GetMultiplier();
                Debug.Log("Gathering multiplier read as " + mult);

                stack = (int)mult * stack;
                Debug.Log("Gathering boosted stack size to " + stack);

                __instance.ClearMultiplier();
            }
        }
    }
}
