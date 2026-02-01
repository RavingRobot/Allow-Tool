using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_HaulUrgently : Designator_SelectableThings
{
	protected override DesignationDef Designation => AllowToolDefOf.HaulUrgentlyDesignation;

	public Designator_HaulUrgently()
	{
		UseDesignatorDef(AllowToolDefOf.HaulUrgentlyDesignator);
	}

	protected override void FinalizeDesignationSucceeded()
	{
		base.FinalizeDesignationSucceeded();
		if (!HugsLibUtility.ShiftIsHeld)
		{
			return;
		}
		foreach (Pawn freeColonist in Find.CurrentMap.mapPawns.FreeColonists)
		{
			freeColonist.jobs.CheckForJobOverride();
		}
	}

	public override AcceptanceReport CanDesignateThing(Thing t)
	{
		return ThingIsRelevant(t) && !t.HasDesignation(AllowToolDefOf.HaulUrgentlyDesignation);
	}

	public override void DesignateThing(Thing thing)
	{
		if (thing.def.designateHaulable)
		{
			thing.ToggleDesignation(DesignationDefOf.Haul, enable: true);
		}
		thing.ToggleDesignation(AllowToolDefOf.HaulUrgentlyDesignation, enable: true);
		thing.SetForbidden(value: false, warnOnFail: false);
		AllowToolController.Instance.HaulUrgentlyCache.ClearCacheForMap(thing.Map);
	}

	private bool ThingIsRelevant(Thing thing)
	{
		if (thing.def == null || thing.Map == null || thing.Position.Fogged(thing.Map))
		{
			return false;
		}
		return (thing.def.alwaysHaulable || thing.def.EverHaulable) && !thing.IsInValidBestStorage();
	}
}
