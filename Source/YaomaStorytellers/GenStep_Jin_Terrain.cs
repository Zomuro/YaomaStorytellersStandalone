using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using HarmonyLib;

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

			/*Traverse traverse = Traverse.Create(base.def.genStep);

			HashSet<IntVec3> allCells = map.AllCells.ToHashSet();
			allCells.ExceptWith(YaomaMapUtility.JianghuJinAllCellsCombined());

			BeachMaker.Init(map);
			RiverMaker riverMaker = traverse.Method("GenerateRiver", map).GetValue<RiverMaker>();//this.GenerateRiver(map); 
			List<IntVec3> list = new List<IntVec3>();
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid fertility = MapGenerator.Fertility;
			MapGenFloatGrid caves = MapGenerator.Caves;
			TerrainGrid terrainGrid = map.terrainGrid;
			foreach (IntVec3 c in allCells) // map.AllCells
			{
				Building edifice = c.GetEdifice(map);
				TerrainDef terrainDef;
				if ((edifice != null && edifice.def.Fillage == FillCategory.Full) || caves[c] > 0f)
				{
					terrainDef = traverse.Method("TerrainFrom", c, map, elevation[c], fertility[c], riverMaker, true).GetValue<TerrainDef>();
					//terrainDef = this.TerrainFrom(c, map, elevation[c], fertility[c], riverMaker, true);
				}
				else
				{
					terrainDef = traverse.Method("TerrainFrom", c, map, elevation[c], fertility[c], riverMaker, false).GetValue<TerrainDef>();
					//terrainDef = this.TerrainFrom(c, map, elevation[c], fertility[c], riverMaker, false);
				}
				if (terrainDef.IsRiver && edifice != null)
				{
					list.Add(edifice.Position);
					edifice.Destroy(DestroyMode.Vanish);
				}
				terrainGrid.SetTerrain(c, terrainDef);
			}
			if (riverMaker != null)
			{
				riverMaker.ValidatePassage(map);
			}
			traverse.Method("RemoveIslands", map).GetValue();
			//this.RemoveIslands(map);
			RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker terrainPatchMaker in map.Biome.terrainPatchMakers)
			{
				terrainPatchMaker.Cleanup();
			}*/
		}
	}
}
