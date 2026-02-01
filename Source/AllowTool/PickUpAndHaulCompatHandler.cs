using System;
using RimWorld;
using Verse;

namespace AllowTool;

public static class PickUpAndHaulCompatHandler
{
	public static void Apply()
	{
		try
		{
			Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("PickUpAndHaul.WorkGiver_HaulToInventory");
			if (!(typeInAnyAssembly == null))
			{
				if (!typeof(WorkGiver_HaulGeneral).IsAssignableFrom(typeInAnyAssembly))
				{
					throw new Exception("Expected work giver to extend WorkGiver_HaulGeneral");
				}
				if (typeInAnyAssembly.GetConstructor(Type.EmptyTypes) == null)
				{
					throw new Exception("Expected work giver to have parameterless constructor");
				}
				WorkGiver_HaulGeneral haulWorkGiver = (WorkGiver_HaulGeneral)Activator.CreateInstance(typeInAnyAssembly);
				WorkGiver_HaulUrgently.JobOnThingDelegate = (Pawn pawn, Thing thing, bool forced) => haulWorkGiver.ShouldSkip(pawn, forced) ? null : haulWorkGiver.JobOnThing(pawn, thing, forced);
				AllowToolController.Logger.Message("Applied compatibility patch for \"Pick Up And Haul\"");
			}
		}
		catch (Exception e)
		{
			AllowToolController.Logger.ReportException(e, null, reportOnceOnly: false, "Pick Up And Haul compatibility layer application");
		}
	}
}
