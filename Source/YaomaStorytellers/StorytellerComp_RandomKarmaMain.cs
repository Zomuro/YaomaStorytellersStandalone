using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
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
			if (Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 1000f))
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
				if (!YaomaStorytellerUtility.settings.KaiyiKarmicKarmaPointScaling) return 1f;
				return 1 + (Math.Abs(karma) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicKarmaPointScalingFactor;
			}
        }

		public float EstKarmaPointScaling(float change)
		{
			if (!YaomaStorytellerUtility.settings.KaiyiKarmicKarmaPointScaling) return 1f;
			return 1 + (Math.Abs(karma + change) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicKarmaPointScalingFactor;
		}

		public void CompExposeData()
		{
			Scribe_Values.Look<float>(ref karma, "karma", 0);
			Scribe_Values.Look<float>(ref cachedCostFactor, "cachedCostFactor", 1, false);
			Scribe_Collections.Look(ref baseIncidentChange, "baseIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectableIncidentCount, "selectableIncidentCount", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref estIncidentChange, "estIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectedIncidents, "selectedIncidents", LookMode.Def);
			Scribe_Values.Look<int>(ref daysCheck, "daysCheck", 0, false);
			Scribe_Values.Look<bool>(ref initKarma, "initKarma", false);
		}

		public override void Initialize()
		{
            if (!initKarma)
            {
				initKarma = true;
				karma = YaomaStorytellerUtility.settings.KaiyiKarmicKarma;
            }

			// start building baseincidentcost dict with one defined in xml
			baseIncidentChange = CompProps.baseIncidentCategoryKarmaChange.ToDictionary(x => x.def, x => x.change);

			// if there are new incidentcategorydefs that are not defined in the properties list, add them with base cost 2 (treat like misc)
			foreach (IncidentCategoryDef i in DefDatabase<IncidentCategoryDef>.AllDefs)
			{
				if (!baseIncidentChange.Keys.Contains(i)) baseIncidentChange.Add(i, 2);
			}

			// for all the incidentdefs, if the incident def indeed has the category in baseincidentcost
			// AND the incident diff isn't already in the list, we add it to the count list
			foreach (IncidentDef i in DefDatabase<IncidentDef>.AllDefs)
            {
				if (baseIncidentChange.Keys.Contains(i.category) && 
					!selectableIncidentCount.Keys.Contains(i)) selectableIncidentCount.Add(i, 1);
			}
			
			// we then set the estIncidentCost:
			// if the incidentdef isn't in there, we add it in as base cost
			// otherwise, it isn't touched at all
			foreach (IncidentDef i in selectableIncidentCount.Keys)
            {
				if(!estIncidentChange.ContainsKey(i)) estIncidentChange.Add(i, baseIncidentChange[i.category] * CostFactor);
			}
			cachedCostFactor = CostFactor;
			//karma = YaomaStorytellerUtility.settings.KaiyiKarmicKarma;

			Log.Message("karma: " + karma);
			Log.Message("changefactor: " + CostFactor);
			Log.Message("basechange: " + baseIncidentChange.Count());
			Log.Message("selectableCount: " + selectableIncidentCount.Count());
			Log.Message("estchange: " + estIncidentChange.Count());
		}

		public void RefreshIncidentChange()
        {
			if (cachedCostFactor == CostFactor) return;
			Dictionary<IncidentDef, float> temp = new Dictionary<IncidentDef, float>();
			foreach (IncidentDef key in estIncidentChange.Keys)
				temp[key] = estIncidentChange[key] * CostFactor / cachedCostFactor;
			estIncidentChange = temp;
			cachedCostFactor = CostFactor;
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
					alteredCats.Add(i.category);
				}

				// then adjusts incident karma price based on that
				//adaptiveIncidentPricing();
				AdaptiveIncidentPricing(alteredCats.Distinct());
				alteredCats.Clear();
			}
		}

		public void AdjustIncidentCount(IncidentDef i)
		{
			if (selectableIncidentCount.Keys.Contains(i)) selectableIncidentCount[i] += 1;
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
					estIncidentChange[temp.Key] = (((float)temp.Value) / weights.Values.Sum()) *
						baseIncidentChange[icd] * weights.Count() * CostFactor;
				}

				weights.Clear();
			}
		}

		public void AdaptiveWeighting(ref Dictionary<IncidentDef, float> weights, IncidentCategoryDef icd)
        {
			// depending on if it costs karma or not, alter weighting
			double power = baseIncidentChange[icd] >= 0 ? -1f : 1f;

			foreach (var id in from incident in selectableIncidentCount
							   where incident.Key.category == icd
							   select incident)
			{
				weights.Add(id.Key, (float) Math.Pow(id.Value, power));
			}
		}

		public List<IncidentCategoryEntry> WeightedIncidentCategories()
        {
			List<IncidentCategoryEntry> cats = Props.categoryWeights.ListFullCopy();

			foreach(var x in cats)
            {
				if (karma < 0 && baseIncidentChange[x.category] >= 0) x.weight *= 1 + Math.Abs(karma) / Math.Abs(YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMin);
				else if (karma > 0 && baseIncidentChange[x.category] < 0) x.weight *= 1 + Math.Abs(karma) / Math.Abs(YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMax);
			}

			return cats;
		}

		public override string ToString()
		{
			return base.ToString() + " " + YaomaStorytellerUtility.settings.KaiyiKarmicKarma;
		}

		public float CostFactor
        {
            get
            {
				return YaomaStorytellerUtility.settings.KaiyiKarmicBasePriceFactor;
			}
        }

		public float karma = 0f;

		public float cachedCostFactor = 1f;

		public Dictionary<IncidentCategoryDef, float> baseIncidentChange = new Dictionary<IncidentCategoryDef, float>();

		public Dictionary<IncidentDef, int> selectableIncidentCount = new Dictionary<IncidentDef, int>();

		public Dictionary<IncidentDef, float> estIncidentChange = new Dictionary<IncidentDef, float>();

		public List<IncidentDef> selectedIncidents = new List<IncidentDef>();

		public List<IncidentCategoryDef> alteredCats = new List<IncidentCategoryDef>();

		public int daysCheck = 0;

		private bool initKarma = false;

	}
}
