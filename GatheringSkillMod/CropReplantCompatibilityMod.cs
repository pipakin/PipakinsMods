using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Pipakin.GatheringMod
{
    [BepInPlugin("com.pipakin.GatheringSkillCropReplantCompatibilityMod", "GatheringSkillCropReplantCompatibilityMod", "1.0.0")]
    [BepInDependency("com.pipakin.GatheringSkillMod")]
    [BepInDependency("com.github.johndowson.CropReplant")]
    class CropReplantCompatibilityMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.GatheringSkillCropReplantCompatibilityMod");

        
        void Awake()
        {
            harmony.PatchAll(typeof(CRCompatability));
        }

        [HarmonyPatch(typeof(CropReplant.PickableExt), "Replant")]
        public static class CRCompatability
        {
            [HarmonyPrefix]
            public static void Prefix(Pickable pickable, Player player)
            {
                //if I'm interacting and it's not picked, I'm gonna pick it!
                if (!Traverse.Create(pickable).Field("m_picked").GetValue<bool>())
                {
                    //add some skillzz!
                    GatheringSkillMod.IncreaseSkill(player, pickable.name);
                }
            }
        }
    }
}
