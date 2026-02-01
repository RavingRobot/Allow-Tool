using System;
using System.Collections.Generic;
using System.Linq;
using AllowTool.Settings;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool;

[StaticConstructorOnStartup]
public class Designator_StripMine : Designator_UnlimitedDragger
{
	private enum SectionCompleteAction
	{
		None,
		ShowWindow,
		CommitSelection
	}

	private const float InvalidCellHighlightAlpha = 0.2f;

	private static readonly Material areaOutlineMaterial = (Material)AllowToolController.Instance.Reflection.GenDrawLineMatMetaOverlay.GetValue(null);

	private readonly Action updateCallback;

	private readonly MapCellHighlighter highlighter;

	private Material designationValidMat;

	private Material designationInvalidMat;

	private IntVec3 lastSelectionStart;

	private CellRect currentSelection;

	private bool updateCallbackScheduled;

	private StripMineWorldSettings worldSettings;

	private Dialog_StripMineConfiguration settingsWindow;

	private StripMineGlobalSettings globalSettings;

	protected override DesignationDef Designation => DesignationDefOf.Mine;

	public Designator_StripMine()
	{
		UseDesignatorDef(AllowToolDefOf.StripMineDesignator);
		highlighter = new MapCellHighlighter(EnumerateHighlightCells);
		base.Dragger.SelectionStart += DraggerOnSelectionStart;
		base.Dragger.SelectionChanged += DraggerOnSelectionChanged;
		base.Dragger.SelectionComplete += DraggerOnSelectionComplete;
		updateCallback = OnUpdate;
	}

	protected override void OnDefAssigned()
	{
		base.Def.GetDragHighlightTexture(delegate(Texture2D tex)
		{
			designationValidMat = GetMaterial(tex, 1f);
			designationInvalidMat = GetMaterial(tex, 0.2f);
		});
		static Material GetMaterial(Texture2D tex, float alpha)
		{
			return MaterialPool.MatFrom(tex, ShaderDatabase.MetaOverlay, new Color(1f, 1f, 1f, alpha));
		}
	}

	public override void Selected()
	{
		base.Selected();
		ScheduleUpdateCallback();
		RevertToSavedWorldSettings();
		RevertToSavedGlobalSettings();
	}

	public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
	{
	}

	public override void DrawMouseAttachments()
	{
		base.DrawMouseAttachments();
		if (GetSelectionCompleteAction() == SectionCompleteAction.CommitSelection)
		{
			AllowToolUtility.DrawMouseAttachedLabel(textColor: new Color(0.8f, 0.8f, 0.8f), text: "StripMine_cursor_autoApply".Translate());
		}
	}

	private void CommitCurrentSelection()
	{
		DesignateCells(EnumerateDesignationCells());
		CommitCurrentOffset();
		ClearCurrentSelection();
	}

	private void ClearCurrentSelection()
	{
		currentSelection = CellRect.Empty;
		lastSelectionStart = IntVec3.Zero;
		highlighter.ClearCachedCells();
	}

	private void DesignateCells(IEnumerable<IntVec3> targetCells)
	{
		IntVec3 intVec = IntVec3.Invalid;
		try
		{
			Map currentMap = Find.CurrentMap;
			DesignationDef mine = DesignationDefOf.Mine;
			HashSet<IntVec3> hashSet = new HashSet<IntVec3>(from d in currentMap.designationManager.SpawnedDesignationsOfDef(mine)
				select d.target.Cell);
			foreach (IntVec3 targetCell in targetCells)
			{
				intVec = targetCell;
				if (!hashSet.Contains(targetCell))
				{
					targetCell.ToggleDesignation(DesignationDefOf.Mine, enable: true);
				}
			}
		}
		catch (Exception arg)
		{
			AllowToolController.Logger.Error($"Error while placing Mine designations (cell {intVec}): {arg}");
		}
	}

	private void OnUpdate()
	{
		updateCallbackScheduled = false;
		Map currentMap = Find.CurrentMap;
		if (currentMap != null && Find.DesignatorManager.SelectedDesignator == this)
		{
			ScheduleUpdateCallback();
			DrawCellRectOutline(currentSelection);
			highlighter.DrawCellHighlights();
		}
		else
		{
			OnDesignatorDeselected();
		}
	}

	private void OnDesignatorDeselected()
	{
		ClearCurrentSelection();
		if (settingsWindow != null)
		{
			settingsWindow.CancelAndClose();
			settingsWindow = null;
		}
		CommitWorldSettings();
		CommitGlobalSettings();
	}

	private void ScheduleUpdateCallback()
	{
		if (!updateCallbackScheduled)
		{
			updateCallbackScheduled = true;
			HugsLibController.Instance.DoLater.DoNextUpdate(updateCallback);
		}
	}

	private IEnumerable<IntVec3> EnumerateGridCells()
	{
		IntVec3 offset = (worldSettings.VariableGridOffset ? lastSelectionStart : worldSettings.LastGridOffset.ToIntVec3);
		return currentSelection.Cells.Where(CellIsOnGridLine);
		bool CellIsOnGridLine(IntVec3 c)
		{
			return (c.x - offset.x) % (worldSettings.HorizontalSpacing + 1) == 0 || (c.z - offset.z) % (worldSettings.VerticalSpacing + 1) == 0;
		}
	}

	private IEnumerable<IntVec3> EnumerateDesignationCells()
	{
		Map map = Find.CurrentMap;
		return from c in EnumerateGridCells()
			where CellIsMineable(map, c)
			select c;
	}

	private IEnumerable<MapCellHighlighter.Request> EnumerateHighlightCells()
	{
		Map map = Find.CurrentMap;
		return from c in EnumerateGridCells()
			select new MapCellHighlighter.Request(c, CellIsMineable(map, c) ? designationValidMat : designationInvalidMat);
	}

	private bool CellIsMineable(Map map, IntVec3 c)
	{
		if (c.Fogged(map))
		{
			return true;
		}
		return c.GetFirstMineable(map)?.def.mineable ?? false;
	}

	private void DraggerOnSelectionStart(CellRect cellRect)
	{
		currentSelection = cellRect;
		lastSelectionStart = base.Dragger.SelectionStartCell;
		highlighter.ClearCachedCells();
	}

	private void DraggerOnSelectionChanged(CellRect cellRect)
	{
		currentSelection = cellRect;
		highlighter.ClearCachedCells();
	}

	private void DraggerOnSelectionComplete(CellRect cellRect)
	{
		switch (GetSelectionCompleteAction())
		{
		case SectionCompleteAction.ShowWindow:
			ShowSettingsWindow();
			break;
		case SectionCompleteAction.CommitSelection:
			CommitCurrentSelection();
			break;
		}
	}

	private SectionCompleteAction GetSelectionCompleteAction()
	{
		bool shiftIsHeld = HugsLibUtility.ShiftIsHeld;
		if (settingsWindow != null)
		{
			if (shiftIsHeld)
			{
				return SectionCompleteAction.CommitSelection;
			}
			return SectionCompleteAction.None;
		}
		bool flag = worldSettings.ShowWindow;
		if (shiftIsHeld)
		{
			flag = !flag;
		}
		if (flag)
		{
			return SectionCompleteAction.ShowWindow;
		}
		return SectionCompleteAction.CommitSelection;
	}

	private void ShowSettingsWindow()
	{
		if (settingsWindow == null)
		{
			settingsWindow = new Dialog_StripMineConfiguration(worldSettings)
			{
				WindowPosition = globalSettings.WindowPosition
			};
			settingsWindow.SettingsChanged += WindowOnSettingsChanged;
			settingsWindow.Closing += WindowOnClosing;
			Find.WindowStack.Add(settingsWindow);
		}
	}

	private void WindowOnSettingsChanged(IConfigurableStripMineSettings stripMineSettings)
	{
		highlighter.ClearCachedCells();
	}

	private void WindowOnClosing(bool accept)
	{
		if (accept)
		{
			CommitCurrentSelection();
		}
		else
		{
			RevertToSavedWorldSettings();
			ClearCurrentSelection();
		}
		globalSettings.WindowPosition = settingsWindow.WindowPosition;
		settingsWindow = null;
	}

	private void RevertToSavedWorldSettings()
	{
		worldSettings = AllowToolController.Instance.WorldSettings.StripMine.Clone();
	}

	private void RevertToSavedGlobalSettings()
	{
		globalSettings = AllowToolController.Instance.Handles.StripMineSettings.Value.Clone();
	}

	private void CommitWorldSettings()
	{
		AllowToolController.Instance.WorldSettings.StripMine = worldSettings.Clone();
	}

	private void CommitGlobalSettings()
	{
		SettingHandle<StripMineGlobalSettings> stripMineSettings = AllowToolController.Instance.Handles.StripMineSettings;
		if (!stripMineSettings.Value.Equals(globalSettings))
		{
			stripMineSettings.Value = globalSettings.Clone();
			HugsLibController.SettingsManager.SaveChanges();
		}
	}

	private void CommitCurrentOffset()
	{
		if (worldSettings.VariableGridOffset)
		{
			worldSettings.LastGridOffset = lastSelectionStart.ToIntVec2;
			CommitWorldSettings();
		}
	}

	private static void DrawCellRectOutline(CellRect rect)
	{
		if (rect.Area != 0)
		{
			float y = AltitudeLayer.MoteLow.AltitudeFor();
			Vector3 vector = new Vector3(rect.minX, y, rect.minZ);
			Vector3 b = new Vector3((float)rect.maxX + 1f, y, rect.minZ);
			Vector3 a = new Vector3(rect.minX, y, (float)rect.maxZ + 1f);
			Vector3 vector2 = new Vector3((float)rect.maxX + 1f, y, (float)rect.maxZ + 1f);
			GenDraw.DrawLineBetween(a, vector2, areaOutlineMaterial);
			GenDraw.DrawLineBetween(vector, b, areaOutlineMaterial);
			GenDraw.DrawLineBetween(a, vector, areaOutlineMaterial);
			GenDraw.DrawLineBetween(vector2, b, areaOutlineMaterial);
		}
	}
}
