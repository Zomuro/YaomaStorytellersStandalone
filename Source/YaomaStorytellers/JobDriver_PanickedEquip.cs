using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;
using Verse.AI;

namespace YaomaStorytellers
{
    public class JobDriver_PanickedEquip : JobDriver_Equip
    {
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_General.Do(delegate
			{
				this.pawn.mindState.droppedWeapon = null;
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDestroyedOrNull(TargetIndex.A);
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate ()
			{
				ThingWithComps thingWithComps = (ThingWithComps)this.job.targetA.Thing;
				ThingWithComps thingWithComps2;
				if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
				{
					thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
				}
				else
				{
					thingWithComps2 = thingWithComps;
					thingWithComps2.DeSpawn(DestroyMode.Vanish);
				}
				this.pawn.equipment.MakeRoomFor(thingWithComps2);
				this.pawn.equipment.AddEquipment(thingWithComps2);
				if (thingWithComps.def.soundInteract != null)
				{
					thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(this.pawn.Position, this.pawn.Map, false));
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield break;
		}

	}
}
