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
    public class JobDefOf_Yaoma
    {

        public static JobDef YaomaStorytellers_Equip;

        static JobDefOf_Yaoma()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf_Yaoma));
        }
    }
}
