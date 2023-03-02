using System.Collections.Generic;
using RimWorld;

namespace YaomaStorytellers
{
	public class StorytellerComp_OnDemand : StorytellerComp
	{
		private StorytellerCompProperties_OnDemand Props
		{
			get
			{
				return (StorytellerCompProperties_OnDemand)this.props;
			}
		}

		public FiringIncident MakeIncident(List<IIncidentTarget> targets, IncidentDef incidentDef)
		{
			IncidentParms parms;
			// for each potential target type
			foreach (IIncidentTarget target in targets)
			{
				// if the incident type is null or target isn't allowed, continue to next target in list
				if (incidentDef == null || !incidentDef.TargetAllowed(target))
				{
					continue;
				}

				// create incident paramaters based on incident category
				parms = this.GenerateParms(incidentDef.category, target);

				// check if this incident can be fired
				if (incidentDef.Worker.CanFireNow(parms))
				{
					// returns a firing incident for later
					return new FiringIncident(incidentDef, this, parms);
				}

			}
			return null;
		}

		// unused for now
		public FiringIncident MakeIncident(List<IIncidentTarget> targets)
		{
			IncidentParms parms;
			// for each potential target type
			foreach (IIncidentTarget target in targets)
			{
				// if the incident type is null or target isn't allowed, continue to next target in list
				if (this.Props.incident == null || !this.Props.incident.TargetAllowed(target))
				{
					continue;
				}

				// create incident paramaters based on incident category
				parms = this.GenerateParms(this.Props.incident.category, target);

				// check if this incident can be fired
				if (this.Props.incident.Worker.CanFireNow(parms))
				{
					// returns a firing incident for later
					return new FiringIncident(this.Props.incident, this, parms);
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
				if (this.Props.incident == null || !this.Props.incident.TargetAllowed(target))
				{
					continue;
				}

				// create incident paramaters based on incident category
				parms = this.GenerateParms(this.Props.incident.category, target);

				// check if this incident can be fired
				if (this.Props.incident.Worker.CanFireNow(parms))
				{
					// returns a firing incident for later
					yield return new FiringIncident(this.Props.incident, this, parms);
				}
			}
			yield break;
		}

		public override string ToString()
		{
			return base.ToString() + " " + this.Props.incident;
		}

	}
}
