using RimWorld;
using Verse;
using HarmonyLib;

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
                    //break;

                case "KaiyiKarmic_Yaoma":
                    return YaomaStorytellerUtility.KaiyiKarmicPreUtility(__instance);
                    //break;

                default: return true;

            };
        }

        

    }
}
