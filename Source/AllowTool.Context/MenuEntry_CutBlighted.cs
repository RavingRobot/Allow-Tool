using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_CutBlighted : BaseContextMenuEntry
{
	protected override string SettingHandleSuffix => "cutBlighted";

	protected override string BaseTextKey => "Designator_context_cut";

	protected override ThingRequestGroup DesignationRequestGroup => ThingRequestGroup.Plant;

	public override ActivationResult Activate(Designator designator, Map map)
	{
		int designationCount = DesignateAllThings(designator, map, (Thing t) => t is Plant { Blighted: not false } plant && !plant.HasDesignation(DesignationDefOf.CutPlant));
		return ActivationResult.FromCount(designationCount, BaseTextKey);
	}
}
