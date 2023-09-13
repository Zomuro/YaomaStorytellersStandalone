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
			Scribe_Collections.Look(ref selectedIncidents, "selectedIncidents", LookMode.Def);
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
	}
}
