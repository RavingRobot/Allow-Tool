using System;
using System.Collections.Generic;
using System.Reflection;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool;

public class ReflectionHandler
{
	public FieldInfo GizmoGridGizmoListField;

	public FieldInfo DraftControllerAutoUndrafterField;

	public FieldInfo DesignatorHasDesignateAllFloatMenuOptionField;

	public FieldInfo GenDrawLineMatMetaOverlay;

	public MethodInfo DesignatorGetDesignationMethod;

	public MethodInfo DesignatorGetRightClickFloatMenuOptionsMethod;

	public MethodInfo DesignationCategoryDefResolveDesignatorsMethod;

	internal void PrepareReflection()
	{
		Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("InspectGizmoGrid", "RimWorld");
		if (typeInAnyAssembly != null)
		{
			GizmoGridGizmoListField = typeInAnyAssembly.GetField("gizmoList", HugsLibUtility.AllBindingFlags);
		}
		DesignatorGetDesignationMethod = typeof(Designator).GetMethod("get_Designation", HugsLibUtility.AllBindingFlags);
		DesignatorHasDesignateAllFloatMenuOptionField = typeof(Designator).GetField("hasDesignateAllFloatMenuOption", HugsLibUtility.AllBindingFlags);
		DesignatorGetRightClickFloatMenuOptionsMethod = typeof(Designator).GetMethod("get_RightClickFloatMenuOptions", HugsLibUtility.AllBindingFlags);
		DraftControllerAutoUndrafterField = typeof(Pawn_DraftController).GetField("autoUndrafter", HugsLibUtility.AllBindingFlags);
		DesignationCategoryDefResolveDesignatorsMethod = typeof(DesignationCategoryDef).GetMethod("ResolveDesignators", HugsLibUtility.AllBindingFlags);
		GenDrawLineMatMetaOverlay = typeof(GenDraw).GetField("LineMatMetaOverlay", BindingFlags.Static | BindingFlags.NonPublic);
		if (GizmoGridGizmoListField == null || GizmoGridGizmoListField.FieldType != typeof(List<Gizmo>) || DesignatorGetDesignationMethod == null || DesignatorGetDesignationMethod.ReturnType != typeof(DesignationDef) || DesignatorHasDesignateAllFloatMenuOptionField == null || DesignatorHasDesignateAllFloatMenuOptionField.FieldType != typeof(bool) || DesignatorGetRightClickFloatMenuOptionsMethod == null || DesignatorGetRightClickFloatMenuOptionsMethod.ReturnType != typeof(IEnumerable<FloatMenuOption>) || DraftControllerAutoUndrafterField == null || DraftControllerAutoUndrafterField.FieldType != typeof(AutoUndrafter) || DesignationCategoryDefResolveDesignatorsMethod == null || DesignationCategoryDefResolveDesignatorsMethod.GetParameters().Length != 0 || GenDrawLineMatMetaOverlay == null || GenDrawLineMatMetaOverlay.FieldType != typeof(Material))
		{
			AllowToolController.Logger.Error("Failed to reflect required members");
		}
	}
}
