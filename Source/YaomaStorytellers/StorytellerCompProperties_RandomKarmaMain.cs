using RimWorld;
using System.Collections.Generic;

namespace YaomaStorytellers
{
    public class StorytellerCompProperties_RandomKarmaMain : StorytellerCompProperties_RandomMain
	{
		public StorytellerCompProperties_RandomKarmaMain()
		{
			this.compClass = typeof(StorytellerComp_RandomKarmaMain);
		}

		public List<IncidentCatBaseChange> baseIncidentCategoryKarmaChange;

	}

	public class IncidentCatBaseChange
    {
		public IncidentCategoryDef def;

		public float change = -1f;
    }
}
