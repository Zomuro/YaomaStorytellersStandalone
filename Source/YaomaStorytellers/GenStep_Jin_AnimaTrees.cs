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
