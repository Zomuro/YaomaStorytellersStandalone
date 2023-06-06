using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

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
			CleanupRockGrid(map);
			CleanupStabilizers(map);
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
				pawn.jobs.StopAll(); // no job
				pawn.pather.StopDead(); // no path
			}
		}

		public void CleanupAnimals(Map map)
		{
			HashSet<Pawn> animals = map.mapPawns.AllPawnsSpawned.Where(x => x.AnimalOrWildMan()).ToHashSet();
			foreach (var animal in animals)
			{
				if (animal.Faction != null || animal.RaceProps.Insect) continue;
				animal.Destroy();
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

		public void CleanupRockGrid(Map map)
		{
			foreach (var cell in map.AllCells.ToHashSet())
			{
				if (YaomaMapUtility.cachedRoomCells.Contains(cell)) continue;
				map.roofGrid.SetRoof(cell, null);
			}

			HashSet<Thing> removeRocks = new HashSet<Thing>();
			HashSet<Thing> natRocks = map.listerThings.AllThings.Where(x => x.def.thingClass == typeof(Mineable)).ToHashSet();
			foreach (var natRock in natRocks)
            {
				if (YaomaMapUtility.cachedRoomCells.Contains(natRock.Position)) continue;
				//removeRocks.Add(natRock);
				natRock.Destroy();
			}
			//foreach (var rock in removeRocks) rock.Destroy();

			
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

	}
}
