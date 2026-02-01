using System;
using HugsLib.Utils;
using UnityEngine;
using Verse;

namespace AllowTool;

public class Dialog_StripMineConfiguration : Window
{
	public delegate void ClosingCallback(bool accept);

	private const int SpacingMinValue = 1;

	private const int SpacingMaxValue = 50;

	private const float RowHeight = 36f;

	private const float Spacing = 4f;

	private const float LabelColumnWidthPercent = 0.666f;

	private readonly IConfigurableStripMineSettings settings;

	public Vector2 WindowPosition
	{
		get
		{
			return new Vector2(windowRect.x, windowRect.y);
		}
		set
		{
			windowRect.x = value.x;
			windowRect.y = value.y;
		}
	}

	public override Vector2 InitialSize => new Vector2(320f, Margin * 2f + 180f + 20f);

	public event Action<IConfigurableStripMineSettings> SettingsChanged;

	public event ClosingCallback Closing;

	public Dialog_StripMineConfiguration(IConfigurableStripMineSettings settings)
	{
		this.settings = settings;
		draggable = true;
		focusWhenOpened = false;
		forceCatchAcceptAndCancelEventEvenIfUnfocused = true;
		preventCameraMotion = false;
		layer = WindowLayer.SubSuper;
	}

	protected override void SetInitialSizeAndPosition()
	{
		Vector2 windowPosition = WindowPosition;
		base.SetInitialSizeAndPosition();
		windowRect.x = windowPosition.x;
		windowRect.y = windowPosition.y;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Listing_Standard listing_Standard = new Listing_Standard
		{
			maxOneColumn = true
		};
		listing_Standard.Begin(inRect);
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleLeft;
		bool changed = false;
		settings.HorizontalSpacing = DoIntSpinner("StripMine_win_horizontalSpacing".Translate(), settings.HorizontalSpacing, listing_Standard, ref changed);
		listing_Standard.Gap(4f);
		settings.VerticalSpacing = DoIntSpinner("StripMine_win_verticalSpacing".Translate(), settings.VerticalSpacing, listing_Standard, ref changed);
		listing_Standard.Gap(4f);
		settings.VariableGridOffset = DoCustomCheckbox("StripMine_win_variableOffset", "StripMine_win_variableOffset_tip", settings.VariableGridOffset, listing_Standard, ref changed);
		listing_Standard.Gap(4f);
		settings.ShowWindow = DoCustomCheckbox("StripMine_win_showWindow", "StripMine_win_showWindow_tip", settings.ShowWindow, listing_Standard, ref changed);
		listing_Standard.Gap(8f);
		Rect rect = listing_Standard.GetRect(36f);
		Rect rect2 = rect.LeftPart(0.334f);
		if (Widgets.ButtonText(rect2, "CancelButton".Translate()))
		{
			CancelAndClose();
		}
		Rect rect3 = rect.RightPartPixels(rect.width - rect2.width - 4f);
		GUI.color = Color.green;
		if (Widgets.ButtonText(rect3, "AcceptButton".Translate()))
		{
			AcceptAndClose();
		}
		GUI.color = Color.white;
		if (changed)
		{
			this.SettingsChanged?.Invoke(settings);
		}
		Text.Anchor = anchor;
		listing_Standard.End();
		ConfineWindowToScreenArea();
	}

	public void CancelAndClose()
	{
		this.Closing?.Invoke(accept: false);
		Close(doCloseSound: false);
	}

	private void AcceptAndClose()
	{
		this.Closing?.Invoke(accept: true);
		Close(doCloseSound: false);
	}

	public override void OnAcceptKeyPressed()
	{
		AcceptAndClose();
		Event.current.Use();
	}

	public override void OnCancelKeyPressed()
	{
		CancelAndClose();
		Event.current.Use();
	}

	private int DoIntSpinner(string label, int value, Listing_Standard listing, ref bool changed)
	{
		Rect rect = listing.GetRect(36f);
		if (DoTipArea(rect) && Event.current.isScrollWheel)
		{
			int delta = ((Event.current.delta.y < 0f) ? 1 : (-1));
			TryChangeValue(delta, ref changed);
			Event.current.Use();
		}
		TextAnchor anchor = Text.Anchor;
		Text.Anchor = TextAnchor.MiddleLeft;
		Rect rect2 = rect.LeftPart(0.666f);
		Widgets.Label(rect2, label);
		Rect rect3 = rect.RightPartPixels(rect.width - rect2.width);
		Text.Anchor = TextAnchor.MiddleCenter;
		Widgets.Label(rect3, value.ToString());
		if (Widgets.ButtonText(rect3.LeftPart(0.333f), "-"))
		{
			TryChangeValue(-1, ref changed);
		}
		if (Widgets.ButtonText(rect3.RightPart(0.333f), "+"))
		{
			TryChangeValue(1, ref changed);
		}
		Text.Anchor = anchor;
		return value;
		void TryChangeValue(int num2, ref bool hasChanged)
		{
			int num = Mathf.Clamp(value + num2 * ((!HugsLibUtility.ShiftIsHeld) ? 1 : 5), 1, 50);
			if (num != value)
			{
				value = num;
				hasChanged = true;
			}
		}
	}

	private bool DoCustomCheckbox(string labelKey, string tooltipKey, bool value, Listing_Standard listing, ref bool changed)
	{
		Rect rect = listing.GetRect(36f);
		DoTipArea(rect, tooltipKey.Translate());
		Widgets.Label(rect, labelKey.Translate());
		Vector2 topLeft = new Vector2(rect.x + rect.width * 0.666f, rect.y + (rect.height - 24f) / 2f);
		bool flag = value;
		Widgets.Checkbox(topLeft, ref value);
		if (value != flag)
		{
			changed = true;
		}
		return value;
	}

	private bool DoTipArea(Rect rect, string tooltip = null)
	{
		if (Mouse.IsOver(rect))
		{
			Widgets.DrawHighlight(rect);
			if (tooltip != null)
			{
				TooltipHandler.TipRegion(rect, tooltip);
			}
			return true;
		}
		return false;
	}

	private void ConfineWindowToScreenArea()
	{
		if (windowRect.x < 0f)
		{
			windowRect.x = 0f;
		}
		if (windowRect.y < 0f)
		{
			windowRect.y = 0f;
		}
		if (windowRect.xMax > (float)UI.screenWidth)
		{
			windowRect.x = (float)UI.screenWidth - windowRect.width;
		}
		if (windowRect.yMax > (float)UI.screenHeight)
		{
			windowRect.y = (float)UI.screenHeight - windowRect.height;
		}
	}
}
