﻿using RimWorld;
using Verse;

namespace YaomaStorytellers
{
    public class KaiyiIncidentRecord : IExposable
    {
        public void ExposeData()
        {
            Scribe_Defs.Look(ref incidDef, "incidDef");
            Scribe_Values.Look(ref change, "change", 1f);
        }

        public IncidentDef incidDef;

        public float change = 1f;

    }
}
