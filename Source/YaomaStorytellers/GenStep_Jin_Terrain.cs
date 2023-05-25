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
		// maintains the cells of the home area when altering terrain- useful for housing
		public override void Generate(Map map, GenStepParams parms)
		{
			// get a record of the cells of the home area + their terrain
			HashSet<Tuple<IntVec3, TerrainDef>> homeCells = new HashSet<Tuple<IntVec3, TerrainDef>>();
			foreach(var cell in map.areaManager.Home.ActiveCells)
            {
				homeCells.Add(new Tuple<IntVec3, TerrainDef>(cell, cell.GetTerrain(map)));
			}

			// perform terrain change as part of genstep_terrain
			base.Generate(map, parms);

			// replace the home area's terrain from homeCells
			foreach(var homeCell in homeCells)
            {
				map.terrainGrid.SetTerrain(homeCell.Item1, homeCell.Item2);
			}

		}
	}
}
