using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace YaomaStorytellers
{
	public class IncidentWorker_Terraform_Jin : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return true; // adjust so that it only fires when Jin is the storyteller
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = Find.AnyPlayerHomeMap;
			foreach (var step in MapGeneratorDefOf_Yaoma.YS_JianghuJin_RefreshTerrain.genSteps.OrderBy(x => x.order))
			{
				step.genStep.Generate(map, default(GenStepParams));
			}
			map.FinalizeInit();

			return true;
		}

	}
}
