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
	public class GenStep_Jin_Cleanup: GenStep
	{
		public override int SeedPart
		{
			get
			{
				return 814675462;
			}
		}

		public override void Generate(Map map, GenStepParams parms)
		{
			CleanupRockChunks(map);
			CleanupAnimals(map);
		}

		public void CleanupRockChunks(Map map)
        {
			HashSet<ThingDef> mineables = Find.World.NaturalRockTypesIn(map.Tile).Select(x => x.building.mineableThing).ToHashSet();
			HashSet<Thing> chunks = new HashSet<Thing>();
			foreach (var chunkDef in mineables) chunks.AddRange(map.listerThings.ThingsOfDef(chunkDef));
			foreach (var chunk in chunks)
			{
				if (chunk.IsInAnyStorage()) continue;
				chunk.Destroy();
			}

			chunks.Clear();
			chunks.Clear();
			mineables.Clear();
		}

		public void CleanupAnimals(Map map)
		{
			HashSet<Pawn> animals = map.mapPawns.AllPawnsSpawned.Where(x => x.AnimalOrWildMan()).ToHashSet();
			foreach (var animal in animals)
			{
				if (animal.Faction != null) continue;
				animal.Destroy();
			}

			animals.Clear();
		}

	}
}
