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

            // KillDaji_Post_Yaoma
            // decrease crimson psychosis severity on pawn kill
            harmony.Patch(AccessTools.Method(typeof(Pawn), "Kill"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(KillDaji_Post_Yaoma)));

            // ApplyMeleeDamageToTarget_Post_DajiLifesteal
            // give melee attacks lifesteal
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ApplyMeleeDamageToTarget_Post_DajiLifesteal)));
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

        // POSTFIX: on pawn kill, reduce a pawn's Crimson Psychosis severity (if they have the hediff) based on the setting
        public static void KillDaji_Post_Yaoma(DamageInfo? __0)
        {
            if(__0?.Instigator as Pawn != null)
            {
                YaomaStorytellerUtility.DeathlessDajiMurderSanity(__0?.Instigator as Pawn);
            }
        }

        // POSTFIX: upon dealing melee damage, heal pawn by a proportion of the damage done (Verb_MeleeAttackDamage)
        public static void ApplyMeleeDamageToTarget_Post_DajiLifesteal(Verb_MeleeAttackDamage __instance, ref DamageWorker.DamageResult __result)
        {
            if (__result is null) return;
            if (__instance.CasterPawn != null) YaomaStorytellerUtility.DeathlessDajiLifestealMelee(__instance.CasterPawn, __result);
        }

    }
}
