using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pipakin.FitnessSkillMod
{
    [BepInPlugin("com.pipakin.FitnessSkillMod", "FitnessSkillMod", "1.0.0")]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    public class FitnessSkill : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.GatheringSkillMod");

        private static ConfigEntry<float> configSkillIncrease;
        private static ConfigEntry<float> maxStaminaMultiplier;
        private static ConfigEntry<float> skillGainIncrement;

        //hopefully this doesn't conflict.
        public const int SKILL_TYPE = 243;

        void Awake()
        {
            harmony.PatchAll();

            configSkillIncrease = Config.Bind("Progression",
                                           "LevelingIncrement",
                                           1.0f,
                                           "Increment to increase skill per use of a full bar of stamina");

            maxStaminaMultiplier = Config.Bind("Stamina",
                                           "MaxMultiplier",
                                           1.5f,
                                           "Maximum stamina multiplier (at level 100). Minimum 1");
            skillGainIncrement = Config.Bind("Stamina",
                                           "GainIncrement",
                                           25f,
                                           "Amount of stamina used before any skill is gained");


            SkillInjector.RegisterNewSkill(SKILL_TYPE, "Fitness", "Affects maximum stamina level", 1.0f, null, Skills.SkillType.Run);
        }

        //Overrides
        // - Player
        //    public void SetMaxStamina(float stamina, bool flashBar)
        //    private void RPC_UseStamina(long sender, float v)

        [HarmonyPatch(typeof(Player), "SetMaxStamina")]
        public static class ApplyFitnessEffects
        {

            [HarmonyPrefix]
            public static void Prefix(ref float stamina, Player __instance)
            {
                //adjust the amount by the multiplier
                var factor = __instance.GetSkillFactor((Skills.SkillType)SKILL_TYPE);
                var amount = (float)Math.Ceiling(factor * (maxStaminaMultiplier.Value - 1.0f) * 75f);

                stamina += amount;
            }
        }

        [HarmonyPatch(typeof(Player), "RPC_UseStamina")]
        public static class IncreaseFitnessSkill
        {

            [HarmonyPrefix]
            public static void Prefix(float v, Player __instance, ZNetView ___m_nview)
            {
                var progress = ___m_nview.GetZDO().GetFloat("fitness_progress", 0f);
                //adjust the amount by the multiplier
                progress += v;

                if (progress > skillGainIncrement.Value)
                {
                    var ratio = progress / __instance.GetMaxStamina();
                    __instance.RaiseSkill((Skills.SkillType)SKILL_TYPE, ratio);
                    ___m_nview.GetZDO().Set("fitness_progress", 0f);
                } else
                {
                    ___m_nview.GetZDO().Set("fitness_progress", progress);
                }
            }
        }
    }
}
