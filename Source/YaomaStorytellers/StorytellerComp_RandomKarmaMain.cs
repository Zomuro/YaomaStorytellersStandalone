﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YaomaStorytellers
{
    // may use this instead of having an random main and karmatracker- just combo the two

    // we need to adjust incidentweights anyways
    public class StorytellerComp_RandomKarmaMain : StorytellerComp_RandomMain //, IExposable
	{
		public StorytellerCompProperties_RandomKarmaMain CompProps
		{
			get
			{
				return (StorytellerCompProperties_RandomKarmaMain)this.props;
			}
		}

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(Props.mtbDays, 60000f, 1000f))
			{
				bool flag = target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon);
				List<IncidentCategoryDef> list = new List<IncidentCategoryDef>();
				IncidentParms parms;
				IncidentDef incidentDef;
				for (; ; )
				{
					IncidentCategoryDef incidentCategoryDef = this.ChooseRandomCategory(target, list);
					parms = this.GenerateParms(incidentCategoryDef, target);
					if (base.TrySelectRandomIncident(base.UsableIncidentsInCategory(incidentCategoryDef, parms), out incidentDef)) break;
					list.Add(incidentCategoryDef);
					if (list.Count >= this.Props.categoryWeights.Count) yield break;
				}
				if (!this.Props.skipThreatBigIfRaidBeacon || !flag || incidentDef.category != IncidentCategoryDefOf.ThreatBig) yield return new FiringIncident(incidentDef, this, parms);

			}
			yield break;
		}

		public virtual IncidentCategoryDef ChooseRandomCategory(IIncidentTarget target, List<IncidentCategoryDef> skipCategories)
		{
			if (!skipCategories.Contains(IncidentCategoryDefOf.ThreatBig))
			{
				int num = Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick;
				if (target.StoryState.LastThreatBigTick >= 0 && (float)num > 60000f * this.Props.maxThreatBigIntervalDays) return IncidentCategoryDefOf.ThreatBig;
			}
			return (from cw in WeightedIncidentCategories()
					where !skipCategories.Contains(cw.category)
					select cw).RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
		}

		public float KarmaPointScaling
        {
            get
            {
				if (!Settings.KaiyiKarmicKarmaPointScaling) return 1f;
				return 1 + (Math.Abs(GameComp.karma) / 100f) * Settings.KaiyiKarmicKarmaPointScalingFactor;
			}
        }

		public float EstKarmaPointScaling(float change)
		{
			if (!Settings.KaiyiKarmicKarmaPointScaling) return 1f;
			return 1 + (Math.Abs(GameComp.karma + change) / 100f) * Settings.KaiyiKarmicKarmaPointScalingFactor;
		}

		public override void Initialize()
		{
            if (!GameComp.initKarma)
            {
				GameComp.initKarma = true;
				GameComp.karma = Settings.KaiyiKarmicKarma;
            }

			// start building baseincidentcost dict with one defined in xml
			GameComp.baseIncidentChange = CompProps.baseIncidentCategoryKarmaChange.ToDictionary(x => x.def, x => x.change);

			// if there are new incidentcategorydefs that are not defined in the properties list, add them with base cost 2 (treat like misc)
			foreach (IncidentCategoryDef i in DefDatabase<IncidentCategoryDef>.AllDefs)
			{
				if (!GameComp.baseIncidentChange.Keys.Contains(i)) GameComp.baseIncidentChange.Add(i, -2f);
			}

			// for all the incidentdefs, if the incident def indeed has the category in baseincidentcost
			// AND the incident diff isn't already in the list, we add it to the count list
			foreach (IncidentDef i in DefDatabase<IncidentDef>.AllDefs)
            {
				if (GameComp.baseIncidentChange.Keys.Contains(i.category) && 
					!GameComp.selectableIncidentCount.Keys.Contains(i)) GameComp.selectableIncidentCount.Add(i, 1);
			}
			
			// we then set the estIncidentCost:
			// if the incidentdef isn't in there, we add it in as base cost
			// otherwise, it isn't touched at all
			foreach (IncidentDef i in GameComp.selectableIncidentCount.Keys)
            {
				if(!GameComp.estIncidentChange.ContainsKey(i)) GameComp.estIncidentChange.Add(i, GameComp.baseIncidentChange[i.category] * CostFactor);
			}
			GameComp.cachedCostFactor = CostFactor;
		}

		public void RefreshIncidentChange()
        {
			if (GameComp.cachedCostFactor == CostFactor) return;
			Dictionary<IncidentDef, float> temp = new Dictionary<IncidentDef, float>();
			foreach (IncidentDef key in GameComp.estIncidentChange.Keys)
				temp[key] = GameComp.estIncidentChange[key] * CostFactor / GameComp.cachedCostFactor;
			GameComp.estIncidentChange = temp;
			GameComp.cachedCostFactor = CostFactor;
		}

		// deals with the incidents selected by the pawn
		public void CompleteIncidentSelection(List<IncidentDef> incidents)
		{
			// no null or empty list
			if (incidents != null && incidents.Any())
			{
				// for each incident increment historic count of selections
				foreach (IncidentDef i in incidents)
				{
					this.AdjustIncidentCount(i);
					GameComp.alteredCats.Add(i.category);
				}

				// then adjusts incident karma price based on that
				AdaptiveIncidentPricing(GameComp.alteredCats.Distinct());
				GameComp.alteredCats.Clear();
			}
		}

		public void AdjustIncidentCount(IncidentDef i)
		{
			if (GameComp.selectableIncidentCount.Keys.Contains(i)) GameComp.selectableIncidentCount[i] += 1;
		}

		public void AdaptiveIncidentPricing(IEnumerable<IncidentCategoryDef> categories)
		{
			Dictionary<IncidentDef, float> weights = new Dictionary<IncidentDef, float>();
			foreach (IncidentCategoryDef icd in categories)
			{
				AdaptiveWeighting(ref weights, icd);

				if (weights.Count() <= 1) continue; // no point weighting a list of only one item
				foreach (var temp in weights)
				{
					//estimated incident cost becomes (weight in category)/(total sum of weights) 
					//	* (number of incidents in category) * (base incident cost of that category)
					GameComp.estIncidentChange[temp.Key] = (((float)temp.Value) / weights.Values.Sum()) *
						GameComp.baseIncidentChange[icd] * weights.Count() * CostFactor;
				}

				weights.Clear();
			}
		}

		public void AdaptiveWeighting(ref Dictionary<IncidentDef, float> weights, IncidentCategoryDef icd)
        {
			// depending on if it costs karma or not, alter weighting
			double power = GameComp.baseIncidentChange[icd] >= 0 ? -1f : 1f;

			foreach (var id in from incident in GameComp.selectableIncidentCount
							   where incident.Key.category == icd
							   select incident)
			{
				weights.Add(id.Key, (float) Math.Pow(id.Value, power));
			}
		}

		public List<IncidentCategoryEntry> WeightedIncidentCategories()
        {
			List<IncidentCategoryEntry> result = new List<IncidentCategoryEntry>();
			float adjWeight;

			foreach (var x in Props.categoryWeights)
            {
				adjWeight = x.weight;
				if (GameComp.karma < 0 && GameComp.baseIncidentChange[x.category] >= 0) adjWeight *= 1 + 3f* Math.Abs(GameComp.karma) / Math.Abs(Settings.KaiyiKarmicKarmaMin);
				else if (GameComp.karma > 0 && GameComp.baseIncidentChange[x.category] < 0) adjWeight *= 1 + 3f* Math.Abs(GameComp.karma) / Math.Abs(Settings.KaiyiKarmicKarmaMax);

				result.Add(new IncidentCategoryEntry() { category = x.category, weight = adjWeight });
			}

			return result;
		}

		public override string ToString()
		{
			return base.ToString() + " " + Settings.KaiyiKarmicKarma;
		}

		public float CostFactor => Settings.KaiyiKarmicBasePriceFactor;

		public YaomaStorytellerSettings Settings => YaomaStorytellerUtility.settings;

		public GameComponent_YaomaStorytellers GameComp => YaomaStorytellerUtility.GameComp;

	}
}
