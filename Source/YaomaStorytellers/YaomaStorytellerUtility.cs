using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace YaomaStorytellers
{
    public static class YaomaStorytellerUtility
    {
        public static bool FarseerFanUtility(Storyteller storyteller)
        {
            // check if there's nothing in the incident queue every 1000 ticks
            if (Find.TickManager.TicksGame % 1000 == 0 && storyteller.incidentQueue.Count == 0)
            {
                if (!DebugSettings.enableStoryteller) return false;

                // if the gameticks <= minDaysPassed, don't predict events at all; simulate grace period
                StorytellerCompProperties_RandomMain r = (StorytellerCompProperties_RandomMain) storyteller.def.comps.
                    FirstOrDefault(x => x.GetType() == typeof(StorytellerCompProperties_RandomMain));
                if (Find.TickManager.TicksGame <= r.minDaysPassed * 60000 * settings.FarseerFanGracePeriodFactor) return false;

                // simulate incidents till we get one
                int counter = 0; List<FiringIncident> fi_sim = null;
                FarseerFanSimulate(storyteller, ref fi_sim, ref counter);
                
                //switch case
                switch (FarseerFanRandomCase())
                {
                    case 1: // original case- don't do much
                        FarseerFanQueue(storyteller, fi_sim, counter);
                        break;
                    case 2:
                        List<FiringIncident> alt_fi_sim = null;
                        FarseerFanAltSim(storyteller, fi_sim, ref alt_fi_sim);
                        FarseerFanOfferAlt(storyteller, fi_sim, alt_fi_sim, counter);
                        break;
                    case 3:
                        StorytellerComp_OnDemand c = (StorytellerComp_OnDemand)storyteller.storytellerComps.
                            FirstOrDefault(x => x.GetType() == typeof(StorytellerComp_OnDemand));
                        FarseerFanOfferDefer(storyteller, fi_sim, counter, c);
                        break;
                }
            }
            // ticks the incident queue- will automatically fire an incident at the right tick
            storyteller.incidentQueue.IncidentQueueTick();
            return false;
        }

        public static void FarseerFanSimulate(Storyteller storyteller, ref List<FiringIncident> fi_sim, ref int counter)
        {
            // simulate incidents until an incident exists
            // uses a counter to figure out how many "storyteller cycles" (every 1000 ticks) for an event
            counter = 0;
            while (fi_sim.EnumerableNullOrEmpty())
            {
                fi_sim = storyteller.MakeIncidentsForInterval().ToList();
                if (fi_sim.Any()) break;
                counter += 1;
            }

        }

        public static int FarseerFanRandomCase()
        {
            Dictionary<int, float> chance = new Dictionary<int, float>(){ 
                { 1, settings.FarseerFanPredictWeight } 
            };
            if (settings.FarseerFanPredictAlt) chance[2] = settings.FarseerFanPredictAltWeight; ; // only if alterate incidents are enabled
            if (settings.FarseerFanPredictDefer) chance[3] = settings.FarseerFanPredictDeferWeight; ; // only if deferred incidents are enabled

            return chance.RandomElementByWeight(x => x.Value).Key; // return random case
        }

        public static void FarseerFanAltSim(Storyteller storyteller, List<FiringIncident> org_fi_sim, ref List<FiringIncident> alt_fi_sim)
        {           
            while (alt_fi_sim.EnumerableNullOrEmpty())
            {
                alt_fi_sim = storyteller.MakeIncidentsForInterval().ToList();
                if (alt_fi_sim.Any() && alt_fi_sim != org_fi_sim) break;
            }
        }

        public static TaggedString FarseerFanAltText(List<FiringIncident> fi_sim, List<FiringIncident> alt_fi_sim)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("YS_FarseerFanAltOffer".Translate());

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("YS_FarseerFanOrgIncidText".Translate());
            foreach(var fi in fi_sim)
            {
                stringBuilder.AppendLine("\t-" + fi.def.LabelCap.ToString());
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("YS_FarseerFanAltIncidText".Translate());
            foreach (var fi in alt_fi_sim)
            {
                stringBuilder.AppendLine("\t-" + fi.def.LabelCap.ToString());
            }

            return stringBuilder.ToString();
        }

        public static void FarseerFanOfferAlt(Storyteller storyteller, List<FiringIncident> fi_sim,
            List<FiringIncident> alt_fi_sim, int counter)
        {
            Action original = delegate ()
            {
                FarseerFanQueue(storyteller, fi_sim, counter);
            };

            Action alternate = delegate () {
                FarseerFanQueue(storyteller, alt_fi_sim, counter);
            };

            Find.WindowStack.Add(new Dialog_MessageBox(FarseerFanAltText(fi_sim, alt_fi_sim),
                "YS_FarseerFanAltIncidSelect".Translate(), alternate,
                "YS_FarseerFanOrgIncidSelect".Translate(), original)
            { doCloseX = false, closeOnClickedOutside = false });
        }

        public static TaggedString FarseerFanDeferText(List<FiringIncident> fi_sim)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("YS_FarseerFanDeferOffer".Translate());

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("YS_FarseerFanDeferText".Translate());
            foreach (var fi in fi_sim)
            {
                stringBuilder.AppendLine("\t-" + fi.def.LabelCap.ToString());
            }

            return stringBuilder.ToString();
        }

        public static FiringIncident FarseerFanThreat(Storyteller storyteller, StorytellerComp_OnDemand c, ref int temp_count)
        {
            List<FiringIncident> fi_sim_temp = null;
            FarseerFanSimulate(storyteller, ref fi_sim_temp, ref temp_count);

            IncidentDef randomDef = DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.ThreatBig ||
                x.category == IncidentCategoryDefOf.ThreatSmall).RandomElement();
            return c.MakeIncident(storyteller.AllIncidentTargets, randomDef);
        }

        public static void FarseerFanOfferDefer(Storyteller storyteller, List<FiringIncident> fi_sim, int counter, StorytellerComp_OnDemand c)
        {
            Action contIncid = delegate ()
            {
                FarseerFanQueue(storyteller, fi_sim, counter);
            };

            Action deferIncid = delegate () {
                int add_counter = 0;
                //FarseerFanThreat(storyteller, c, ref add_counter);
                fi_sim.Add(FarseerFanThreat(storyteller, c, ref add_counter));
                FarseerFanQueue(storyteller, fi_sim, counter + add_counter);
            };

            Find.WindowStack.Add(new Dialog_MessageBox(FarseerFanDeferText(fi_sim),
                "YS_FarseerFanDeferSelect".Translate(), deferIncid,
                "YS_FarseerFanDeferContinueSelect".Translate(), contIncid)
            { doCloseX = false, closeOnClickedOutside = false });
        }

        public static void FarseerFanQueue(Storyteller storyteller, List<FiringIncident> fi_sim, int counter)
        {
            int ticks = Find.TickManager.TicksGame + counter * 1000;
            int ticksAbs = ticks + Find.TickManager.gameStartAbsTick;
            // for each random incident found:
            foreach (FiringIncident fi in fi_sim)
            {
                // if the incident cannot be fired (say, a duplicate incident), skips it.
                if (!fi.def.Worker.CanFireNow(fi.parms)) continue;

                // adds incident to the queue
                storyteller.incidentQueue.Add(new QueuedIncident(fi, ticks));

                // sends the "Stellar augury" letter to the player, letting them know what incident and when
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("YS_LetterLabelFarseerFan".Translate(fi.def.label),
                    "YS_LetterFarseerFan".Translate(FarseerFanDate(ticksAbs, FarseerFanTileCheck(fi)), fi.def.label) + 
                        endings.RandomElement().Translate(),
                    LetterDefOf.NeutralEvent, null, null));
            }
        }

        public static int FarseerFanTileCheck(FiringIncident fi)
        {
            return fi.parms.target.Tile < 0 ? Find.CurrentMap.Tile : fi.parms.target.Tile;
        }

        public static string FarseerFanDate(long ticksAbs, int tileID)
        {
            if (settings.FarseerFanPredictionDetail)
                return GenDate.DateFullStringWithHourAt(ticksAbs, Find.WorldGrid.LongLatOf(tileID));

            else return GenDate.DateFullStringAt(ticksAbs, Find.WorldGrid.LongLatOf(tileID));
        }

        // Adjust setting karma value with purchases and the such.
        public static void KaiyiKarmicAdjustKarma(StorytellerComp_RandomKarmaMain kt, float adjust)
        {
            kt.GameComp.karma = Math.Max(settings.KaiyiKarmicKarmaMin, Math.Min(kt.GameComp.karma + adjust, settings.KaiyiKarmicKarmaMax));
        }

        // Adjust price multiplier on incidents using this. UNUSED
        public static void KaiyiKarmicIncreasePriceFactor(float adjust)
        {
            settings.KaiyiKarmicBasePriceFactor = Math.Max(0f, Math.Min(settings.KaiyiKarmicBasePriceFactor + adjust, 5f));
        }

        public static void KaiyiKarmicRandomIncident(ref FiringIncident fi)
        {
            fi.parms.points *= KarmaTracker.KarmaPointScaling; // scale incident by point scaling

            // if the incident has a category recognized by the KarmaTracker
            // and has a negative karma cost (i.e. negative events) 
            if (KarmaTracker.GameComp.baseIncidentChange.Keys.Contains(fi.def.category) &&
                KarmaTracker.GameComp.baseIncidentChange[fi.def.category] > 0)
            {
                //gain a half of the value of karma from negative events if they randomly occur
                KaiyiKarmicAdjustKarma(KarmaTracker, KarmaTracker.GameComp.estIncidentChange[fi.def] / 4f);

                // notifies player of their gain
                Find.LetterStack.ReceiveLetter("YS_LetterLabelKaiyiKarmicGain".Translate(),
                    "YS_LetterKaiyiKarmicGain".Translate(Math.Abs(Math.Round(KarmaTracker.GameComp.estIncidentChange[fi.def], 2))),
                    LetterDefOf.NeutralEvent);
            }
        }

        public static FiringIncident KaiyiKarmicReplaceIncident(Storyteller storyteller, FiringIncident fi)
        {
            if (storyteller.def != StorytellerDefOf.KaiyiKarmic_Yaoma) return fi; // skip prefix if the storyteller isn't Kaiyi

            if (KarmaTracker.GameComp.selectedIncidents.NullOrEmpty()) // if the KarmaTracker's selected incidents are empty or null
            {
                KaiyiKarmicRandomIncident(ref fi); // scales points accordingly + allow players to gain karma from negative incidents.
                return fi;
            }

            // No OnDemand comp, return original incident (can't replace incident w/o it)
            if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                   typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return fi;

            // select a random incidentDef from KarmaTracker's selected incidentDefs
            // and create a firing incident using StorytellerComp_OnDemand
            KaiyiIncidentRecord randomRecord = KarmaTracker.GameComp.selectedIncidents.RandomElement();
            FiringIncident fiReplace = c.MakeIncident(storyteller.AllIncidentTargets, randomRecord.incidDef);

            if (fiReplace == null) // if the replaced incident can't be fired, fire the original
            {
                KaiyiKarmicRandomIncident(ref fi); // scales points accordingly + allow players to gain karma from negative incidents.

                if(randomRecord.change > 0)
                {
                    Messages.Message("YS_MessageKarmaReplaceFailReplaceOnly".Translate(),
                            MessageTypeDefOf.SilentInput, false);
                }
                else
                {
                    Messages.Message("YS_MessageKarmaReplaceFail".Translate(randomRecord.change * -1f),
                            MessageTypeDefOf.SilentInput, false); // notify incident replacment failed- will try again later
                    KaiyiKarmicAdjustKarma(KarmaTracker, randomRecord.change * -1f); // perform a refund of the selected incident record
                }
                KarmaTracker.GameComp.selectedIncidents.Remove(randomRecord); // remove from selected incidents list
                return fi;
            } 

            fiReplace.parms.points *= KarmaTracker.KarmaPointScaling; // adjust replaced incident points using scaling
            KarmaTracker.GameComp.selectedIncidents.Remove(randomRecord); // remove from selected incidents list
            if(KarmaTracker.GameComp.selectedIncidents.NullOrEmpty()) // notify if all selected incidents are done
            {
                Messages.Message("YS_MessageKarmaReplaceDone".Translate(),
                            MessageTypeDefOf.SilentInput, false);
            }

            return fiReplace;
        }

        public static void KaiyiKarmicPostUtility(Storyteller storyteller)
        {
            // at the middle of the day (30000 ticks)
            if (GenLocalDate.DayTick(Find.AnyPlayerHomeMap) == 60000 / 2)
            {
                // if KarmaTracker is null, skip
                if (KarmaTracker is null) return;

                // decrease dayCheck (number of days till Kaiyi's incident fires)
                // if dayCheck is still > 0 afterwards, wait for tomorrow
                KarmaTracker.GameComp.daysCheck -= 1;
                if (KarmaTracker.GameComp.daysCheck > 0) return;

                // if there are any selected incidents still there, return and wait for all them to be done
                if (!KarmaTracker.GameComp.selectedIncidents.NullOrEmpty()) return;

                // get StorytellerComp_OnDemand and nullcheck it
                if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                    typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return;

                // attempts to fire the incidentDef linked to StorytellerComp_OnDemand
                // if fired, set the days time to the KarmaTracker's days values
                foreach (FiringIncident fi in c.MakeIncidents(storyteller.AllIncidentTargets))
                {
                    // later on- set days check to whatever value is in settings
                    if (storyteller.TryFire(fi)) KarmaTracker.GameComp.daysCheck = settings.KaiyiKarmicTradeDays;
                }
            }
        }

        public static void KaiyiKarmicSelectableIncidents(ref List<DebugMenuOption> selectable, StorytellerComp_RandomKarmaMain karmaTracker)
        {
            String labelCost = "";

            foreach (IncidentDef iDef in KaiyiKarmicWeightedSelection(karmaTracker, settings.KaiyiKarmicMaxChoices)
                .OrderByDescending(x => karmaTracker.GameComp.estIncidentChange[x])
                .ThenBy(x => x.LabelCap.ToString()))
            {
                labelCost = iDef.LabelCap.ToString() + " (" + Math.Round(karmaTracker.GameComp.estIncidentChange[iDef], 2) + ")";

                selectable.Add(new DebugMenuOption(labelCost, DebugMenuOptionMode.Action, delegate ()
                {
                    Dialog_KarmaTrade karmaDialog = (Find.WindowStack.currentlyDrawnWindow as Dialog_KarmaTrade);
                    List<KaiyiIncidentRecord> incidentsSelected = karmaDialog.selected;

                    if (incidentsSelected.Count < 5)
                    {
                        float estFinalVal = karmaTracker.GameComp.karma + karmaDialog.EstIncidentChange(karmaTracker) + karmaTracker.GameComp.estIncidentChange[iDef];
                        if (estFinalVal <= settings.KaiyiKarmicKarmaMin)
                        {
                            Messages.Message("YS_MessageKaiyiKarmicIncidentDebtFloor".Translate(), MessageTypeDefOf.SilentInput, false);
                        }
                        else if(estFinalVal >= settings.KaiyiKarmicKarmaMax)
                        {
                            //incidentsSelected.Add(iDef);
                            incidentsSelected.Add(new KaiyiIncidentRecord() {incidDef = iDef, 
                                change = KarmaTracker.GameComp.estIncidentChange[iDef]});
                            karmaDialog.RemoveSelectableIncident(iDef);

                            // add portion that removes from option list this incident record or something
                            Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
                            Messages.Message("YS_MessageKaiyiKarmicIncidentReachedMax".Translate(), MessageTypeDefOf.SilentInput, false);
                        }
                        else
                        {
                            //incidentsSelected.Add(iDef);
                            incidentsSelected.Add(new KaiyiIncidentRecord() { incidDef = iDef, 
                                change = KarmaTracker.GameComp.estIncidentChange[iDef]});
                            karmaDialog.RemoveSelectableIncident(iDef);

                            // add portion that removes from option list this incident record or something
                            Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
                        }
                        
                    }
                    else Messages.Message("YS_MessageKaiyiKarmicIncidentsFilled".Translate(), MessageTypeDefOf.RejectInput, false);
                }));
            }
        }

        public static List<DebugMenuOption> KaiyiKarmicSelectableIncidents(List<IncidentDef> incidentDefs, StorytellerComp_RandomKarmaMain karmaTracker)
        {
            String labelCost = "";
            List<DebugMenuOption> selectable = new List<DebugMenuOption>();

            foreach (IncidentDef iDef in incidentDefs)
            {
                labelCost = iDef.LabelCap.ToString() + " (" + Math.Round(karmaTracker.GameComp.estIncidentChange[iDef], 2) + ")";

                selectable.Add(new DebugMenuOption(labelCost, DebugMenuOptionMode.Action, delegate ()
                {
                    Dialog_KarmaTrade karmaDialog = (Find.WindowStack.currentlyDrawnWindow as Dialog_KarmaTrade);
                    List<KaiyiIncidentRecord> incidentsSelected = karmaDialog.selected;

                    if (incidentsSelected.Count < 5)
                    {
                        float estFinalVal = karmaTracker.GameComp.karma + karmaDialog.EstIncidentChange(karmaTracker) + karmaTracker.GameComp.estIncidentChange[iDef];
                        if (estFinalVal <= settings.KaiyiKarmicKarmaMin)
                        {
                            Messages.Message("YS_MessageKaiyiKarmicIncidentDebtFloor".Translate(), MessageTypeDefOf.SilentInput, false);
                        }
                        else if (estFinalVal >= settings.KaiyiKarmicKarmaMax)
                        {
                            //incidentsSelected.Add(iDef);
                            incidentsSelected.Add(new KaiyiIncidentRecord()
                            {
                                incidDef = iDef,
                                change = KarmaTracker.GameComp.estIncidentChange[iDef]
                            });
                            karmaDialog.RemoveSelectableIncident(iDef);

                            // add portion that removes from option list this incident record or something
                            Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
                            Messages.Message("YS_MessageKaiyiKarmicIncidentReachedMax".Translate(), MessageTypeDefOf.SilentInput, false);
                        }
                        else
                        {
                            //incidentsSelected.Add(iDef);
                            incidentsSelected.Add(new KaiyiIncidentRecord()
                            {
                                incidDef = iDef,
                                change = KarmaTracker.GameComp.estIncidentChange[iDef]
                            });
                            karmaDialog.RemoveSelectableIncident(iDef);

                            // add portion that removes from option list this incident record or something
                            Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
                        }

                    }
                    else Messages.Message("YS_MessageKaiyiKarmicIncidentsFilled".Translate(), MessageTypeDefOf.RejectInput, false);
                }));
            }

            return selectable;
        }

        public static List<IncidentDef> KaiyiKarmicGetFreshSelectableIncidents(StorytellerComp_RandomKarmaMain karmaTracker)
        {
            List<IncidentDef> selectable = new List<IncidentDef>();

            foreach (IncidentDef iDef in KaiyiKarmicWeightedSelection(karmaTracker, settings.KaiyiKarmicMaxChoices)
                .OrderByDescending(x => karmaTracker.GameComp.estIncidentChange[x])
                .ThenBy(x => x.LabelCap.ToString()))
            {
                selectable.Add(iDef);
            }

            return selectable;
        }

        public static bool KaiyiKarmicIsSelectable(IncidentDef incident)
        {
            if (incident != IncidentDefOf_Yaoma.Resurrection_Yaoma && incident != IncidentDefOf_Yaoma.KarmaTrade_Yaoma &&
                incident.TargetAllowed(Find.CurrentMap) &&
                incident.Worker.CanFireNow(StorytellerUtility.DefaultParmsNow(incident.category, Find.CurrentMap))) return true;
            return false;
        }

        public static IEnumerable<IncidentDef> KaiyiKarmicWeightedSelection(StorytellerComp_RandomKarmaMain karmaTracker, int count)
        {
            // grab incidents that a) that actually can be fired b) can target the colony map c) have a base incident chance > 0 within its own category
            List<IncidentDef> incidents = karmaTracker.GameComp.selectableIncidentCount.Keys.Where(x => KaiyiKarmicIsSelectable(x) && KaiyiKarmicGetFinalIncidChance(x, karmaTracker) > 0).ToList();

            // creates "total" weight for each category from the filtered list
            Dictionary<IncidentCategoryDef, float> categoryTotalWeight = new Dictionary<IncidentCategoryDef, float>();
            foreach(var i in incidents)
            {
                if (categoryTotalWeight.ContainsKey(i.category)) categoryTotalWeight[i.category] += KaiyiKarmicGetFinalIncidChance(i, karmaTracker);
                else categoryTotalWeight.Add(i.category, KaiyiKarmicGetFinalIncidChance(i, karmaTracker));
            }

            // we now attempt to retrive the incident category level weighting
            List<IncidentCategoryEntry> compWeights = karmaTracker.WeightedIncidentCategories();
            Dictionary<IncidentDef, float> incidentWeights = new Dictionary<IncidentDef, float>();
            foreach (var i in incidents)
            {
                IncidentCategoryEntry entry = compWeights.FirstOrDefault(x => x.category == i.category); // look for incident comp weight
                float weight = entry != null ? entry.weight : 0.25f; // if it isn't in the categories of the main storytellercomp, it's usually a very unlikely incident

                // create a combined weight, equal to category weight * final chance of incidient within said category / the total weighting of all the final incident weights of that category
                incidentWeights.Add(i, weight * KaiyiKarmicGetFinalIncidChance(i, karmaTracker) / categoryTotalWeight[i.category]);
            }

            // grabs count amount of incidents, with replacement
            for (int x = 0; x < count; x++) yield return incidentWeights.RandomElementByWeight(i => i.Value).Key;

            yield break;
        }

        public static float KaiyiKarmicGetFinalIncidChance(IncidentDef def, StorytellerComp_RandomKarmaMain karmaTracker)
        {
            Traverse traverse = Traverse.Create(karmaTracker);
            return traverse.Method("IncidentChanceFinal", def).GetValue<float>();
        }

        public static void DeathlessDajiUtility(Storyteller storyteller)
        {
            // at dayTick = 0 (midnight for the map)
            if (GenLocalDate.DayTick(Find.AnyPlayerHomeMap) == 0)
            {
                // grabs StorytellerComp_OnDemand and does a null check
                if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                    typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return;

                // for each target possible
                foreach (FiringIncident fi in c.MakeIncidents(storyteller.AllIncidentTargets))
                {
                    // fires the incident linked to StorytellerComp_OnDemand (Resurrection_Daji)
                    storyteller.TryFire(fi);
                }
            }
        }

        // used to enable the storyteller image toggle function; when used, it switches between clean and bloody versions
        public static bool DeathlessDajiDefUtility(StorytellerDef storytellerDef)
        {
            if (settings.DajiBloodyPortrait && storytellerDef == StorytellerDefOf.DeathlessDaji_Yaoma && 
                storytellerDef.HasModExtension<StorytellerDajiToggle_ModExtension>())
            {
                StorytellerDajiToggle_ModExtension extension = storytellerDef.GetModExtension<StorytellerDajiToggle_ModExtension>();
                if (extension is null || extension.portraitTinyAlt.NullOrEmpty() || extension.portraitLargeAlt.NullOrEmpty())
                {
                    return true;
                }

                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    storytellerDef.portraitTinyTex = ContentFinder<Texture2D>.Get(extension.portraitTinyAlt, true);
                    storytellerDef.portraitLargeTex = ContentFinder<Texture2D>.Get(extension.portraitLargeAlt, true);
                });
                for (int i = 0; i < storytellerDef.comps.Count; i++)
                {
                    storytellerDef.comps[i].ResolveReferences(storytellerDef);
                }
                return false;
            }

            return true;
        }

        // additional mechanic: killing pawns reduces madness
        public static void DeathlessDajiMurderSanity(Pawn instigator)
        {
            if (Find.Storyteller.def == StorytellerDefOf.DeathlessDaji_Yaoma && settings.DajiMurderSanity && instigator != null)
            {
                HealthUtility.AdjustSeverity(instigator, HediffDefOf_Yaoma.DeathlessDaji_Hediff_Yaoma, settings.DajiMurderSanitySevReduce * -1f);
            }
        }

        // additional mechanic: lifesteal on all melee damage
        public static void DeathlessDajiLifestealMelee(Pawn attacker, DamageWorker.DamageResult result)
        {
            if (Find.Storyteller.def != StorytellerDefOf.DeathlessDaji_Yaoma || !settings.DajiLifestealMelee) return;
            
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            attacker.health.hediffSet.GetHediffs<Hediff_Injury>(ref injuries, (Hediff_Injury x) => x.CanHealNaturally() || x.CanHealFromTending());
            if (injuries.NullOrEmpty()) return;

            if (injuries.TryRandomElement(out Hediff_Injury injury))
            {
                injury.Heal(result.totalDamageDealt * settings.DajiLifestealMeleePercent);
            }
        }

        public static void JianghuJinPostUtility(Storyteller storyteller)
        {
            if (GenLocalDate.DayTick(Find.AnyPlayerHomeMap) == 60000 / 2)
            {
                // get StorytellerComp_OnDemandRegular and nullcheck it
                if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                      typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return;

                GameComp.teraformDays += 1;
                if (GameComp.teraformDays < settings.JianghuJinTerraformDays) return;

                // attempts to fire the incidentDef linked to StorytellerComp_OnDemand
                // if fired, set the days time to the setting value
                foreach (FiringIncident fi in c.MakeIncidents(storyteller.AllIncidentTargets))
                {
                    // later on- set days check to whatever value is in settings
                    if (storyteller.TryFire(fi)) GameComp.teraformDays = 0;
                }
            }
        }

        // random ending parts for the "Stellar augury" letter
        public static List<String> endings = new List<string> { "YS_LetterFarseerFan_Ending1",
                        "YS_LetterFarseerFan_Ending2", "YS_LetterFarseerFan_Ending3",
                        "YS_LetterFarseerFan_Ending4", "YS_LetterFarseerFan_Ending5"};

        // reference to settings in utility
        public static YaomaStorytellerSettings settings = LoadedModManager.GetMod<YaomaStorytellerMod>().GetSettings<YaomaStorytellerSettings>();

        public static StorytellerComp_RandomKarmaMain KarmaTracker
        {
            get
            {
                if(cachedKarmaTracker is null)
                {
                    StorytellerComp comp = Find.Storyteller.storytellerComps.FirstOrDefault(x => x is StorytellerComp_RandomKarmaMain);
                    if(comp != null) cachedKarmaTracker = comp as StorytellerComp_RandomKarmaMain;
                }
                return cachedKarmaTracker;
            }
        }

        public static GameComponent_YaomaStorytellers GameComp
        {
            get
            {
                return Current.Game.GetComponent<GameComponent_YaomaStorytellers>();
            }
        }

        private static StorytellerComp_RandomKarmaMain cachedKarmaTracker;

        public static System.Random rand = new System.Random();
    }
}
