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

		public float StablizeRadius
        {
            get
            {
				return FuelComp is null? Props.maxRange : (Props.maxRange - Props.minRange) * FuelComp.FuelPercentOfMax + Props.minRange;
            }
        }

		public override string CompInspectStringExtra()
		{
			return $"{"YS_StabilizerRangeCurr".Translate()}: {StablizeRadius}";
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (this.parent.def.building != null)
			{
				if(Props.minRange > 0)
                {
					yield return new StatDrawEntry(StatCategoryDefOf.Building, "YS_StabilizerRangeMin".Translate(),
						Props.minRange.ToString(), "YS_StabilizerRangeMinDesc".Translate(), 99991, null, null, false);
				}

                if (Props.scaleWithFuel && FuelComp != null)
                {
					yield return new StatDrawEntry(StatCategoryDefOf.Building, "YS_StabilizerRangeCurr".Translate(),
						StablizeRadius.ToString(), "YS_StabilizerRangeCurrDesc".Translate(), 99992, null, null, false);
				}

				yield return new StatDrawEntry(StatCategoryDefOf.Building, "YS_StabilizerRangeMax".Translate(),
						Props.maxRange.ToString(), "YS_StabilizerRangeMaxDesc".Translate(), 99993, null, null, false);

			}
			yield break;
		}

		public override void PostDrawExtraSelectionOverlays()
		{
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
					cachedMaxStablize = (from x in GenRadial.RadialCellsAround(parent.Position, Props.maxRange, true)
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
			cachedMaxStablize = new List<IntVec3>();
			cachedCurrStablize = new List<IntVec3>();
		}

		private float cachedPercent = 0f;

		private CompRefuelable cachedComp;

		private List<IntVec3> cachedMaxStablize = new List<IntVec3>();

		private List<IntVec3> cachedCurrStablize = new List<IntVec3>();
	}
}
