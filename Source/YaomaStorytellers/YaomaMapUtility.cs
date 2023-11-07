using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    public static class YaomaMapUtility
    {
        // gets the cells of all the rooms for Jin to decay and respect when terraforming + add in areas stablizers / totems cover
        public static HashSet<IntVec3> JianghuJinAllRoomCells(Map map)
        {
            // nullchecks rooms
            List<Room> rooms = map.regionGrid.allRooms;
            if (rooms.NullOrEmpty()) return null;

            // gets all cells in rooms that aren't psychologically outdoors or smaller than 4 cells
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            foreach(var room in rooms)
            {
                // rooms that are outdoors or too small are not included
                if (room.OutdoorsForWork || room.CellCount < 4) continue;
                cells.AddRange(room.Cells);
                cells.AddRange(room.BorderCells);
            }

            cachedRoomCells = cells;
            return cells;
        }

        public static HashSet<IntVec3> JianghuJinAllStabilizerCells(Map map)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();

            // get all cells in the area of the stabilizers
            List<Building> stabilizers = map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf_Yaoma.YS_StabilizerArrayJin).ToList();
            stabilizers.AddRange(map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf_Yaoma.YS_StabilizerTotemJin));
            foreach (var building in stabilizers)
            {
                cells.AddRange(building.TryGetComp<CompStablizer_Jin>().StablizeCurrCells);
            }

            cachedStabilizerCells = cells;
            return cells;
        }

        public static HashSet<IntVec3> JianghuJinAllSpecialCells(Map map, List<SetupProtectionRange> protectedDefs)
        {
            // basically only used for anima tree
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            if (YaomaStorytellerUtility.settings.JianghuJinTreeSave || protectedDefs.NullOrEmpty())
            {
                cachedSpecialCells = cells; // cache cells
                return cells;
            }

            foreach(var defRange in protectedDefs)
            {
                List<Thing> things = map.listerThings.ThingsOfDef(defRange.thingDef); // get list of things
                if (things.NullOrEmpty()) continue; // if there's nothing - continue.
                foreach (var thing in things) // else, get the cell of the thing and all cells on the map within range.
                {
                    cells.AddRange(GenRadial.RadialCellsAround(thing.Position, defRange.range, true));
                }
            }

            cachedSpecialCells = cells; // cache cells
            return cells; // return
        }

        public static HashSet<IntVec3> JianghuJinAllCellsCombined()
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            cells.AddRange(cachedRoomCells);
            cells.AddRange(cachedStabilizerCells);
            cells.AddRange(cachedSpecialCells);
            return cells;
        }

        public static void JianghuJinSimDecay(ref HashSet<IntVec3> cells, float decayProb, RoomDecaySetting decaySetting = RoomDecaySetting.Absolute)
        {
            if (cells is null) 
            {
                cells = new HashSet<IntVec3>();
                return;
            }

            decayProb = Mathf.Clamp(decayProb, 0f, 1f);
            if (decayProb >= 1)
            {
                cells.Clear();
                return;
            }
            if (decayProb <= 0) return;

            HashSet<IntVec3> removeCells = new HashSet<IntVec3>();
            DecaySettingResult(decayProb, cells, ref removeCells, decaySetting);
            cells.ExceptWith(removeCells);
        }

        public static HashSet<IntVec3> JianghuJinSimDecay(HashSet<IntVec3> cells, float decayProb, RoomDecaySetting decaySetting = RoomDecaySetting.Absolute)
        {
            if (cells is null) return new HashSet<IntVec3>();

            decayProb = Mathf.Clamp(decayProb, 0f, 1f);
            if (decayProb >= 1) return new HashSet<IntVec3>();
            if (decayProb <= 0) return cells;

            HashSet<IntVec3> roomCells = new HashSet<IntVec3>();
            roomCells.AddRange(cells);
            HashSet<IntVec3> removeCells = new HashSet<IntVec3>();

            DecaySettingResult(decayProb, roomCells, ref removeCells, decaySetting);
            roomCells.ExceptWith(removeCells);
            return roomCells;
        }

        public static void DecaySettingResult(float decayProb, HashSet<IntVec3> roomCells, ref HashSet<IntVec3> removeCells, RoomDecaySetting decaySetting)
        {
            switch (decaySetting)
            {
                // uses only one random check to determine if a cell should be removed
                case RoomDecaySetting.Absolute:
                    foreach (var cell in roomCells)
                        if (UnityEngine.Random.Range(0f, 1f) <= decayProb) removeCells.Add(cell);
                    return;

                // randomly gets a "base cell", and adds adjacent cells (8 way) if possible
                case RoomDecaySetting.Adjacent:
                    HashSet<IntVec3> cells = new HashSet<IntVec3>();
                    cells.AddRange(roomCells);
                    int count = (int) (decayProb * cells.Count);
                    while(count > 0)
                    {
                        IntVec3 baseCell = cells.RandomElement();
                        cells.Remove(baseCell);
                        removeCells.Add(baseCell);
                        count--;
                        bool end = false;
                        while (!end)
                        {
                            baseCell = baseCell.RandomAdjacentCell8Way();
                            if (cells.Contains(baseCell))
                            {
                                cells.Remove(baseCell);
                                removeCells.Add(baseCell);
                                count--;
                            }
                            else end = true;
                        }
                    }
                    return;

                // takes into account surrounding room cells to determine if the cell should be removed
                case RoomDecaySetting.Augmented:
                    HashSet<IntVec3> cellsTwo = new HashSet<IntVec3>();
                    cellsTwo.AddRange(roomCells);
                    foreach (var cell in roomCells)
                    {
                        IEnumerable<IntVec3> adj = GenAdjFast.AdjacentCellsCardinal(cell);
                        float keepFactor = 1 + cellsTwo.Intersect(adj).Count() / 4f;
                        if (UnityEngine.Random.Range(0f, 1f) <= decayProb / keepFactor)
                        {
                            cellsTwo.Remove(cell);
                            removeCells.Add(cell);
                        }
                    }
                    return;
                default:
                    return;
            }
        }

        public static void ClearCache()
        {
            cachedRoomCells = null;
            cachedStabilizerCells = null;
            cachedSpecialCells = null;
        }

        public static HashSet<IntVec3> cachedRoomCells;

        public static HashSet<IntVec3> cachedStabilizerCells;

        public static HashSet<IntVec3> cachedSpecialCells;

        public static System.Random rand = new System.Random();

    }
}
