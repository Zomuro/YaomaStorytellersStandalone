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
			DeepProfiler.Start("IncidentWorker_Terraform");
			Map map = Find.AnyPlayerHomeMap;
			if (map is null)
			{
				DeepProfiler.End();
				return false;
			}
			string seed = Find.World.info.seedString;
			Log.Message("original seed: " + seed);

			Find.World.info.seedString = RandomString(rand.Next(4, 10));
			Log.Message("new seed: " + Find.World.info.seedString);
			foreach (var step in MapGeneratorDefOf_Yaoma.YS_JianghuJin_RefreshTerrain.genSteps.OrderBy(x => x.order))
			{
				Log.Message(step.genStep.def.defName);
				step.genStep.Generate(map, default(GenStepParams));
			}
			map.FinalizeInit();
			Find.World.info.seedString = seed;
			DeepProfiler.End();
			return true;

			/*
			try
            {
				Find.World.info.seedString = RandomString(rand.Next(4, 10));
				Log.Message("new seed: " + Find.World.info.seedString);
				foreach (var step in MapGeneratorDefOf_Yaoma.YS_JianghuJin_RefreshTerrain.genSteps.OrderBy(x => x.order))
				{
					Log.Message(step.genStep.def.defName);
					step.genStep.Generate(map, default(GenStepParams));
				}
				map.FinalizeInit();
				Find.World.info.seedString = seed;
				DeepProfiler.End();
				return true;
			}

            catch
			{
				Find.World.info.seedString = seed;
				DeepProfiler.End();
				return false;
            }*/
		}

		public string RandomString(int length)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-+<>";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[rand.Next(s.Length)]).ToArray());
		}

		public System.Random rand = new System.Random();

	}
}
