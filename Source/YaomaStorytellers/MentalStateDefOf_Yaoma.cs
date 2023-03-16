using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    [DefOf]
    public static class MentalStateDefOf_Yaoma
    {
        // may just let this be unused
        public static MentalStateDef YS_BerserkDaji;

        static MentalStateDefOf_Yaoma()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MentalStateDefOf_Yaoma));
        }
    }
}
