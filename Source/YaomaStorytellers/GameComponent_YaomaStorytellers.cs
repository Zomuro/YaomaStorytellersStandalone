using RimWorld;
using System.Collections.Generic;
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

		public List<KaiyiIncidentRecord> selectedIncidents = new List<KaiyiIncidentRecord>();

		public List<IncidentCategoryDef> alteredCats = new List<IncidentCategoryDef>();

		public int daysCheck = 0;

		public bool initKarma = false;

		// Jianghu Jin field

		public int teraformDays = 0;

		public GameComponent_YaomaStorytellers(Game game)
		{
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref karma, "karma", 0);
			Scribe_Values.Look(ref cachedCostFactor, "cachedCostFactor", 1, false);
			Scribe_Collections.Look(ref baseIncidentChange, "baseIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectableIncidentCount, "selectableIncidentCount", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref estIncidentChange, "estIncidentCost", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref selectedIncidents, "selectedIncidents", LookMode.Deep);
			Scribe_Values.Look(ref daysCheck, "daysCheck", 0, false);
			Scribe_Values.Look(ref initKarma, "initKarma", false);
			Scribe_Values.Look(ref teraformDays, "teraformDays", 0);
		}

		public override void StartedNewGame()
		{
			base.StartedNewGame();
		}

		public override void LoadedGame()
		{
			base.LoadedGame();
		}

		public string GetWarningString(StorytellerDef orgDef, StorytellerDef currDef)
		{
			if (orgDef == StorytellerDefOf.KaiyiKarmic_Yaoma) return "YS_StorytellerChangeWarningKaiyi".Translate(orgDef.LabelCap, currDef.LabelCap);
			else if (orgDef == StorytellerDefOf.JianghuJin_Yaoma) return "YS_StorytellerChangeWarningJin".Translate(orgDef.LabelCap, currDef.LabelCap);
			return "this is a test string, you shouldn't see this";
		}

		public void ResetExposedData(StorytellerDef def)
        {
			if (def == StorytellerDefOf.KaiyiKarmic_Yaoma) ResetKaiyiKarmicSave();
			else if (def == StorytellerDefOf.JianghuJin_Yaoma) ResetJianghuJinSave();
		}

		public void ResetKaiyiKarmicSave()
        {
			karma = 0f;
			cachedCostFactor = 1f;
			baseIncidentChange = new Dictionary<IncidentCategoryDef, float>();
			selectableIncidentCount = new Dictionary<IncidentDef, int>();
			estIncidentChange = new Dictionary<IncidentDef, float>();
			selectedIncidents = new List<KaiyiIncidentRecord>();
			alteredCats = new List<IncidentCategoryDef>();
			daysCheck = 0;
			initKarma = false;
		}

		public void ResetJianghuJinSave()
		{
			teraformDays = 0;
		}
	}
}
