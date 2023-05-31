using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
	public class StorytellerComp_OnDemandRegular : StorytellerComp_OnDemand
	{
		private StorytellerCompProperties_OnDemand Props
		{
			get
			{
				return (StorytellerCompProperties_OnDemand)this.props;
			}
		}

		public void CompExposeData()
		{
			Scribe_Values.Look<int>(ref days, "days", 0);
		}

		public int days = 0;

	}
}
