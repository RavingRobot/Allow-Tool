using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_Forbid : Designator_Replacement
{
	public Designator_Forbid()
	{
		replacedDesignator = new RimWorld.Designator_Forbid();
		UseDesignatorDef(AllowToolDefOf.ForbidDesignator);
	}

	public override AcceptanceReport CanDesignateThing(Thing thing)
	{
		if (thing.Position.Fogged(thing.Map))
		{
			return false;
		}
		CompForbiddable compForbiddable = (thing as ThingWithComps)?.GetComp<CompForbiddable>();
		return compForbiddable != null && !compForbiddable.Forbidden;
	}

	public override void DesignateThing(Thing t)
	{
		t.SetForbidden(value: true, warnOnFail: false);
	}
}
