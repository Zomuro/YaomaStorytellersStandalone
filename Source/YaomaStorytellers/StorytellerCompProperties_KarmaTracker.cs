using RimWorld;

namespace YaomaStorytellers
{
	public class StorytellerCompProperties_KarmaTracker : StorytellerCompProperties
	{
		public StorytellerCompProperties_KarmaTracker()
		{
			this.compClass = typeof(StorytellerComp_KarmaTracker);
		}

		public int days = 7;

	}
}
