using BepInEx.Configuration;
using ModConfigEnforcer;

namespace Pipakin.FitnessSkillMod
{
    public class FitnessConfig
    {
        private ConfigVariable<float> configSkillIncrease;
        private ConfigVariable<float> maxStaminaMultiplier;
        private ConfigVariable<float> regenStaminaMultiplier;
        private ConfigVariable<float> baseStaminaRegen;
        private ConfigVariable<float> baseStamina;
        private ConfigVariable<float> skillGainIncrement;

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

            ConfigManager.RegisterMod(id, config);

            configSkillIncrease = ConfigManager.RegisterModConfigVariable<float>(id, 
                                           "LevelingIncrement",
                                           1.0f,
                                           "Progression",
                                           "Increment to increase skill per use of a full bar of stamina",
                                           true);

            baseStamina = ConfigManager.RegisterModConfigVariable<float>(id,
                                           "BaseMaximum",
                                           75f,
                                           "Stamina",
                                           "Base Max Stamina. The default is 75 which is the same as the umodded game",
                                           true);
            maxStaminaMultiplier = ConfigManager.RegisterModConfigVariable<float>(id,
                                           "MaxMultiplier",
                                           1.5f,
                                           "Stamina",
                                           "Maximum stamina multiplier (at level 100). Minimum 1",
                                           true);
            baseStaminaRegen = ConfigManager.RegisterModConfigVariable<float>(id,
                                           "BaseRegen",
                                           5f,
                                           "Stamina",
                                           "Base Regen. The default is 5 which is the same as the umodded game",
                                           true);
            regenStaminaMultiplier = ConfigManager.RegisterModConfigVariable<float>(id,
                                           "RegenMultiplier",
                                           1.5f,
                                           "Stamina",
                                           "Stamina regen multiplier (at level 100). Minimum 1",
                                           true);

            skillGainIncrement = ConfigManager.RegisterModConfigVariable<float>(id,
                                           "GainIncrement",
                                           25f,
                                           "Stamina",
                                           "Amount of stamina used before any skill is gained",
                                           true);
        }
    }
}
