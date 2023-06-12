using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_Terrain : GenStep_Terrain
	{
		// maintains the cells of the home area when altering terrain- useful for housing
		public override void Generate(Map map, GenStepParams parms)
		{
			// get a record of the cells of the rooms + their terrain
			HashSet<Tuple<IntVec3, TerrainDef>> homeCells = new HashSet<Tuple<IntVec3, TerrainDef>>();

			// decays cells in the cached room cells (doesn't matter at this stage)
			YaomaMapUtility.JianghuJinSimDecay(ref YaomaMapUtility.cachedRoomCells,
				YaomaStorytellerUtility.settings.JianghuJinDecayProbTerrain,
				YaomaStorytellerUtility.settings.JianghuJinRoomIntSetting[YaomaStorytellerUtility.settings.JianghuJinDecayTerrainInt]);

			foreach (var cell in YaomaMapUtility.JianghuJinAllCellsCombined())
			{
				homeCells.Add(new Tuple<IntVec3, TerrainDef>(cell, cell.GetTerrain(map)));
			}

			// perform terrain change as part of genstep_terrain
			base.Generate(map, parms);

			// replace the home area's terrain from homeCells
			foreach(var homeCell in homeCells)
            {
				if (!homeCell.Item1.InBounds(map)) continue;
				map.terrainGrid.SetTerrain(homeCell.Item1, homeCell.Item2);
			}


		}
	}
}
