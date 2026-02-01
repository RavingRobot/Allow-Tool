using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(CompForbiddable), "CompGetGizmosExtra")]
internal static class CompForbiddable_Gizmos_Patch
{
	private static readonly Gizmo[] resultArray = new Gizmo[1];

	[HarmonyPostfix]
	public static void InjectDesignatorFunctionality(ref IEnumerable<Gizmo> __result)
	{
		Command_Toggle command_Toggle = CommandFromEnumerator(__result);
		if (command_Toggle != null)
		{
			AllowThingToggleHandler.EnhanceStockAllowToggle(command_Toggle);
			resultArray[0] = command_Toggle;
			__result = resultArray;
		}
	}

	private static Command_Toggle CommandFromEnumerator(IEnumerable<Gizmo> enumerator)
	{
		using IEnumerator<Gizmo> enumerator2 = enumerator.GetEnumerator();
		if (!enumerator2.MoveNext())
		{
			return null;
		}
		return enumerator2.Current as Command_Toggle;
	}
}
