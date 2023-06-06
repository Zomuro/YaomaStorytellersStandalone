using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_ScatterLumpsMineable : GenStep_ScatterLumpsMineable
    {
		public virtual void Generate(Map map, GenStepParams parms, HashSet<IntVec3> forbidCells)
		{
			minSpacing = 5f;
			warnOnFail = false;
			int num = CalculateFinalCount(map);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec;
				if (!TryFindScatterCell(map, out intVec))
				{
					return;
				}
				ScatterAt(intVec, map, parms, forbidCells, 1);
				usedSpots.Add(intVec);
			}
			usedSpots.Clear();
		}

		protected virtual void ScatterAt(IntVec3 c, Map map, GenStepParams parms, HashSet<IntVec3> forbidCells, int stackCount = 1)
		{
			ThingDef thingDef = ChooseThingDef();
			if (thingDef == null) return;
			int numCells = (forcedLumpSize > 0) ? forcedLumpSize : thingDef.building.mineableScatterLumpSizeRange.RandomInRange;
			recentLumpCells.Clear();
			foreach (IntVec3 intVec in GridShapeMaker.IrregularLump(c, map, numCells))
			{
				if (forbidCells.Contains(intVec)) continue;
				GenSpawn.Spawn(thingDef, intVec, map, WipeMode.Vanish);
				recentLumpCells.Add(intVec);
			}
		}

	}
}
