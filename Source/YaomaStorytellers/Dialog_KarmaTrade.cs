using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    public class Dialog_KarmaTrade : Dialog_DebugOptionListLister
	{
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(960, 720);
			}
		}

		public override bool IsDebug
		{
			get
			{
				return false;
			}
		}

		public Dialog_KarmaTrade(IEnumerable<DebugMenuOption> options) : base(options)
		{
			selectableCache = options.ToList();
			options = SelectableIncidents;
			KarmaRing = ContentFinder<Texture2D>.Get("UI/Dialogs/KaiyiKarmicRing", true);
			karmaTracker = Find.Storyteller.storytellerComps.FirstOrDefault(x =>
						x is StorytellerComp_RandomKarmaMain) as StorytellerComp_RandomKarmaMain;
			closeOnClickedOutside = false;
			forcePause = true;
			closeOnCancel = false;
			doCloseX = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			// nullchecks karma tracker
			if (karmaTracker == null) return;

			// filter section (taken from Dialog_DebugOptionListLister_
			GUI.SetNextControlName("FilterIncidents");
			if (Event.current.type == EventType.KeyDown && (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent || KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent))
			{
				return;
			}
			filter = Widgets.TextField(new Rect(0f, 0f, 200f, 30f), this.filter);
			if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && this.focusFilter)
			{
				GUI.FocusControl("FilterIncidents");
				filter = "";
				focusFilter = false;
			}
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight = 0f;
			}

			Rect availRect = new Rect(inRect);
			availRect.yMin += 40f;

			// Rect to show all incident choices to choose from
			Rect allOptionsRect = new Rect(availRect);
			allOptionsRect.width = availRect.width / 3;
			allOptionsRect = allOptionsRect.ContractedBy(3f);
			Widgets.DrawBoxSolidWithOutline(allOptionsRect, Color.clear, Color.gray);

			// Rect for pricing breakdown
			Rect pricingRect = new Rect(availRect);
			pricingRect.x += availRect.width / 3;
			pricingRect.height = (int) availRect.height / 3;
			pricingRect.width = availRect.width / 3;
			pricingRect = pricingRect.ContractedBy(3f);
			Widgets.DrawBoxSolidWithOutline(pricingRect, Color.clear, Color.gray);

			// Rect for final trade decision
			Rect tradeRect = new Rect(availRect);
			tradeRect.x += availRect.width * 2 / 3;
			tradeRect.height = (int)availRect.height / 3;
			tradeRect.width = availRect.width / 3;
			tradeRect = tradeRect.ContractedBy(3f);
			Widgets.DrawBoxSolidWithOutline(tradeRect, Color.clear, Color.gray);

			// Rect for selected incidents
			Rect selectedRect = new Rect(availRect);
			selectedRect.xMin += availRect.width / 3;
			selectedRect.yMin += (int) availRect.height / 3;
			selectedRect = selectedRect.ContractedBy(3f);
			Widgets.DrawBoxSolidWithOutline(selectedRect, Color.clear, Color.gray);

			// Title for listing of incidents
			Text.Font = GameFont.Medium;
			Rect allOptionsRectTitle = allOptionsRect.ContractedBy(5f);
			allOptionsRectTitle.x += 5;
			Widgets.Label(allOptionsRectTitle, "YS_KarmaTradeFates".Translate());
			Text.Font = GameFont.Small;

			// Listing that shows all the incidents that could be selected
			Rect allOptionsRectListing = new Rect(allOptionsRect).ContractedBy(10f);
			allOptionsRectListing.yMin += 30f;
			float allOptionsHeight = totalOptionsHeight;
			if (allOptionsHeight < allOptionsRectListing.height)
			{
				allOptionsHeight = allOptionsRectListing.height;
			}
			Rect allOptionsRectView = new Rect(allOptionsRectListing.x, allOptionsRectListing.y, allOptionsRectListing.width - 16f, allOptionsHeight);
			Widgets.BeginScrollView(allOptionsRectListing, ref scrollPosition, allOptionsRectView, true);
			listing = new Listing_Standard(inRect, () => scrollPosition);
			listing.ColumnWidth = (allOptionsRectListing.width - 18f);
			listing.Begin(allOptionsRectView);
			DoListingItems();
			listing.End();
			Widgets.EndScrollView();

			// Title for pricing breakdown
			Text.Font = GameFont.Medium;
			Rect pricingRectTitle = pricingRect.ContractedBy(5f);
			pricingRectTitle.x += 5;
			Widgets.Label(pricingRectTitle, "YS_KarmaTradePricing".Translate());
			Text.Font = GameFont.Small;

			// Listing that shows all the pricing breakdown
			Rect pricingRectListing = new Rect(pricingRect).ContractedBy(10f);
			pricingRectListing.yMin += 30f;
			Rect pricingRectListingView = new Rect(pricingRectListing);
			pricingRectListingView.width -= 16f;
			pricingRectListingView.height = pricingListingHeight;

			Widgets.BeginScrollView(pricingRectListing, ref scrollPositionPricing, pricingRectListingView, true);
			listing_Pricing = new Listing_Standard(inRect, () => scrollPositionPricing);
			listing_Pricing.ColumnWidth = (int) (pricingRectListingView.width);
			listing_Pricing.Begin(pricingRectListing);
			listing_Pricing.LabelDouble("YS_KarmaTradeCurrKarma".Translate(), karmaTracker.GameComp.karma.ToString("F2"));
			Text.Font = GameFont.Tiny;
			foreach (var i in selected)
            {
				listing_Pricing.LabelDouble(i.incidDef.LabelCap, ConvertToCurrency(karmaTracker.GameComp.estIncidentChange[i.incidDef]));
			}
			Text.Font = GameFont.Small;
			listing_Pricing.GapLine();
			listing_Pricing.LabelDouble("YS_KarmaTradeFinalKarma".Translate(), KarmaConstrain(karmaTracker.GameComp.karma + EstIncidentChange(karmaTracker)).ToString("F2"));
			listing_Pricing.End();
			pricingListingHeight = listing_Pricing.CurHeight;
			Widgets.EndScrollView();
			

			// Title for selected incidents
			Text.Font = GameFont.Medium;
			Rect selectedRectTitle = selectedRect.ContractedBy(5f);
			selectedRectTitle.x += 5;
			Widgets.Label(selectedRectTitle, "YS_KarmaTradeCurrentDeal".Translate());
			Text.Font = GameFont.Small;

			// interface to show and change selected incidents
			Rect selectedRectInterface = new Rect(selectedRect).ContractedBy(10f);
			selectedRectInterface.yMin += 30f;

			// draw karma ring textures/images
			Widgets.DrawTextureFitted(selectedRectInterface, KarmaRing, 0.2f); // inner ring, to surround the counter
			Widgets.DrawTextureFitted(selectedRectInterface, KarmaRing, 0.6f, 
				new Vector2((float)KarmaRing.width, (float)KarmaRing.height), new Rect(0f, 0f, 1f, 1f), 120f, null); // decorative
			Widgets.DrawTextureFitted(selectedRectInterface, KarmaRing, 1f, 
				new Vector2((float)KarmaRing.width, (float)KarmaRing.height), new Rect(0f, 0f, 1f, 1f), 240f, null); // located approx where the selected incidents are

			// Selection review section
			Text.Font = GameFont.Tiny;
			Action action;
			Rect selectedOption = new Rect(selectedRectInterface);
			selectedOption.x = selectedRectInterface.center.x;
			selectedOption.y = selectedRectInterface.center.y;
			selectedOption.width = 100;
			selectedOption.height = 50;

			string text = "";
			float angle = 270f;
			float increment = selected.Any() ? (360f / selected.Count()) : 0f;
			foreach (KaiyiIncidentRecord i in selected)
			{
				text = i.incidDef.LabelCap.ToString() + " (" + karmaTracker.GameComp.estIncidentChange[i.incidDef].ToString("F2") + ")";
				if (Widgets.ButtonText(RectRadialPosition(selectedOption, selectedRectInterface.height / 3 + 30f, angle),
					text, true, true, true, TextAnchor.MiddleCenter))
				{
					action = delegate ()
					{
						selected.Remove(i); // remove from list
						options.Add(CreateOption(i.incidDef)); // add selection back... at the end of the list.
						Messages.Message("YS_MessageKarmaTradeRemoveSelected".Translate(i.incidDef.label),
							MessageTypeDefOf.SilentInput, false);
					};
					action();
					break;
				}

				angle += increment;
			}

			// show count of incidents selected
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(selectedRectInterface, "YS_KarmaTradeIncidentCount".Translate(selected.Count().ToString()));

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;

			// Title for finishing up trades
			Text.Font = GameFont.Medium;
			Rect tradeRectTitle = tradeRect.ContractedBy(5f);
			tradeRectTitle.x += 5;
			Widgets.Label(tradeRectTitle, "YS_KarmaTradeActions".Translate());
			Text.Font = GameFont.Small;

			// Buttons for completing trade (and potentially rerolling)
			Text.Font = GameFont.Small;
			Rect offerButton = new Rect(tradeRect);
			offerButton.width = 100;
			offerButton.height = 50;

			offerButton.x += tradeRect.width / 2 - offerButton.width * 1.25f;
			offerButton.y += tradeRect.height / 2 - offerButton.height / 2;

			if (YaomaStorytellerUtility.settings.KaiyiKarmicRerollIncidents) offerButton.y -= offerButton.height / 2;
			if (selected.Count > 0)
			{
				if (Widgets.ButtonText(offerButton, "YS_KarmaTradeMakeOffer".Translate(), true, true, true, TextAnchor.MiddleCenter))
				{
					if (EstIncidentChange(karmaTracker) < 0 && karmaTracker.GameComp.karma < 0)
					{
						action = delegate ()
						{
							Tuple<Action, Action> actions = DebtResolutionDialogActions(karmaTracker);
							Find.WindowStack.Add(new Dialog_MessageBox("YS_KarmaTradeDebtResolution".Translate(),
								"YS_KarmaTradeDebtResolutionConfirm".Translate(), actions.Item2,
								"YS_KarmaTradeDebtResolutionReturn".Translate(), actions.Item1,
								null, false, null, null, WindowLayer.Dialog));
						};
					}
					else action = delegate ()
					{
						SelectionInform(EstIncidentChange(karmaTracker), selected);
						karmaTracker.GameComp.selectedIncidents = selected;
						YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(karmaTracker, EstIncidentChange(karmaTracker));
						karmaTracker.CompleteIncidentSelection(selected.Select(x => x.incidDef).ToList());
						Messages.Message("YS_MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
						this.Close(true);
					};
					action();
				}

			}
			else
			{
				if (Widgets.ButtonText(offerButton, "YS_KarmaTradeRejectTrade".Translate(), true, true, true, TextAnchor.MiddleCenter))
				{
					SelectionInform(0, selected);
					Messages.Message("YS_MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
					this.Close(true);
				}
			}

			// clear out all selected incidents in Dialog
			Rect clearButton = new Rect(offerButton);
			clearButton.x += clearButton.width * 1.5f;
			if (Widgets.ButtonText(clearButton, "YS_KarmaTradeClearSelections".Translate(), true, true, true, TextAnchor.MiddleCenter))
			{
				foreach (var i in selected) options.Add(CreateOption(i.incidDef)); // add selection back... at the end of the list.
				selected.Clear();

				Messages.Message("YS_MessageKarmaTradeRemoveAllSelect".Translate(), MessageTypeDefOf.SilentInput, false);
			}

			if (!YaomaStorytellerUtility.settings.KaiyiKarmicRerollIncidents) // end method early when rerolls aren't allowed
			{
				Text.Anchor = TextAnchor.UpperLeft;
				return;
			}

			// setup reroll button
			Rect rerollButton = new Rect(offerButton);
			rerollButton.x = tradeRect.x + tradeRect.width / 2 - offerButton.width * 0.5f;
			rerollButton.y += 1.5f * offerButton.height;

			// when rerolling selections
			float rerollCost = YaomaStorytellerUtility.settings.KaiyiKarmicRerollBaseCost * karmaTracker.CostFactor * rerollMult * -1f; // i.e. -20
			float karmaTillFloor = YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMin - karmaTracker.GameComp.karma; // (-500 - 0 = -500; -500 - (-490) = -10)
			if (karmaTillFloor > rerollCost) // if the amount of karma to reach min karma > reroll cost, prevent rerolls
			{
				if (Widgets.ButtonText(rerollButton, "YS_KarmaTradeNoMoreReroll".Translate(), true, true, Color.gray, true, TextAnchor.MiddleCenter))
				{
					Messages.Message("YS_MessageKarmaNoMoreReroll".Translate(), MessageTypeDefOf.SilentInput, false);
				}
			}
			else // allow rerolls
			{
				if (Widgets.ButtonText(rerollButton, "YS_KarmaTradeReroll".Translate(rerollCost.ToString("F2")), true, true, true, TextAnchor.MiddleCenter))
				{
					// clear selected incidents in Dialog, then reroll incidents you could select
					selected.Clear();
					YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(karmaTracker, rerollCost);
					RefreshSelectableIncidents();
					rerollMult += 0.5f;
				}
			}

			Text.Anchor = TextAnchor.UpperLeft;
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.IsDownEvent)
			{
				this.ChangeHighlightedOption();
			}
			int highlightedIndex = this.HighlightedIndex;
			for (int i = 0; i < this.options.Count; i++)
			{
				DebugMenuOption debugMenuOption = this.options[i];
				bool highlight = highlightedIndex == i;
				if (debugMenuOption.mode == DebugMenuOptionMode.Action)
				{
					this.DebugActionTrade(debugMenuOption.label, debugMenuOption.method, highlight);
				}
				if (debugMenuOption.mode == DebugMenuOptionMode.Tool)
				{
					base.DebugToolMap(debugMenuOption.label, debugMenuOption.method, highlight);
				}
			}
		}

		public DebugMenuOption CreateOption(IncidentDef iDef, int index = 0)
        {
			String labelCost = iDef.LabelCap.ToString() + " (" + Math.Round(karmaTracker.GameComp.estIncidentChange[iDef], 2) + ")";

			Action action = delegate ()
			{
				//Dialog_KarmaTrade karmaDialog = (Find.WindowStack.currentlyDrawnWindow as Dialog_KarmaTrade);
				//List<KaiyiIncidentRecord> incidentsSelected = karmaDialog.selected;

				List<KaiyiIncidentRecord> incidentsSelected = selected;

				if (incidentsSelected.Count < 5)
				{
					float estFinalVal = karmaTracker.GameComp.karma + EstIncidentChange(karmaTracker) + karmaTracker.GameComp.estIncidentChange[iDef];
					if (estFinalVal <= YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMin)
					{
						Messages.Message("YS_MessageKaiyiKarmicIncidentDebtFloor".Translate(), MessageTypeDefOf.SilentInput, false);
					}
					else if (estFinalVal >= YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMax)
					{
						//incidentsSelected.Add(iDef);
						incidentsSelected.Add(new KaiyiIncidentRecord() { incidDef = iDef, 
							change = karmaTracker.GameComp.estIncidentChange[iDef]});

						RemoveSelectableIncident(iDef);

						// add portion that removes from option list this incident record or something
						Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
						Messages.Message("YS_MessageKaiyiKarmicIncidentReachedMax".Translate(), MessageTypeDefOf.SilentInput, false);
					}
					else
					{
						//incidentsSelected.Add(iDef);
						incidentsSelected.Add(new KaiyiIncidentRecord() { incidDef = iDef, 
							change = karmaTracker.GameComp.estIncidentChange[iDef]});
						RemoveSelectableIncident(iDef);

						// add portion that removes from option list this incident record or something
						Messages.Message("YS_MessageKaiyiKarmicIncidentNum".Translate(incidentsSelected.Count.ToString()), MessageTypeDefOf.SilentInput, false);
					}

				}
				else Messages.Message("YS_MessageKaiyiKarmicIncidentsFilled".Translate(), MessageTypeDefOf.RejectInput, false);
			};

			return new DebugMenuOption(labelCost, DebugMenuOptionMode.Action, action);
		}

		public string ConvertToCurrency(float karma)
        {
			if (karma > 0) return ("+" + karma.ToString("F2"));
			else if (karma < 0) return (karma.ToString("F2"));
			else return "";
        }

		protected bool DebugActionTrade(string label, Action action, bool highlight)
		{
			bool result = false;
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (this.listing.ButtonDebug(label, highlight))
			{
				action();
				result = true;
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += 24f;
			}
			return result;
		}

		public float EstIncidentChange(StorytellerComp_RandomKarmaMain kt)
		{
			if (selected.Count == 0) return 0;
			return (from i in selected select kt.GameComp.estIncidentChange[i.incidDef]).Sum();
		}

		public void DebtResolutionAdditions(StorytellerComp_RandomKarmaMain kt)
		{
			if (EstIncidentChange(kt) >= 0 || kt.GameComp.karma >= 0) return;

			List<IncidentDef> negOptions = (from x in kt.GameComp.selectableIncidentCount
											where kt.GameComp.baseIncidentChange[x.Key.category] > 0
											select x.Key).ToList();

			int count = (int)Math.Ceiling(kt.EstKarmaPointScaling(EstIncidentChange(kt)) - 1);
            while (count > 0)
            {
				IncidentDef randIDef = negOptions.RandomElement();
				selected.Add(new KaiyiIncidentRecord(){incidDef = randIDef, change = kt.GameComp.estIncidentChange[randIDef]});
				count--;
			}
		}

		public Tuple<Action, Action> DebtResolutionDialogActions(StorytellerComp_RandomKarmaMain kt)
		{
			if (EstIncidentChange(kt) >= 0 || kt.GameComp.karma >= 0) return null;

			Action actionReturn = delegate ()
			{
			};

			Action actionConfirm = delegate ()
			{
				SelectionInform(EstIncidentChange(karmaTracker), selected);
				YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(karmaTracker, EstIncidentChange(karmaTracker));
				karmaTracker.CompleteIncidentSelection(selected.Select(x=>x.incidDef).ToList());
				Messages.Message("YS_MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
				DebtResolutionAdditions(kt);
				karmaTracker.GameComp.selectedIncidents = selected;
				this.Close(true);
			};

			return new Tuple<Action,Action>(actionReturn, actionConfirm);
		}

		public void SelectionInform(float karmaChange, List<KaiyiIncidentRecord> selection)
		{
			string text = "";
            if (selection.NullOrEmpty())
            {
				Find.LetterStack.ReceiveLetter("YS_LetterLabelKaiyiKarmicNoDeal".Translate(),
					"YS_LetterKaiyiKarmicNoDeal".Translate(), LetterDefOf.NeutralEvent, null);
				return;
			}

			if (karmaChange >= 0) text = "YS_LetterKaiyiKarmicDealDonePositive".Translate(Math.Abs(Math.Round(karmaChange, 2)));
			else text = "YS_LetterKaiyiKarmicDealDoneNegative".Translate(Math.Abs(Math.Round(karmaChange, 2)));

			foreach (IncidentDef iDef in selection.Select(x=> x.incidDef))
			{
				text += "\n" + iDef.LabelCap.ToString();
			}

			Find.LetterStack.ReceiveLetter("YS_LetterLabelKaiyiKarmicDealDone".Translate(),
				text, LetterDefOf.NeutralEvent, null);
		}

		public float KarmaConstrain(float value)
        {
			return Math.Max(YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMin, Math.Min(value, YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMax));
        }

		public Rect RectRadialPosition(Rect rect , float mag, float angle)
        {
			float direction = angle * Mathf.PI / 180;
			Rect output = new Rect(rect);
			output.x = rect.x + (mag * Mathf.Cos(direction)) - rect.width/2;
			output.y = rect.y + (mag * Mathf.Sin(direction)) - rect.height/2;
			return output;
		}

		public void RefreshSelectableIncidents()
        {
			selectableCache = new List<DebugMenuOption>();
			YaomaStorytellerUtility.KaiyiKarmicSelectableIncidents(ref selectableCache, karmaTracker);
			options = SelectableIncidents;
		}

		public void RemoveSelectableIncident(IncidentDef def)
        {
			String labelCost = def.LabelCap.ToString() + " (" + Math.Round(karmaTracker.GameComp.estIncidentChange[def], 2) + ")";
			DebugMenuOption removeTarget = options.First(x => x.label == labelCost);
			options.Remove(removeTarget);
		}

		public List<DebugMenuOption> SelectableIncidents
        {
            get
            {
				if (selectableCache is null)
                {
					YaomaStorytellerUtility.KaiyiKarmicSelectableIncidents(ref selectableCache, karmaTracker);
				}
				return selectableCache;
            }
        }

		private float rerollMult = 1f;

		private bool focusFilter;

		private List<DebugMenuOption> selectableCache;

		protected Vector2 scrollPositionPricing;

		public float pricingListingHeight;

		protected Listing_Standard listing_Selected;

		protected Listing_Standard listing_Pricing;

		public List<KaiyiIncidentRecord> selected = new List<KaiyiIncidentRecord>();

		public StorytellerComp_RandomKarmaMain karmaTracker;

		public bool confirmation = false;

		private Texture2D KarmaRing;
	}
}
