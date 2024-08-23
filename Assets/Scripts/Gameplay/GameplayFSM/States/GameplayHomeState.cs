using YB.HFSM;

public class GameplayHomeState : State
{
	private readonly GameConfigService _gameConfig;
	private readonly EventsService _events;

	public GameplayHomeState(GameConfigService gameConfig, EventsService events)
	{
		_gameConfig = gameConfig;
		_events = events;
	}

	protected override void OnEnter()
	{
		_events.UIUpdateConfigStartNumAgents += OnUIUpdateConfigStartNumAgents;
		_events.UIUpdateConfigStartNumInterests += OnUIUpdateConfigStartNumInterests;
		_events.UIUpdateConfigStartNumPredators += OnUIUpdateConfigStartNumPredators;

		_events.UIUpdateConfigSpeedAgents += OnUIUpdateConfigSpeedAgents;
		_events.UIUpdateConfigSpeedPredators += OnUIUpdateConfigSpeedPredators;

		_events.UIUpdateConfigSpawnCountAgents += OnUIUpdateConfigSpawnCountAgents;
		_events.UIUpdateConfigSpawnRateInterests += OnUIUpdateConfigSpawnRateInterests;

		_events.UIUpdateConfigNightMode += OnUIUpdateConfigNightMode;

		_events.GameplayInitialized?.Invoke();
	}
	protected override void OnExit()
	{
		_events.UIUpdateConfigStartNumAgents -= OnUIUpdateConfigStartNumAgents;
		_events.UIUpdateConfigStartNumInterests -= OnUIUpdateConfigStartNumInterests;
		_events.UIUpdateConfigStartNumPredators -= OnUIUpdateConfigStartNumPredators;

		_events.UIUpdateConfigSpeedAgents -= OnUIUpdateConfigSpeedAgents;
		_events.UIUpdateConfigSpeedPredators -= OnUIUpdateConfigSpeedPredators;

		_events.UIUpdateConfigSpawnCountAgents -= OnUIUpdateConfigSpawnCountAgents;
		_events.UIUpdateConfigSpawnRateInterests -= OnUIUpdateConfigSpawnRateInterests;

		_events.UIUpdateConfigNightMode -= OnUIUpdateConfigNightMode;
	}

	private void OnUIUpdateConfigStartNumAgents(int value)
	{
		_gameConfig.startNumAgents = value;
	}
	private void OnUIUpdateConfigStartNumInterests(int value)
	{
		_gameConfig.startNumInterests = value;
	}
	private void OnUIUpdateConfigStartNumPredators(int value)
	{
		_gameConfig.startNumPredators = value;
	}

	private void OnUIUpdateConfigSpeedAgents(float value)
	{
		_gameConfig.speedAgents = value;
	}
	private void OnUIUpdateConfigSpeedPredators(float value)
	{
		_gameConfig.speedPredators = value;
	}

	private void OnUIUpdateConfigSpawnCountAgents(int value)
	{
		_gameConfig.spawnCountAgents = value;
	}
	private void OnUIUpdateConfigSpawnRateInterests(float value)
	{
		_gameConfig.spawnRateInterests = value;
	}

	private void OnUIUpdateConfigNightMode(bool value)
	{
		_gameConfig.nightMode = value;
	}
}