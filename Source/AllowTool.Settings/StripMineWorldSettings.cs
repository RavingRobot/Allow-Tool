using Verse;

namespace AllowTool.Settings;

public class StripMineWorldSettings : IExposable, IConfigurableStripMineSettings
{
	private const int DefaultSpacingX = 5;

	private const int DefaultSpacingY = 5;

	private int hSpacing = 5;

	private int vSpacing = 5;

	private bool variableGridOffset = true;

	private bool showWindow = true;

	private IntVec2 lastGridOffset;

	public int HorizontalSpacing
	{
		get
		{
			return hSpacing;
		}
		set
		{
			hSpacing = value;
		}
	}

	public int VerticalSpacing
	{
		get
		{
			return vSpacing;
		}
		set
		{
			vSpacing = value;
		}
	}

	public bool VariableGridOffset
	{
		get
		{
			return variableGridOffset;
		}
		set
		{
			variableGridOffset = value;
		}
	}

	public bool ShowWindow
	{
		get
		{
			return showWindow;
		}
		set
		{
			showWindow = value;
		}
	}

	public IntVec2 LastGridOffset
	{
		get
		{
			return lastGridOffset;
		}
		set
		{
			lastGridOffset = value;
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref hSpacing, "hSpacing", 5);
		Scribe_Values.Look(ref vSpacing, "vSpacing", 5);
		Scribe_Values.Look(ref variableGridOffset, "variableOffset", defaultValue: true);
		Scribe_Values.Look(ref showWindow, "showWindow", defaultValue: true);
		Scribe_Values.Look(ref lastGridOffset, "lastOffset");
	}

	public StripMineWorldSettings Clone()
	{
		return (StripMineWorldSettings)MemberwiseClone();
	}
}
