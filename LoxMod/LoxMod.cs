using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pipakin.LoxMod
{
	[BepInPlugin("com.pipakin.LoxMod", "LoxMod", "1.0.0")]
	public class LoxMod : BaseUnityPlugin
	{
		private readonly Harmony harmony = new Harmony("com.pipakin.LoxMod");


		public static ConfigEntry<float> configLoxlingScale;
		public static ConfigEntry<string> configLoxlingName;
		public static ConfigEntry<float> configLoxlingGrowupTime;
		public static ConfigEntry<float> configLoxProcreationTime;
		public static ConfigEntry<float> configLoxProcreationDistance;
		public static ConfigEntry<float> configLoxProcreationLimitDistance;
		private static ConfigEntry<int> configLoxProcreationLovePoints;
		private static ConfigEntry<int> configLoxProcreationCreatureLimit;

		public static Vector3 LoxlingScale
		{
			get { return new Vector3(configLoxlingScale?.Value ?? 0.2f, configLoxlingScale?.Value ?? 0.2f, configLoxlingScale?.Value ?? 0.2f); }
		}

		public static string LoxlingName
		{
			get { return configLoxlingName?.Value ?? "Loxling"; }
		}

		public static float LoxlingGrowupTime
        {
			get { return configLoxlingGrowupTime?.Value ?? 7200f; }
        }

		public static float LoxProcreationTime
        {
			get { return configLoxProcreationTime?.Value ?? 300f; }
        }

		public static float LoxProcreationDistance
        {
			get { return configLoxProcreationDistance?.Value ?? 40f; }
		}

		public static float LoxProcreationLimitDistance
		{
			get { return configLoxProcreationLimitDistance?.Value ?? 160f; }
		}
		private static int LoxProcreationLovePoints
        {
			get { return configLoxProcreationLovePoints?.Value ?? 4; }
        }

		private static int LoxProcreationCreatureLimit
        {
			get { return configLoxProcreationCreatureLimit?.Value ?? 6; }
        }

		void Awake()
        {
			configLoxlingScale = Config.Bind("Baby", "Scale", 0.2f, "Scale to display baby loxes at.");
			configLoxlingName = Config.Bind("Baby", "Name", "Loxling", "Name of baby loxes");
			configLoxlingGrowupTime = Config.Bind("Baby", "GrowupTime", 7200f, "Number of seconds for baby to grow up");
			configLoxProcreationTime = Config.Bind("Procreation", "Time", 300f, "Number of seconds for parent to be pregnant");
			configLoxProcreationDistance = Config.Bind("Procreation", "Distance", 40f, "Distance to find procreation partner");
			configLoxProcreationLimitDistance = Config.Bind("Procreation", "LimitDistance", 160f, "Distance to check for limit");
			configLoxProcreationLovePoints = Config.Bind("Procreation", "LovePoints", 4, "Number of interactions required to become pregnant");
			configLoxProcreationCreatureLimit = Config.Bind("Procreation", "LimitCount", 6, "Max number of creatures in range");

			harmony.PatchAll(typeof(Character_Awake_Hooks));
			harmony.PatchAll(typeof(Character_GetHoverName_Hooks));
			harmony.PatchAll(typeof(CharacterDrop_GenerateDropList_Hooks));
			harmony.PatchAll(typeof(Procreation_Procreate_Hooks));
			harmony.PatchAll(typeof(Procreation_ReadyForProcreation_Hooks));
		}

		[HarmonyPatch(typeof(CharacterDrop), "GenerateDropList")]
		public static class CharacterDrop_GenerateDropList_Hooks
		{
			[HarmonyPostfix]
			public static void Postfix(ref List<KeyValuePair<GameObject, int>> __result, Character ___m_character)
			{
				if (___m_character.IsLox() && ___m_character.IsLoxling())
				{
					//Debug.Log("No loxling drops.");
					__result = new List<KeyValuePair<GameObject, int>>();
				}
			}
		}

		[HarmonyPatch(typeof(Character), "GetHoverName")]
		public static class Character_GetHoverName_Hooks
		{
			[HarmonyPostfix]
			public static void Postfix(Character __instance, ref string __result)
			{
				if(__instance.IsLox() && __instance.IsLoxling())
                {
					__result = LoxlingName;
                }
			}
		}

		[HarmonyPatch(typeof(Character), "Awake")]
		public static class Character_Awake_Hooks
		{

            [HarmonyPostfix]
			public static void Postfix(Character __instance)
            {
				if (__instance.IsLox())
                {
					if (__instance.IsLoxling())
					{
						__instance.SetAsLoxling();
					}

					var proc = __instance.GetComponent<Procreation>();
					if(proc == null)
                    {
						proc = __instance.gameObject.AddComponent<Procreation>();
					}
					proc.m_offspring = __instance.gameObject;
					proc.m_pregnancyDuration = LoxProcreationTime;
					proc.m_totalCheckRange = LoxProcreationLimitDistance;
					proc.m_partnerCheckRange = LoxProcreationDistance;
					proc.m_requiredLovePoints = LoxProcreationLovePoints;
					proc.m_maxCreatures = LoxProcreationCreatureLimit;

					var comm = __instance.GetComponent<Tameable>();
					comm.m_commandable = true;
				}
            }
		}

		[HarmonyPatch(typeof(Procreation), "ReadyForProcreation")]
		public static class Procreation_ReadyForProcreation_Hooks
		{
			[HarmonyPostfix]
			public static void Postfix(Character ___m_character, ref bool __result)
			{
				if(___m_character.IsLoxling())
                {
					//Debug.LogError("Loxlings cannot procreate!");
					__result = false;
                }
			}
		}

		[HarmonyPatch(typeof(Procreation), "Procreate")]
        public static class Procreation_Procreate_Hooks
        {
            [HarmonyPrefix]
            public static bool Prefix(ZNetView ___m_nview, Character ___m_character, BaseAI ___m_baseAI, Tameable ___m_tameable, Procreation __instance)
            {
				if(!___m_nview.IsValid() || !___m_nview.IsOwner() || !___m_character.IsTamed())
				{
					return true;
				}
				if (ZNetScene.instance.GetPrefab(___m_nview.GetZDO().GetPrefab()).name == "Lox")
				{
					//might need to override
					if (__instance.IsPregnant())
					{
						//Debug.LogWarning("Lox is pregnant!");
						var trav = Traverse.Create(__instance);
						if (trav.Method("IsDue").GetValue<bool>())
						{
							//Debug.LogWarning("Lox is due!");
							trav.Method("ResetPregnancy").GetValue();
							GameObject gameObject = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("Lox".GetStableHashCode()), __instance.transform.position - __instance.transform.forward * __instance.m_spawnOffset, Quaternion.LookRotation(-__instance.transform.forward, Vector3.up));
							Character component = gameObject.GetComponent<Character>();
							Character character = __instance.GetComponent<Character>();
							if ((bool)component)
							{
								component.SetTamed(character.IsTamed());
								component.SetLevel(Mathf.Max(__instance.m_minOffspringLevel, character.GetLevel()));
								component.SetAsLoxling();
							}
							__instance.m_birthEffects.Create(gameObject.transform.position, Quaternion.identity);
						}
					}
					else
					{
						if(___m_character.IsLoxling())
                        {
							Debug.LogError("No breeding for loxlings");
							return false;
                        }
						if (UnityEngine.Random.value <= __instance.m_pregnancyChance || ___m_baseAI.IsAlerted() || ___m_tameable.IsHungry())
						{
							return false;
						}
						var prefab = ZNetScene.instance.GetPrefab("Lox".GetStableHashCode());
						int nrOfInstances = SpawnSystem.GetNrOfInstances(prefab, __instance.transform.position, __instance.m_totalCheckRange);
						if (nrOfInstances < __instance.m_maxCreatures && SpawnSystem.GetNrOfInstances(prefab, __instance.transform.position, __instance.m_partnerCheckRange, eventCreaturesOnly: false, procreationOnly: true) >= 2)
						{
							__instance.GetComponent<Tameable>().m_petEffect.Create(__instance.transform.position, Quaternion.identity);
							int @int = ___m_nview.GetZDO().GetInt("lovePoints");
							@int++;
							___m_nview.GetZDO().Set("lovePoints", @int);
							if (@int >= __instance.m_requiredLovePoints)
							{
								___m_nview.GetZDO().Set("lovePoints", 0);
								___m_nview.GetZDO().Set("pregnant", ZNet.instance.GetTime().Ticks);
							}
						}else
                        {
							//Debug.LogError("Bad loxling numbers.");
						}
					}
					return false;
				}
				//Debug.LogWarning(ZNetScene.instance.GetPrefab(___m_nview.GetZDO().GetPrefab()).name + " is not a lox.");
				return true;
			}
        }

    }
}
