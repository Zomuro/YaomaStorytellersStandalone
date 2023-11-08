using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

			// pre-emptively destroy hives, prior to the mapgen to avoid a thread error
			List<Thing> hives = map.listerThings.ThingsOfDef(ThingDefOf.Hive).ToList();
			foreach (var hive in hives) hive.Destroy();

			// display when the mapgen is loading
			// disabled asynchronous- while async is nicer looking, this should avoid the issues with threading
			LongEventHandler.QueueLongEvent(delegate ()
			{
				foreach (var step in MapGeneratorDefOf_Yaoma.YS_JianghuJin_RefreshTerrain.genSteps.OrderBy(x => x.order))
				{
					DeepProfiler.Start(step.genStep.def.defName);
					step.genStep.Generate(map, default(GenStepParams));
					DeepProfiler.End();
				}

				map.FinalizeInit();
			}, "YS_JinTerraformMapPage", false, new Action<Exception>(GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap), true);

			base.SendStandardLetter("YS_LetterLabelJianghuJin".Translate(), "YS_LetterJianghuJin".Translate(),
				LetterDefOf.NeutralEvent, parms, null, Array.Empty<NamedArgument>());

			return true;

		}
	}
}
