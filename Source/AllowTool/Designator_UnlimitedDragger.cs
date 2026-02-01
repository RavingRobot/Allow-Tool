using Verse;

namespace AllowTool;

public abstract class Designator_UnlimitedDragger : Designator_DefBased
{
	protected UnlimitedAreaDragger Dragger { get; }

	public override int DraggableDimensions => 2;

	public override bool DragDrawMeasurements => true;

	protected Designator_UnlimitedDragger()
	{
		Dragger = new UnlimitedAreaDragger();
	}

	public override void Selected()
	{
		Dragger.BeginListening(this);
	}

	public override AcceptanceReport CanDesignateCell(IntVec3 c)
	{
		return false;
	}
}
