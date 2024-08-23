using YB.HFSM;
using UnityEngine;
using System;

public class GameplayPlayState : State, IDisposable
{
	private readonly GameConfigService _gameConfig;
	private readonly EventsService _events;
	private readonly Factory _factory;

	private Game _game;

	public GameplayPlayState(GameConfigService gameConfig, EventsService events, Factory factory)
	{
		_gameConfig = gameConfig;
		_events = events;
		_factory = factory;
	}

	protected override void OnEnter()
	{
		_events.UIUpdateConfigSpeedAgents += OnUIUpdateConfigSpeedAgents;
		_events.UIUpdateConfigSpeedPredators += OnUIUpdateConfigSpeedPredators;

		_events.UIUpdateConfigSpawnCountAgents += OnUIUpdateConfigSpawnCountAgents;
		_events.UIUpdateConfigSpawnRateInterests += OnUIUpdateConfigSpawnRateInterests;

		_events.UIUpdateConfigNightMode += OnUIUpdateConfigNightMode;

		_events.UISpawnAgents += OnUISpawnAgents;
		_events.UISpawnInterests += OnUISpawnInterests;
		_events.UISpawnPredators += OnUISpawnPredators;

		_events.GameplayTakeInfo += OnGameplayTakeInfo;

		_game = new Game(_gameConfig, _events, _factory);
		_game.Init();

		_events.GameplayStart?.Invoke();
	}

	protected override void OnExit()
	{
		_events.UIUpdateConfigSpeedAgents -= OnUIUpdateConfigSpeedAgents;
		_events.UIUpdateConfigSpeedPredators -= OnUIUpdateConfigSpeedPredators;

		_events.UIUpdateConfigSpawnCountAgents -= OnUIUpdateConfigSpawnCountAgents;
		_events.UIUpdateConfigSpawnRateInterests -= OnUIUpdateConfigSpawnRateInterests;

		_events.UIUpdateConfigNightMode -= OnUIUpdateConfigNightMode;

		_events.UISpawnAgents -= OnUISpawnAgents;
		_events.UISpawnInterests -= OnUISpawnInterests;
		_events.UISpawnPredators -= OnUISpawnPredators;

		_events.GameplayTakeInfo -= OnGameplayTakeInfo;

		_game.Dispose();
		_game = null;
	}

	private void OnUIUpdateConfigSpeedAgents(float value)
	{
		_game.SetVelocity(UnitType.Agent, value);
	}
	private void OnUIUpdateConfigSpeedPredators(float value)
	{
		_game.SetVelocity(UnitType.Predator, value);
	}

	private void OnUIUpdateConfigSpawnCountAgents(int value)
	{
		_game.SetSpawnCountAgents(value);
	}
	private void OnUIUpdateConfigSpawnRateInterests(float value)
	{
		_game.SetSpawnRateInterests(value);
	}

	private void OnUIUpdateConfigNightMode(bool value)
	{
		_game.SetNightMode(value);
	}

	private void OnUISpawnAgents(int count)
	{
		_game.AddSpawnUnitsQuery(UnitType.Agent, count);
	}
	private void OnUISpawnInterests(int count)
	{
		_game.AddSpawnUnitsQuery(UnitType.Interest, count);
	}
	private void OnUISpawnPredators(int count)
	{
		_game.AddSpawnUnitsQuery(UnitType.Predator, count);
	}

	private void OnGameplayTakeInfo()
	{
		_game.TakeInfo();
	}

	protected override void OnUpdate()
	{
		_game.OnUpdate(Time.deltaTime);
	}

	public void Dispose()
	{
		_game?.Dispose();
	}
}