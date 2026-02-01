using System;
using System.Collections.Generic;
using System.Linq;
using AllowTool.Settings;
using HugsLib.Utils;
using RimWorld;
using Verse;
using Verse.AI;

namespace AllowTool;

public static class PartyHuntHandler
{
	private delegate bool HuntingTargetFilter(Pawn target, Pawn hunter);

	private readonly struct HuntingTargetCandidate(Pawn target, int distanceSquared) : IComparable<HuntingTargetCandidate>
	{
		public readonly Pawn target = target;

		private readonly int distanceSquared = distanceSquared;

		public int CompareTo(HuntingTargetCandidate other)
		{
			return distanceSquared.CompareTo(other.distanceSquared);
		}
	}

	private const float MaxPartyMemberDistance = 20f;

	private const float MaxFinishOffDistance = 20f;

	private static readonly HuntingTargetFilter HuntingTargetAttackFilter = (Pawn target, Pawn hunter) => !target.HasDesignation(DesignationDefOf.Tame) && (!target.Downed || (CanDoCommonerWork(hunter) && !WorldSettings.AutoFinishOff));

	private static readonly HuntingTargetFilter HuntingTargetFinishFilter = (Pawn target, Pawn _) => target.Downed && !target.HasDesignation(AllowToolDefOf.FinishOffDesignation);

	private static readonly List<HuntingTargetCandidate> huntingTargetCandidates = new List<HuntingTargetCandidate>();

	private static PartyHuntSettings WorldSettings => AllowToolController.Instance.WorldSettings.PartyHunt;

	public static Gizmo TryGetGizmo(Pawn pawn)
	{
		if (pawn.Name == null || !pawn.Drafted || !AllowToolController.Instance.Handles.PartyHuntSetting)
		{
			return null;
		}
		return new Command_PartyHunt(pawn);
	}

	public static void OnPawnUndrafted(Pawn pawn)
	{
		WorldSettings.TogglePawnPartyHunting(pawn, enable: false);
	}

	public static void DoBehaviorForPawn(JobDriver_Wait driver)
	{
		Pawn pawn = driver.pawn;
		if (!AllowToolController.Instance.Handles.PartyHuntSetting || !WorldSettings.PawnIsPartyHunting(pawn))
		{
			return;
		}
		Verb verb = pawn.TryGetAttackVerb(null, !pawn.IsColonist);
		if (pawn.Faction == null || driver.job.def != JobDefOf.Wait_Combat || !AllowToolUtility.PawnCapableOfViolence(pawn) || pawn.stances.FullBodyBusy)
		{
			return;
		}
		if (pawn.drafter.FireAtWill)
		{
			float maxDistance = (verb.verbProps.IsMeleeAttack ? 2f : verb.verbProps.range);
			Pawn pawn2 = TryFindHuntingTarget(pawn, verb.verbProps.minRange, maxDistance, HuntingTargetAttackFilter);
			if (pawn2 != null)
			{
				pawn.TryStartAttack(pawn2);
				ResetAutoUndraftTimer(pawn.drafter);
			}
		}
		if (!pawn.stances.FullBodyBusy && WorldSettings.AutoFinishOff && CanDoCommonerWork(pawn) && !AnyHuntingPartyMembersInCombat(pawn, 20f))
		{
			TryFindHuntingTarget(pawn, 0f, 20f, HuntingTargetFinishFilter)?.ToggleDesignation(AllowToolDefOf.FinishOffDesignation, enable: true);
			Job job = WorkGiver_FinishOff.CreateInstance().TryGetJobInRange(pawn, 20f);
			if (job != null)
			{
				pawn.jobs.StartJob(job, JobCondition.Ongoing, null, resumeCurJobAfterwards: true);
				pawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(JobDefOf.Goto, pawn.Position));
			}
		}
	}

	private static bool AnyHuntingPartyMembersInCombat(Pawn centerPawn, float maxPartyMemberDistance)
	{
		return centerPawn.Map.mapPawns.FreeColonists.Where((Pawn p) => WorldSettings.PawnIsPartyHunting(p) && centerPawn.Position.DistanceTo(p.Position) <= maxPartyMemberDistance).Any((Pawn p) => p.stances.FullBodyBusy);
	}

	private static Pawn TryFindHuntingTarget(Pawn searcher, float minDistance, float maxDistance, HuntingTargetFilter targetFilter)
	{
		float minDistanceSquared = minDistance * minDistance;
		float maxDistanceSquared = maxDistance * maxDistance;
		huntingTargetCandidates.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned = searcher.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn = allPawnsSpawned[i];
			if (validator(pawn))
			{
				huntingTargetCandidates.Add(new HuntingTargetCandidate(pawn, (searcher.Position - pawn.Position).LengthHorizontalSquared));
			}
		}
		huntingTargetCandidates.Sort();
		return (huntingTargetCandidates.Count > 0) ? huntingTargetCandidates[0].target : null;
		bool validator(Pawn pawn2)
		{
			if (pawn2 == null)
			{
				return false;
			}
			int lengthHorizontalSquared = (searcher.Position - pawn2.Position).LengthHorizontalSquared;
			if ((float)lengthHorizontalSquared < minDistanceSquared || (float)lengthHorizontalSquared > maxDistanceSquared)
			{
				return false;
			}
			if (pawn2.Position.Fogged(searcher.Map) || !searcher.CanSee(pawn2))
			{
				return false;
			}
			return pawn2.RaceProps != null && pawn2.RaceProps.Animal && pawn2.Faction == null && (!WorldSettings.HuntDesignatedOnly || pawn2.HasDesignation(DesignationDefOf.Hunt)) && (targetFilter == null || targetFilter(pawn2, searcher));
		}
	}

	private static void ResetAutoUndraftTimer(Pawn_DraftController draftController)
	{
		AutoUndrafter autoUndrafter = (AutoUndrafter)AllowToolController.Instance.Reflection.DraftControllerAutoUndrafterField.GetValue(draftController);
		autoUndrafter.Notify_Drafted();
	}

	private static bool CanDoCommonerWork(Pawn pawn)
	{
		return !pawn.WorkTagIsDisabled(WorkTags.Commoner);
	}
}
