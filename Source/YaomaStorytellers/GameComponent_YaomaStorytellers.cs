using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class GameComponent_YaomaStorytellers: GameComponent
    {
		// Kaiyi the Karmic fields

		public float karma = 0f;

		public float cachedCostFactor = 1f;

		public Dictionary<IncidentCategoryDef, float> baseIncidentChange = new Dictionary<IncidentCategoryDef, float>();

		public Dictionary<IncidentDef, int> selectableIncidentCount = new Dictionary<IncidentDef, int>();

		public Dictionary<IncidentDef, float> estIncidentChange = new Dictionary<IncidentDef, float>();

		public List<IncidentDef> selectedIncidents = new List<IncidentDef>();

		public List<IncidentCategoryDef> alteredCats = new List<IncidentCategoryDef>();

		public int daysCheck = 0;

		private bool initKarma = false;

		// Jianghu Jin field

		public int regularDays = 0;

		public GameComponent_YaomaStorytellers(Game game)
		{
		}

		public override void ExposeData()
		{
			Scribe_Values.Look<float>(ref karma, "karma", 0);
			Scribe_Values.Look<float>(ref cachedCostFactor, "cachedCostFactor", 1, false);
			Scribe_Collections.Look(ref baseIncidentChange, "baseIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectableIncidentCount, "selectableIncidentCount", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref estIncidentChange, "estIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectedIncidents, "selectedIncidents", LookMode.Def);
			Scribe_Values.Look<int>(ref daysCheck, "daysCheck", 0, false);
			Scribe_Values.Look<bool>(ref initKarma, "initKarma", false);
			Scribe_Values.Look<int>(ref regularDays, "regularDays", 0);
		}

		public override void StartedNewGame()
		{
			base.StartedNewGame();
			KaiyiInitialize();
		}

		public void KaiyiInitialize()
		{
			if (!initKarma)
			{
				initKarma = true;
				karma = Settings.KaiyiKarmicKarma;
			}

			KaiyiSetupIncidentTables();
		}

		public override void LoadedGame()
		{
			base.LoadedGame();

			KaiyiSetupIncidentTables();
		}

		public void KaiyiSetupIncidentTables()
        {
			if (KarmaMain is null) return;
			baseIncidentChange = KarmaMain.baseIncidentCategoryKarmaChange.ToDictionary(x => x.def, x => x.change);

			// if there are new incidentcategorydefs that are not defined in the properties list, add them with base cost 2 (treat like misc)
			foreach (IncidentCategoryDef i in DefDatabase<IncidentCategoryDef>.AllDefs)
			{
				if (!baseIncidentChange.Keys.Contains(i)) baseIncidentChange.Add(i, 2);
			}

			// for all the incidentdefs, if the incident def indeed has the category in baseincidentcost
			// AND the incident diff isn't already in the list, we add it to the count list
			foreach (IncidentDef i in DefDatabase<IncidentDef>.AllDefs)
			{
				if (baseIncidentChange.Keys.Contains(i.category) &&
					!selectableIncidentCount.Keys.Contains(i)) selectableIncidentCount.Add(i, 1);
			}

			// we then set the estIncidentCost:
			// if the incidentdef isn't in there, we add it in as base cost
			// otherwise, it isn't touched at all
			foreach (IncidentDef i in selectableIncidentCount.Keys)
			{
				if (!estIncidentChange.ContainsKey(i)) estIncidentChange.Add(i, baseIncidentChange[i.category] * CostFactor);
			}
			cachedCostFactor = CostFactor;
		}

		public float CostFactor
		{
			get
			{
				return Settings.KaiyiKarmicBasePriceFactor;
			}
		}

		public StorytellerCompProperties_RandomKarmaMain KarmaMain
        {
            get
            {
				return StorytellerDefOf.KaiyiKarmic_Yaoma.comps.FirstOrDefault(x => x.GetType() == typeof(StorytellerCompProperties_RandomKarmaMain)) as StorytellerCompProperties_RandomKarmaMain;
			}
        }

		public YaomaStorytellerSettings Settings
		{
			get
			{
				return YaomaStorytellerUtility.settings;
			}
		}
	}
}
