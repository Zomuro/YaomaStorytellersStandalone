using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // if the storyteller being potentially switched FROM is NOT Kaiyi, run the normal contents of Page_SelectStorytellerInGame
            if (YaomaStorytellerUtility.settings.KaiyiKarmicSavePersist || Current.Game.storyteller.def != StorytellerDefOf.KaiyiKarmic_Yaoma)
            {
                YaomaStorytellerUtility.GameComp.ResetJianghuJinSave(); // reset Jin's sole saved value (days count to fire her incident) - it should be reset anyways on st change
                return true;
            }

            Traverse traverse = Traverse.Create(__instance);
            traverse.Method("DrawPageTitle", new[] { typeof(Rect) }).GetValue(__0);
            Rect mainRect = traverse.Method("GetMainRect", new[] { typeof(Rect), typeof(float), typeof(bool) }).GetValue<Rect>(__0, 0f, false);
            Storyteller storyteller = Current.Game.storyteller;
            StorytellerDef def = Current.Game.storyteller.def;
            StorytellerUI.DrawStorytellerSelectionInterface(mainRect, ref storyteller.def, ref storyteller.difficultyDef, ref storyteller.difficulty, 
                traverse.Field("selectedStorytellerInfoListing").GetValue<Listing_Standard>());

            if (storyteller.def != def)
            {
                StorytellerSelectionDialog(def);
            }

            return false;
        }

        public static void StorytellerSelectionDialog(StorytellerDef orgDef)
        {
            Storyteller storyteller = Current.Game.storyteller;

            // setup continue (finialize change in storyteller) or cancel (return to original storyteller, probably Kaiyi)
            Action contChange = delegate () 
            {
                YaomaStorytellerUtility.GameComp.ResetExposedData(orgDef);
                storyteller.Notify_DefChanged(); 
            };
            Action cancelChange = delegate () { storyteller.def = orgDef; };

            Find.WindowStack.Add(new Dialog_MessageBox("YS_KaiyiKarmicChangeWarning".Translate(orgDef.LabelCap, storyteller.def.LabelCap),
                "YS_KaiyiKarmicChangeContinue".Translate(), contChange,
                "YS_KaiyiKarmicChangeCancel".Translate(), cancelChange)
            { doCloseX = false, closeOnClickedOutside = false });
        }

    }
}
