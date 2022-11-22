using RimWorld;

namespace YaomaStorytellers
{
    [DefOf]
    public static class StorytellerDefOf
    {
        public static StorytellerDef FarseerFan_Yaoma;
        public static StorytellerDef KaiyiKarmic_Yaoma;
        public static StorytellerDef DeathlessDaji_Yaoma;

        static StorytellerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StorytellerDefOf));
        }

    }
}
