using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_RocksFromGrid : GenStep_RocksFromGrid
	{
		public override void Generate(Map map, GenStepParams parms)
		{
			if (map.TileInfo.WaterCovered) return;
			map.regionAndRoomUpdater.Enabled = false;

			HashSet<IntVec3> roomCells = YaomaMapUtility.JianghuJinSimDecay(YaomaMapUtility.cachedRoomCells,
				YaomaStorytellerUtility.settings.JianghuJinDecayProbRooms,
				YaomaStorytellerUtility.settings.JianghuJinRoomIntSetting[YaomaStorytellerUtility.settings.JianghuJinDecayRoomsInt]);
			roomCells.AddRange(YaomaMapUtility.cachedStabilizerCells);

			float gridMult = 0.7f;
			List<RoofThreshold> list = new List<RoofThreshold>()
			{
				new RoofThreshold
				{
					roofDef = RoofDefOf.RoofRockThick,
					minGridVal = gridMult * 1.14f
				},
				new RoofThreshold
				{
					roofDef = RoofDefOf.RoofRockThin,
					minGridVal = gridMult * 1.04f
				}
			};

			MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid caves = MapGenerator.Caves;
			foreach (IntVec3 cell in map.AllCells.ToHashSet())
			{
				if (roomCells.Contains(cell)) continue;
				float elev = elevation[cell];
				if (elev > gridMult)
				{
					if (caves[cell] <= 0f)
					{
						ThingDef def = RockDefAt(cell);
						GenSpawn.Spawn(def, cell, map, WipeMode.Vanish);
					}
					for (int i = 0; i < list.Count; i++)
					{
						if (elev > list[i].minGridVal)
						{
							map.roofGrid.SetRoof(cell, list[i].roofDef);
							break;
						}
					}
				}
			}
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> toRemove = new List<IntVec3>();
			foreach (IntVec3 cell in map.AllCells.ToHashSet())
			{
				if (!visited[cell])
				{
					if (IsNaturalRoofAt(cell, map))
					{
						toRemove.Clear();
						map.floodFiller.FloodFill(cell, (IntVec3 x) => IsNaturalRoofAt(x, map), delegate (IntVec3 x)
						{
							visited[x] = true;
							toRemove.Add(x);
						}, int.MaxValue, false, null);
						if (toRemove.Count < 20)
						{
							for (int j = 0; j < toRemove.Count; j++)
							{
								map.roofGrid.SetRoof(toRemove[j], null);
							}
						}
					}
				}
			}
			GenStep_Jin_ScatterLumpsMineable genStep_MineableLumps = new GenStep_Jin_ScatterLumpsMineable();
			genStep_MineableLumps.maxValue = maxMineableValue;
			float density = 10f;
			switch (Find.WorldGrid[map.Tile].hilliness)
			{
				case Hilliness.Flat:
					density = 4f;
					break;
				case Hilliness.SmallHills:
					density = 8f;
					break;
				case Hilliness.LargeHills:
					density = 11f;
					break;
				case Hilliness.Mountainous:
					density = 15f;
					break;
				case Hilliness.Impassable:
					density = 16f;
					break;
			}
			genStep_MineableLumps.countPer10kCellsRange = new FloatRange(density, density);
			genStep_MineableLumps.Generate(map, parms, roomCells);
			map.regionAndRoomUpdater.Enabled = true;
		}

		private bool IsNaturalRoofAt(IntVec3 c, Map map)
		{
			return c.Roofed(map) && c.GetRoof(map).isNatural;
		}

		private float maxMineableValue = float.MaxValue;

		private const int MinRoofedCellsPerGroup = 20;

		private class RoofThreshold
		{
			public RoofDef roofDef;

			public float minGridVal;
		}
	}
}
