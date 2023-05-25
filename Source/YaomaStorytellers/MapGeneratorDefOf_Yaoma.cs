using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace YaomaStorytellers
{
	[DefOf]
	public static class MapGeneratorDefOf_Yaoma
	{
		static MapGeneratorDefOf_Yaoma()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MapGeneratorDefOf_Yaoma));
		}

		public static MapGeneratorDef YS_JianghuJin_RefreshTerrain;

	}
}
