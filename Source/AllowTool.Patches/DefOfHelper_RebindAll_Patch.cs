using HarmonyLib;
using RimWorld;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(DefOfHelper), "RebindAllDefOfs")]
internal static class DefOfHelper_RebindAll_Patch
{
	[HarmonyPostfix]
	public static void HookBeforeImpliedDefsGeneration(bool earlyTryMode)
	{
		if (earlyTryMode)
		{
			AllowToolController.Instance.OnBeforeImpliedDefGeneration();
		}
	}
}
