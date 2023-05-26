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
			CleanupPawns(map);
			CleanupPlants(map);
			CleanupSteamGesyers(map);
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
		}

		public void CleanupPawns(Map map)
        {
			HashSet<Pawn> pawns = map.mapPawns.AllPawnsSpawned.ToHashSet();
			foreach(var pawn in pawns)
            {
				// if pawn is animal or wild man with no faction, kill them all when terraforming - reduce them to ashes
				if (pawn.AnimalOrWildMan() && pawn.Faction is null)
                {
					pawn.Destroy();
					continue;
				}

				// prevent pathfinding error from coming up
				pawn.jobs.StopAll(); 
            }
		}

		public void CleanupAnimals(Map map)
		{
			HashSet<Pawn> animals = map.mapPawns.AllPawnsSpawned.Where(x => x.AnimalOrWildMan()).ToHashSet();
			foreach (var animal in animals)
			{
				if (animal.Faction != null) continue;
				animal.Destroy();
			}
		}

		public void CleanupSteamGesyers(Map map)
		{
			HashSet<Thing> gesyers = map.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser).ToHashSet();
			foreach (var gesyer in gesyers)
			{
				List<Thing> thingList = gesyer.Position.GetThingList(map);
				if (thingList.FirstOrDefault(x => x.def == ThingDefOf.GeothermalGenerator) is null) gesyer.DeSpawn();
			}

		}

		public void CleanupPlants(Map map)
		{
			// grab all plants
			HashSet<Thing> plants = map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Plant)).ToHashSet();
			foreach (var plant in plants)
			{
				// plants in growing zones OR in home area are not culled
				if (map.zoneManager.ZoneAt(plant.Position) as Zone_Growing != null || 
					map.areaManager.Home[plant.Position]) continue;

				// if def in forbidCleanPlants, skip
				if (!forbidCleanPlants.NullOrEmpty() &&
					forbidCleanPlants.Contains(plant.def)) continue;

				//else destroy plants
				plant.Destroy();
			}

		}

		public List<ThingDef> forbidCleanPlants;

	}
}
