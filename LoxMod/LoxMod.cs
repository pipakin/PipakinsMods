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
using ModConfigEnforcer;

namespace Pipakin.LoxMod
{
	[BepInPlugin("com.pipakin.LoxMod", "LoxMod", "2.0.0")]
	[BepInDependency("pfhoenix.modconfigenforcer")]
	public class LoxMod : BaseUnityPlugin
	{
		const string MOD_ID = "com.pipakin.LoxMod";
		private readonly Harmony harmony = new Harmony(MOD_ID);

		public static LoxConfig loxConfig = new LoxConfig();

		void Awake()
		{
			loxConfig.InitConfig(MOD_ID, Config);

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
					__result = loxConfig.LoxlingName;
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
					proc.m_pregnancyDuration = loxConfig.LoxProcreationTime;
					proc.m_totalCheckRange = loxConfig.LoxProcreationLimitDistance;
					proc.m_partnerCheckRange = loxConfig.LoxProcreationDistance;
					proc.m_requiredLovePoints = loxConfig.LoxProcreationLovePoints;
					proc.m_maxCreatures = loxConfig.LoxProcreationCreatureLimit;

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
							//Debug.LogError("No breeding for loxlings");
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
