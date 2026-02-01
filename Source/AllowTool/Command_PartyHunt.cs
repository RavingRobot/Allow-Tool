using System.Collections.Generic;
using AllowTool.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool;

public class Command_PartyHunt : Command_Toggle
{
	private static readonly Vector2 overlayIconOffset = new Vector2(59f, 57f);

	private readonly Pawn pawn;

	private static PartyHuntSettings WorldSettings => AllowToolController.Instance.WorldSettings.PartyHunt;

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			yield return AllowToolUtility.MakeCheckmarkOption("setting_partyHuntFinish_label", null, () => WorldSettings.AutoFinishOff, delegate(bool b)
			{
				WorldSettings.AutoFinishOff = b;
			});
			yield return AllowToolUtility.MakeCheckmarkOption("setting_partyHuntDesignated_label", null, () => WorldSettings.HuntDesignatedOnly, delegate(bool b)
			{
				WorldSettings.HuntDesignatedOnly = b;
			});
			yield return AllowToolUtility.MakeCheckmarkOption("setting_partyHuntUnforbid_label", null, () => WorldSettings.UnforbidDrops, delegate(bool b)
			{
				WorldSettings.UnforbidDrops = b;
			});
		}
	}

	public Command_PartyHunt(Pawn pawn)
	{
		this.pawn = pawn;
		icon = AllowToolDefOf.Textures.partyHunt;
		defaultLabel = "PartyHuntToggle_label".Translate();
		defaultDesc = "PartyHuntToggle_desc".Translate();
		isActive = () => WorldSettings.PawnIsPartyHunting(pawn);
		toggleAction = ToggleAction;
		hotKey = KeyBindingDefOf.Misc9;
		disabledReason = TryGetDisabledReason(pawn);
		disabled = disabledReason != null;
	}

	private void ToggleAction()
	{
		WorldSettings.TogglePawnPartyHunting(pawn, !WorldSettings.PawnIsPartyHunting(pawn));
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms renderParams)
	{
		GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, renderParams);
		if (Event.current.type == EventType.Repaint)
		{
			AllowToolUtility.DrawRightClickIcon(topLeft.x + overlayIconOffset.x, topLeft.y + overlayIconOffset.y);
		}
		return result;
	}

	public override bool InheritFloatMenuInteractionsFrom(Gizmo other)
	{
		return false;
	}

	private string TryGetDisabledReason(Pawn forPawn)
	{
		return forPawn.WorkTagIsDisabled(WorkTags.Violent) ? "IsIncapableOfViolenceShort".Translate().CapitalizeFirst() : ((TaggedString)null);
	}
}
