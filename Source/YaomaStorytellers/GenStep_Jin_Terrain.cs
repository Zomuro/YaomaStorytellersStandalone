using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_Terrain : GenStep_Terrain
	{
		// maintains the cells of the home area when altering terrain- useful for housing
		public override void Generate(Map map, GenStepParams parms)
		{
			// room cells = indoor cells; certain cells in the hashset are removed to allow them to be "damaged"
			// decays cells in the cached room cells (doesn't matter at this stage)
			YaomaMapUtility.JianghuJinSimDecay(ref YaomaMapUtility.cachedRoomCells,
				YaomaStorytellerUtility.settings.JianghuJinDecayProbTerrain,
				YaomaStorytellerUtility.settings.JianghuJinRoomIntSetting[YaomaStorytellerUtility.settings.JianghuJinDecayTerrainInt]);

			// create traverse using holder definition
			Traverse traverse = Traverse.Create(this);
			HashSet<IntVec3> allCells = map.AllCells.ToHashSet(); // gets all cells of map
			allCells.ExceptWith(YaomaMapUtility.JianghuJinAllCellsCombined()); // excludes cells from terrain change

			// creates beach and river makers
			BeachMaker.Init(map);
			RiverMaker riverMaker = traverse.Method("GenerateRiver", new Type[] { typeof(Map) }).GetValue<RiverMaker>(map);

			// get map gen grids
			List<IntVec3> list = new List<IntVec3>();
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid fertility = MapGenerator.Fertility;
			MapGenFloatGrid caves = MapGenerator.Caves;
			TerrainGrid terrainGrid = map.terrainGrid;

			foreach (IntVec3 c in allCells) // for all allowed cells
			{
				Building edifice = c.GetEdifice(map);
				TerrainDef terrainDef;

				if ((edifice != null && edifice.def.Fillage == FillCategory.Full) || caves[c] > 0f)
				{
					terrainDef = traverse.Method("TerrainFrom", new Type[] { typeof(IntVec3), typeof(Map), typeof(float), typeof(float), typeof(RiverMaker), typeof(bool) })
						.GetValue<TerrainDef>(c, map, elevation[c], fertility[c], riverMaker, true);
				}
				else
                {
					terrainDef = traverse.Method("TerrainFrom", new Type[]{typeof(IntVec3), typeof(Map), typeof(float), typeof(float), typeof(RiverMaker), typeof(bool) })
						.GetValue<TerrainDef>(c, map, elevation[c], fertility[c], riverMaker, false);
				}

				if (terrainDef.IsRiver && edifice != null)
				{
					list.Add(edifice.Position);
					edifice.Destroy(DestroyMode.Vanish);
				}
				terrainGrid.SetTerrain(c, terrainDef);
			}

			if (riverMaker != null) riverMaker.ValidatePassage(map);

			traverse.Method("RemoveIslands", new Type[] { typeof(Map) }).GetValue(map);
			RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker terrainPatchMaker in map.Biome.terrainPatchMakers) terrainPatchMaker.Cleanup();
		}
	}
}
