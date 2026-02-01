using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AllowTool;

internal class HaulUrgentlyCacheHandler
{
	private readonly struct ThingsCacheEntry
	{
		private const float ExpireTime = 1f;

		private readonly float createdTime;

		public List<Thing> DesignatedThings { get; }

		public List<Thing> DesignatedHaulableThings { get; }

		public ThingsCacheEntry(float currentTime, List<Thing> designatedThings, List<Thing> designatedHaulableThings)
		{
			createdTime = currentTime;
			DesignatedThings = designatedThings;
			DesignatedHaulableThings = designatedHaulableThings;
		}

		public bool IsValid(float currentTime)
		{
			return createdTime > 0f && currentTime < createdTime + 1f;
		}
	}

	private class NoStorageSpaceTracker
	{
		private readonly struct CacheEntry
		{
			public int ExpirationUpdate { get; }

			public GlobalTargetInfo Target { get; }

			public CacheEntry(int expirationUpdate, GlobalTargetInfo target)
			{
				ExpirationUpdate = expirationUpdate;
				Target = target;
			}
		}

		private const int RecacheUpdateInterval = 50;

		private readonly HaulUrgentlyCacheHandler cacheHandler;

		private readonly List<CacheEntry> targetCache = new List<CacheEntry>();

		private readonly List<GlobalTargetInfo> outputList = new List<GlobalTargetInfo>();

		private readonly Comparison<GlobalTargetInfo> consistentTargetOrderComparer = (GlobalTargetInfo t1, GlobalTargetInfo t2) => t1.Thing.thingIDNumber.CompareTo(t2.Thing.thingIDNumber);

		private int cachedForMapId = -1;

		private HashSet<Thing> reservedThingsCache;

		private int reservedThingsCacheExpirationUpdate = int.MinValue;

		public NoStorageSpaceTracker(HaulUrgentlyCacheHandler cacheHandler)
		{
			this.cacheHandler = cacheHandler;
		}

		public void ProcessDesignations(Map map, int currentUpdate, float currentTime)
		{
			PruneExpiredEntries();
			VerifyCachedMap();
			IReadOnlyList<Thing> designatedThings = cacheHandler.GetDesignatedThingsForMap(map, currentTime);
			if (designatedThings.Count != 0)
			{
				UpdateCachedReservations();
				ProcessDesignatedThings();
			}
			void ProcessDesignatedThings()
			{
				int num = currentUpdate % 50;
				for (int i = 0; i < designatedThings.Count; i++)
				{
					Thing thing = designatedThings[i];
					if (thing.thingIDNumber % 50 == num && thing.Spawned && !reservedThingsCache.Contains(thing) && HasNoHaulDestination(thing))
					{
						targetCache.Insert(0, new CacheEntry(currentUpdate + 50 - 1, new GlobalTargetInfo(thing)));
					}
				}
			}
			void PruneExpiredEntries()
			{
				int num = targetCache.Count - 1;
				while (num >= 0 && currentUpdate > targetCache[num].ExpirationUpdate)
				{
					targetCache.RemoveAt(num);
					num--;
				}
			}
			void UpdateCachedReservations()
			{
				if (currentUpdate > reservedThingsCacheExpirationUpdate + 50)
				{
					reservedThingsCacheExpirationUpdate = currentUpdate + 50;
					reservedThingsCache = GetReservedThingsOnMap(map);
				}
			}
			void VerifyCachedMap()
			{
				if (cachedForMapId != map.uniqueID)
				{
					cachedForMapId = map.uniqueID;
					ClearCache();
				}
			}
		}

		public List<GlobalTargetInfo> GetDesignatedThingsWithoutStorage()
		{
			outputList.Clear();
			for (int i = 0; i < targetCache.Count; i++)
			{
				outputList.Add(targetCache[i].Target);
			}
			outputList.Sort(consistentTargetOrderComparer);
			return outputList;
		}

		public void ClearCache()
		{
			targetCache.Clear();
		}

		private HashSet<Thing> GetReservedThingsOnMap(Map map)
		{
			return new HashSet<Thing>(map.reservationManager.AllReservedThings());
		}

		private bool HasNoHaulDestination(Thing t)
		{
			IntVec3 foundCell;
			IHaulDestination haulDestination;
			return !StoreUtility.TryFindBestBetterStorageFor(t, null, t.Map, StoreUtility.CurrentStoragePriorityOf(t), Faction.OfPlayer, out foundCell, out haulDestination);
		}
	}

	private readonly Dictionary<Map, ThingsCacheEntry> cacheEntries = new Dictionary<Map, ThingsCacheEntry>();

	private readonly HashSet<Thing> workThingsSet = new HashSet<Thing>();

	private readonly NoStorageSpaceTracker noStorageTracker;

	public HaulUrgentlyCacheHandler()
	{
		noStorageTracker = new NoStorageSpaceTracker(this);
	}

	public IReadOnlyList<Thing> GetDesignatedThingsForMap(Map map, float currentTime)
	{
		RecacheIfNeeded(map, currentTime);
		return cacheEntries[map].DesignatedThings;
	}

	public IReadOnlyList<Thing> GetDesignatedAndHaulableThingsForMap(Map map, float currentTime)
	{
		RecacheIfNeeded(map, currentTime);
		return cacheEntries[map].DesignatedHaulableThings;
	}

	public List<GlobalTargetInfo> GetDesignatedThingsWithoutStorageSpace()
	{
		return noStorageTracker.GetDesignatedThingsWithoutStorage();
	}

	public void ClearCacheForMap(Map map)
	{
		cacheEntries.Remove(map);
	}

	public void ClearCacheForAllMaps()
	{
		cacheEntries.Clear();
	}

	public void ProcessCacheEntries(int currentFrame, float currentTime)
	{
		if (AllowToolController.Instance.Handles.StorageSpaceAlertSetting.Value)
		{
			Map currentMap = Find.CurrentMap;
			if (currentMap != null)
			{
				noStorageTracker.ProcessDesignations(currentMap, currentFrame, currentTime);
			}
			else
			{
				noStorageTracker.ClearCache();
			}
		}
	}

	private void RecacheIfNeeded(Map map, float currentTime)
	{
		if (!cacheEntries.TryGetValue(map, out var value) || !value.IsValid(currentTime))
		{
			List<Thing> list = value.DesignatedThings ?? new List<Thing>();
			GetHaulUrgentlyDesignatedThings(map, list);
			List<Thing> list2 = value.DesignatedHaulableThings ?? new List<Thing>();
			GetMapHaulables(map, list, list2);
			cacheEntries[map] = new ThingsCacheEntry(currentTime, list, list2);
		}
	}

	private void GetHaulUrgentlyDesignatedThings(Map map, ICollection<Thing> targetList)
	{
		targetList.Clear();
		List<Designation> allDesignations = map.designationManager.AllDesignations;
		for (int i = 0; i < allDesignations.Count; i++)
		{
			Designation designation = allDesignations[i];
			if (designation.def == AllowToolDefOf.HaulUrgentlyDesignation && designation.target.Thing != null)
			{
				targetList.Add(designation.target.Thing);
			}
		}
	}

	private void GetMapHaulables(Map map, IReadOnlyList<Thing> intersectWith, ICollection<Thing> targetList)
	{
		targetList.Clear();
		for (int i = 0; i < intersectWith.Count; i++)
		{
			workThingsSet.Add(intersectWith[i]);
		}
		List<Thing> list = map.listerHaulables.ThingsPotentiallyNeedingHauling();
		for (int j = 0; j < list.Count; j++)
		{
			if (workThingsSet.Contains(list[j]))
			{
				targetList.Add(list[j]);
			}
		}
		workThingsSet.Clear();
	}
}
