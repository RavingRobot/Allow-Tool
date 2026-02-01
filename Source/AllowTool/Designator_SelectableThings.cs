using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AllowTool;

public abstract class Designator_SelectableThings : Designator_UnlimitedDragger
{
	private Material highlightMaterial;

	protected Designator_SelectableThings()
	{
		MapCellHighlighter highlighter = new MapCellHighlighter(SelectHighlightedCells);
		Action<CellRect> value = delegate
		{
			highlighter.ClearCachedCells();
		};
		base.Dragger.SelectionStart += value;
		base.Dragger.SelectionChanged += value;
		base.Dragger.SelectionComplete += value;
		base.Dragger.SelectionUpdate += delegate
		{
			highlighter.DrawCellHighlights();
		};
	}

	protected override void OnDefAssigned()
	{
		base.Def.GetDragHighlightTexture(delegate(Texture2D tex)
		{
			highlightMaterial = MaterialPool.MatFrom(tex, ShaderDatabase.MetaOverlay, Color.white);
		});
	}

	public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap == null)
		{
			return;
		}
		ThingGrid thingGrid = currentMap.thingGrid;
		CellRect cellRect = base.Dragger.SelectedArea.ClipInsideMap(currentMap);
		List<Thing> list = new List<Thing>();
		int num = 0;
		foreach (IntVec3 cell in cellRect.Cells)
		{
			List<Thing> list2 = thingGrid.ThingsListAtFast(cell);
			for (int i = 0; i < list2.Count; i++)
			{
				if (CanDesignateThing(list2[i]).Accepted)
				{
					list.Add(list2[i]);
					num++;
				}
			}
		}
		DesignateMultiThing(list);
		if (num > 0)
		{
			if (base.Def.messageSuccess != null)
			{
				Messages.Message(base.Def.messageSuccess.Translate(num.ToString()), MessageTypeDefOf.SilentInput);
			}
			FinalizeDesignationSucceeded();
		}
		else
		{
			if (base.Def.messageFailure != null)
			{
				Messages.Message(base.Def.messageFailure.Translate(), MessageTypeDefOf.RejectInput);
			}
			FinalizeDesignationFailed();
		}
	}

	private void DesignateMultiThing(IEnumerable<Thing> things)
	{
		foreach (Thing thing in things)
		{
			DesignateThing(thing);
		}
	}

	private IEnumerable<MapCellHighlighter.Request> SelectHighlightedCells()
	{
		List<Thing> allTheThings = base.Map.listerThings.AllThings;
		for (int i = 0; i < allTheThings.Count; i++)
		{
			Thing thing = allTheThings[i];
			if (thing.def.selectable && base.Dragger.SelectedArea.Contains(thing.Position) && CanDesignateThing(thing).Accepted)
			{
				yield return new MapCellHighlighter.Request(thing.Position, highlightMaterial);
			}
		}
	}
}
