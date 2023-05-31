using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

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
			//YaomaMapUtility.JianghuJinAllRoomBorderCells(map);
		}

		/*public IEnumerable<Tuple<BiomeDef, float>> BiomeScoring(Tile ws, int tileID)
		{ 
			List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				BiomeDef biomeDef = allDefsListForReading[i];
				if (biomeDef.implemented && biomeDef.canBuildBase)
				{
					yield return new Tuple<BiomeDef, float> (biomeDef, biomeDef.Worker.GetScore(ws, tileID));
				}
			}
			yield break;
		}*/

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
