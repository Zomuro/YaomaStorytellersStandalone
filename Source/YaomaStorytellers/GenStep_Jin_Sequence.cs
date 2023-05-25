using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace YaomaStorytellers
{
	public class GenStep_Jin_Sequence : GenStep
	{
		public override int SeedPart
		{
			get
			{
				return 142858723;
			}
		}

		public float ClampedProb
        {
            get
            {
				return Mathf.Clamp(probability, 0, 1f);
            }
        }

		public override void Generate(Map map, GenStepParams parms)
		{
			RunGenSteps(map, parms);
			//RunGenProbs(map, parms);
		}

		public void RunGenSteps(Map map, GenStepParams parms)
        {
			if (genStepDefs.NullOrEmpty()) return;
			if (ClampedProb <= 0 || UnityEngine.Random.Range(0f, 1f) > ClampedProb) return;
			foreach (var stepDef in genStepDefs) stepDef.genStep.Generate(map, parms);
		}

		public void RunGenProbs(Map map, GenStepParams parms)
		{
			if (genStepProbs.NullOrEmpty()) return;
			foreach (var stepProb in genStepProbs)
            {
				if (stepProb.ClampedProb <= 0 || UnityEngine.Random.Range(0f, 1f) > stepProb.ClampedProb) continue;
				stepProb.genStepDef.genStep.Generate(map, parms);
			}
		}

		public List<GenStepDef> genStepDefs;

		public float probability = 0f;

		public List<GenStepProb> genStepProbs;

		public class GenStepProb
        {
			public GenStepDef genStepDef;

			public float probability = 0f;

			public float ClampedProb
			{
				get
				{
					return Mathf.Clamp(probability, 0, 1f);
				}
			}


		}

	}
}
