using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
	public class IncidentWorker_KarmaTrade_Kaiyi : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			karmaTracker = Find.Storyteller.storytellerComps.FirstOrDefault(x =>
						x is StorytellerComp_RandomKarmaMain) as StorytellerComp_RandomKarmaMain;
			if (karmaTracker == null) return false;
			
			// refresh costs if settings for costFactor was changed in between
			karmaTracker.RefreshIncidentChange();

			List<DebugMenuOption> selectables_f = new List<DebugMenuOption>();
			YaomaStorytellerUtility.KaiyiKarmicSelectableIncidents(ref selectables_f, karmaTracker);
			Find.WindowStack.Add(new Dialog_KarmaTrade(selectables_f));

			String translateString = "YS_KaiyiKarmicDialogueIntro" + Rand.RangeInclusive(1, 5).ToString();
			Find.WindowStack.Add(new Dialog_MessageBox(translateString.Translate(),
				null, null, null, null, null, false, null, null, WindowLayer.Dialog));

			base.SendStandardLetter("YS_LetterLabelKaiyiKarmic".Translate(),
				"YS_LetterKaiyiKarmic".Translate(),
				LetterDefOf.PositiveEvent, parms, null,
				Array.Empty<NamedArgument>());
			
			return true;
		}

		public bool selectableIncidents(IncidentDef incident)
		{
			if (incident != IncidentDefOf_Yaoma.Resurrection_Yaoma && incident != IncidentDefOf_Yaoma.KarmaTrade_Yaoma &&
				incident.TargetAllowed(Find.CurrentMap) &&
				incident.Worker.CanFireNow(StorytellerUtility.DefaultParmsNow(incident.category, Find.CurrentMap))) return true;
			return false;
		}

		public StorytellerComp_RandomKarmaMain karmaTracker;
	}
}
