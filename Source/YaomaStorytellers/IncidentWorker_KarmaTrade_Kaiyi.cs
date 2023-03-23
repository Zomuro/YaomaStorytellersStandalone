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
			karmaTracker.RefreshIncidentCosts();

			// rework this so that the dialog itself handles this:
			// allows it to reroll the incidents by itself
			/*List<DebugMenuOption> selectables_f = new List<DebugMenuOption>();
			String labelCost = "";


			foreach (IncidentDef iDef in karmaTracker.selectableIncidentCount.Keys.Where(x => this.selectableIncidents(x))
				.OrderByDescending(x => karmaTracker.estIncidentCost[x])
				.ThenBy(x => x.LabelCap.ToString()))
			{
				labelCost = iDef.LabelCap.ToString() + " (" + Math.Round(karmaTracker.estIncidentCost[iDef], 2) + ")";

				selectables_f.Add(new DebugMenuOption(labelCost, DebugMenuOptionMode.Action, delegate ()
				{
					IncidentParms parmSim = StorytellerUtility.DefaultParmsNow(iDef.category, Find.AnyPlayerHomeMap);
					if (iDef.pointsScaleable)
					{
						parmSim = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle ||
							x is StorytellerComp_RandomMain).GenerateParms(iDef.category, parmSim.target);
					}

					List<IncidentDef> incidentsSelected = (Find.WindowStack.currentlyDrawnWindow as Dialog_KarmaTrade).selected;

					if (incidentsSelected.Count < 5)
					{
						incidentsSelected.Add(iDef);
						Messages.Message("MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
					}
					else Messages.Message("MessageKaiyiKarmicIncidentsFilled".Translate(), MessageTypeDefOf.RejectInput, false);
				}));
			}*/

			//Find.WindowStack.Add(new Dialog_KarmaTrade(selectables_f));

			List<DebugMenuOption> selectables_f = new List<DebugMenuOption>();
			YaomaStorytellerUtility.KaiyiKarmicSelectableIncidents(ref selectables_f, karmaTracker);
			Find.WindowStack.Add(new Dialog_KarmaTrade(selectables_f));

			//Find.TickManager.Pause();
			String translateString = "KaiyiKarmicDialogueIntro" + Rand.RangeInclusive(1, 5).ToString();
			Find.WindowStack.Add(new Dialog_MessageBox(translateString.Translate(),
				null, null, null, null, null, false, null, null, WindowLayer.Dialog));

			base.SendStandardLetter("LetterLabelKaiyiKarmic".Translate(),
				"LetterKaiyiKarmic".Translate(),
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
