using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AllowTool;

public class JobDriver_FinishOff : JobDriver
{
	private const int PrepareSwingDuration = 60;

	private const float VictimSkullMoteChance = 0.25f;

	private const float OpportunityTargetMaxRange = 8f;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (job.playerForced)
		{
			base.TargetA.Thing.ToggleDesignation(AllowToolDefOf.FinishOffDesignation, enable: true);
		}
		AddFailCondition(JobHasFailed);
		yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		Thing skullMote = null;
		yield return new Toil
		{
			initAction = delegate
			{
				Pawn victim = job.targetA.Thing as Pawn;
				skullMote = TryMakeSkullMote(victim, 0.25f);
				AllowToolDefOf.EffecterWeaponGlint.Spawn().Trigger(pawn, job.targetA.Thing);
			},
			defaultDuration = 60,
			defaultCompleteMode = ToilCompleteMode.Delay
		};
		yield return new Toil
		{
			initAction = delegate
			{
				if (base.job.targetA.Thing is Pawn pawn && base.job.verbToUse != null)
				{
					base.job.verbToUse.TryStartCastOn(pawn);
					DoSocialImpact(pawn);
					DoExecution(base.pawn, pawn);
					if (skullMote != null && !skullMote.Destroyed)
					{
						skullMote.Destroy();
					}
					if (!base.job.playerForced)
					{
						Job job = WorkGiver_FinishOff.CreateInstance().TryGetJobInRange(base.pawn, 8f);
						if (job != null)
						{
							base.pawn.jobs.jobQueue.EnqueueFirst(job);
						}
					}
				}
			},
			defaultCompleteMode = ToilCompleteMode.Instant
		};
	}

	private void DoExecution(Pawn slayer, Pawn victim)
	{
		IntVec3 position = victim.Position;
		int num = Mathf.Max(GenMath.RoundRandom(victim.BodySize * 8f), 1);
		for (int i = 0; i < num; i++)
		{
			victim.health.DropBloodFilth();
		}
		BodyPartRecord bodyPartRecord = victim.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).FirstOrDefault();
		int num2 = ((bodyPartRecord != null) ? Mathf.Clamp((int)victim.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20) : 20);
		DamageInfo damageInfo = new DamageInfo(DamageDefOf.ExecutionCut, num2, -1f, -1f, slayer, bodyPartRecord);
		victim.TakeDamage(damageInfo);
		if (!victim.Dead)
		{
			victim.Kill(damageInfo);
		}
		if ((bool)AllowToolController.Instance.Handles.FinishOffUnforbidsSetting)
		{
			UnforbidAdjacentThingsTo(position, base.Map);
		}
	}

	private void DoSocialImpact(Pawn victim)
	{
		bool isPrisonerOfColony = victim.IsPrisonerOfColony;
		if (AllowToolUtility.PawnIsFriendly(victim))
		{
			ThoughtUtility.GiveThoughtsForPawnExecuted(victim, pawn, PawnExecutionKind.GenericBrutal);
		}
		if (victim.RaceProps != null && victim.RaceProps.intelligence == Intelligence.Animal)
		{
			pawn.records.Increment(RecordDefOf.AnimalsSlaughtered);
		}
		if (isPrisonerOfColony)
		{
			TaleRecorder.RecordTale(TaleDefOf.ExecutedPrisoner, pawn, victim);
		}
	}

	private Thing TryMakeSkullMote(Pawn victim, float chance)
	{
		if (victim?.RaceProps != null && victim.RaceProps.intelligence == Intelligence.Humanlike && Rand.Chance(chance))
		{
			ThingDef mote_ThoughtBad = ThingDefOf.Mote_ThoughtBad;
			MoteBubble moteBubble = (MoteBubble)ThingMaker.MakeThing(mote_ThoughtBad);
			moteBubble.SetupMoteBubble(ThoughtDefOf.WitnessedDeathAlly.Icon, null);
			moteBubble.Attach(victim);
			return GenSpawn.Spawn(moteBubble, victim.Position, victim.Map);
		}
		return null;
	}

	private bool JobHasFailed()
	{
		return !(base.TargetThingA is Pawn { Spawned: not false, Dead: false, Downed: not false } pawn) || !pawn.HasDesignation(AllowToolDefOf.FinishOffDesignation);
	}

	private void UnforbidAdjacentThingsTo(IntVec3 center, Map map)
	{
		IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
		foreach (IntVec3 intVec in adjacentCellsAndInside)
		{
			IntVec3 intVec2 = center + intVec;
			if (intVec2.InBounds(map))
			{
				AllowToolUtility.ToggleForbiddenInCell(intVec2, map, makeForbidden: false);
			}
		}
	}
}
