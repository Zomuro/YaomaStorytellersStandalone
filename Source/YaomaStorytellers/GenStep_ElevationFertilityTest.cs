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
	public class GenStep_ElevationFertilityTest : GenStep
	{

		public override int SeedPart
		{
			get
			{
				return 826504671;
			}
		}

		// Token: 0x060058C9 RID: 22729 RVA: 0x001DEF10 File Offset: 0x001DD110
		public override void Generate(Map map, GenStepParams parms)
		{
			NoiseRenderer.renderSize = new IntVec2(map.Size.x, map.Size.z);
			ModuleBase moduleBase = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev base");
			float num = 1f;
			switch (map.TileInfo.hilliness)
			{
				case Hilliness.Flat:
					num = MapGenTuning.ElevationFactorFlat;
					break;
				case Hilliness.SmallHills:
					num = MapGenTuning.ElevationFactorSmallHills;
					break;
				case Hilliness.LargeHills:
					num = MapGenTuning.ElevationFactorLargeHills;
					break;
				case Hilliness.Mountainous:
					num = MapGenTuning.ElevationFactorMountains;
					break;
				case Hilliness.Impassable:
					num = MapGenTuning.ElevationFactorImpassableMountains;
					break;
			}
			moduleBase = new Multiply(moduleBase, new Const((double)num));
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev world-factored");
			if (map.TileInfo.hilliness == Hilliness.Mountainous || map.TileInfo.hilliness == Hilliness.Impassable)
			{
				ModuleBase moduleBase2 = new DistFromAxis((float)map.Size.x * 0.42f);
				moduleBase2 = new Clamp(0.0, 1.0, moduleBase2);
				moduleBase2 = new Invert(moduleBase2);
				moduleBase2 = new ScaleBias(1.0, 1.0, moduleBase2);
				Rot4 random;
				do
				{
					random = Rot4.Random;
				}
				while (random == Find.World.CoastDirectionAt(map.Tile));
				if (random == Rot4.North)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
					moduleBase2 = new Translate(0.0, 0.0, (double)(-(double)map.Size.z), moduleBase2);
				}
				else if (random == Rot4.East)
				{
					moduleBase2 = new Translate((double)(-(double)map.Size.x), 0.0, 0.0, moduleBase2);
				}
				else if (random == Rot4.South)
				{
					moduleBase2 = new Rotate(0.0, 90.0, 0.0, moduleBase2);
				}

				NoiseDebugUI.StoreNoiseRender(moduleBase2, "mountain");
				moduleBase = new Add(moduleBase, moduleBase2);
				NoiseDebugUI.StoreNoiseRender(moduleBase, "elev + mountain");
			}

			float b = map.TileInfo.WaterCovered ? 0f : float.MaxValue;
			//MapGenFloatGrid elevation = MapGenerator.Elevation;
			MapGenFloatGrid elevation = new MapGenFloatGrid(map);
			foreach (IntVec3 intVec in map.AllCells)
			{
				elevation[intVec] = Mathf.Min(moduleBase.GetValue(intVec), b);
			}

			ModuleBase moduleBase3 = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
			moduleBase3 = new ScaleBias(0.5, 0.5, moduleBase3);
			NoiseDebugUI.StoreNoiseRender(moduleBase3, "noiseFert base");

			//MapGenFloatGrid fertility = MapGenerator.Fertility;
			MapGenFloatGrid fertility = new MapGenFloatGrid(map);
			foreach (IntVec3 intVec2 in map.AllCells)
			{
				fertility[intVec2] = moduleBase3.GetValue(intVec2);
			}
		}

		// Token: 0x040036B0 RID: 14000
		private const float ElevationFreq = 0.021f;

		// Token: 0x040036B1 RID: 14001
		private const float FertilityFreq = 0.021f;

		// Token: 0x040036B2 RID: 14002
		private const float EdgeMountainSpan = 0.42f;
	}
}
