using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_Fog : GenStep_Fog
	{
		public override void Generate(Map map, GenStepParams parms)
		{
			base.Generate(map, parms);
			if (!map.IsPlayerHome) return;
            foreach(var pawn in map.mapPawns.AllPawns)
            {
				if(pawn.Faction != null && pawn.IsColonist)
                {
					FloodFillerFog.FloodUnfog(pawn.PositionHeld, map);
                }
            }
		}

	}
}
