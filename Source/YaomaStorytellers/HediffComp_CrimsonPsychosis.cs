using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class HediffComp_CrimsonPsychosis : HediffComp
    {

        public override void CompExposeData()
        {
            base.CompExposeData();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            // if the hediff is at max severity
            if (parent.Severity >= parent.def.maxSeverity)
            {
                // every ten seconds, if the pawn isn't berserk, force the pawn to become berserk
                if (Find.TickManager.TicksGame % 600 == 0 && parent.pawn.MentalStateDef != MentalStateDefOf.Berserk)
                {
                    parent.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, 
                        null, true, false, null, false, false, false);
                }
                return;
            }

            // approximately every quandrum, decrease severity of crimson psychosis by 0.25 severity
            // it should take a whole year to recover
            parent.Severity -= 0.25f / (900000f);
        }
    }
}
