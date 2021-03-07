using BepInEx;
using HarmonyLib;

namespace PickableTimeFixMod
{
    [BepInPlugin("com.pipakin.PickableTimeFixMod", "PickableTimeFixMod", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class PickableTimeFixMod : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("com.pipakin.PickableTimeFixMod");

        void Awake()
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Pickable), "SetPicked")]
        public static class FixPickableTime
        {
            public class PickState
            {
                public long picked_time;
                public bool picked;
            }

            [HarmonyPrefix]
            public static void Prefix(ZNetView ___m_nview, bool ___m_picked, ref PickState __state)
            {
                __state = new PickState();
                __state.picked_time = ___m_nview.GetZDO().GetLong("picked_time", 0L);
                __state.picked = ___m_picked;
            }

            [HarmonyPostfix]
            public static void Postfix(ZNetView ___m_nview, bool ___m_picked, ref PickState __state)
            {
                //if we're not changing the state, don't change the pick time.
                //Don't do anything if we're the client. Trust the server in that case.
                if (__state != null && __state.picked == ___m_picked && ___m_nview.IsOwner())
                {
                    ___m_nview.GetZDO().Set("picked_time", __state.picked_time);
                }
            }
        }
    }
}
