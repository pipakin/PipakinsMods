using BepInEx.Configuration;
using ModConfigEnforcer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipakin.GatheringMod
{
    class GatheringConfig
    {
        private ConfigVariable<float> configSkillIncrease;
        private ConfigVariable<string> ignorePickables;

        private ConfigVariable<bool> estimatesEnabled;
        private ConfigVariable<int> configSimpleEstimateLevel;
        private ConfigVariable<int> configDetailedEstimateLevel;


        private ConfigVariable<bool> dropsEnabled;
        private ConfigVariable<string> dropMode;
        private ConfigVariable<int> maxDropMultiplier;


        public void InitConfig(string id, ConfigFile config)
        {
            config.Bind<int>("General", "NexusID", 342, "Nexus mod ID for updates");

            ConfigManager.RegisterMod(id, config);

            configSkillIncrease = ConfigManager.RegisterModConfigVariable(id,
                                           "LevelingIncrement",
                                           1.0f,
                                           "Progression",
                                           "Increment to increase skill per interaction",
                                           false);

            ignorePickables = ConfigManager.RegisterModConfigVariable(id,
                                           "IgnorePickables",
                                           "",
                                           "Progression",
                                           "Pickables to ignore. Comma seperated list of object names, e.g. Pickable_Carrot,Pickable_CarrotSeeds",
                                           false);

            estimatesEnabled = ConfigManager.RegisterModConfigVariable(id,
                                           "Enabled",
                                           true,
                                           "TimeEstimate",
                                           "Enable showing estimates. Disable this if you have another mod you want to use estimates from.",
                                           false);

            configSimpleEstimateLevel = ConfigManager.RegisterModConfigVariable(id,
                                           "Simple",
                                           1,
                                           "TimeEstimate",
                                           "Level at which to show simple estimates",
                                           false);

            configDetailedEstimateLevel = ConfigManager.RegisterModConfigVariable(id,
                                           "Detailed",
                                           10,
                                           "TimeEstimate",
                                           "Level at which to show detailed (to the minute) estimates",
                                           false);

            dropsEnabled = ConfigManager.RegisterModConfigVariable(id,
                                           "Enabled",
                                           true,
                                           "Drops",
                                           "Enable changing drop amounts.",
                                           false);

            dropMode = ConfigManager.RegisterModConfigVariable(id,
                                           "Mode",
                                           "Linear",
                                           "Drops",
                                           "Mode for increasing the drop rate. Valid values are Linear, Random, and PartialRandom",
                                           false);

            maxDropMultiplier = ConfigManager.RegisterModConfigVariable(id,
                                           "MaxMultiplier",
                                           4,
                                           "Drops",
                                           "Maximum drop multiplier (at level 100). Minimum 1",
                                           false);
        }

        public float SkillIncrease
        {
            get { return configSkillIncrease.Value; }
        }

        public List<String> IgnorePickables
        {
            get { return ignorePickables.Value.Split(',').Select(x => x.Trim()).Where(x => x != "").ToList(); }
        }

        public bool EstimatesEnabled
        {
            get { return estimatesEnabled.Value; }
        }

        public int SimpleEstimateLevel
        {
            get { return configSimpleEstimateLevel.Value; }
        }

        public int DetailedEstimateLevel
        {
            get { return configDetailedEstimateLevel.Value; }
        }

        public bool DropsEnabled
        {
            get { return dropsEnabled.Value; }
        }

        public GatheringSkillMod.DropMode DropMode
        {
            get { return (GatheringSkillMod.DropMode)Enum.Parse(typeof(GatheringSkillMod.DropMode), dropMode.Value); }
        }
        
        public int MaxDropMultiplier
        {
            get { return maxDropMultiplier.Value; }
        }
    }
}
