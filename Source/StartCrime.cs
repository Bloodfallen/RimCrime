using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Verse.AI
{

    public static class StealFindTarget
	{
		// Token: 0x06002CA3 RID: 11427 RVA: 0x0010AB98 File Offset: 0x00108D98
		public static Pawn FindPawnToSteal(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return null;
			}
            StealFindTarget.tmpTargets.Clear();
			List<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn2 = allPawnsSpawned[i];
				if ((pawn2.Faction == pawn.Faction || (pawn2.IsPrisoner && pawn2.HostFaction == pawn.Faction)) && pawn2.RaceProps.Humanlike && pawn2 != pawn && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn) && (pawn2.CurJob == null || !pawn2.CurJob.exitMapOnArrival))
				{
					StealFindTarget.tmpTargets.Add(pawn2);
				}
			}
			if (!StealFindTarget.tmpTargets.Any<Pawn>())
			{
				return null;
			}
			Pawn result = StealFindTarget.tmpTargets.RandomElement<Pawn>();
			StealFindTarget.tmpTargets.Clear();
			return result;
		}
		private static List<Pawn> tmpTargets = new List<Pawn>();
    }


	public class MentalStateWorker_Steal : MentalStateWorker
	{
		// Token: 0x06002BFC RID: 11260 RVA: 0x001089F5 File Offset: 0x00106BF5
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && StealFindTarget.FindPawnToSteal(pawn) != null;
		}
	}


    public class MentalState_Steal : MentalState
	{
		// Token: 0x06002C3F RID: 11327 RVA: 0x00109A9A File Offset: 0x00107C9A
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.target, "target", false);
		}

		// Token: 0x06002C40 RID: 11328 RVA: 0x00012CD2 File Offset: 0x00010ED2
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}

		// Token: 0x06002C41 RID: 11329 RVA: 0x00109AB3 File Offset: 0x00107CB3
		public override void PreStart()
		{
			base.PreStart();
			this.TryFindNewTarget();
		}

		// Token: 0x06002C42 RID: 11330 RVA: 0x00109AC4 File Offset: 0x00107CC4
		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (this.target != null || this.target.Dead)
			{
				base.RecoverFromState();
			}
			if (this.pawn.IsHashIntervalTick(120) && !this.IsTargetStillValidAndReachable())
			{
				if (!this.TryFindNewTarget())
				{
					base.RecoverFromState();
					return;
				}
				Messages.Message("MessageMurderousRageChangedTarget".Translate(this.pawn.NameShortColored, this.target.Label, this.pawn.Named("PAWN"), this.target.Named("TARGET")).Resolve().AdjustedFor(this.pawn, "PAWN", true), this.pawn, MessageTypeDefOf.NegativeEvent, true);
				base.MentalStateTick();
			}
		}

		// Token: 0x06002C43 RID: 11331 RVA: 0x00109BA0 File Offset: 0x00107DA0
		public override TaggedString GetBeginLetterText()
		{
			if (this.target == null)
			{
				Log.Error("No target. This should have been checked in this mental state's worker.");
				return "";
			}
			return this.def.beginLetter.Formatted(this.pawn.NameShortColored, this.target.NameShortColored, this.pawn.Named("PAWN"), this.target.Named("TARGET")).AdjustedFor(this.pawn, "PAWN", true).Resolve().CapitalizeFirst();
		}

		// Token: 0x06002C44 RID: 11332 RVA: 0x00109C40 File Offset: 0x00107E40
		private bool TryFindNewTarget()
		{
			this.target = StealFindTarget.FindPawnToSteal(this.pawn);
			return this.target != null;
		}

		// Token: 0x06002C45 RID: 11333 RVA: 0x00109C5C File Offset: 0x00107E5C
		public bool IsTargetStillValidAndReachable()
		{
			return this.target != null && this.target.SpawnedParentOrMe != null && (!(this.target.SpawnedParentOrMe is Pawn) || this.target.SpawnedParentOrMe == this.target) && this.pawn.CanReach(this.target.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, true, false, TraverseMode.ByPawn);
		}

		// Token: 0x04001AFD RID: 6909
		public Pawn target;

		// Token: 0x04001AFE RID: 6910
		private const int NoLongerValidTargetCheckInterval = 120;
	}



    

    }

