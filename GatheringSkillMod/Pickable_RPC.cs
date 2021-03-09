using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipakin.GatheringMod
{
    public static class Pickable_RPC
    {
        public static void RegisterSetMultiplierRPC(this Pickable instance)
        {
            var m_nview = Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();
            ZDO zDO = m_nview.GetZDO();
            if (zDO != null)
            {
                m_nview.Register<float>("SetMultiplier", (long id, float mult) => instance.RPC_SetMultiplier(id, mult));
            }
        }
        public static void SetMultiplier(this Pickable instance, float multiplier)
        {
            var m_nview = Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();
            m_nview.InvokeRPC("SetMultiplier", multiplier);
        }

        public static void RPC_SetMultiplier(this Pickable instance, long sender, float multiplier)
        {
            var m_nview = Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();

            var ZDO = m_nview.GetZDO();
            if(ZDO != null)
            {
                ZDO.Set("gathering_drop_mult", multiplier);
            }
        }

        public static void ClearMultiplier(this Pickable instance)
        {
            var m_nview = Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();

            var ZDO = m_nview.GetZDO();
            if (ZDO != null)
            {
                ZDO.Set("gathering_drop_mult", 1.0f);
            }
        }

        public static float GetMultiplier(this Pickable instance)
        {
            var m_nview = Traverse.Create(instance).Field("m_nview").GetValue<ZNetView>();

            var ZDO = m_nview.GetZDO();
            if (ZDO != null)
            {
                return ZDO.GetFloat("gathering_drop_mult", 1.0f);
            }

            return 1.0f;
        }
    }
}
