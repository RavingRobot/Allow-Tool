using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_Allow : Designator_Replacement
{
	public Designator_Allow()
	{
		replacedDesignator = new Designator_Unforbid();
		UseDesignatorDef(AllowToolDefOf.AllowDesignator);
	}

	public override AcceptanceReport CanDesignateThing(Thing thing)
	{
		if (thing.Position.Fogged(thing.Map))
		{
			return false;
		}
		return ((thing as ThingWithComps)?.GetComp<CompForbiddable>())?.Forbidden ?? false;
	}

	public override void DesignateThing(Thing t)
	{
		t.SetForbidden(value: false, warnOnFail: false);
	}
}
