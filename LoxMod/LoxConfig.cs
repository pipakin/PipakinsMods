using BepInEx.Configuration;
using ModConfigEnforcer;
using UnityEngine;

namespace Pipakin.LoxMod
{
    public class LoxConfig
	{
		public ConfigVariable<float> configLoxlingScale;
		public ConfigVariable<string> configLoxlingName;
		public ConfigVariable<float> configLoxlingGrowupTime;
		public ConfigVariable<float> configLoxProcreationTime;
		public ConfigVariable<float> configLoxProcreationDistance;
		public ConfigVariable<float> configLoxProcreationLimitDistance;
		private ConfigVariable<int> configLoxProcreationLovePoints;
		private ConfigVariable<int> configLoxProcreationCreatureLimit;

		public Vector3 LoxlingScale
		{
			get { return new Vector3(configLoxlingScale?.Value ?? 0.2f, configLoxlingScale?.Value ?? 0.2f, configLoxlingScale?.Value ?? 0.2f); }
		}

		public string LoxlingName
		{
			get { return configLoxlingName?.Value ?? "Loxling"; }
		}

		public float LoxlingGrowupTime
		{
			get { return configLoxlingGrowupTime?.Value ?? 7200f; }
		}

		public float LoxProcreationTime
		{
			get { return configLoxProcreationTime?.Value ?? 300f; }
		}

		public float LoxProcreationDistance
		{
			get { return configLoxProcreationDistance?.Value ?? 40f; }
		}

		public float LoxProcreationLimitDistance
		{
			get { return configLoxProcreationLimitDistance?.Value ?? 160f; }
		}
		public int LoxProcreationLovePoints
		{
			get { return configLoxProcreationLovePoints?.Value ?? 4; }
		}

		public int LoxProcreationCreatureLimit
		{
			get { return configLoxProcreationCreatureLimit?.Value ?? 6; }
		}

		public void InitConfig(string id, ConfigFile config)
		{
			config.Bind<int>("General", "NexusID", 546, "Nexus mod ID for updates");

			ConfigManager.RegisterMod(id, config);

			configLoxlingScale = ConfigManager.RegisterModConfigVariable<float>(id, "Scale", 0.2f, "Baby", "Scale to display baby loxes at.", false);
			configLoxlingName = ConfigManager.RegisterModConfigVariable<string>(id, "Name", "Loxling", "Baby", "Name of baby loxes", true);
			configLoxlingGrowupTime = ConfigManager.RegisterModConfigVariable<float>(id, "GrowupTime", 7200f, "Baby", "Number of seconds for baby to grow up", false);
			configLoxProcreationTime = ConfigManager.RegisterModConfigVariable<float>(id, "Time", 300f, "Procreation", "Number of seconds for parent to be pregnant", false);
			configLoxProcreationDistance = ConfigManager.RegisterModConfigVariable<float>(id, "Distance", 40f, "Procreation", "Distance to find procreation partner", false);
			configLoxProcreationLimitDistance = ConfigManager.RegisterModConfigVariable<float>(id, "LimitDistance", 160f, "Procreation", "Distance to check for limit", false);
			configLoxProcreationLovePoints = ConfigManager.RegisterModConfigVariable<int>(id, "LovePoints", 4, "Procreation", "Number of interactions required to become pregnant", false);
			configLoxProcreationCreatureLimit = ConfigManager.RegisterModConfigVariable<int>(id, "LimitCount", 6, "Procreation", "Max number of creatures in range", false);

		}
	}
}
