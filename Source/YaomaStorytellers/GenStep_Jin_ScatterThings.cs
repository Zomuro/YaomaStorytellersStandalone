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
	public class GenStep_Jin_ScatterThings : GenStep_ScatterThings
	{
		public override void Generate(Map map, GenStepParams parms)
		{
			// despawn scattered things before generating more of it
			foreach(var thing in map.listerThings.ThingsOfDef(thingDef)) thing.DeSpawn();

			base.Generate(map, parms);
		}
	}
}
