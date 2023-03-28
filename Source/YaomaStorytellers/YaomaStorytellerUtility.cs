using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace YaomaStorytellers
{
    public static class YaomaStorytellerUtility
    {
        public static bool FarseerFanUtility(Storyteller storyteller)
        {
            // check if there's nothing in the incident queue every 1000 ticks
            if (Find.TickManager.TicksGame % 1000 == 0 && storyteller.incidentQueue.Count == 0)
            {
                // if the gameticks <= minDaysPassed, don't predict events at all; simulate grace period
                StorytellerCompProperties_RandomMain r = (StorytellerCompProperties_RandomMain)storyteller.def.comps.
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
                        FarseerFanAltSim(storyteller, ref alt_fi_sim);
                        FarseerFanOfferAlt(storyteller, fi_sim, alt_fi_sim, counter);
                        break;
                    case 3:
                        StorytellerComp_OnDemand c = (StorytellerComp_OnDemand)storyteller.storytellerComps.
                            FirstOrDefault(x => x.GetType() == typeof(StorytellerComp_OnDemand));
                        FarseerFanOfferDefer(storyteller, fi_sim, counter, c);
                        //FarseerFanQueue(storyteller, fi_sim, counter);
                        break;
                }

                // properly set the incident time
                //FarseerFanQueue(storyteller, fi_sim, counter);
            }
            // ticks the incident queue- will automatically fire an incident at the right tick
            storyteller.incidentQueue.IncidentQueueTick();
            return false;
        }

        private static void FarseerFanSimulate(Storyteller storyteller, ref List<FiringIncident> fi_sim, ref int counter)
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

        private static int FarseerFanRandomCase()
        {
            List<int> behavior = new List<int>() {1};
            if (settings.FarseerFanPredictAlt) behavior.Add(2); // only if alterate incidents are enabled
            if (settings.FarseerFanPredictDefer) behavior.Add(3); // only if deferred incidents are enabled

            return behavior[rand.Next(behavior.Count)]; // return random case
        }

        private static void FarseerFanAltSim(Storyteller storyteller, ref List<FiringIncident> alt_fi_sim)
        {
            while (alt_fi_sim.EnumerableNullOrEmpty())
            {
                alt_fi_sim = storyteller.MakeIncidentsForInterval().ToList();
                if (alt_fi_sim.Any()) break;
            }
        }

        private static TaggedString FarseerFanAltText(List<FiringIncident> fi_sim, List<FiringIncident> alt_fi_sim)
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

        private static void FarseerFanOfferAlt(Storyteller storyteller, List<FiringIncident> fi_sim,
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

        private static TaggedString FarseerFanDeferText(List<FiringIncident> fi_sim)
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

        private static FiringIncident FarseerFanThreat(Storyteller storyteller, StorytellerComp_OnDemand c, ref int temp_count)
        {
            List<FiringIncident> fi_sim_temp = null;
            FarseerFanSimulate(storyteller, ref fi_sim_temp, ref temp_count);

            IncidentDef randomDef = DefDatabase<IncidentDef>.AllDefs.Where(x => x.category == IncidentCategoryDefOf.ThreatBig ||
                x.category == IncidentCategoryDefOf.ThreatSmall).RandomElement();
            return c.MakeIncident(storyteller.AllIncidentTargets, randomDef);
        }

        private static void FarseerFanOfferDefer(Storyteller storyteller, List<FiringIncident> fi_sim, int counter, StorytellerComp_OnDemand c)
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

        private static void FarseerFanQueue(Storyteller storyteller, List<FiringIncident> fi_sim, int counter)
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
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("LetterLabelFarseerFan".Translate(fi.def.label),
                    "LetterFarseerFan".Translate(FarseerFanDate(ticksAbs, FarseerFanTileCheck(fi)), fi.def.label) + 
                        endings.RandomElement().Translate(),
                    LetterDefOf.NeutralEvent, null, null));
            }
        }

        private static int FarseerFanTileCheck(FiringIncident fi)
        {
            return fi.parms.target.Tile < 0 ? Find.CurrentMap.Tile : fi.parms.target.Tile;
        }

        private static string FarseerFanDate(long ticksAbs, int tileID)
        {
            if (settings.FarseerFanPredictionDetail)
                return GenDate.DateFullStringWithHourAt(ticksAbs, Find.WorldGrid.LongLatOf(tileID));

            else return GenDate.DateFullStringAt(ticksAbs, Find.WorldGrid.LongLatOf(tileID));
        }


        public static bool KaiyiKarmicPreUtility(Storyteller storyteller)
        {
            storyteller.incidentQueue.IncidentQueueTick();

            if (Find.TickManager.TicksGame % 1000 == 0)
            {
                // if the storyteller doesn't have a StorytellerComp_KarmaTracker or StorytellerComp_OnDemand, skip
                if (KarmaTracker is null) return true;

                // if the Karma Tracker has no "selected incidents", roll a random incident
                if (!KarmaTracker.selectedIncidents.Any())
                {
                    foreach (FiringIncident fi in storyteller.MakeIncidentsForInterval())
                        YaomaStorytellerUtility.KaiyiKarmicRandomIncident(storyteller, KarmaTracker, fi);
                }
                else // when the Karma Tracker has "selected incidents" left
                {
                    // gets StorytellerComp_OnDemand for replacing the incident to occur, if null skip everything
                    if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                   typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return true;
                    // for each incident rolled
                    foreach (FiringIncident fi in storyteller.MakeIncidentsForInterval())
                        YaomaStorytellerUtility.KaiyiKarmicReplaceIncident(storyteller, KarmaTracker, c, fi);
                }
            }
            return false;
        }

        // Adjust setting karma value with purchases and the such.
        public static void KaiyiKarmicAdjustKarma(StorytellerComp_RandomKarmaMain kt, float adjust)
        {
            kt.karma = Math.Max(settings.KaiyiKarmicKarmaMin, Math.Min(kt.karma + adjust, settings.KaiyiKarmicKarmaMax));
        }

        // Adjust price multiplier on incidents using this.
        public static void KaiyiKarmicIncreasePriceFactor(float adjust)
        {
            settings.KaiyiKarmicBasePriceFactor = Math.Max(0f, Math.Min(settings.KaiyiKarmicBasePriceFactor + adjust, 5f));
        }

        public static void KaiyiKarmicRandomIncident(Storyteller storyteller, StorytellerComp_RandomKarmaMain kt, FiringIncident fi)
        {
            fi.parms.points *= kt.KarmaPointScaling;

            // if the incident has a category recognized by the KarmaTracker
            // and has a negative karma cost (i.e. negative events) 
            if (storyteller.TryFire(fi) && kt.baseIncidentChange.Keys.Contains(fi.def.category) &&
                kt.baseIncidentChange[fi.def.category] > 0)
            {
                //gain 25% karma from negative events if they randomly occur: 
                //you only get full value from negative events you choose
                YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(kt, kt.estIncidentChange[fi.def] / 4f);
                //kt.karma += kt.estIncidentCost[fi.def] / 4f;

                // notifies player of their gain
                Find.LetterStack.ReceiveLetter("LetterLabelKaiyiKarmicGain".Translate(),
                    "LetterKaiyiKarmicGain".Translate(Math.Abs(Math.Round(kt.estIncidentChange[fi.def] / 4f, 2))),
                    LetterDefOf.NeutralEvent);
            }
        }

        public static void KaiyiKarmicReplaceIncident(Storyteller storyteller, StorytellerComp_RandomKarmaMain karmaTracker, 
            StorytellerComp_OnDemand c, FiringIncident fi)
        {
            // if the KarmaTracker's selected incidents are empty or null
            if (karmaTracker.selectedIncidents.NullOrEmpty())
            {
                // fire the original incident
                fi.parms.points *= karmaTracker.KarmaPointScaling;
                storyteller.TryFire(fi);
                return;
            }

            // otherwise, select a random incidentDef from KarmaTracker's selected incidentDefs
            // and create a firing incident using StorytellerComp_OnDemand
            IncidentDef randomDef = karmaTracker.selectedIncidents.RandomElement();
            FiringIncident fiReplace = c.MakeIncident(storyteller.AllIncidentTargets, randomDef);

            // null check- if the incident can't be fired, or otherwise turns null, skips to next
            if (fiReplace == null) return;
            fiReplace.parms.points *= karmaTracker.KarmaPointScaling;
            // if replacement incident gets fired, remove the random incidentDef from selected incidents
            if (storyteller.TryFire(fiReplace)) karmaTracker.selectedIncidents.Remove(randomDef);
        }

        public static void KaiyiKarmicPostUtility(Storyteller storyteller)
        {
            // at the start of each day (dayTick = 0)
            if (GenLocalDate.DayTick(Find.AnyPlayerHomeMap) == 0)
            {
                // get KarmaTracker
                // if it is null or has any "selected incidents", skip
                if (KarmaTracker is null ||
                    KarmaTracker.selectedIncidents.Any()) return;

                // decrease dayCheck (number of days till Kaiyi's incident fires)
                // if dayCheck is still > 0 afterwards, wait for tomorrow
                KarmaTracker.daysCheck -= 1;
                if (KarmaTracker.daysCheck > 0) return;

                // get StorytellerComp_OnDemand and nullcheck it
                if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                    typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return;

                // attempts to fire the incidentDef linked to StorytellerComp_OnDemand
                // if fired, set the days time to the KarmaTracker's days values
                foreach (FiringIncident fi in c.MakeIncidents(storyteller.AllIncidentTargets))
                {
                    // later on- set days check to whatever value is in settings
                    if (storyteller.TryFire(fi)) KarmaTracker.daysCheck = settings.KaiyiKarmicTradeDays;
                }
            }
        }

        public static void KaiyiKarmicSelectableIncidents(ref List<DebugMenuOption> selectable, StorytellerComp_RandomKarmaMain karmaTracker)
        {
            String labelCost = "";
            //karmaTracker.selectableIncidentCount.Keys.Where(x => KaiyiKarmicIsSelectable(x)

            foreach (IncidentDef iDef in KaiyiKarmicWeightedSelection(karmaTracker, settings.KaiyiKarmicMaxChoices)
                .OrderByDescending(x => karmaTracker.estIncidentChange[x])
                .ThenBy(x => x.LabelCap.ToString()))
            {
                labelCost = iDef.LabelCap.ToString() + " (" + Math.Round(karmaTracker.estIncidentChange[iDef], 2) + ")";

                selectable.Add(new DebugMenuOption(labelCost, DebugMenuOptionMode.Action, delegate ()
                {
                    IncidentParms parmSim = StorytellerUtility.DefaultParmsNow(iDef.category, Find.AnyPlayerHomeMap);
                    if (iDef.pointsScaleable)
                    {
                        parmSim = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle ||
                            x is StorytellerComp_RandomMain).GenerateParms(iDef.category, parmSim.target);
                    }

                    Dialog_KarmaTrade karmaDialog = (Find.WindowStack.currentlyDrawnWindow as Dialog_KarmaTrade);
                    List<IncidentDef> incidentsSelected = karmaDialog.selected;

                    if (incidentsSelected.Count < 5)
                    {
                        float estFinalVal = karmaTracker.karma + karmaDialog.EstIncidentChange(karmaTracker) + karmaTracker.estIncidentChange[iDef];
                        if (estFinalVal <= settings.KaiyiKarmicKarmaMin)
                        {
                            Messages.Message("YS_MessageKaiyiKarmicIncidentDebtFloor".Translate(), MessageTypeDefOf.SilentInput, false);
                        }
                        else if(estFinalVal >= settings.KaiyiKarmicKarmaMax)
                        {
                            incidentsSelected.Add(iDef);
                            Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
                            Messages.Message("YS_MessageKaiyiKarmicIncidentReachedMax".Translate(), MessageTypeDefOf.SilentInput, false);
                        }
                        else
                        {
                            incidentsSelected.Add(iDef);
                            Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
                        }
                        
                    }
                    else Messages.Message("YS_MessageKaiyiKarmicIncidentsFilled".Translate(), MessageTypeDefOf.RejectInput, false);
                }));
            }
        }

        // check if adding a value will cause karma to increase/decrease beyond limit - unused for the moment
        public static bool KaiyiKarmicKarmaOOB(float value)
        {
            if (value > settings.KaiyiKarmicKarmaMax || value < settings.KaiyiKarmicKarmaMin) return true;

            return false;
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
            List<IncidentDef> incidents = karmaTracker.selectableIncidentCount.Keys.Where(x => KaiyiKarmicIsSelectable(x)).ToList();

            Dictionary<IncidentCategoryDef, int> categoryCount = new Dictionary<IncidentCategoryDef, int>();
            foreach(var i in incidents)
            {
                if (categoryCount.ContainsKey(i.category)) categoryCount[i.category]++;
                else categoryCount.Add(i.category, 1);
            }
            List<IncidentCategoryEntry> compWeights = karmaTracker.WeightedIncidentCategories();

            // if we want to select a number of incidents > incidents that are even selectable, just send the whole list
            if (count > incidents.Count) 
            {
                foreach (var i in incidents) yield return i;
                yield break;
            } 

            // else we want to go and make a dictionary of weights
            Dictionary<IncidentDef, float> incidentWeights = new Dictionary<IncidentDef, float>();
            foreach (var i in incidents)
            {
                IncidentCategoryEntry entry = compWeights.FirstOrDefault(x => x.category == i.category);
                float weight = entry != null ? entry.weight : 1f;
                incidentWeights.Add(i, weight / categoryCount[i.category]);
            }

            // get list of incident choices *count* long using weighted rng
            float culm = 0;
            for (int x = 0; x < count; x++)
            {
                float total = incidentWeights.Values.Sum();
                float random = UnityEngine.Random.value;
                for(int y = 0; y < incidentWeights.Count(); y++)
                {
                    culm += incidentWeights.ElementAt(y).Value;
                    if(random * total < culm)
                    {
                        IncidentDef iDef = incidentWeights.ElementAt(y).Key;
                        yield return iDef;
                        incidentWeights.Remove(iDef);
                        break;
                    }
                }
            }

            yield break;
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
            if (instigator != null && settings.DajiMurderSanity && Find.Storyteller.def == StorytellerDefOf.DeathlessDaji_Yaoma)
            {
                HealthUtility.AdjustSeverity(instigator, HediffDefOf_Yaoma.DeathlessDaji_Hediff_Yaoma, settings.DajiMurderSanitySevReduce * -1f);
            }
        }

        // additional mechanic: lifesteal on all melee damage
        public static void DeathlessDajiLifestealMelee(Pawn attacker, DamageWorker.DamageResult result)
        {
            if (!settings.DajiLifestealMelee) return;
            
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            attacker.health.hediffSet.GetHediffs<Hediff_Injury>(ref injuries, (Hediff_Injury x) => x.CanHealNaturally() || x.CanHealFromTending());
            if (injuries.NullOrEmpty()) return;

            if (injuries.TryRandomElement(out Hediff_Injury injury))
            {
                injury.Heal(result.totalDamageDealt * settings.DajiLifestealMeleePercent);
            }
        }

        // random ending parts for the "Stellar augury" letter
        public static List<String> endings = new List<string> { "LetterFarseerFan_Ending1",
                        "LetterFarseerFan_Ending2", "LetterFarseerFan_Ending3",
                        "LetterFarseerFan_Ending4", "LetterFarseerFan_Ending5"};

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

        private static StorytellerComp_RandomKarmaMain cachedKarmaTracker;
        
        public static System.Random rand = new System.Random();
    }
}
