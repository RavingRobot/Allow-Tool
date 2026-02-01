using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AllowTool.Context;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AllowTool.Patches;

[HarmonyPatch(typeof(InspectGizmoGrid), "DrawInspectGizmoGridFor")]
internal static class InspectGizmoGrid_DrawInspectGizmoGridFor_Patch
{
	private static bool patchApplied;

	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> ClearReverseDesignators(IEnumerable<CodeInstruction> instructions)
	{
		FieldInfo gizmoListField = AccessTools.Field(typeof(InspectGizmoGrid), "gizmoList");
		MethodInfo clearListMethod = AccessTools.Method(typeof(List<Gizmo>), "Clear");
		if (gizmoListField == null || gizmoListField.FieldType != typeof(List<Gizmo>))
		{
			throw new Exception("Failed to reflect InspectGizmoGrid.gizmoList");
		}
		if (clearListMethod == null)
		{
			throw new Exception("Failed to reflect List.Clear");
		}
		CodeInstruction[] instructionsArr = instructions.ToArray();
		patchApplied = false;
		CodeInstruction prevInstruction = null;
		CodeInstruction[] array = instructionsArr;
		foreach (CodeInstruction instruction in array)
		{
			yield return instruction;
			if (prevInstruction != null && prevInstruction.LoadsField(gizmoListField) && instruction.Calls(clearListMethod))
			{
				yield return new CodeInstruction(OpCodes.Call, new Action(DesignatorContextMenuController.ClearReverseDesignatorPairs).Method);
				patchApplied = true;
			}
			prevInstruction = instruction;
		}
		if (!patchApplied)
		{
			AllowToolController.Logger.Warning("Failed to transpile method DrawInspectGizmoGridFor");
		}
	}
}
