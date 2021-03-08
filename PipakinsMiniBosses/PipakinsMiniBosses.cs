using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipakinsMiniBosses
{
    [BepInPlugin("com.pipakin.PipakinsMiniBosses", "PipakinsMiniBosses", "0.1")]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    public class PipakinsMiniBosses : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.PipakinsMiniBosses");

        void Awake()
        {
            harmony.PatchAll();
        }
    }
}
