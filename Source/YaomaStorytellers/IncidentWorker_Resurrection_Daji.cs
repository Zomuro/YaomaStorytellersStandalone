﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace YaomaStorytellers
{
    public class IncidentWorker_Resurrection_Daji : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (Find.Storyteller.def != StorytellerDefOf.DeathlessDaji_Yaoma) return false;
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			// grabs all the corpses on the target map, and for every corpse
			Map map = (Map)parms.target;
			List<Pawn> pawns = new List<Pawn>();
			foreach (Corpse c in (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse) select (Corpse)x).ToList())
			{
				// mechanoid check - if settings don't allow it, don't resurrect mechanoids
				pawn = c.InnerPawn;
				if (pawn.RaceProps.IsMechanoid && YaomaStorytellerUtility.settings.DajiRessurectMechsDisable) continue;

				// adjust severity of crimson psychosis
				if (this.def.pawnHediff != null) 
					HealthUtility.AdjustSeverity(pawn, this.def.pawnHediff, YaomaStorytellerUtility.settings.DajiCrimsonSeverityGain);

				// resurrect (w/o side effects for now)
				ResurrectionUtility.Resurrect(c.InnerPawn);

				pawns.Add(pawn);
				// nab nearest weapon that the pawn can find in the 8 cells around it
				if (!pawn.RaceProps.IsMechanoid && !YaomaStorytellerUtility.settings.DajiRetrieveWeaponsDisable)
                {
					Thing found = FindNearestWeapon(pawn);

					if (found != null) pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Equip, found));
				}
			}

			base.SendStandardLetter("YS_LetterLabelDeathlessDaji".Translate(), "YS_LetterDeathlessDaji".Translate(),
				LetterDefOf.NegativeEvent, parms, pawns, Array.Empty<NamedArgument>());
			return true;
		}

		public Thing FindNearestWeapon(Thing t)
		{
			// look for weapon in cardinal cells
			foreach (IntVec3 i in GenAdjFast.AdjacentCells8Way(t))
			{
				// look for weapon
				weapon = i.GetThingList(t.Map)?.FirstOrDefault(x => x.def.IsWeapon);
				// if the weapon is there AND isn't reserved by another pawn with a job
				if (weapon != null && ReservationUtility.CanReserve(t as Pawn, weapon)) return weapon;
			}
			return null;
		}

		public Pawn pawn;

		public Thing weapon;
	}
}
