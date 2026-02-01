using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HugsLib.Utils;
using RimWorld;
using Verse;

namespace AllowTool;

public class Designator_SelectSimilar : Designator_SelectableThings
{
	private class SelectionDefConstraint
	{
		public readonly Def thingDef;

		public readonly Def stuffDef;

		public int occurrences = 1;

		public SelectionDefConstraint(Def thingDef, Def stuffDef)
		{
			this.thingDef = thingDef;
			this.stuffDef = stuffDef;
		}
	}

	private const string ConstraintListSeparator = ", ";

	private const int MaxNumListedConstraints = 5;

	private readonly Dictionary<int, SelectionDefConstraint> selectionConstraints = new Dictionary<int, SelectionDefConstraint>();

	private bool constraintsNeedReindexing;

	private string readableConstraintList;

	private bool AnySelectionConstraints => selectionConstraints.Count > 0;

	private bool SelectingSingleCell => base.Dragger.SelectionInProgress && base.Dragger.SelectedArea.Area == 1;

	public Designator_SelectSimilar()
	{
		UseDesignatorDef(AllowToolDefOf.SelectSimilarDesignator);
	}

	public override void Selected()
	{
		base.Selected();
		ReindexSelectionConstraints();
	}

	public override AcceptanceReport CanDesignateThing(Thing thing)
	{
		return thing.def != null && thing.def.selectable && thing.def.label != null && !BlockedByFog(thing.Position, thing.Map) && (ThingMatchesSelectionConstraints(thing) || SelectingSingleCell) && SelectionLimitAllowsAdditionalThing();
	}

	public override void DesignateThing(Thing t)
	{
		TrySelectThing(t);
	}

	public override void DesignateMultiCell(IEnumerable<IntVec3> vanillaCells)
	{
		List<IntVec3> source = base.Dragger.SelectedArea.Cells.ToList();
		if (SelectingSingleCell)
		{
			ProcessSingleCellClick(source.FirstOrDefault());
		}
		else
		{
			base.DesignateMultiCell(vanillaCells);
		}
		TryCloseArchitectMenu();
	}

	public override void DrawMouseAttachments()
	{
		if (constraintsNeedReindexing)
		{
			ReindexSelectionConstraints();
		}
		string text = ((!SelectionLimitAllowsAdditionalThing()) ? ((string)"SelectSimilar_cursor_limit".Translate()) : ((!AnySelectionConstraints) ? ((string)"SelectSimilar_cursor_needConstraint".Translate()) : ((string)"SelectSimilar_cursor_nowSelecting".Translate(readableConstraintList))));
		AllowToolUtility.DrawMouseAttachedLabel(text);
		base.DrawMouseAttachments();
	}

	public bool SelectionLimitAllowsAdditionalThing()
	{
		return Find.Selector.NumSelected < AllowToolController.Instance.Handles.SelectionLimitSetting.Value || SelectingSingleCell || HugsLibUtility.AltIsHeld;
	}

	public void ReindexSelectionConstraints()
	{
		try
		{
			Selector selector = Find.Selector;
			constraintsNeedReindexing = false;
			selectionConstraints.Clear();
			readableConstraintList = "";
			if (selector.NumSelected == 0)
			{
				return;
			}
			foreach (object selectedObject in selector.SelectedObjects)
			{
				Thing thing = selectedObject as Thing;
				if (thing?.def != null && thing.def.selectable)
				{
					int constraintHashForThing = GetConstraintHashForThing(thing);
					selectionConstraints.TryGetValue(constraintHashForThing, out var value);
					if (value == null)
					{
						value = (selectionConstraints[constraintHashForThing] = new SelectionDefConstraint(thing.def, thing.Stuff));
					}
					value.occurrences++;
				}
			}
			List<SelectionDefConstraint> list = selectionConstraints.Values.ToList();
			StringBuilder stringBuilder = new StringBuilder();
			list.Sort((SelectionDefConstraint selectionDefConstraint3, SelectionDefConstraint selectionDefConstraint4) => -selectionDefConstraint3.occurrences.CompareTo(selectionDefConstraint4.occurrences));
			for (int num = 0; num < list.Count; num++)
			{
				bool flag = num >= list.Count - 1;
				SelectionDefConstraint selectionDefConstraint2 = list[num];
				if (num < 4 || flag)
				{
					if (selectionDefConstraint2.thingDef.label != null)
					{
						stringBuilder.Append(selectionDefConstraint2.thingDef.label.CapitalizeFirst());
						if (selectionDefConstraint2.stuffDef?.label != null)
						{
							stringBuilder.AppendFormat(" ({0})", selectionDefConstraint2.stuffDef.label.CapitalizeFirst());
						}
						if (!flag)
						{
							stringBuilder.Append(", ");
						}
					}
					continue;
				}
				stringBuilder.Append("SelectSimilar_numMoreTypes".Translate(list.Count - num));
				break;
			}
			readableConstraintList = stringBuilder.ToString();
		}
		catch (Exception e)
		{
			AllowToolController.Logger.ReportException(e);
		}
	}

	public bool TrySelectThing(Thing thing)
	{
		Selector selector = Find.Selector;
		if (!CanDesignateThing(thing).Accepted || selector.IsSelected(thing))
		{
			return false;
		}
		selector.SelectedObjects.Add(thing);
		SelectionDrawer.Notify_Selected(thing);
		if (!AnySelectionConstraints)
		{
			ReindexSelectionConstraints();
		}
		else
		{
			constraintsNeedReindexing = true;
		}
		return true;
	}

	private void ProcessSingleCellClick(IntVec3 cell)
	{
		if (!HugsLibUtility.ShiftIsHeld)
		{
			Find.Selector.ClearSelection();
			ReindexSelectionConstraints();
		}
		if (!cell.IsValid)
		{
			return;
		}
		IEnumerable<Thing> enumerable = Find.CurrentMap.thingGrid.ThingsAt(cell);
		foreach (Thing item in enumerable)
		{
			if (TrySelectThing(item))
			{
				break;
			}
		}
	}

	private bool BlockedByFog(IntVec3 cell, Map map)
	{
		return map.fogGrid.IsFogged(cell) && !DebugSettings.godMode;
	}

	private void TryCloseArchitectMenu()
	{
		if (Find.Selector.NumSelected != 0 && Find.MainTabsRoot.OpenTab == MainButtonDefOf.Architect)
		{
			Find.MainTabsRoot.EscapeCurrentTab();
		}
	}

	private bool ThingMatchesSelectionConstraints(Thing thing)
	{
		return !AnySelectionConstraints || selectionConstraints.ContainsKey(GetConstraintHashForThing(thing));
	}

	private int GetConstraintHashForThing(Thing thing)
	{
		int num = thing.def.shortHash;
		if (thing.Stuff != null)
		{
			num += thing.Stuff.shortHash * 31;
		}
		return num;
	}
}
