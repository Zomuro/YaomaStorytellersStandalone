using RimWorld;
using System.Linq;
using Verse;
using Verse.Noise;

namespace YaomaStorytellers
{
    public class GenStep_Jin_CavesTerrain : GenStep_CavesTerrain
	{
		public override void Generate(Map map, GenStepParams parms)
		{
			if (!Find.World.HasCaves(map.Tile)) return;
			Perlin perlin = new Perlin(0.079999998211860657, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
			Perlin perlin2 = new Perlin(0.15999999642372131, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
			MapGenFloatGrid caves = MapGenerator.Caves;

			foreach (IntVec3 intVec in map.AllCells.ToHashSet())
			{
                if (YaomaMapUtility.JianghuJinAllCellsCombined().Contains(intVec))
                {
					map.terrainGrid.SetTerrain(intVec, intVec.GetTerrain(map));
					continue;
				}

				if (caves[intVec] > 0f && !intVec.GetTerrain(map).IsRiver)
				{
					float num = (float)perlin.GetValue((double)intVec.x, 0.0, (double)intVec.z);
					float num2 = (float)perlin2.GetValue((double)intVec.x, 0.0, (double)intVec.z);
					if (num > 0.93f)
					{
						map.terrainGrid.SetTerrain(intVec, TerrainDefOf.WaterShallow);
					}
					else if (num2 > 0.55f)
					{
						map.terrainGrid.SetTerrain(intVec, TerrainDefOf.Gravel);
					}
				}
			}
		}
	}
}
