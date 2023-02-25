using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace YaomaStorytellers
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches // the reason shit here works
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Zomuro.YaomaStorytellers");

            // StorytellerTick_Post_Yaoma
            // postfix for both Deathless Daji + Kaiyi the Karmic
            harmony.Patch(AccessTools.Method(typeof(Storyteller), "StorytellerTick"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(StorytellerTick_Post_Yaoma)));

            // StorytellerTick_Pre_Yaoma
            // prefix for Farseer Fan + Kaiyi the Karmic
            harmony.Patch(AccessTools.Method(typeof(Storyteller), "StorytellerTick"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(StorytellerTick_Pre_Yaoma)));

            // StorytellerDefDajiToggle_Prefix
            // prefix to enable togglable storyteller portraits (but hardcoded to Deathless Daji)
            harmony.Patch(AccessTools.Method(typeof(StorytellerDef), "ResolveReferences"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(StorytellerDefDajiToggle_Prefix)));
        }

        // POSTFIX: cause incidents to occur each day or week based on conditions for Deathless Daji or Kaiyi the Karmic
        public static void StorytellerTick_Post_Yaoma(Storyteller __instance)
        {
            switch (__instance.def.defName) 
            {
                case "DeathlessDaji_Yaoma": 
                    YaomaStorytellerUtility.DeathlessDajiUtility(__instance);
                    break;

                case "KaiyiKarmic_Yaoma": 
                    YaomaStorytellerUtility.KaiyiKarmicPostUtility(__instance);
                    break;

                default: break;

            };
        }

        // PREFIX: redirect StorytellerTick to work for Farseer Fan or Kaiyi the Karmic
        public static bool StorytellerTick_Pre_Yaoma(Storyteller __instance)
        {
            if (!DebugSettings.enableStoryteller) return false;

            switch (__instance.def.defName)
            {
                case "FarseerFan_Yaoma":
                    return YaomaStorytellerUtility.FarseerFanUtility(__instance);

                case "KaiyiKarmic_Yaoma":
                    return YaomaStorytellerUtility.KaiyiKarmicPreUtility(__instance);

                default: return true;

            };
        }

        // PREFIX: redirect StorytellerDef to use an alternative image (though this is specifically for Deathless Daji)
        public static bool StorytellerDefDajiToggle_Prefix(StorytellerDef __instance)
        {
            return YaomaStorytellerUtility.DeathlessDajiDefUtility(__instance);
        }

    }
}
