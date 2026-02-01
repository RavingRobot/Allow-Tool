using HarmonyLib;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(DesignationCategoryDef))]
[HarmonyPatch("ResolveDesignators")]
[HarmonyPriority(300)]
internal static class DesignationCategoryDef_ResolveDesignators_Patch
{
	[HarmonyPostfix]
	public static void InjectAllowToolDesignators()
	{
		AllowToolController.Instance.OnDesignationCategoryResolveDesignators();
	}
}
