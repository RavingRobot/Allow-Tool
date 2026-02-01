using RimWorld;
using Verse;

namespace AllowTool.Context;

public class MenuEntry_StripAll : BaseContextMenuEntry
{
	protected override string BaseTextKey => "Designator_context_strip";

	protected override string SettingHandleSuffix => "stripAll";

	public override ActivationResult Activate(Designator designator, Map map)
	{
		Faction playerFaction = Faction.OfPlayer;
		int designationCount = DesignateAllThings(designator, map, (Thing t) => t.Faction != playerFaction);
		return ActivationResult.FromCount(designationCount, BaseTextKey);
	}
}
