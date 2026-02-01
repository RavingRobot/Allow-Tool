using AllowTool.Context;
using HarmonyLib;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Designator), "CreateReverseDesignationGizmo")]
internal static class Designator_CreateReverseDesignationGizmo_Patch
{
	[HarmonyPostfix]
	internal static void CreateReverseDesignationGizmo_Postfix(Designator __instance, Command_Action __result)
	{
		if (__result != null)
		{
			DesignatorContextMenuController.RegisterReverseDesignatorPair(__instance, __result);
		}
	}
}
