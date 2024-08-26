using NativeTrees;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using YB.HFSM;
using Random = UnityEngine.Random;
using RandomMath = Unity.Mathematics.Random;

public class GameDayState : State
{
	private Game _game;

	private GameConfigService _config;
	private EventsService _events;
	private Factory _unitFactory;

	private float _spawnInterestTimer;

	private bool _targetsReGenerate;

	public GameDayState(Game game, GameConfigService config, EventsService events, Factory unitFactory)
	{
		_game = game;

		_config = config;
		_events = events;
		_unitFactory = unitFactory;
	}

	protected override void OnEnter()
	{
		_targetsReGenerate = true;

		_game.spawnUnitsQuery = SpawnUnitsQuery;
		_game.updateOctree = UpdateOctree;
		_game.updateTargets = UpdateTargets;

		UnityEngine.Debug.Log("Game::Day");
	}

	protected override void OnExit()
	{
		_spawnInterestTimer = 0.0f;
	}

	protected override void OnUpdate()
	{
		SpawnInterests();
	}

	private void SpawnInterests()
	{
		if (_game.NumInterests < _config.maxNumInterests)
		{
			_spawnInterestTimer += Time.deltaTime;

			if (_spawnInterestTimer >= _game.SpawnRateInterests)
			{
				_game.AddSpawnUnitsQuery(UnitType.Interest, 1);

				_spawnInterestTimer %= _game.SpawnRateInterests;
			}
		}
	}

	private void SpawnUnitsQuery(UnitType unitType, int count, NativeArray<SpawnParams> spawnArray)
	{
		for (int i = 0; i < count; i++)
		{
			spawnArray[i] = new SpawnParams()
			{
				unitType = unitType,
				position = RandomUtils.PointInBounds(_config.areaBounds),
				direction = Random.insideUnitSphere
			};
		}
	}

	public void UpdateOctree(NativeArray<Unit> units, NativeArray<Target> targets, NativeOctree<Unit> octree, NativeReference<bool> octreeIsRebuild)
	{
		if (octreeIsRebuild.Value)
		{
			Stopwatch sw = Stopwatch.StartNew();

			octree.Clear();

			OctreeInsertUnitsJob octreeInsertUnitsJob = new OctreeInsertUnitsJob()
			{
				units = units,
				octree = octree,
			};
			JobHandle octreeInsertUnitsHandle = octreeInsertUnitsJob.Schedule();
			
			OctreeNearestTargetJob octreeNearestTargetJob = new OctreeNearestTargetJob()
			{
				units = units,
				targets = targets,
				octree = octree
			};
			octreeNearestTargetJob.Schedule(octreeInsertUnitsHandle).Complete();

			sw.Stop();
			UnityEngine.Debug.Log($"octree: {sw.Elapsed.TotalMilliseconds}ms");

			octreeIsRebuild.Value = false;
		}
	}

	private TargetJob UpdateTargets(NativeArray<Unit> units, NativeArray<Target> targets, NativeArray<RandomMath> randomGenerators)
	{
		TargetJob targetJob = new TargetJob()
		{
			units = units,
			targets = targets,
			randomGenerators = randomGenerators,
			reGenerate = _targetsReGenerate,
			areaBounds = _config.areaBounds,
			asylumAreaBounds = _config.areaBounds
		};

		_targetsReGenerate = false;

		return targetJob;
	}
}
