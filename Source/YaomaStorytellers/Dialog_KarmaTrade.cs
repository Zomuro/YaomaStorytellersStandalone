using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

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
			GUI.SetNextControlName("DebugFilter");
			if (Event.current.type == EventType.KeyDown && (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent || KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent))
			{
				return;
			}
			this.filter = Widgets.TextField(new Rect(0f, 0f, 200f, 30f), this.filter);
			if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.Repaint) && this.focusFilter)
			{
				GUI.FocusControl("DebugFilter");
				this.filter = "";
				this.focusFilter = false;
			}
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight = 0f;
			}

			// left half title for the potential incidents selectable
			Rect leftHalf = new Rect(inRect);
			leftHalf.yMin += 40f;
			Text.Font = GameFont.Medium;
			Widgets.Label(leftHalf, "KarmaTradeFates".Translate());

			// divider between left and right half of the window
			Color colorTemp = GUI.color;
			GUI.color = Color.grey;
			Widgets.DrawLineVertical(this.InitialSize.x * 1 / 2, 0, this.InitialSize.y);
			GUI.color = colorTemp;

			// create right half
			Rect rightHalf = new Rect(leftHalf);
			rightHalf.xMin = this.InitialSize.x * 1 / 2 + 16f;

			// right half title, showing selected incidents and karma calcs
			Widgets.Label(rightHalf, "KarmaTradeFatesSelected".Translate());

			// Incident selector section (options)
			Rect leftHalfOptions = new Rect(leftHalf);
			leftHalfOptions.yMin += 40f;
			Rect rightHalfOptions = new Rect(rightHalf);
			rightHalfOptions.yMin += 40f;

			int num = (int)(this.InitialSize.x / 200f);
			float num2 = (this.totalOptionsHeight + 24f * (float)(num - 1)) / (float)num;
			if (num2 < leftHalfOptions.height)
			{
				num2 = leftHalfOptions.height;
			}
			Rect rect = new Rect(0f, 0f, leftHalfOptions.width - 16f, num2);
			leftHalfOptions.width = this.InitialSize.x * 1 / 2 - 16f;
			leftHalfOptions.height = this.InitialSize.x - 110f;
			Widgets.BeginScrollView(leftHalfOptions, ref this.scrollPosition, rect, true);
			this.listing = new Listing_Standard(inRect, () => this.scrollPosition);
			this.listing.ColumnWidth = (rect.width - 17f * (float)(num - 1)) / (float)num;
			this.listing.Begin(rect);
			this.DoListingItems();
			this.listing.End();
			Widgets.EndScrollView();

			// right half subdivision (top, mid, bottom)
			rightHalf.yMin += 40f;
			Rect rightHalfTop = new Rect(rightHalf);
			rightHalfTop.yMax = rightHalf.yMin + rightHalf.height / 6;

			Rect rightHalfMid = new Rect(rightHalf);
			rightHalfMid.yMin = rightHalfTop.yMax;
			rightHalfMid.yMax = rightHalf.yMin + rightHalf.height * 5 / 6;

			Rect rightHalfBottom = new Rect(rightHalf);
			rightHalfBottom.yMin += rightHalf.height * 5/6;

			// divider between top/mid and mid/Bottom in the right half of the window
			colorTemp = GUI.color;
			GUI.color = Color.grey;
			Widgets.DrawLineHorizontal(rightHalf.x, rightHalf.yMin, rightHalf.width); // title/stuff divider
			Widgets.DrawLineHorizontal(rightHalfMid.x, rightHalfMid.yMin, rightHalfMid.width); // top/mid divider
			Widgets.DrawLineHorizontal(rightHalfMid.x, rightHalfMid.yMax, rightHalfMid.width); // mid/bottom divider
			GUI.color = colorTemp;

			// rightHalfMid image
			Widgets.DrawTextureFitted(rightHalfMid, KarmaRing, 0.5f);

			// shows current karma
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(rightHalfTop, "KarmaTradeOverallKarma".Translate(Math.Round(karmaTracker.Karma, 2),
				Math.Round(
					KarmaConstrain(karmaTracker.Karma + EstIncidentCost(karmaTracker)
					), 2)));

			// Selection review section
			Text.Font = GameFont.Tiny;
			Action action;
			Rect selectedOption = new Rect(rightHalfMid);
			selectedOption.x = rightHalfMid.center.x;
			selectedOption.y = rightHalfMid.center.y;
			selectedOption.width = 100;
			selectedOption.height = 50;

			string text = ""; 
			float angle = 270f;
			float increment = selected.Any() ? (360f / selected.Count()) : 0f;
			foreach (IncidentDef iDef in selected)
			{
				text = iDef.LabelCap.ToString() + " (" + Math.Round(karmaTracker.estIncidentCost[iDef], 2) + ")";
				if (Widgets.ButtonText(RectRadialPosition(selectedOption, rightHalfMid.width/3 + 20f, angle),
					text, true, true, true, TextAnchor.MiddleCenter))
				{
					action = delegate ()
					{
						selected.Remove(iDef);
						Messages.Message("MessageKarmaTradeRemoveSelected".Translate(iDef.label),
							MessageTypeDefOf.SilentInput, false);
					};
					action();
					break;
				}

				angle += increment;
			}

			// show number of incidents selected out of 5
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Medium;
			Widgets.Label(rightHalfMid, "KarmaTradeIncidentCount".Translate(selected.Count().ToString()));

			// Completion button - "Make offer"
			Text.Font = GameFont.Small;
			Rect offerButton = new Rect(rightHalfBottom);
			offerButton.width = 100;
			offerButton.height = 50;
			offerButton.x += rightHalfBottom.width / 2 - offerButton.width * 3/2;
			offerButton.y += rightHalfBottom.height / 2 - offerButton.height / 2;

			Rect rerollButton = new Rect(offerButton);
			rerollButton.x += 2*offerButton.width;

			if (karmaTracker.selectedIncidents.Count > 0)
			{
				if (Widgets.ButtonText(offerButton, "KarmaTradeMakeOffer".Translate(), true, true, true))
				{
					if (EstIncidentCost(karmaTracker) < 0 && karmaTracker.Karma < 0)
					{
						action = delegate ()
						{
							Tuple<Action, Action> actions = DebtResolutionDialogActions(karmaTracker);
							Find.WindowStack.Add(new Dialog_MessageBox("KarmaTradeDebtResolution".Translate(),
								"KarmaTradeDebtResolutionConfirm".Translate(), actions.Item2,
								"KarmaTradeDebtResolutionReturn".Translate(), actions.Item1,
								null, false, null, null, WindowLayer.Dialog));
						};
					}

					else action = delegate ()
					{
						SelectionInform(this.EstIncidentCost(karmaTracker), selected);
						karmaTracker.selectedIncidents = selected;
						//karmaTracker.karma += this.EstIncidentCost(karmaTracker);
						YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(EstIncidentCost(karmaTracker));
						karmaTracker.CompleteIncidentSelection(selected);
						Messages.Message("MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
						this.Close(true);
					};
					action();
				}

			}

			else
			{
				if (Widgets.ButtonText(offerButton, "KarmaTradeRejectTrade".Translate(), true, true, true))
				{
					SelectionInform(0, selected);

					Messages.Message("MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
					this.Close(true);
				}
			}

			// when rerolling selections
			float rerollCost = YaomaStorytellerUtility.settings.KaiyiKarmicRerollBaseCost * karmaTracker.CostFactor * rerollMult * -1f; // i.e. -20
			float karmaTillFloor = YaomaStorytellerUtility.settings.KaiyiKarmicKarmaMin - karmaTracker.Karma; // (-500 - 0 = -500; -500 - (-490) = -10)

			if (karmaTillFloor > rerollCost) // if the amount of karma to reach min karma > reroll cost, prevent rerolls
            {
				if (Widgets.ButtonText(rerollButton, "KarmaTradeNoMoreReroll".Translate(), true, true, Color.gray, true))
				{
					Messages.Message("MessageKarmaNoMoreReroll".Translate(), MessageTypeDefOf.SilentInput, false);
				}
			}
            else // allow rerolls
            {
				if (Widgets.ButtonText(rerollButton, "KarmaTradeReroll".Translate(rerollCost.ToString("F2")), true, true, true))
				{
					Log.Message(rerollCost.ToString());
					Log.Message(karmaTillFloor.ToString());

					// clear selected incidents, then reroll incidents you could select
					karmaTracker.selectedIncidents.Clear();
					YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(rerollCost);
					RefreshSelectableIncidents();
					
					rerollMult += 0.5f;
				}
			}
			
			//Text.Font = GameFont.Small;
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

		protected bool DebugActionTrade(string label, Action action, bool highlight)
		{
			bool result = false;
			if (!base.FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (this.listing.ButtonDebug(label, highlight))
			{
				//this.Close(true);
				action();
				result = true;
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				this.totalOptionsHeight += 24f;
			}
			return result;
		}

		public float EstIncidentCost(StorytellerComp_RandomKarmaMain kt)
		{
			if (selected.Count == 0) return 0;
			return (from i in selected select kt.estIncidentCost[i]).Sum();
		}

		public void DebtResolutionAdditions(StorytellerComp_RandomKarmaMain kt)
		{
			if (EstIncidentCost(kt) >= 0 || kt.Karma >= 0) return;

			List<IncidentDef> negOptions = (from x in kt.selectableIncidentCount
											where kt.baseIncidentCost[x.Key.category] > 0
											select x.Key).ToList();

			int count = (int)Math.Ceiling(kt.EstKarmaPointScaling(EstIncidentCost(kt)) - 1);
            while (count > 0)
            {
				IncidentDef randIDef = negOptions.RandomElement();
				selected.Add(randIDef);
				count--;
			}
		}

		public Tuple<Action, Action> DebtResolutionDialogActions(StorytellerComp_RandomKarmaMain kt)
		{
			if (EstIncidentCost(kt) >= 0 || kt.Karma >= 0) return null;

			Action actionReturn = delegate ()
			{
			};

			Action actionConfirm = delegate ()
			{
				this.SelectionInform(this.EstIncidentCost(karmaTracker), selected);
				YaomaStorytellerUtility.KaiyiKarmicAdjustKarma(EstIncidentCost(karmaTracker));
				//karmaTracker.karma += this.EstIncidentCost(karmaTracker);
				karmaTracker.CompleteIncidentSelection(selected);
				Messages.Message("MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
				DebtResolutionAdditions(kt);
				karmaTracker.selectedIncidents = selected;
				Log.Message(selected.ToStringSafeEnumerable());
				this.Close(true);
			};

			return new Tuple<Action,Action>(actionReturn, actionConfirm);
		}

		public void SelectionInform(float karmaChange, List<IncidentDef> selection)
		{
			string text = "";
            if (selection.NullOrEmpty())
            {
				Find.LetterStack.ReceiveLetter("LetterLabelKaiyiKarmicNoDeal".Translate(),
					"LetterKaiyiKarmicNoDeal".Translate(), LetterDefOf.NeutralEvent, null);
				return;
			}

			if (karmaChange >= 0) text = "LetterKaiyiKarmicDealDonePositive".Translate(Math.Abs(Math.Round(karmaChange, 2)));
			else text = "LetterKaiyiKarmicDealDoneNegative".Translate(Math.Abs(Math.Round(karmaChange, 2)));

			foreach (IncidentDef iDef in selection)
			{
				text += "\n" + iDef.LabelCap.ToString();
			}

			Find.LetterStack.ReceiveLetter("LetterLabelKaiyiKarmicDealDone".Translate(),
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

		protected Listing_Standard listing_Selected;

		public List<IncidentDef> selected = new List<IncidentDef>();

		public StorytellerComp_RandomKarmaMain karmaTracker;

		public bool confirmation = false;

		private Texture2D KarmaRing;
	}
}
