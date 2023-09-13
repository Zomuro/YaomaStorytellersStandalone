using RimWorld;
using System;
using System.Collections.Generic;

namespace YaomaStorytellers
{
    public class StorytellerComp_OnDemand : StorytellerComp
	{
		private StorytellerCompProperties_OnDemand Props
		{
			get
			{
				return (StorytellerCompProperties_OnDemand)props;
			}
		}

		public FiringIncident MakeIncident(List<IIncidentTarget> targets, IncidentDef incidentDef)
		{
			IncidentParms parms;
			// for each potential target type
			foreach (IIncidentTarget target in targets)
			{
				// if the incident type is null or target isn't allowed, continue to next target in list
				if (incidentDef == null || !incidentDef.TargetAllowed(target)) continue;

				// create incident paramaters based on incident category
				parms = GenerateParms(incidentDef.category, target);

				// check if this incident can be fired
				if (incidentDef.Worker.CanFireNow(parms))
				{
					// returns a firing incident for later
					return new FiringIncident(incidentDef, this, parms);
				}

			}
			return null;
		}

		public IEnumerable<FiringIncident> MakeIncidents(List<IIncidentTarget> targets)
		{
			IncidentParms parms;
			// for each potential target type
			foreach (IIncidentTarget target in targets)
			{
				// if the incident type is null or target isn't allowed, continue to next target in list
				if (Props.incident == null || !Props.incident.TargetAllowed(target)) continue;

				// create incident paramaters based on incident category
				parms = GenerateParms(Props.incident.category, target);

				// check if this incident can be fired
				if (Props.incident.Worker.CanFireNow(parms))
				{
					// returns a firing incident for later
					yield return new FiringIncident(Props.incident, this, parms);
				}
			}
			yield break;
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.incident;
		}

	}
}
