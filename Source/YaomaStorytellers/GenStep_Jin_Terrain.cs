using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace YaomaStorytellers
{
	public class GenStep_Jin_Terrain : GenStep_Terrain
	{
		public override void Generate(Map map, GenStepParams parms)
		{
			Traverse terrainGenStep = Traverse.Create<GenStep_Terrain>();
			Log.Message("genstep: " + (terrainGenStep is null).ToString());
			BeachMaker.Init(map);
			Log.Message("test");
			RiverMaker riverMaker = terrainGenStep.Method("GenerateRiver", map).GetValue<RiverMaker>();
			Log.Message("riverMaker: " + (riverMaker is null).ToString());

			List<IntVec3> list = new List<IntVec3>();
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid fertility = MapGenerator.Fertility;
			MapGenFloatGrid caves = MapGenerator.Caves;
			TerrainGrid terrainGrid = map.terrainGrid;

			foreach (IntVec3 c in map.AllCells)
			{
				if (!map.areaManager.Home.ActiveCells.Contains(c)) continue;

				Building edifice = c.GetEdifice(map);
				TerrainDef terrainDef;
				if ((edifice != null && edifice.def.Fillage == FillCategory.Full) || caves[c] > 0f)
				{
					Log.Message("test");
					//terrainDef = this.TerrainFrom(c, map, elevation[c], fertility[c], riverMaker, true);
					terrainDef = terrainGenStep.Method(name:  "TerrainFrom", c, map, elevation[c], fertility[c], riverMaker, true).GetValue<TerrainDef>();
				}
				else
				{
					Log.Message("test");
					//terrainDef = this.TerrainFrom(c, map, elevation[c], fertility[c], riverMaker, false);
					terrainDef = terrainGenStep.Method(name: "TerrainFrom", c, map, elevation[c], fertility[c], riverMaker, false).GetValue<TerrainDef>();
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

			//this.RemoveIslands(map);
			Log.Message("islandremove");
			terrainGenStep.Method("RemoveIslands", map).GetValue();

			RoofCollapseCellsFinder.RemoveBulkCollapsingRoofs(list, map);
			BeachMaker.Cleanup();
			foreach (TerrainPatchMaker terrainPatchMaker in map.Biome.terrainPatchMakers)
			{
				terrainPatchMaker.Cleanup();
			}
		}

		public static RiverMaker TraverseRiverMaker(Traverse traverse, Map map)
        {
			return traverse.Method(name: "GenerateRiver", map).GetValue<RiverMaker>();
		}

	}
}
