
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pipakin.LoxMod
{
    public static class CharacterExt
    {
        private static ZNetView GetZNetView(this Character instance)
        {
            return Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();
        }

        public static bool IsLox(this Character instance)
        {
            var nview = instance.GetZNetView();
            if (!nview.IsValid())
                return false;

            return ZNetScene.instance.GetPrefab(nview.GetZDO().GetPrefab()).name == "Lox";
        }

        public static bool IsLoxling(this Character instance)
        {
            var nview = instance.GetZNetView();
            if (!nview.IsValid())
                return false;

            //return true;
            return nview.GetZDO().GetBool("loxmod_isloxling", false);
        }

        public static void SetAsLoxling(this Character instance)
        {
            var nview = instance.GetZNetView();
            if (!nview.IsValid())
                return;

            nview.GetZDO().Set("loxmod_isloxling", true);
            instance.transform.localScale = LoxMod.loxConfig.LoxlingScale;
            instance.GetComponent<CharacterDrop>().SetDropsEnabled(false);

            var component = instance.GetComponent<Growup>();
            if(component == null)
            {
                component = instance.gameObject.AddComponent<Growup>();
            }
            component.m_grownPrefab = ZNetScene.instance.GetPrefab("Lox");
            component.m_growTime = LoxMod.loxConfig.LoxlingGrowupTime;

            List<EffectList.EffectData> toRemove = new List<EffectList.EffectData>();

            foreach(var effect in instance.m_deathEffects.m_effectPrefabs)
            {
                Debug.Log("Effect found:");
                foreach(var comp in effect.m_prefab.gameObject.GetComponents<Ragdoll>())
                {
                    Debug.Log(" - Component: " + comp.name);
                    toRemove.Add(effect);
                }
            }
            instance.m_deathEffects.m_effectPrefabs = instance.m_deathEffects.m_effectPrefabs.Where(x => !toRemove.Contains(x)).ToArray();

            instance.SetMaxHealth(10f);

        }
    }
}
