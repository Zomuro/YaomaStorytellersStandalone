using RimWorld;

namespace YaomaStorytellers
{
    public class StorytellerCompProperties_OnDemand : StorytellerCompProperties
	{
		public StorytellerCompProperties_OnDemand()
		{
			this.compClass = typeof(StorytellerComp_OnDemand);
		}

		public IncidentDef incident;

	}
}
