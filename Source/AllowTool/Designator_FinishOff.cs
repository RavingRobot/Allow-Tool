using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_FinishOff : Designator_SelectableThings
{
	private const int MeleeSkillLevelRequired = 6;

	protected override DesignationDef Designation => AllowToolDefOf.FinishOffDesignation;

	public override string Desc
	{
		get
		{
			if ((bool)AllowToolController.Instance.Handles.FinishOffSkillRequirement)
			{
				return string.Format("{0}\n\n{1}", base.Desc, "Finish_off_skillRequired".Translate(6));
			}
			return base.Desc;
		}
	}

	public static bool IsValidDesignationTarget(Thing t)
	{
		Pawn pawn = t as Pawn;
		return pawn?.def != null && !pawn.Dead && pawn.Downed;
	}

	public static AcceptanceReport PawnMeetsSkillRequirement(Pawn pawn, Pawn targetPawn)
	{
		if (pawn == null)
		{
			return AcceptanceReport.WasRejected;
		}
		if (!AllowToolUtility.PawnCapableOfViolence(pawn))
		{
			return new AcceptanceReport("IsIncapableOfViolenceShort".Translate());
		}
		bool flag = targetPawn?.RaceProps != null && targetPawn.RaceProps.Animal;
		bool flag2 = pawn.skills != null && (!AllowToolController.Instance.Handles.FinishOffSkillRequirement || pawn.skills.GetSkill(SkillDefOf.Melee).Level >= 6);
		if (!flag && !flag2)
		{
			return new AcceptanceReport("Finish_off_pawnSkillRequired".Translate(6));
		}
		return AcceptanceReport.WasAccepted;
	}

	public static AcceptanceReport FriendlyPawnIsValidTarget(Thing t)
	{
		return (!AllowToolUtility.PawnIsFriendly(t) || HugsLibUtility.ShiftIsHeld) ? AcceptanceReport.WasAccepted : new AcceptanceReport("Finish_off_floatMenu_reason_friendly".Translate());
	}

	public Designator_FinishOff()
	{
		UseDesignatorDef(AllowToolDefOf.FinishOffDesignator);
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		if (!IsValidDesignationTarget(t) || t.HasDesignation(AllowToolDefOf.FinishOffDesignation))
		{
			return false;
		}
		return FriendlyPawnIsValidTarget(t);
	}

	public override void DesignateThing(Thing t)
	{
		if (CanDesignateThing(t).Accepted)
		{
			t.ToggleDesignation(AllowToolDefOf.FinishOffDesignation, enable: true);
		}
	}
}
