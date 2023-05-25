using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace YaomaStorytellers
{
    public static class TileGenerationTestUtil
    {

		[DebugAction("Map", "GenerateMapCurrTile", false, false, false, 0, false, allowedGameStates = AllowedGameStates.Playing, hideInSubMenu = false)]
		private static void GenerateMapCurrTile()
		{
			MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
			mapParent.Tile = Find.CurrentMap.Tile;
			mapParent.SetFaction(Faction.OfPlayer);
			Find.WorldObjects.Add(mapParent);
			GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, new IntVec3(50, 1, 50), null);
		}
	}
}
