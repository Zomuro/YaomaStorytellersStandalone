using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches // the reason shit here works
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("Zomuro.YaomaStorytellers");

            // StorytellerTick_Post_Yaoma: postfix for both Deathless Daji + Kaiyi the Karmic
            harmony.Patch(AccessTools.Method(typeof(Storyteller), "StorytellerTick"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(StorytellerTick_Post_Yaoma)));

            // StorytellerTick_Pre_Yaoma: prefix for Farseer Fan
            harmony.Patch(AccessTools.Method(typeof(Storyteller), "StorytellerTick"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(StorytellerTick_Pre_Yaoma)));

            // MakeIncidentsForInterval_Pre_Yaoma: prefix for Kaiyi the Karmic to modify incidents for mechanic
            harmony.Patch(AccessTools.Method(typeof(Storyteller), "MakeIncidentsForInterval"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeIncidentsForInterval_Pre_Yaoma)));

            // StorytellerDefDajiToggle_Prefix: prefix to enable togglable storyteller portraits (but hardcoded to Deathless Daji)
            harmony.Patch(AccessTools.Method(typeof(StorytellerDef), "ResolveReferences"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(StorytellerDefDajiToggle_Prefix)));

            // KillDaji_Post_Yaoma: decrease crimson psychosis severity on pawn kill
            harmony.Patch(AccessTools.Method(typeof(Pawn), "Kill"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(KillDaji_Post_Yaoma)));

            // GenerateInitialHediffs_Postfix: decrease crimson psychosis severity on pawn kill
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateInitialHediffs"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(GenerateInitialHediffs_Postfix)));

            // ApplyMeleeDamageToTarget_Post_DajiLifesteal: give melee attacks lifesteal
            harmony.Patch(AccessTools.Method(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget"),
                null, new HarmonyMethod(typeof(HarmonyPatches), nameof(ApplyMeleeDamageToTarget_Post_DajiLifesteal)));

            // DoWindowContentsKaiyi_Pre_Confirm: code for switching away from Kaiyi ingame
            harmony.Patch(AccessTools.Method(typeof(Page_SelectStorytellerInGame), "DoWindowContents"),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DoWindowContentsKaiyi_Pre_Confirm)));
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
                case "JianghuJin_Yaoma":
                    YaomaStorytellerUtility.JianghuJinPostUtility(__instance);
                    break;

                default: break;
            };
        }

        // PREFIX: redirect StorytellerTick to work for Farseer Fan or Kaiyi the Karmic
        public static bool StorytellerTick_Pre_Yaoma(Storyteller __instance)
        {
            if (__instance.def == StorytellerDefOf.FarseerFan_Yaoma) return YaomaStorytellerUtility.FarseerFanUtility(__instance);
            return true;
        }

        // PREFIX: redirect MakeIncidentsForInterval to work for Kaiyi the Karmic
        public static bool MakeIncidentsForInterval_Pre_Yaoma(Storyteller __instance, ref IEnumerable<FiringIncident> __result)
        {
            if (__instance.def != StorytellerDefOf.KaiyiKarmic_Yaoma) return true;

            List<FiringIncident> incidents = new List<FiringIncident>();

            // generates a random incident via StorytellerComps - replaces incident with a selected Kaiyi incident as needed
            foreach (var comp in __instance.storytellerComps)
            {
                foreach (FiringIncident randIncident in __instance.MakeIncidentsForInterval(comp, __instance.AllIncidentTargets))
                {
                    incidents.Add(YaomaStorytellerUtility.KaiyiKarmicReplaceIncident(__instance, randIncident));
                }
            }

            // generates a random incident as part of a quest
            foreach (var quest in Find.QuestManager.QuestsListForReading)
            {
                if (quest.State != QuestState.Ongoing) continue;

                foreach (var part in quest.PartsListForReading)
                {
                    IIncidentMakerQuestPart incidentMakerQuestPart = part as IIncidentMakerQuestPart;
                    if (incidentMakerQuestPart is null || ((QuestPartActivable)part).State != QuestPartState.Enabled) continue;

                    foreach (FiringIncident questIncident in incidentMakerQuestPart.MakeIntervalIncidents())
                    {
                        questIncident.sourceQuestPart = part;
                        questIncident.parms.quest = quest;
                        incidents.Add(questIncident);
                    }
                }
            }

            __result = incidents;
            return false;
        }

        // PREFIX: redirect StorytellerDef to use an alternative image (though this is specifically for Deathless Daji)
        public static bool StorytellerDefDajiToggle_Prefix(StorytellerDef __instance)
        {
            return YaomaStorytellerUtility.DeathlessDajiDefUtility(__instance);
        }

        // POSTFIX: on pawn kill, reduce a pawn's Crimson Psychosis severity (if they have the hediff) based on the setting
        public static void KillDaji_Post_Yaoma(DamageInfo? __0)
        {
            if (__0?.Instigator as Pawn != null)
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

        // PREFIX: if the storyteller when the Page_SelectStorytellerInGame window is up is Kaiyi, run through the alternative window contents
        public static bool DoWindowContentsKaiyi_Pre_Confirm(Page_SelectStorytellerInGame __instance, Rect __0)
        {
            // see gameComp for all the conditions
            if (PrefixVerifyStoryteller()) return true;
            Traverse traverse = Traverse.Create(__instance);
            traverse.Method("DrawPageTitle", new[] { typeof(Rect) }).GetValue(__0);
            Rect mainRect = traverse.Method("GetMainRect", new[] { typeof(Rect), typeof(float), typeof(bool) }).GetValue<Rect>(__0, 0f, false);
            Storyteller storyteller = Current.Game.storyteller;
            StorytellerDef def = Current.Game.storyteller.def;
            StorytellerUI.DrawStorytellerSelectionInterface(mainRect, ref storyteller.def, ref storyteller.difficultyDef, ref storyteller.difficulty, 
                traverse.Field("selectedStorytellerInfoListing").GetValue<Listing_Standard>());
            if (storyteller.def != def) StorytellerSelectionDialog(def); // if another storyteller is selected, open up dialog
            return false;
        }

        // helper method to verify if prefix should be fired
        public static bool PrefixVerifyStoryteller()
        {
            Storyteller storyteller = Current.Game.storyteller;
            bool storytellerCheck = storyteller.def != StorytellerDefOf.KaiyiKarmic_Yaoma && storyteller.def != StorytellerDefOf.JianghuJin_Yaoma; // check not Jin or Kaiyi
            bool checkKaiyi = storyteller.def == StorytellerDefOf.KaiyiKarmic_Yaoma && YaomaStorytellerUtility.settings.KaiyiKarmicSavePersist; // check if Kaiyi and setting on
            bool checkJin = storyteller.def == StorytellerDefOf.JianghuJin_Yaoma && YaomaStorytellerUtility.settings.JianghuJinSavePersist;  // check if Jin and setting on
            if (storytellerCheck || checkKaiyi || checkJin) return true; // if one of the conditions is true, set true and skip harmony prefix
            return false;
        }

        // helper method to open up dialog to confirm storyteller selection
        public static void StorytellerSelectionDialog(StorytellerDef orgDef)
        {
            Storyteller storyteller = Current.Game.storyteller;
            Action contChange = delegate () // setup continue (finialize change in storyteller) 
            {
                YaomaStorytellerUtility.GameComp.ResetExposedData(orgDef);
                storyteller.Notify_DefChanged(); 
            };
            Action cancelChange = delegate () { storyteller.def = orgDef; }; // or cancel (return to original storyteller)

            Find.WindowStack.Add(new Dialog_MessageBox(YaomaStorytellerUtility.GameComp.GetWarningString(orgDef, storyteller.def),
                "YS_StorytellerChangeContinue".Translate(), contChange,
                "YS_StorytellerChangeCancel".Translate(), cancelChange)
            { doCloseX = false, closeOnClickedOutside = false });
        }

        // POSTFIX: add Crimson Psychosis to pawns based on rng, using a range of values (see how to define curve via settings)
        public static void GenerateInitialHediffs_Postfix(Pawn __0, PawnGenerationRequest __1)
        {
            if (Find.Storyteller.def != StorytellerDefOf.DeathlessDaji_Yaoma) return;

            if (__0.health.hediffSet.HasHediff(HediffDefOf_Yaoma.DeathlessDaji_Hediff_Yaoma)) return; // if pawn already has crimson psychosis, don't add it

            YaomaStorytellerSettings modSetting = YaomaStorytellerUtility.settings;
            if (UnityEngine.Random.Range(0f, 1f) < modSetting.DajiCrimsonChance) 
                HealthUtility.AdjustSeverity(__0, HediffDefOf_Yaoma.DeathlessDaji_Hediff_Yaoma, 
                    UnityEngine.Random.Range(((float)modSetting.DajiCrimsonSevRange.min) / 100f, ((float)modSetting.DajiCrimsonSevRange.max) / 100f));            
        }

    }
}
