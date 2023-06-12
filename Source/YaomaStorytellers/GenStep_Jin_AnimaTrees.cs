using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class GenStep_Jin_AnimaTrees : GenStep_AnimaTrees
	{
		public override void Generate(Map map, GenStepParams parms)
		{
			// check for Anima Tree- if none exist, spawn one.
			if (!map.listerThings.ThingsOfDef(treeDef).NullOrEmpty()) return;
			base.Generate(map, parms);
		}
	}
}
