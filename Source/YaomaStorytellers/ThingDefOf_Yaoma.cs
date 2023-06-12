using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    [DefOf]
    public class ThingDefOf_Yaoma
    {

        public static ThingDef YS_StabilizerTotemJin;

        public static ThingDef YS_StabilizerArrayJin;

        static ThingDefOf_Yaoma()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Yaoma));
        }
    }
}
