using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Pipakin.FitnessSkillMod
{
    [BepInPlugin("com.pipakin.FitnessSkillMod", "FitnessSkillMod", "1.0.2")]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    public class FitnessSkill : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.GatheringSkillMod");

        private static ConfigEntry<float> configSkillIncrease;
        private static ConfigEntry<float> maxStaminaMultiplier;
        private static ConfigEntry<float> regenStaminaMultiplier;
        private static ConfigEntry<float> baseStaminaRegen;
        private static ConfigEntry<float> baseStamina;
        private static ConfigEntry<float> skillGainIncrement;
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        private static Sprite LoadCustomTexture()
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(directoryName, "fitness.png");
            Texture2D texture2D = LoadTexture(filepath);
            return Sprite.Create(texture2D, new Rect(0f, 0f, 32f, 32f), Vector2.zero);
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

        //hopefully this doesn't conflict.
        public const int SKILL_TYPE = 243;

        void Awake()
        {
            harmony.PatchAll();

            configSkillIncrease = Config.Bind("Progression",
                                           "LevelingIncrement",
                                           1.0f,
                                           "Increment to increase skill per use of a full bar of stamina");

            baseStamina = Config.Bind("Stamina",
                                           "BaseMaximum",
                                           75f,
                                           "Base Max Stamina. The default is 75 which is the same as the umodded game");
            maxStaminaMultiplier = Config.Bind("Stamina",
                                           "MaxMultiplier",
                                           1.5f,
                                           "Maximum stamina multiplier (at level 100). Minimum 1");
            baseStaminaRegen = Config.Bind("Stamina",
                                           "BaseRegen",
                                           5f,
                                           "Base Regen. The default is 5 which is the same as the umodded game");
            regenStaminaMultiplier = Config.Bind("Stamina",
                                           "RegenMultiplier",
                                           1.5f,
                                           "Stamina regen multiplier (at level 100). Minimum 1");

            skillGainIncrement = Config.Bind("Stamina",
                                           "GainIncrement",
                                           25f,
                                           "Amount of stamina used before any skill is gained");


            SkillInjector.RegisterNewSkill(SKILL_TYPE, "Fitness", "Affects maximum stamina level", 1.0f, LoadCustomTexture());
        }

        [HarmonyPatch(typeof(Player), "SetMaxStamina")]
        public static class ApplyFitnessEffects
        {

            [HarmonyPrefix]
            public static void Prefix(ref float stamina, Player __instance)
            {
                try
                {
                    //adjust the amount by the multiplier
                    var factor = __instance.GetSkillFactor((Skills.SkillType)SKILL_TYPE);

                    var amount = (float)Math.Ceiling(factor * (maxStaminaMultiplier.Value - 1.0f) * baseStamina.Value);

                    stamina += amount + baseStamina.Value - 75f; //offset from base of 75.
                } catch(Exception e)
                {
                    Debug.LogError("Error adusting base stamina: " + e.ToString());
                }
            }
        }

        [HarmonyPatch(typeof(Player), "UpdateStats")]
        public static class ApplyFitnessRegen
        {

            [HarmonyPrefix]
            public static void Prefix(Player __instance, ref float ___m_staminaRegen)
            {
                try
                {
                    //adjust the amount by the multiplier
                    var factor = __instance.GetSkillFactor((Skills.SkillType)SKILL_TYPE);
                    var amount = (float)Math.Ceiling((factor * (regenStaminaMultiplier.Value - 1.0f) + 1.0f) * baseStaminaRegen.Value);

                    ___m_staminaRegen = amount;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error adusting stamina regen: " + e.ToString());
                }
            }
        }

        [HarmonyPatch(typeof(Player), "RPC_UseStamina")]
        public static class IncreaseFitnessSkill
        {

            [HarmonyPrefix]
            public static void Prefix(float v, Player __instance, ZNetView ___m_nview)
            {
                try
                {
                    var progress = ___m_nview.GetZDO().GetFloat("fitness_progress", 0f);
                    //adjust the amount by the multiplier
                    progress += v;

                    if (progress > skillGainIncrement.Value)
                    {
                        var ratio = progress / __instance.GetMaxStamina();
                        __instance.RaiseSkill((Skills.SkillType)SKILL_TYPE, ratio * configSkillIncrease.Value);
                        ___m_nview.GetZDO().Set("fitness_progress", 0f);
                    } else
                    {
                        ___m_nview.GetZDO().Set("fitness_progress", progress);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error increasing fitness skill: " + e.ToString());
                }
            }
        }
    }
}
