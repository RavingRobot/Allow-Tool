using System.Collections.Generic;
using HugsLib.Utils;
using RimWorld;
using Verse;
using Verse.Sound;

namespace AllowTool;

public class Designator_AllowAll : Designator_DefBased
{
	public Designator_AllowAll()
	{
		UseDesignatorDef(AllowToolDefOf.AllowAllDesignator);
	}

	public override void Selected()
	{
		Find.DesignatorManager.Deselect();
		if (CheckCanInteract())
		{
			AllowAllTheThings();
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		return false;
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 loc)
	{
		return false;
	}

	private void AllowAllTheThings()
	{
		bool shiftIsHeld = HugsLibUtility.ShiftIsHeld;
		bool controlIsHeld = HugsLibUtility.ControlIsHeld;
		Map currentMap = Find.CurrentMap;
		if (currentMap == null)
		{
			return;
		}
		List<Thing> allThings = Find.CurrentMap.listerThings.AllThings;
		int num = 0;
		for (int i = 0; i < allThings.Count; i++)
		{
			Thing thing = allThings[i];
			CompForbiddable compForbiddable = (thing as ThingWithComps)?.GetComp<CompForbiddable>();
			bool flag = currentMap.fogGrid.IsFogged(thing.Position);
			CompRottable comp;
			if (compForbiddable != null && !flag && compForbiddable.Forbidden && (controlIsHeld || (thing.def != null && thing.def.EverHaulable)) && (shiftIsHeld || !(thing is Corpse) || (comp = (thing as ThingWithComps).GetComp<CompRottable>()) == null || (int)comp.Stage < 1))
			{
				compForbiddable.Forbidden = false;
				num++;
			}
		}
		if (num > 0)
		{
			if (base.Def.messageSuccess != null)
			{
				Messages.Message(base.Def.messageSuccess.Translate(num.ToString()), MessageTypeDefOf.SilentInput);
			}
			base.Def.soundSucceeded.PlayOneShotOnCamera();
		}
		else if (base.Def.messageFailure != null)
		{
			Messages.Message(base.Def.messageFailure.Translate(), MessageTypeDefOf.RejectInput);
		}
	}
}
