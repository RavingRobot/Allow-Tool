using System.Collections.Generic;
using Verse;

namespace AllowTool.Settings;

public class PartyHuntSettings : IExposable
{
	private HashSet<int> partyHuntingPawns = new HashSet<int>();

	private bool autoFinishOff = true;

	private bool huntDesignatedOnly;

	private bool unforbidDrops;

	public bool AutoFinishOff
	{
		get
		{
			return autoFinishOff;
		}
		set
		{
			autoFinishOff = value;
		}
	}

	public bool HuntDesignatedOnly
	{
		get
		{
			return huntDesignatedOnly;
		}
		set
		{
			huntDesignatedOnly = value;
		}
	}

	public bool UnforbidDrops
	{
		get
		{
			return unforbidDrops;
		}
		set
		{
			unforbidDrops = value;
		}
	}

	public void ExposeData()
	{
		List<int> list = new List<int>(partyHuntingPawns);
		Scribe_Collections.Look(ref list, "pawns", LookMode.Undefined);
		partyHuntingPawns = new HashSet<int>(list);
		Scribe_Values.Look(ref autoFinishOff, "finishOff", defaultValue: true);
		Scribe_Values.Look(ref huntDesignatedOnly, "designatedOnly", defaultValue: false);
		Scribe_Values.Look(ref unforbidDrops, "unforbid", defaultValue: false);
	}

	public bool PawnIsPartyHunting(Pawn pawn)
	{
		return partyHuntingPawns.Contains(pawn.thingIDNumber);
	}

	public void TogglePawnPartyHunting(Pawn pawn, bool enable)
	{
		int thingIDNumber = pawn.thingIDNumber;
		if (enable)
		{
			partyHuntingPawns.Add(thingIDNumber);
		}
		else
		{
			partyHuntingPawns.Remove(thingIDNumber);
		}
	}
}
