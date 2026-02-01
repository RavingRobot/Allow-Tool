using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AllowTool.Context;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(Command), "GizmoOnGUIInt", new Type[]
{
	typeof(Rect),
	typeof(GizmoRenderParms)
})]
internal static class Command_GizmoOnGUI_Patch
{
	private static bool overlayInjected;

	[HarmonyPrepare]
	private static void PrePatch()
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			if (!overlayInjected)
			{
				AllowToolController.Logger.Error("Command_GizmoOnGUI infix could not be applied.");
			}
		});
	}

	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> DrawRightClickIcon(IEnumerable<CodeInstruction> instructions, MethodBase method)
	{
		MethodInfo expectedMethod = AccessTools.Method(typeof(Command), "DrawIcon", new Type[3]
		{
			typeof(Rect),
			typeof(Material),
			typeof(GizmoRenderParms)
		});
		overlayInjected = false;
		if (expectedMethod == null)
		{
			AllowToolController.Logger.Error("Failed to reflect required method: " + Environment.StackTrace);
		}
		foreach (CodeInstruction instruction in instructions)
		{
			if (expectedMethod != null && instruction.opcode == OpCodes.Callvirt && expectedMethod.Equals(instruction.operand))
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Ldarga, (short)1);
				yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Rect), "min"));
				yield return new CodeInstruction(OpCodes.Call, new Action<Command, Vector2>(DesignatorContextMenuController.DrawCommandOverlayIfNeeded).Method);
				overlayInjected = true;
			}
			yield return instruction;
		}
	}

	[HarmonyPostfix]
	public static void InterceptInteraction(ref GizmoResult __result, Command __instance)
	{
		if (__result.State == GizmoState.Interacted || __result.State == GizmoState.OpenedFloatMenu)
		{
			Designator designator = DesignatorContextMenuController.TryResolveCommandToDesignator(__instance);
			if (designator != null && DesignatorContextMenuController.TryProcessDesignatorInput(designator))
			{
				__result = new GizmoResult(GizmoState.Clear, __result.InteractEvent);
			}
		}
	}
}
