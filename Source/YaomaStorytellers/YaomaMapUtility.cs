using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;

namespace YaomaStorytellers
{
    public static class YaomaMapUtility
    {
        // gets the cells of all the rooms for Jin to decay and respect when terraforming
        public static HashSet<IntVec3> JianghuJinAllRoomCells(Map map)
        {
            List<Room> rooms = map.regionGrid.allRooms;
            if (rooms.NullOrEmpty()) return null;

            HashSet<Room> colonyRooms = map.listerBuildings.allBuildingsColonist.Select(x => x.GetRoom()).ToHashSet();

            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            foreach(var room in rooms)
            {
                if (room.PsychologicallyOutdoors || !colonyRooms.Contains(room)) continue;
                cells.AddRange(room.Cells);
            }

            cachedRoomCells = cells;
            return cells;
        }

        // decays the cells to simulate rock infiltration
        public static void JianghuJinSimDecay(ref HashSet<IntVec3> cells, float baseProb, RoomDecaySetting decaySetting = RoomDecaySetting.StableAdj)
        {
            if (cells is null) 
            {
                cells = new HashSet<IntVec3>();
                return;
            }

            float keepProb = Mathf.Clamp(baseProb, 0f, 1f);
            if (keepProb <= 0)
            {
                cells.Clear();
                return;
            }

            HashSet<IntVec3> removeCells = new HashSet<IntVec3>();
            foreach (var cell in cells)
            {
                float keepFactor = 1f;
                if (decaySetting != RoomDecaySetting.Absolute)
                {
                    IEnumerable<IntVec3> adj = GenAdjFast.AdjacentCellsCardinal(cell);
                    keepFactor += DecaySettingMult(decaySetting) * cells.Except(removeCells).Union(adj).Count() / 8f;
                }
                if (UnityEngine.Random.Range(0f, 1f) > keepProb * keepFactor) removeCells.Add(cell);
            }

            cells.ExceptWith(removeCells);
        }

        public static HashSet<IntVec3> JianghuJinSimDecay(HashSet<IntVec3> cells, float baseProb, RoomDecaySetting decaySetting = RoomDecaySetting.StableAdj)
        {
            if (cells is null) return new HashSet<IntVec3>();

            float keepProb = Mathf.Clamp(baseProb, 0f, 1f);
            if (keepProb <= 0) return new HashSet<IntVec3>();

            HashSet<IntVec3> removeCells = new HashSet<IntVec3>();

            foreach (var cell in cells)
            {
                float keepFactor = 1f;
                if(decaySetting != RoomDecaySetting.Absolute)
                {
                    IEnumerable<IntVec3> adj = GenAdjFast.AdjacentCellsCardinal(cell);
                    keepFactor += DecaySettingMult(decaySetting) * cells.Except(removeCells).Union(adj).Count() / 8f;
                }
                if (UnityEngine.Random.Range(0f, 1f) > keepProb * keepFactor) removeCells.Add(cell);
            }
            return cells.Except(removeCells).ToHashSet();
        }

        public static float DecaySettingMult(RoomDecaySetting decaySetting = RoomDecaySetting.StableAdj)
        {
            switch (decaySetting)
            {
                case RoomDecaySetting.StableAdj:
                    return 1f;
                case RoomDecaySetting.UnstableAdj:
                    return -1f;
                default:
                    return 0f;
            }
        }

        /*public static void JianghuJinSimDecay(ref HashSet<IntVec3> cells)
        {
            if (cells is null) return;

            // sets the base chance of the cell not being thrown out
            // for each cardinal adjacent cell, increases chance by 25% of base
            float keepProb = 0.5f;
            HashSet<IntVec3> removeCells = new HashSet<IntVec3>();

            foreach (var cell in cells)
            {
                IEnumerable<IntVec3> adj = GenAdjFast.AdjacentCellsCardinal(cell);
                float keepFactor = 1f + cells.Except(removeCells).Intersect(adj).Count() / 4f;
                if (UnityEngine.Random.Range(0f, 1f) > keepProb * keepFactor) removeCells.Add(cell);
            }

            cells.ExceptWith(removeCells);
        }*/

        /*public static HashSet<IntVec3> JianghuJinDecayedRoomCells(Map map, float baseProb)
        {
            HashSet<IntVec3> cells = cachedRoomCells ?? JianghuJinAllRoomCells(map);
            JianghuJinSimDecay(ref cells, baseProb);
            return cells;
        }*/

        public static void ClearCache()
        {
            cachedRoomCells = null;
        }

        public static HashSet<IntVec3> cachedRoomCells;

        public static System.Random rand = new System.Random();

    }
}
