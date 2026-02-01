using Verse;

namespace AllowTool;

public class Designator_SelectSimilarReverse : Designator_SelectSimilar
{
	public override Designator PickUpReverseDesignator()
	{
		return new Designator_SelectSimilar();
	}

	public override AcceptanceReport CanDesignateThing(Thing thing)
	{
		return thing.def != null && thing.def.selectable && thing.def.label != null && thing.Map != null && !thing.Map.fogGrid.IsFogged(thing.Position);
	}

	protected override void FinalizeDesignationSucceeded()
	{
		Designator des = PickUpReverseDesignator();
		Find.DesignatorManager.Select(des);
		base.FinalizeDesignationSucceeded();
	}
}
