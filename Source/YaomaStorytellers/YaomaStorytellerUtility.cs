using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public static class YaomaStorytellerUtility
    {
        public static bool FarseerFanUtility(Storyteller storyteller)
        {
            // check if there's nothing in the incident queue every 1000 ticks
            if (Find.TickManager.TicksGame % 1000 == 0 && storyteller.incidentQueue.Count == 0)
            {
                // if the gameticks <= minDaysPassed in StorytellerCompProperties_RandomMain, don't predict events at all
                // simulate grace period
                StorytellerCompProperties_RandomMain r = (StorytellerCompProperties_RandomMain)storyteller.def.comps.
                    FirstOrDefault(x => x.GetType() == typeof(StorytellerCompProperties_RandomMain));
                if (Find.TickManager.TicksGame <= r.minDaysPassed * 60000 * settings.FarseerFanGracePeriodFactor) return false;

                // simulate incidents till we get one
                int counter = 0; List<FiringIncident> fi_sim = null;
                FarseerFanSimulate(storyteller, ref fi_sim, ref counter);

                // if the counted cycles > (the number of cycles in mtbDays * max period factor), retry next cycle
                //if (counter > r.mtbDays * 60 * settings.FarseerFanMaxPeriodFactor) return false;

                // properly set the incident time
                FarseerFanQueue(storyteller, fi_sim, counter);
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
                    "LetterFarseerFan".Translate(FarseerFanDate(ticksAbs, fi.parms.target.Tile), fi.def.label) + 
                        endings.RandomElement().Translate(),
                    LetterDefOf.NeutralEvent, null, null));
            }
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
                if (!(Find.Storyteller.storytellerComps.FirstOrDefault(x =>
                    x is StorytellerComp_KarmaTracker) is StorytellerComp_KarmaTracker karmaTracker)) return true;

                // if the Karma Tracker has no "selected incidents", roll a random incident
                if (!karmaTracker.selectedIncidents.Any())
                {
                    foreach (FiringIncident fi in storyteller.MakeIncidentsForInterval())
                        YaomaStorytellerUtility.KaiyiKarmicRandomIncident(storyteller, karmaTracker, fi);
                }
                else // when the Karma Tracker has "selected incidents" left
                {
                    // gets StorytellerComp_OnDemand for replacing the incident to occur, if null skip everything
                    if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                   typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return true;
                    // for each incident rolled
                    foreach (FiringIncident fi in storyteller.MakeIncidentsForInterval())
                        YaomaStorytellerUtility.KaiyiKarmicReplaceIncident(storyteller, karmaTracker, c, fi);
                }
            }
            return false;
        }

        public static void KaiyiKarmicRandomIncident(Storyteller storyteller, StorytellerComp_KarmaTracker kt, FiringIncident fi)
        {
            fi.parms.points *= kt.karmaPointScaling;

            // if the incident has a category recognized by the KarmaTracker
            // and has a negative karma cost (i.e. negative events) 
            if (storyteller.TryFire(fi) && kt.baseIncidentCost.Keys.Contains(fi.def.category) &&
                kt.baseIncidentCost[fi.def.category] > 0)
            {
                //gain 25% karma from negative events if they randomly occur: 
                //you only get full value from negative events you choose
                kt.karma += kt.estIncidentCost[fi.def] / 4f;

                // notifies player of their gain
                Find.LetterStack.ReceiveLetter("LetterLabelKaiyiKarmicGain".Translate(),
                    "LetterKaiyiKarmicGain".Translate(Math.Abs(Math.Round(kt.estIncidentCost[fi.def] / 4f, 2))),
                    LetterDefOf.NeutralEvent);
            }
        }

        public static void KaiyiKarmicReplaceIncident(Storyteller storyteller, StorytellerComp_KarmaTracker karmaTracker, 
            StorytellerComp_OnDemand c, FiringIncident fi)
        {
            // if the KarmaTracker's selected incidents are empty or null
            if (karmaTracker.selectedIncidents.NullOrEmpty())
            {
                // fire the original incident
                fi.parms.points *= karmaTracker.karmaPointScaling;
                storyteller.TryFire(fi);
                return;
            }

            // otherwise, select a random incidentDef from KarmaTracker's selected incidentDefs
            // and create a firing incident using StorytellerComp_OnDemand
            IncidentDef randomDef = karmaTracker.selectedIncidents.RandomElement();
            FiringIncident fiReplace = c.MakeIncident(storyteller.AllIncidentTargets, randomDef);

            // null check- if the incident can't be fired, or otherwise turns null, skips to next
            if (fiReplace == null) return;
            fiReplace.parms.points *= karmaTracker.karmaPointScaling;
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
                if (!(Find.Storyteller.storytellerComps.FirstOrDefault(x =>
                    x is StorytellerComp_KarmaTracker) is StorytellerComp_KarmaTracker karmaTracker) || 
                    karmaTracker.selectedIncidents.Any()) return;

                // decrease dayCheck (number of days till Kaiyi's incident fires)
                // if dayCheck is still > 0 afterwards, wait for tomorrow
                karmaTracker.daysCheck -= 1;
                if (karmaTracker.daysCheck > 0) return;

                // get StorytellerComp_OnDemand and nullcheck it
                if (!(storyteller.storytellerComps.FirstOrDefault(x => x.GetType() ==
                                                    typeof(StorytellerComp_OnDemand)) is StorytellerComp_OnDemand c)) return;

                // attempts to fire the incidentDef linked to StorytellerComp_OnDemand
                // if fired, set the days time to the KarmaTracker's days values
                foreach (FiringIncident fi in c.MakeIncidents(storyteller.AllIncidentTargets))
                {
                    if (storyteller.TryFire(fi)) karmaTracker.daysCheck = karmaTracker.Props.days;
                }
            }
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

        // random ending parts for the "Stellar augury" letter
        public static List<String> endings = new List<string> { "LetterFarseerFan_Ending1",
                        "LetterFarseerFan_Ending2", "LetterFarseerFan_Ending3",
                        "LetterFarseerFan_Ending4", "LetterFarseerFan_Ending5"};

        // reference to settings in utility
        public static YaomaStorytellerSettings settings = LoadedModManager.GetMod<YaomaStorytellerMod>().GetSettings<YaomaStorytellerSettings>();
    }
}
