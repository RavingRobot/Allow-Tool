using System;
using System.Xml.Serialization;
using HugsLib.Settings;
using UnityEngine;

namespace AllowTool.Settings;

[Serializable]
public class StripMineGlobalSettings : SettingHandleConvertible, IEquatable<StripMineGlobalSettings>
{
	[XmlElement]
	public Vector2 WindowPosition { get; set; }

	public override bool ShouldBeSaved => !Equals(new StripMineGlobalSettings());

	public override void FromString(string settingValue)
	{
		SettingHandleConvertibleUtility.DeserializeValuesFromString(settingValue, (object)this);
	}

	public bool Equals(StripMineGlobalSettings other)
	{
		return other != null && other.WindowPosition == WindowPosition;
	}

	public override string ToString()
	{
		return SettingHandleConvertibleUtility.SerializeValuesToString((object)this);
	}

	public StripMineGlobalSettings Clone()
	{
		return (StripMineGlobalSettings)MemberwiseClone();
	}
}
