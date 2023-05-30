﻿using System;
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
                if (room.PsychologicallyOutdoors) continue;
                cells.AddRange(room.Cells);
                cells.AddRange(room.BorderCells);
            }
            cachedRoomCells = cells;
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