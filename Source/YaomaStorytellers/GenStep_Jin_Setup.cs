using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace YaomaStorytellers
{
	public class GenStep_Jin_Setup : GenStep
	{
		public override int SeedPart
		{
			get
			{
				return 827727203;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			MapGenerator.mapBeingGenerated = map;
			RockNoises.Init(map);

			MapGenerator.SetVar(MapGenerator.ElevationName, new MapGenFloatGrid(MapGenerator.mapBeingGenerated));
			MapGenerator.SetVar(MapGenerator.FertilityName, new MapGenFloatGrid(MapGenerator.mapBeingGenerated));
			MapGenerator.SetVar(MapGenerator.CavesName, new MapGenFloatGrid(MapGenerator.mapBeingGenerated));

			if (YaomaStorytellerUtility.settings.JianghuJinBiomeChange)
            {
				map.TileInfo.biome = AvailBiomeScoring(map.TileInfo, map.Tile, YaomaStorytellerUtility.settings.JianghuJinBiomeChangeUnlocked)?.
					RandomElementByWeight(x => x.Item2).Item1 ?? BiomeDefOf.TemperateForest;
			}

			if (YaomaStorytellerUtility.settings.JianghuJinHillinessChange)
            {
				map.TileInfo.hilliness = hillList.RandomElement();
			}

			YaomaMapUtility.ClearCache();
			YaomaMapUtility.JianghuJinAllRoomCells(map);
			YaomaMapUtility.JianghuJinAllStabilizerCells(map);
		}

		public IEnumerable<Tuple<BiomeDef, float>> AvailBiomeScoring(Tile ws, int tileID, bool unlocked = false)
		{
			List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				BiomeDef biomeDef = allDefsListForReading[i];
				if (biomeDef.implemented && biomeDef.canBuildBase)
				{
					if (!unlocked && biomeDef.Worker.GetScore(ws, tileID) < 0) continue;

					yield return new Tuple<BiomeDef, float>(biomeDef, biomeDef.Worker.GetScore(ws, tileID));
				}
			}
			yield break;
		}

		public List<Hilliness> hillList = new List<Hilliness>() 
		{
			Hilliness.Flat,
			Hilliness.SmallHills,
			Hilliness.LargeHills,
			Hilliness.Mountainous,
			Hilliness.Impassable
		};
	}
}
