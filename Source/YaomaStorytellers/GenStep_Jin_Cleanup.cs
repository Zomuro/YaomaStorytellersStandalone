using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_Cleanup: GenStep
	{
		public override int SeedPart => 814675462;

		public override void Generate(Map map, GenStepParams parms)
		{
			roomStabilizerCells = YaomaMapUtility.JianghuJinAllCellsCombined();
			CleanupRockChunks(map);
			CleanupPawns(map);
			CleanupPlants(map);
			CleanupSteamGesyers(map);
			CleanupRockGrid(map);
			CleanupStabilizers(map);
		}

		public void CleanupRockChunks(Map map)
        {
			HashSet<Thing> chunks = map.listerThings.ThingsInGroup(ThingRequestGroup.Chunk).ToHashSet();
			foreach (var chunk in chunks)
			{
				if (roomStabilizerCells.Contains(chunk.Position)) continue;
				chunk.Destroy();
			}
		}

		public void CleanupPawns(Map map)
        {
			HashSet<Pawn> pawns = map.mapPawns.AllPawnsSpawned.ToHashSet();
			foreach(var pawn in pawns)
            {
				// if pawn is animal or wild man with no faction, kill them all when terraforming - reduce them to ashes
				if (pawn.Spawned && pawn.AnimalOrWildMan() && (pawn.Faction == null || pawn.RaceProps.Insect) && !pawn.IsPrisonerInPrisonCell())
                {
					pawn.Destroy();
					continue;
				}

				// prevent pathfinding error from coming up
				pawn.jobs.StopAll(); // no job
				pawn.pather.StopDead(); // no path
			}
		}

		public void CleanupSteamGesyers(Map map)
		{
			List<Thing> gesyers = map.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser).ToList();
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
				// plants in indoor rooms or stablizer range are safe
				if (roomStabilizerCells.Contains(plant.Position)) continue;

				// if def in forbidCleanPlants, skip
				if (!forbidCleanPlants.NullOrEmpty() &&
					forbidCleanPlants.Contains(plant.def)) continue;

				//else destroy plants
				plant.Destroy();
			}
		}

		public void CleanupRockGrid(Map map)
		{
			foreach (var cell in map.AllCells.ToHashSet())
			{
				if (roomStabilizerCells.Contains(cell)) continue;
				map.roofGrid.SetRoof(cell, null);
			}

			HashSet<Thing> natRocks = map.listerThings.AllThings.Where(x => x.def.thingClass == typeof(Mineable)).ToHashSet();
			foreach (var natRock in natRocks)
            {
				if (roomStabilizerCells.Contains(natRock.Position)) continue;
				natRock.Destroy();
			}
		}

		public void CleanupStabilizers(Map map)
		{
			// grab all stabilizers
			List<Building> stabArray = map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf_Yaoma.YS_StabilizerArrayJin).ToList();
			List<Building> stabTotems = map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf_Yaoma.YS_StabilizerTotemJin).ToList();

			foreach (var array in stabArray)
			{
				CompStablizer_Jin stabComp = array.TryGetComp<CompStablizer_Jin>();
				stabComp.FuelComp.ConsumeFuel(stabComp.FuelComp.Fuel);
				stabComp.ClearStablizeCellsCache();
			}
			foreach (var totem in stabTotems) totem.Destroy();
		}

		public List<ThingDef> forbidCleanPlants;

		public HashSet<IntVec3> roomStabilizerCells = new HashSet<IntVec3>();

	}
}
