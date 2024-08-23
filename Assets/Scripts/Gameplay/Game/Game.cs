using Cysharp.Threading.Tasks;
using NativeTrees;
using System;
//using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;
using RandomMath = Unity.Mathematics.Random;

public class Game : IDisposable
{
	/// <summary>
	/// optimization for parallel jobs
	/// </summary>
	private const int _JOB_PARALLEL_COUNT = 64; //innerloopBatchCount

	private GameConfigService _config;
	private EventsService _events;
	private Factory _factory;

	private GameStateMachine _gameFSM;

	private NativeOctree<Unit> _octree;
	private NativeArray<bool> _octreeIsRebuild;

	private NativeArray<RandomMath> _randomGenerators;

	private NativeList<Unit> _unitsList;
	private NativeList<Target> _targetsList;
	private TransformAccessArray _transformAccessArray;
	private NativeArray<int> _numUnits;
	private NativeArray<int> _uniqueIndex;

	public int NumAgents => _numUnits[(int)UnitType.Agent];
	public int NumInterests => _numUnits[(int)UnitType.Interest];
	public int NumPredators => _numUnits[(int)UnitType.Predator];

	private NativeArray<float> _velocities;
	private NativeArray<float> _idleRadiuses;
	private NativeArray<float> _unitBoundsSizes;

	public int SpawnCountAgents { get; private set; }
	public float SpawnRateInterests { get; private set; }

	private bool _nightMode;
	private bool DayMode() => !_nightMode;
	private bool NightMode() => _nightMode;

	public delegate void SpawnUnitsQuery(UnitType unitType, int count, NativeArray<SpawnParams> spawnArray);
	public SpawnUnitsQuery spawnUnitsQuery;

	public delegate void UpdateOctree(NativeArray<Unit> units, NativeArray<Target> targets, NativeOctree<Unit> octree, NativeArray<bool> octreeIsRebuild);
	public UpdateOctree updateOctree;

	public delegate TargetJob UpdateTargets(NativeArray<Unit> units, NativeArray<Target> targets, NativeArray<RandomMath> randomGenerators);
	public UpdateTargets updateTargets;

	private NativeList<SpawnParams> _spawnParamsList;
	private bool _isSpawnUnits;

	private NativeList<DestroyParams> _destroyParamsList;
	private bool _isDestroyUnits;

	private bool _infoIsDirty;

	public Game(GameConfigService config, EventsService events, Factory factory)
	{
		_config = config;
		_events = events;
		_factory = factory;

		SpawnCountAgents = _config.spawnCountAgents;
		SpawnRateInterests = _config.spawnRateInterests;

		_nightMode = _config.nightMode;

		GameDayState gameDayState = new GameDayState(this, _config, _events, _factory);
		GameNightState gameNightState = new GameNightState(this, _config, _events, _factory);

		_gameFSM = new GameStateMachine(gameDayState, gameNightState);

		gameDayState.AddTransition(gameNightState, NightMode);
		gameNightState.AddTransition(gameDayState, DayMode);
	}

	public void Init()
	{
		_gameFSM.Init();

		_octree = new NativeOctree<Unit>(_config.octreeBounds, Allocator.Persistent);
		_octreeIsRebuild = new NativeArray<bool>(1, Allocator.Persistent);

		_randomGenerators = new NativeArray<RandomMath>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
		for (int i = 0; i < _randomGenerators.Length; i++)
		{
			_randomGenerators[i] = RandomMath.CreateFromIndex((uint)i);
		}

		_unitsList = new NativeList<Unit>(Allocator.Persistent);
		_targetsList = new NativeList<Target>(Allocator.Persistent);
		_transformAccessArray = new TransformAccessArray(0);

		Array unitTypes = Enum.GetValues(typeof(UnitType));

		_numUnits = new NativeArray<int>(unitTypes.Length, Allocator.Persistent);
		_uniqueIndex = new NativeArray<int>(1, Allocator.Persistent);

		_velocities = new NativeArray<float>(unitTypes.Length, Allocator.Persistent);
		SetVelocity(UnitType.Agent, _config.speedAgents);
		SetVelocity(UnitType.Predator, _config.speedPredators);
		SetVelocity(UnitType.Interest, 0.1f);

		_idleRadiuses = new NativeArray<float>(unitTypes.Length, Allocator.Persistent);
		_idleRadiuses[(int)UnitType.Agent] = _factory.Get(UnitType.Agent).UnitData.idleRadius;
		_idleRadiuses[(int)UnitType.Predator] = _factory.Get(UnitType.Predator).UnitData.idleRadius;
		_idleRadiuses[(int)UnitType.Interest] = _factory.Get(UnitType.Interest).UnitData.idleRadius;

		_unitBoundsSizes = new NativeArray<float>(unitTypes.Length, Allocator.Persistent);
		_unitBoundsSizes[(int)UnitType.Agent] = _factory.Get(UnitType.Agent).UnitData.boundsSize;
		_unitBoundsSizes[(int)UnitType.Predator] = _factory.Get(UnitType.Predator).UnitData.boundsSize;
		_unitBoundsSizes[(int)UnitType.Interest] = _factory.Get(UnitType.Interest).UnitData.boundsSize;

		StartGame().Forget();
	}

	private async UniTaskVoid StartGame()
	{
		await UniTask.NextFrame(PlayerLoopTiming.LastPostLateUpdate);
		
		AddSpawnUnitsQuery(UnitType.Agent, _config.startNumAgents);
		AddSpawnUnitsQuery(UnitType.Interest, _config.startNumInterests);
		AddSpawnUnitsQuery(UnitType.Predator, _config.startNumPredators);
	}

	public void SetVelocity(UnitType unitType, float value)
	{
		_velocities[(int)unitType] = value;
	}

	public void SetSpawnRateInterests(float value)
	{
		SpawnRateInterests = value;
	}

	public void SetSpawnCountAgents(int value)
	{
		SpawnCountAgents = value;
	}

	public void SetNightMode(bool value)
	{
		_nightMode = value;
	}

	public void TakeInfo()
	{
		_infoIsDirty = true;
	}

	public void OnUpdate(float deltaTime)
	{
		_gameFSM.Update();

		SpawnUnits();

		int unitsCount = _unitsList.Length;

		NativeArray<Unit> units = _unitsList.AsDeferredJobArray();
		NativeArray<Target> targets = _targetsList.AsDeferredJobArray();

		TargetJob targetJob = updateTargets(units, targets, _randomGenerators);
		JobHandle targetHandle = targetJob.Schedule(unitsCount, _JOB_PARALLEL_COUNT);
		
		MoveJob moveJob = new MoveJob()
		{
			units = units,
			targets = targets,
			velocities = _velocities,
			deltaTime = deltaTime,
		};
		JobHandle moveHandle = moveJob.Schedule(_transformAccessArray, targetHandle);

		NativeList<SpawnParams> spawnParams = new NativeList<SpawnParams>(1024, Allocator.TempJob);
		NativeList<DestroyParams> destroyParams = new NativeList<DestroyParams>(unitsCount, Allocator.TempJob);
		
		TargetGoalJob targetGoalJob = new TargetGoalJob()
		{
			units = units,
			targets = targets,
			outSpawnParams = spawnParams.AsParallelWriter(),
			inSpawnCountAgents = SpawnCountAgents,
			outDestroyParams = destroyParams.AsParallelWriter(),
			outOctreeIsRebuild = _octreeIsRebuild
		};
		JobHandle targetGoalHandle = targetGoalJob.Schedule(unitsCount, _JOB_PARALLEL_COUNT, moveHandle);
		targetGoalHandle.Complete();

		if (spawnParams.Length > 0)
		{
			AddSpawnUnitsQuery(spawnParams.AsArray());
		}
		spawnParams.Dispose();

		if (destroyParams.Length > 0)
		{
			AddDestroyUnitsQuery(destroyParams.AsArray());
		}
		destroyParams.Dispose();
		
		DestroyUnits();

		updateOctree(units, targets, _octree, _octreeIsRebuild);

		UpdateInfo();
	}

	public void AddSpawnUnitsQuery(UnitType unitType, int count)
	{
		NativeArray<SpawnParams> spawnArray = new NativeArray<SpawnParams>(count, Allocator.Temp);

		spawnUnitsQuery(unitType, count, spawnArray);

		AddSpawnUnitsQuery(spawnArray);
	}
	public void AddSpawnUnitsQuery(UnitType unitType, int count, float3 position)
	{
		NativeArray<SpawnParams> spawnArray = new NativeArray<SpawnParams>(count, Allocator.Temp);
		
		for (int i = 0; i < count; i++)
		{
			spawnArray[i] = new SpawnParams()
			{
				unitType = unitType,
				position = position,
				direction = Random.insideUnitSphere
			};
		}

		AddSpawnUnitsQuery(spawnArray);
	}
	public void AddSpawnUnitsQuery(NativeArray<SpawnParams> spawnArray)
	{
		if (_spawnParamsList.IsEmpty)
		{
			_spawnParamsList = new NativeList<SpawnParams>(spawnArray.Length, Allocator.TempJob);
		}

		_spawnParamsList.AddRange(spawnArray);

		spawnArray.Dispose();

		_isSpawnUnits = true;
		_infoIsDirty = true;
	}

	public void AddDestroyUnitsQuery(NativeArray<DestroyParams> destroyArray)
	{
		if (_destroyParamsList.IsEmpty)
		{
			_destroyParamsList = new NativeList<DestroyParams>(destroyArray.Length, Allocator.TempJob);
		}

		_destroyParamsList.AddRange(destroyArray);

		destroyArray.Dispose();

		_isDestroyUnits = true;
		_infoIsDirty = true;
	}

	private void SpawnUnits()
	{
		if (_isSpawnUnits)
		{
			//var sw = Stopwatch.StartNew();

			SpawnJob spawnJob = new SpawnJob()
			{
				inSpawnParams = _spawnParamsList,
				inIdleRadiuses = _idleRadiuses,
				inUnitBoundsSizes = _unitBoundsSizes,
				numUnits = _numUnits,
				uniqueIndex = _uniqueIndex,
				inFactory = _factory,
				outUnits = _unitsList,
				outTargets = _targetsList,
				outTransformAccessArray = _transformAccessArray,
			};
			spawnJob.Execute();

			//sw.Stop();
			//UnityEngine.Debug.Log($"spawn: {sw.Elapsed.TotalMilliseconds}");

			_spawnParamsList.Dispose();

			_isSpawnUnits = false;
		}
	}

	private void DestroyUnits()
	{
		if (_isDestroyUnits)
		{
			//var sw = Stopwatch.StartNew();

			DestroyJob destroyJob = new DestroyJob()
			{
				inDestroyParams = _destroyParamsList,
				numUnits = _numUnits,
				inFactory = _factory,
				outUnits = _unitsList,
				outTargets = _targetsList,
				outTransformAccessArray = _transformAccessArray
			};
			destroyJob.Execute();

			//sw.Stop();
			//UnityEngine.Debug.Log($"destroy: {sw.Elapsed.TotalMilliseconds}");

			_destroyParamsList.Dispose();

			_isDestroyUnits = false;
		}
	}

	private void UpdateInfo()
	{
		if (_infoIsDirty)
		{
			GameInfo info = new GameInfo()
			{
				countAgents = NumAgents,
				countInterests = NumInterests,
				countPredators = NumPredators
			};

			_events.GameplayUpdateInfo?.Invoke(info);

			_infoIsDirty = false;
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < _unitsList.Length; i++)
		{
			_factory.Get(_unitsList[i].unitType).Dispose(_transformAccessArray[i].gameObject);
		}

		_octree.Dispose();
		_octreeIsRebuild.Dispose();
		_randomGenerators.Dispose();
		_unitsList.Dispose();
		_targetsList.Dispose();
		_transformAccessArray.Dispose();
		_numUnits.Dispose();
		_uniqueIndex.Dispose();
		_velocities.Dispose();
		_idleRadiuses.Dispose();
		_unitBoundsSizes.Dispose();

		
		UnityEngine.Debug.Log("Game::disposed");
	}
}