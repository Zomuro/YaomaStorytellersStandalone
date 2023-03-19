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
			this.selectableCache = options.ToList();
			this.options = SelectableIncidents;
			this.KarmaRing = ContentFinder<Texture2D>.Get("UI/Dialogs/KaiyiKarmicRing", true);
			this.karmaTracker = Find.Storyteller.storytellerComps.FirstOrDefault(x =>
						x is StorytellerComp_KarmaTracker) as StorytellerComp_KarmaTracker;
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
			Widgets.Label(rightHalfTop, "KarmaTradeOverallKarma".Translate(Math.Round(karmaTracker.karma, 2),
				Math.Round(karmaTracker.karma + estIncidentCost(karmaTracker), 2)));

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
			offerButton.x += rightHalfBottom.width / 2 - offerButton.width / 2;
			offerButton.y += rightHalfBottom.height / 2 - offerButton.height / 2;

			if (Widgets.ButtonText(offerButton, "KarmaTradeMakeOffer".Translate(), true, true, true))
			{
				if(estIncidentCost(karmaTracker) < 0 && karmaTracker.karma < 0)
                {
					action = delegate ()
					{
						Tuple<Action, Action> actions = debtResolutionDialogActions(karmaTracker);
						Find.WindowStack.Add(new Dialog_MessageBox("KarmaTradeDebtResolution".Translate(),
							"KarmaTradeDebtResolutionConfirm".Translate(), actions.Item2,
							"KarmaTradeDebtResolutionReturn".Translate(), actions.Item1, 
							null, false, null, null, WindowLayer.Dialog));
					};
				}

				else action = delegate ()
				{
					this.selectionInform(this.estIncidentCost(karmaTracker), selected);
					karmaTracker.selectedIncidents = selected;
					karmaTracker.karma += this.estIncidentCost(karmaTracker);
					karmaTracker.completeIncidentSelection(selected);
					Messages.Message("MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
					this.Close(true);
				};
				action();
				//this.Close(true);
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

		public float estIncidentCost(StorytellerComp_KarmaTracker kt)
		{
			if (selected.Count == 0) return 0;
			return (from i in selected select kt.estIncidentCost[i]).Sum();
		}

		public void debtResolutionAdditions(StorytellerComp_KarmaTracker kt)
		{
			if (estIncidentCost(kt) >= 0 || kt.karma >= 0) return;

			List<IncidentDef> negOptions = (from x in kt.selectableIncidentCount
											where kt.baseIncidentCost[x.Key.category] > 0
											select x.Key).ToList();

			int count = (int)Math.Ceiling(kt.estKarmaPointScaling(estIncidentCost(kt)) - 1);
            while (count > 0)
            {
				IncidentDef randIDef = negOptions.RandomElement();
				selected.Add(randIDef);
				count--;
			}
		}

		public Tuple<Action, Action> debtResolutionDialogActions(StorytellerComp_KarmaTracker kt)
		{
			if (estIncidentCost(kt) >= 0 || kt.karma >= 0) return null;

			Action actionReturn = delegate ()
			{
			};

			Action actionConfirm = delegate ()
			{
				this.selectionInform(this.estIncidentCost(karmaTracker), selected);
				karmaTracker.karma += this.estIncidentCost(karmaTracker);
				karmaTracker.completeIncidentSelection(selected);
				Messages.Message("MessageKarmaTradeEnd".Translate(), MessageTypeDefOf.SilentInput, false);
				debtResolutionAdditions(kt);
				karmaTracker.selectedIncidents = selected;
				Log.Message(selected.ToStringSafeEnumerable());
				this.Close(true);
			};

			return new Tuple<Action,Action>(actionReturn, actionConfirm);
		}

		public void selectionInform(float karmaChange, List<IncidentDef> selection)
		{
			string text = "";
			if (karmaChange >= 0) text = "LetterKaiyiKarmicDealDonePositive".Translate(Math.Abs(Math.Round(karmaChange, 2)));
			else text = "LetterKaiyiKarmicDealDoneNegative".Translate(Math.Abs(Math.Round(karmaChange, 2)));

			foreach (IncidentDef iDef in selection)
			{
				text += "\n" + iDef.LabelCap.ToString();
			}

			Find.LetterStack.ReceiveLetter("LetterLabelKaiyiKarmicDealDone".Translate(),
				text, LetterDefOf.NeutralEvent, null);
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
			selectableCache = null;
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

		private bool focusFilter;

		private List<DebugMenuOption> selectableCache;

		protected Listing_Standard listing_Selected;

		public List<IncidentDef> selected = new List<IncidentDef>();

		public StorytellerComp_KarmaTracker karmaTracker;

		public bool confirmation = false;

		private Texture2D KarmaRing;
	}
}
