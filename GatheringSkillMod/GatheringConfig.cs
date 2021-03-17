using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.GatheringMod.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pipakin.GatheringMod
{
    class GatheringConfig
    {
        private GenericConfigVariable<float> configSkillIncrease;
        private GenericConfigVariable<string> ignorePickables;

        private GenericConfigVariable<bool> estimatesEnabled;
        private GenericConfigVariable<int> configSimpleEstimateLevel;
        private GenericConfigVariable<int> configDetailedEstimateLevel;


        private GenericConfigVariable<bool> dropsEnabled;
        private GenericConfigVariable<string> dropMode;
        private GenericConfigVariable<int> maxDropMultiplier;


        public void InitConfig(string id, ConfigFile config)
        {
            config.Bind<int>("General", "NexusID", 342, "Nexus mod ID for updates");

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ModConfigEnforcer");

            if(assembly != null)
            {
                Debug.Log("[GatheringMod] Mod Config Enforcer detected, registering mod...");
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
                                           "Increment to increase skill per interaction",
                                           false);

            ignorePickables = new GenericConfigVariable<string>(assembly,
                                           config,
                                           id,
                                           "IgnorePickables",
                                           "",
                                           "Progression",
                                           "Pickables to ignore. Comma seperated list of object names, e.g. Pickable_Carrot,Pickable_CarrotSeeds",
                                           false);

            estimatesEnabled = new GenericConfigVariable<bool>(assembly,
                                           config,
                                           id,
                                           "Enabled",
                                           true,
                                           "TimeEstimate",
                                           "Enable showing estimates. Disable this if you have another mod you want to use estimates from.",
                                           false);

            configSimpleEstimateLevel = new GenericConfigVariable<int>(assembly,
                                           config,
                                           id,
                                           "Simple",
                                           1,
                                           "TimeEstimate",
                                           "Level at which to show simple estimates",
                                           false);

            configDetailedEstimateLevel = new GenericConfigVariable<int>(assembly,
                                           config,
                                           id,
                                           "Detailed",
                                           10,
                                           "TimeEstimate",
                                           "Level at which to show detailed (to the minute) estimates",
                                           false);

            dropsEnabled = new GenericConfigVariable<bool>(assembly,
                                           config,
                                           id,
                                           "Enabled",
                                           true,
                                           "Drops",
                                           "Enable changing drop amounts.",
                                           false);

            dropMode = new GenericConfigVariable<string>(assembly,
                                           config,
                                           id,
                                           "Mode",
                                           "Linear",
                                           "Drops",
                                           "Mode for increasing the drop rate. Valid values are Linear, Random, and PartialRandom",
                                           false);

            maxDropMultiplier = new GenericConfigVariable<int>(assembly,
                                           config,
                                           id,
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
