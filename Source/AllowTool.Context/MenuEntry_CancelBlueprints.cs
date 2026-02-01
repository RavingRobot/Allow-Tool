using Verse;

namespace AllowTool.Context;

public class MenuEntry_CancelBlueprints : BaseContextMenuEntry
{
	protected override string SettingHandleSuffix => "cancelBlueprints";

	protected override string BaseTextKey => "Designator_context_cancel_build";

	public override ActivationResult Activate(Designator designator, Map map)
	{
		int num = 0;
		Thing[] array = map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).ToArray();
		foreach (Thing thing in array)
		{
			thing.Destroy(DestroyMode.Cancel);
			num++;
		}
		return ActivationResult.FromCount(num, BaseMessageKey);
	}
}
