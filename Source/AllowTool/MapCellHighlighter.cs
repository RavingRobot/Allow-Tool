using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AllowTool;

public class MapCellHighlighter
{
	public class Request
	{
		public readonly IntVec3 Cell;

		public readonly Material Material;

		public Request(IntVec3 cell, Material material)
		{
			Cell = cell;
			Material = material;
		}
	}

	private class CachedHighlight
	{
		public readonly Vector3 DrawPosition;

		public readonly Material Material;

		public CachedHighlight(Vector3 drawPosition, Material material)
		{
			DrawPosition = drawPosition;
			Material = material;
		}
	}

	private readonly List<CachedHighlight> cachedHighlightQuadPositions = new List<CachedHighlight>();

	private readonly Func<IEnumerable<Request>> cellSelector;

	private readonly float recacheInterval;

	private readonly AltitudeLayer drawAltitude;

	private float nextHighlightRecacheTime;

	public MapCellHighlighter(Func<IEnumerable<Request>> cellSelector, float recacheInterval = 0.5f, AltitudeLayer drawAltitude = AltitudeLayer.MetaOverlays)
	{
		this.cellSelector = cellSelector ?? throw new ArgumentNullException("cellSelector");
		this.recacheInterval = recacheInterval;
		this.drawAltitude = drawAltitude;
	}

	public void ClearCachedCells()
	{
		nextHighlightRecacheTime = 0f;
		cachedHighlightQuadPositions.Clear();
	}

	public void DrawCellHighlights()
	{
		if (Time.time >= nextHighlightRecacheTime)
		{
			RecacheCellPositions();
		}
		DrawCachedCellHighlights();
	}

	private void RecacheCellPositions()
	{
		nextHighlightRecacheTime = Time.time + recacheInterval;
		cachedHighlightQuadPositions.Clear();
		float y = drawAltitude.AltitudeFor();
		foreach (Request item in cellSelector())
		{
			cachedHighlightQuadPositions.Add(new CachedHighlight(new Vector3((float)item.Cell.x + 0.5f, y, (float)item.Cell.z + 0.5f), item.Material));
		}
	}

	private void DrawCachedCellHighlights()
	{
		for (int i = 0; i < cachedHighlightQuadPositions.Count; i++)
		{
			CachedHighlight cachedHighlight = cachedHighlightQuadPositions[i];
			Graphics.DrawMesh(MeshPool.plane10, cachedHighlight.DrawPosition, Quaternion.identity, cachedHighlight.Material, 0);
		}
	}

	private Color Color32ToColor(Color32 c)
	{
		return new Color
		{
			r = (float)(int)c.r / 255f,
			g = (float)(int)c.g / 255f,
			b = (float)(int)c.b / 255f,
			a = (float)(int)c.a / 255f
		};
	}
}
