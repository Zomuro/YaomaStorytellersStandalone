using RimWorld;

namespace YaomaStorytellers
{
    [DefOf]
    public static class IncidentDefOf_Yaoma
    {
        public static IncidentDef Resurrection_Yaoma;
        public static IncidentDef KarmaTrade_Yaoma;

        static IncidentDefOf_Yaoma()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf_Yaoma));
        }
    }
}
