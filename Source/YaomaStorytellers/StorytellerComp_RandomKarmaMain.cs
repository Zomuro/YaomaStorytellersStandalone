using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
	// may use this instead of having an random main and karmatracker- just combo the two

	// we need to adjust incidentweights anyways
	public class StorytellerComp_RandomKarmaMain : StorytellerComp_RandomMain
	{
		public float KarmaPointScaling
        {
            get
            {
				/*if (karma >= 0) return 1 + (Math.Abs(karma) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicScalingPositive;
				else return 1 + (Math.Abs(karma) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicScalingNegative;*/

				return 1 + (Math.Abs(karma) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicKarmaPointScalingFactor;
			}
        }

		public float EstKarmaPointScaling(float change)
		{
			/*if (karma + change >= 0) return 1 + (Math.Abs(karma + change) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicScalingPositive;
			else return 1 + (Math.Abs(karma + change) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicScalingNegative;*/

			return 1 + (Math.Abs(karma + change) / 100f) * YaomaStorytellerUtility.settings.KaiyiKarmicKarmaPointScalingFactor;
		}

		public virtual void CompExposeData()
		{
			// these two- consider removing them if unused
			Scribe_Values.Look<float>(ref this.karma, "karma", 0, false);
			Scribe_Values.Look<float>(ref this.cachedCostFactor, "cachedCostFactor", 1, false);

			Scribe_Collections.Look(ref baseIncidentCost, "baseIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectableIncidentCount, "selectableIncidentCount", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref estIncidentCost, "estIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectedIncidents, "selectedIncidents", LookMode.Def);
			Scribe_Values.Look<int>(ref this.daysCheck, "daysCheck", 0, false);
		}

		public override void Initialize()
		{
			foreach (IncidentDef i in DefDatabase<IncidentDef>.AllDefs)
				if (baseIncidentCost.Keys.Contains(i.category)) selectableIncidentCount.Add(i, 1);
			foreach (IncidentDef i in selectableIncidentCount.Keys)
				estIncidentCost.Add(i, baseIncidentCost[i.category] * CostFactor);
			cachedCostFactor = CostFactor;
			karma = YaomaStorytellerUtility.settings.KaiyiKarmicKarma;
		}

		public void RefreshIncidentCosts()
        {
			if (cachedCostFactor == CostFactor) return;
			Dictionary<IncidentDef, float> temp = new Dictionary<IncidentDef, float>();
			foreach (IncidentDef key in estIncidentCost.Keys)
				temp[key] = estIncidentCost[key] * CostFactor / cachedCostFactor;
			estIncidentCost = temp;
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
					estIncidentCost[temp.Key] = (((float)temp.Value) / weights.Values.Sum()) *
						baseIncidentCost[icd] * weights.Count() * CostFactor;
				}

				weights.Clear();
			}
		}

		public void AdaptiveWeighting(ref Dictionary<IncidentDef, float> weights, IncidentCategoryDef icd)
        {
			// depending on if it costs karma or not, alter weighting
			double power = baseIncidentCost[icd] >= 0 ? -1f : 1f;

			foreach (var id in from incident in selectableIncidentCount
							   where incident.Key.category == icd
							   select incident)
			{
				weights.Add(id.Key, (float) Math.Pow(id.Value, power));
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + YaomaStorytellerUtility.settings.KaiyiKarmicKarma;
		}

		public float Karma
        {
            get
            {
				return YaomaStorytellerUtility.settings.KaiyiKarmicKarma;
			}
        }

		public float CostFactor
        {
            get
            {
				return YaomaStorytellerUtility.settings.KaiyiKarmicBasePriceFactor;
			}
        }

		// with these two- replace usage of these with setting karma and cost factor
		public float karma = 0;

		public float cachedCostFactor = 1f;

		public Dictionary<IncidentCategoryDef, float> baseIncidentCost = new Dictionary<IncidentCategoryDef, float>
		{
			{IncidentCategoryDefOf.ThreatBig, 2f},
			{IncidentCategoryDefOf.ThreatSmall, 1.5f},
			{IncidentCategoryDefOf.DiseaseAnimal, 1f},
			{IncidentCategoryDefOf.DiseaseHuman, 1f},
			{IncidentCategoryDefOf.Misc, -2f},
			{IncidentCategoryDefOf.ShipChunkDrop, -3f},
			{IncidentCategoryDefOf.FactionArrival, -4f},
			{IncidentCategoryDefOf.OrbitalVisitor, -4f},
			{IncidentCategoryDefOf.AllyAssistance, -6f }
		};
		public Dictionary<IncidentDef, int> selectableIncidentCount = new Dictionary<IncidentDef, int>();
		public Dictionary<IncidentDef, float> estIncidentCost = new Dictionary<IncidentDef, float>();
		public List<IncidentDef> selectedIncidents = new List<IncidentDef>();

		public List<IncidentCategoryDef> alteredCats = new List<IncidentCategoryDef>();

		public int daysCheck = 0;

	}
}
