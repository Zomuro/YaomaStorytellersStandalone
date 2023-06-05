using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace YaomaStorytellers
{
    public class CompProperties_Stablizer_Jin : CompProperties
    {
        public float minRange = 0f;

        public float maxRange = 1f;

        public bool scaleWithFuel = false;

        public bool destroyOnTerraform = true;
    }
}
