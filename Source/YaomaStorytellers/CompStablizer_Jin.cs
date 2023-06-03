using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    [StaticConstructorOnStartup]
    public class CompStablizer_Jin : CompRefuelable
    {
		public CompProperties_Stablizer_Jin JinProps
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
				return JinProps.maxRange * FuelPercentOfMax;
            }
        }

		public override void PostDrawExtraSelectionOverlays()
		{
			// shows maximum range of stablization field
			GenDraw.DrawFieldEdges(StablizeMaxCells, Color.white);

			// shows current range of stablization field
			GenDraw.DrawFieldEdges(StablizeCurrCells, Color.yellow);
		}

		public List<IntVec3> StablizeMaxCells
        {
            get
            {
                if (cachedMaxStablize.NullOrEmpty())
                {
					cachedMaxStablize = (from x in GenRadial.RadialCellsAround(parent.Position, JinProps.maxRange, true)
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
				if (cachedCurrStablize.NullOrEmpty() || FuelPercentOfMax != cachedPercent)
				{
					cachedCurrStablize = (from x in GenRadial.RadialCellsAround(parent.Position, StablizeRadius, true)
										 where x.InBounds(Find.CurrentMap)
										 select x).ToList();
				}
				return cachedCurrStablize;
			}
		}

		public void ClearStablizeCellsCache()
        {
			cachedPercent = 0f;
			cachedMaxStablize = null;
			cachedCurrStablize = null;
		}

		private float cachedPercent = 0f;

		private List<IntVec3> cachedMaxStablize;

		private List<IntVec3> cachedCurrStablize;

	}
}
