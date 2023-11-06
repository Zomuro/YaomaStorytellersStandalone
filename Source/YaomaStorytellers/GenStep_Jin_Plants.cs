using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_Plants : GenStep_Plants
    {
		public override void Generate(Map map, GenStepParams parms)
		{
			float currentPlantDensity = map.wildPlantSpawner.CurrentPlantDensity;
			float currentWholeMapNumDesiredPlants = map.wildPlantSpawner.CurrentWholeMapNumDesiredPlants;

			HashSet<IntVec3> safeCells = map.AllCells.ToHashSet(); // get whole map
			safeCells.ExceptWith(YaomaMapUtility.cachedSpecialCells); // remove special cells to add plants

			foreach (IntVec3 c in safeCells) // grab hashset of all cells, no point in randomizing order
			{
				if (!Rand.Chance(0.001f)) map.wildPlantSpawner.CheckSpawnWildPlantAt(c, currentPlantDensity, 
					currentWholeMapNumDesiredPlants, true); // because cells get randomly selected anyways
			}
		}

	}
}
