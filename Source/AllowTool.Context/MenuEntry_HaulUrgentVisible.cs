using System;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_HaulUrgentVisible : BaseContextMenuEntry
{
	protected override string BaseTextKey => "Designator_context_urgent_visible";

	protected override string BaseMessageKey => "Designator_context_urgent";

	protected override string SettingHandleSuffix => "haulUrgentVisible";

	protected override ThingRequestGroup DesignationRequestGroup => ThingRequestGroup.HaulableEver;

	public override ActivationResult Activate(Designator designator, Map map)
	{
		Predicate<Thing> thingIsVisible = GetVisibleThingFilter();
		int num = DesignateAllThings(designator, map, (Thing t) => MenuEntry_HaulUrgentAll.CanAutoDesignateThingForUrgentHauling(t) && thingIsVisible(t));
		return (num > 0) ? ActivationResult.Success(BaseTextKey, num) : ActivationResult.Failure(BaseMessageKey);
	}
}
