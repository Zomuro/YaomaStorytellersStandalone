using System;
using System.Linq;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
	public class IncidentWorker_Terraform_Jin : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (Find.Storyteller.def != StorytellerDefOf.JianghuJin_Yaoma) return false;
			return true; // adjust so that it only fires when Jin is the storyteller
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = Find.AnyPlayerHomeMap;
			if (map is null) return false;

			foreach (var step in MapGeneratorDefOf_Yaoma.YS_JianghuJin_RefreshTerrain.genSteps.OrderBy(x => x.order))
			{
				DeepProfiler.Start(step.genStep.def.defName);
				step.genStep.Generate(map, default(GenStepParams));
				DeepProfiler.End();
			}
			map.FinalizeInit();

			base.SendStandardLetter("YS_LetterLabelJianghuJin".Translate(), "YS_LetterJianghuJin".Translate(),
				LetterDefOf.NeutralEvent, parms, null, Array.Empty<NamedArgument>());

			return true;

		}

		// used to alter world seedstring to get new tile results - hold for now
		public string RandomString(int length)
		{
			/*string seed = Find.World.info.seedString;
			Log.Message("original seed: " + seed);

			Find.World.info.seedString = RandomString(rand.Next(4, 10));
			Log.Message("new seed: " + Find.World.info.seedString);*/
			//Find.World.info.seedString = seed;
			//Log.Message(step.genStep.def.defName);

			const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-+<>";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[rand.Next(s.Length)]).ToArray());
		}



		public System.Random rand = new System.Random();

	}
}
