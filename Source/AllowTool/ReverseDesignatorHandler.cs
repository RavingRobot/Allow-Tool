using System;
using System.Collections.Generic;
using Verse;

namespace AllowTool;

public static class ReverseDesignatorHandler
{
	internal static void InjectReverseDesignators(ReverseDesignatorDatabase database)
	{
		List<Designator> allDesignators = database.AllDesignators;
		foreach (ReverseDesignatorDef allDef in DefDatabase<ReverseDesignatorDef>.AllDefs)
		{
			try
			{
				if (AllowToolController.Instance.Handles.IsReverseDesignatorEnabled(allDef))
				{
					Designator designator = InstantiateThingDesignator(allDef);
					if (Current.Game.Rules.DesignatorAllowed(designator))
					{
						allDesignators.Add(designator);
					}
				}
			}
			catch (Exception innerException)
			{
				throw new Exception("Failed to create reverse designator", innerException);
			}
		}
		AllowToolController.Instance.ScheduleDesignatorDependencyRefresh();
	}

	private static Designator InstantiateThingDesignator(ReverseDesignatorDef reverseDef)
	{
		Type type = reverseDef.designatorClass ?? reverseDef.designatorDef.designatorClass;
		try
		{
			return (Designator)Activator.CreateInstance(type);
		}
		catch (Exception innerException)
		{
			throw new Exception("Failed to instantiate designator " + type.FullName + " (def " + reverseDef.defName + ")", innerException);
		}
	}
}
