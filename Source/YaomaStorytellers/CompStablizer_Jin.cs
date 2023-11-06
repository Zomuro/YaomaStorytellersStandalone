using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    public class CompStablizer_Jin : ThingComp
    {
		public CompProperties_Stablizer_Jin Props
		{
			get
			{
				return (CompProperties_Stablizer_Jin) props;
			}
		}

		public float FinalMaxRange => Mathf.Max(Props.maxRange * YaomaStorytellerUtility.settings.JianghuJinRangeFactor, FinalMinRange);

		public float FinalMinRange => Props.minRange * YaomaStorytellerUtility.settings.JianghuJinRangeFactor;

		public float StablizeRadius
        {
            get
            {
				return FuelComp is null ? FinalMaxRange : (FinalMaxRange - FinalMinRange) * FuelComp.FuelPercentOfMax + FinalMinRange;
			}
        }

		public override string CompInspectStringExtra()
		{
			return $"{"YS_StabilizerRangeCurr".Translate()}: {StablizeRadius}";
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (parent.def.building != null)
			{
				if(Props.minRange > 0)
                {
					yield return new StatDrawEntry(StatCategoryDefOf.Building, "YS_StabilizerRangeMin".Translate(),
						FinalMinRange.ToString("F2"), "YS_StabilizerRangeMinDesc".Translate(), 99991, null, null, false);
				}

                if (Props.scaleWithFuel && FuelComp != null)
                {
					yield return new StatDrawEntry(StatCategoryDefOf.Building, "YS_StabilizerRangeCurr".Translate(),
						StablizeRadius.ToString("F2"), "YS_StabilizerRangeCurrDesc".Translate(), 99992, null, null, false);
				}

				yield return new StatDrawEntry(StatCategoryDefOf.Building, "YS_StabilizerRangeMax".Translate(),
						FinalMaxRange.ToString("F2"), "YS_StabilizerRangeMaxDesc".Translate(), 99993, null, null, false);

			}
			yield break;
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			if (cachedFactor != YaomaStorytellerUtility.settings.JianghuJinRangeFactor) ClearStablizeCellsCache();

			// shows maximum range of stablization field
			GenDraw.DrawFieldEdges(StablizeMaxCells, Color.white);

			// shows current range of stablization field
			if(Props.scaleWithFuel)
            {
				GenDraw.DrawFieldEdges(StablizeCurrCells, Color.green);
			}
		}

		public CompRefuelable FuelComp
        {
            get
            {
				if(cachedComp is null)
                {
					ThingComp foundComp = parent.AllComps.FirstOrDefault(x => x.GetType() == typeof(CompRefuelable));
					cachedComp = foundComp != null? foundComp as CompRefuelable : null;
				}
				return cachedComp;
			}
        }

		public List<IntVec3> StablizeMaxCells
        {
            get
            {
                if (cachedMaxStablize.NullOrEmpty())
                {
					cachedMaxStablize = (from x in GenRadial.RadialCellsAround(parent.Position, FinalMaxRange, true)
										 where x.InBounds(Find.CurrentMap)
										 select x).ToList();
				}
				return cachedMaxStablize;
			}
        }

		public List<IntVec3> StablizeCurrCells
		{
			get
			{
				if (FuelComp is null || !Props.scaleWithFuel) return StablizeMaxCells;
				if (cachedCurrStablize.NullOrEmpty() || FuelComp.FuelPercentOfMax != cachedPercent)
				{
					cachedCurrStablize = (from x in GenRadial.RadialCellsAround(parent.Position, StablizeRadius, true)
										 where x.InBounds(Find.CurrentMap)
										 select x).ToList();
					cachedPercent = FuelComp.FuelPercentOfMax;
				}
				return cachedCurrStablize;
			}
		}

		public void ClearStablizeCellsCache()
        {
			cachedPercent = 0f;
			cachedFactor = YaomaStorytellerUtility.settings.JianghuJinRangeFactor;
			cachedMaxStablize = new List<IntVec3>();
			cachedCurrStablize = new List<IntVec3>();
		}

		private float cachedPercent = 0f;

		private float cachedFactor = -1f;

		private CompRefuelable cachedComp;

		private List<IntVec3> cachedMaxStablize = new List<IntVec3>();

		private List<IntVec3> cachedCurrStablize = new List<IntVec3>();
	}
}
