using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.FitnessSkillMod.Config;
using System;
using System.Linq;
using UnityEngine;

namespace Pipakin.FitnessSkillMod
{
    public class FitnessConfig
    {
        private GenericConfigVariable<float> configSkillIncrease;
        private GenericConfigVariable<float> maxStaminaMultiplier;
        private GenericConfigVariable<float> regenStaminaMultiplier;
        private GenericConfigVariable<float> baseStaminaRegen;
        private GenericConfigVariable<float> baseStamina;
        private GenericConfigVariable<float> skillGainIncrement;

        public float SkillIncrease
        {
            get { return configSkillIncrease.Value; }
        }

        public float MaxStaminaMultiplier
        {
            get { return maxStaminaMultiplier.Value; }
        }

        public float RegenStaminaMultiplier
        {
            get { return regenStaminaMultiplier.Value; }
        }

        public float BaseStaminaRegen
        {
            get { return baseStaminaRegen.Value; }
        }

        public float BaseStamina
        {
            get { return baseStamina.Value; }
        }

        public float SkillGainIncrement
        {
            get { return skillGainIncrement.Value; }
        }

        public void InitConfig(string id, ConfigFile config)
        {
            config.Bind<int>("General", "NexusID", 388, "Nexus mod ID for updates");

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ModConfigEnforcer");

            if (assembly != null)
            {
                Debug.Log("[FitnessMod] Mod Config Enforcer detected, registering mod...");
                var configManagerType = assembly.GetType("ModConfigEnforcer.ConfigManager");
                Traverse.Create(configManagerType).Method("RegisterMod", id, config).GetValue(id, config);
            }
            else
            {
                Debug.Log("Mod Config Enforcer not detected.");
            }

            configSkillIncrease = new GenericConfigVariable<float>(assembly,
                                           config,
                                           id,
                                           "LevelingIncrement",
                                           1.0f,
                                           "Progression",
                                           "Increment to increase skill per use of a full bar of stamina",
                                           true);

            baseStamina = new GenericConfigVariable<float>(assembly,
                                           config,
                                           id,
                                           "BaseMaximum",
                                           75f,
                                           "Stamina",
                                           "Base Max Stamina. The default is 75 which is the same as the umodded game",
                                           true);
            maxStaminaMultiplier = new GenericConfigVariable<float>(assembly,
                                           config,
                                           id,
                                           "MaxMultiplier",
                                           1.5f,
                                           "Stamina",
                                           "Maximum stamina multiplier (at level 100). Minimum 1",
                                           true);
            baseStaminaRegen = new GenericConfigVariable<float>(assembly,
                                           config,
                                           id,
                                           "BaseRegen",
                                           5f,
                                           "Stamina",
                                           "Base Regen. The default is 5 which is the same as the umodded game",
                                           true);
            regenStaminaMultiplier = new GenericConfigVariable<float>(assembly,
                                           config,
                                           id,
                                           "RegenMultiplier",
                                           1.5f,
                                           "Stamina",
                                           "Stamina regen multiplier (at level 100). Minimum 1",
                                           true);

            skillGainIncrement = new GenericConfigVariable<float>(assembly,
                                           config,
                                           id,
                                           "GainIncrement",
                                           25f,
                                           "Stamina",
                                           "Amount of stamina used before any skill is gained",
                                           true);
        }
    }
}
