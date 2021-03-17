using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.LoxMod.Config;
using System;
using System.Linq;
using UnityEngine;

namespace Pipakin.LoxMod
{
    public class LoxConfig
	{
		public GenericConfigVariable<float> configLoxlingScale;
		public GenericConfigVariable<string> configLoxlingName;
		public GenericConfigVariable<float> configLoxlingGrowupTime;
		public GenericConfigVariable<float> configLoxProcreationTime;
		public GenericConfigVariable<float> configLoxProcreationDistance;
		public GenericConfigVariable<float> configLoxProcreationLimitDistance;
		private GenericConfigVariable<int> configLoxProcreationLovePoints;
		private GenericConfigVariable<int> configLoxProcreationCreatureLimit;

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

			var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ModConfigEnforcer");

			if (assembly != null)
			{
				Debug.Log("[LoxMod] Mod Config Enforcer detected, registering mod...");
				var configManagerType = assembly.GetType("ModConfigEnforcer.ConfigManager");
				Traverse.Create(configManagerType).Method("RegisterMod", id, config).GetValue(id, config);
			}
			else
			{
				Debug.Log("Mod Config Enforcer not detected.");
			}

			configLoxlingScale = new GenericConfigVariable<float>(assembly, config, id, "Scale", 0.2f, "Baby", "Scale to display baby loxes at.", false);
			configLoxlingName = new GenericConfigVariable<string>(assembly, config, id, "Name", "Loxling", "Baby", "Name of baby loxes", true);
			configLoxlingGrowupTime = new GenericConfigVariable<float>(assembly, config, id, "GrowupTime", 7200f, "Baby", "Number of seconds for baby to grow up", false);
			configLoxProcreationTime = new GenericConfigVariable<float>(assembly, config, id, "Time", 300f, "Procreation", "Number of seconds for parent to be pregnant", false);
			configLoxProcreationDistance = new GenericConfigVariable<float>(assembly, config, id, "Distance", 40f, "Procreation", "Distance to find procreation partner", false);
			configLoxProcreationLimitDistance = new GenericConfigVariable<float>(assembly, config, id, "LimitDistance", 160f, "Procreation", "Distance to check for limit", false);
			configLoxProcreationLovePoints = new GenericConfigVariable<int>(assembly, config, id, "LovePoints", 4, "Procreation", "Number of interactions required to become pregnant", false);
			configLoxProcreationCreatureLimit = new GenericConfigVariable<int>(assembly, config, id, "LimitCount", 6, "Procreation", "Max number of creatures in range", false);

		}
	}
}
