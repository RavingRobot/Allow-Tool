using System.Collections.Generic;
using HugsLib.Utils;
using RimWorld;
using Verse;
using Verse.AI;

namespace AllowTool;

public class WorkGiver_FinishOff : WorkGiver_Scanner
{
	private static readonly Pawn[] emptyPawnsArray = new Pawn[0];

	private static bool WorkGiverEnabled => AllowToolController.Instance.Handles.IsDesignatorEnabled(AllowToolDefOf.FinishOffDesignator) || AllowToolController.Instance.Handles.IsReverseDesignatorEnabled(AllowToolDefOf.ReverseFinishOff);

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public static FloatMenuOption InjectThingFloatOptionIfNeeded(Thing target, Pawn selPawn)
	{
		if (Designator_FinishOff.IsValidDesignationTarget(target) && WorkGiverEnabled)
		{
			JobFailReason.Clear();
			WorkGiver_FinishOff giver = CreateInstance();
			Job job = giver.JobOnThing(selPawn, target, forced: true);
			FloatMenuOption option = new FloatMenuOption("Finish_off_floatMenu".Translate(target.LabelShort), delegate
			{
				selPawn.jobs.TryTakeOrderedJobPrioritizedWork(job, giver, target.Position);
			});
			option = FloatMenuUtility.DecoratePrioritizedTask(option, selPawn, target);
			if (job == null)
			{
				option.Disabled = true;
				if (JobFailReason.HaveReason)
				{
					option.Label = "CannotGenericWork".Translate(giver.def.verb, target.LabelShort, target) + " (" + JobFailReason.Reason + ")";
				}
			}
			return option;
		}
		return null;
	}

	public static WorkGiver_FinishOff CreateInstance()
	{
		return new WorkGiver_FinishOff
		{
			def = AllowToolDefOf.FinishOff
		};
	}

	public Job TryGetJobInRange(Pawn pawn, float maxRange)
	{
		float num = maxRange * maxRange;
		foreach (Thing potentialTarget in GetPotentialTargets(pawn))
		{
			if ((float)pawn.Position.DistanceToSquared(potentialTarget.Position) < num)
			{
				Job job = JobOnThing(pawn, potentialTarget);
				if (job != null)
				{
					return job;
				}
			}
		}
		return null;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (Designator_FinishOff.IsValidDesignationTarget(t) && pawn.CanReserveAndReach(t, PathEndMode.Touch, Danger.Deadly))
		{
			AcceptanceReport acceptanceReport = Designator_FinishOff.FriendlyPawnIsValidTarget(t);
			if (forced && !acceptanceReport.Accepted)
			{
				JobFailReason.Is(acceptanceReport.Reason);
			}
			else
			{
				AcceptanceReport acceptanceReport2 = Designator_FinishOff.PawnMeetsSkillRequirement(pawn, t as Pawn);
				if (!acceptanceReport2.Accepted)
				{
					JobFailReason.Is(acceptanceReport2.Reason);
				}
				else if (forced || t.HasDesignation(AllowToolDefOf.FinishOffDesignation))
				{
					Verb verb = pawn.meleeVerbs?.TryGetMeleeVerb(t);
					if (verb != null)
					{
						Job job = JobMaker.MakeJob(AllowToolDefOf.FinishOffPawn, t);
						job.verbToUse = verb;
						job.killIncappedTarget = true;
						return job;
					}
				}
			}
		}
		return null;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		if (WorkGiverEnabled)
		{
			return GetPotentialTargets(pawn);
		}
		return emptyPawnsArray;
	}

	private IEnumerable<Thing> GetPotentialTargets(Pawn pawn)
	{
		IReadOnlyList<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn target = pawns[i];
			if (Designator_FinishOff.IsValidDesignationTarget(target))
			{
				yield return target;
			}
		}
	}
}
