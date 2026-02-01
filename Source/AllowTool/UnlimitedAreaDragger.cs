using System;
using HugsLib;
using Verse;

namespace AllowTool;

public class UnlimitedAreaDragger
{
	private Designator owningDesignator;

	private bool listening;

	private bool updateScheduled;

	public bool SelectionInProgress { get; private set; }

	public CellRect SelectedArea { get; private set; }

	public IntVec3 SelectionStartCell { get; private set; } = IntVec3.Invalid;

	public event Action<CellRect> SelectionStart;

	public event Action<CellRect> SelectionChanged;

	public event Action<CellRect> SelectionComplete;

	public event Action<CellRect> SelectionUpdate;

	public void BeginListening(Designator parentDesignator)
	{
		owningDesignator = parentDesignator;
		listening = true;
		RegisterForNextUpdate();
	}

	public void StopListening()
	{
		listening = false;
	}

	private void OnSelectionStarted()
	{
		SelectionInProgress = true;
		SelectionStartCell = ClampPositionToMapRect(Find.CurrentMap, UI.MouseCell());
		this.SelectionStart?.Invoke(CellRect.SingleCell(SelectionStartCell));
	}

	private void OnSelectedAreaChanged()
	{
		this.SelectionChanged?.Invoke(SelectedArea);
	}

	private void OnSelectionEnded()
	{
		SelectionInProgress = false;
		this.SelectionComplete?.Invoke(SelectedArea);
		SelectionStartCell = IntVec3.Invalid;
		SelectedArea = CellRect.Empty;
	}

	private void Update()
	{
		updateScheduled = false;
		Map currentMap = Find.CurrentMap;
		if ((listening && currentMap == null) || Find.MapUI.designatorManager.SelectedDesignator != owningDesignator)
		{
			StopListening();
		}
		if (!listening)
		{
			return;
		}
		RegisterForNextUpdate();
		DesignationDragger dragger = Find.MapUI.designatorManager.Dragger;
		if (!SelectionInProgress && dragger.Dragging)
		{
			OnSelectionStarted();
		}
		else if (SelectionInProgress && !dragger.Dragging)
		{
			OnSelectionEnded();
		}
		if (SelectionInProgress)
		{
			IntVec3 pos = UI.MouseCell();
			IntVec3 second = ClampPositionToMapRect(currentMap, pos);
			CellRect cellRect = CellRect.FromLimits(SelectionStartCell, second);
			if (cellRect != SelectedArea)
			{
				SelectedArea = cellRect;
				OnSelectedAreaChanged();
			}
			this.SelectionUpdate?.Invoke(SelectedArea);
		}
	}

	private IntVec3 ClampPositionToMapRect(Map map, IntVec3 pos)
	{
		return CellRect.WholeMap(map).ClosestCellTo(pos);
	}

	private void RegisterForNextUpdate()
	{
		if (!updateScheduled)
		{
			HugsLibController.Instance.DoLater.DoNextUpdate(Update);
			updateScheduled = true;
		}
	}
}
