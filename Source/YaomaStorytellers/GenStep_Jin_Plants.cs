using RimWorld;
using System.Linq;
using Verse;
using UnityEngine;

namespace YaomaStorytellers
{
    public class GenStep_Jin_Plants : GenStep_Plants
    {
		public override void Generate(Map map, GenStepParams parms)
		{
			float currentPlantDensity = map.wildPlantSpawner.CurrentPlantDensity;
			float currentWholeMapNumDesiredPlants = map.wildPlantSpawner.CurrentWholeMapNumDesiredPlants;
			foreach (IntVec3 c in map.AllCells.ToHashSet()) // grab hashset of all cells, no point in randomizing order
			{
				if (!Rand.Chance(0.001f)) map.wildPlantSpawner.CheckSpawnWildPlantAt(c, currentPlantDensity, 
					currentWholeMapNumDesiredPlants, true); // because cells get randomly selected anyways
			}
		}

	}
}
