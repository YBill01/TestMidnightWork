using NativeTrees;
using Unity.Collections;
using UnityEngine;
using YB.HFSM;
using RandomMath = Unity.Mathematics.Random;

public class GameNightState : State
{
	private Game _game;

	private GameConfigService _config;
	private EventsService _events;
	private Factory _unitFactory;

	private bool _targetsReGenerate;

	public GameNightState(Game game, GameConfigService config, EventsService events, Factory unitFactory)
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

		UnityEngine.Debug.Log("Game::Night");
	}

	protected override void OnExit()
	{
		
	}

	private void SpawnUnitsQuery(UnitType unitType, int count, NativeArray<SpawnParams> spawnArray)
	{
		Bounds bounds = unitType is UnitType.Agent ? _config.asylumAreaBounds : _config.areaBounds;

		for (int i = 0; i < count; i++)
		{
			spawnArray[i] = new SpawnParams()
			{
				unitType = unitType,
				position = RandomUtils.PointInBounds(bounds),
				direction = Random.insideUnitSphere
			};
		}
	}

	public void UpdateOctree(NativeArray<Unit> units, NativeArray<Target> targets, NativeOctree<Unit> octree, NativeReference<bool> octreeIsRebuild)
	{
		// some realisation...
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
			asylumAreaBounds = _config.asylumAreaBounds
		};

		_targetsReGenerate = false;

		return targetJob;
	}
}