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
    [BepInPlugin("com.pipakin.FitnessSkillMod", "FitnessSkillMod", "2.0.0")]
    [BepInDependency("pfhoenix.modconfigenforcer", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    public class FitnessSkill : BaseUnityPlugin
    {
        const string MOD_ID = "com.pipakin.FitnessSkillMod";
        private readonly Harmony harmony = new Harmony(MOD_ID);

        private static FitnessConfig fitnessConfig = new FitnessConfig();
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        private static Sprite LoadCustomTexture()
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(directoryName, "fitness.png");
            if (File.Exists(filepath))
            {
                Texture2D texture2D = LoadTexture(filepath);
                return Sprite.Create(texture2D, new Rect(0f, 0f, 32f, 32f), Vector2.zero);
            }
            else
            {
                Debug.LogError("Unable to load skill icon! Make sure you place the fitness.png file in the plugins directory!");
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

        //hopefully this doesn't conflict.
        public const int SKILL_TYPE = 243;

        void Awake()
        {
            fitnessConfig.InitConfig(MOD_ID, Config);

            harmony.PatchAll(typeof(ApplyFitnessEffects));
            harmony.PatchAll(typeof(ApplyFitnessRegen));
            harmony.PatchAll(typeof(IncreaseFitnessSkill));

            SkillInjector.RegisterNewSkill(SKILL_TYPE, "Fitness", "Affects maximum stamina level", 1.0f, LoadCustomTexture(), Skills.SkillType.Run);
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

                    var amount = (float)Math.Ceiling(factor * (fitnessConfig.MaxStaminaMultiplier - 1.0f) * fitnessConfig.BaseStamina);

                    stamina += amount + fitnessConfig.BaseStamina - 75f; //offset from base of 75.
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
                    var amount = (float)Math.Ceiling((factor * (fitnessConfig.RegenStaminaMultiplier - 1.0f) + 1.0f) * fitnessConfig.BaseStaminaRegen);

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

                    if (progress > fitnessConfig.SkillGainIncrement)
                    {
                        var ratio = progress / __instance.GetMaxStamina();
                        __instance.RaiseSkill((Skills.SkillType)SKILL_TYPE, ratio * fitnessConfig.SkillIncrease);
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
