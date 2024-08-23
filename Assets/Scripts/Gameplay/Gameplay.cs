using UnityEngine;

public class Gameplay : MonoBehaviour
{
	[SerializeField]
	private GameConfigData startGameConfig;

	[Space(10)]
	[SerializeField]
	private Factory factory;
	
	private GameplayStateMachine _gameplayFSM;

	private void Awake()
	{
		GameConfigService gameConfigService = App.Instance.Services.Add(new GameConfigService());

		gameConfigService.octreeBounds = startGameConfig.octreeBounds;
		gameConfigService.areaBounds = startGameConfig.areaBounds;
		gameConfigService.asylumAreaBounds = startGameConfig.asylumAreaBounds;
		gameConfigService.startNumAgents = startGameConfig.startNumAgents;
		gameConfigService.startNumInterests = startGameConfig.startNumInterests;
		gameConfigService.startNumPredators = startGameConfig.startNumPredators;
		gameConfigService.speedAgents = startGameConfig.speedAgents;
		gameConfigService.speedPredators = startGameConfig.speedPredators;
		gameConfigService.spawnCountAgents = startGameConfig.spawnCountAgents;
		gameConfigService.spawnRateInterests = startGameConfig.spawnRateInterests;
		gameConfigService.maxNumInterests = startGameConfig.maxNumInterests;
		gameConfigService.nightMode = startGameConfig.nightMode;

		EventsService events = App.Instance.Services.Get<EventsService>();

		GameplayHomeState gameplayHomeState = new GameplayHomeState(gameConfigService, events);
		GameplayPlayState gameplayPlayState = new GameplayPlayState(gameConfigService, events, factory);

		_gameplayFSM = new GameplayStateMachine(gameplayHomeState, gameplayPlayState);

		events.UIGameBack += gameplayPlayState.AddEventTransition(gameplayHomeState);
		events.UIGameStart += gameplayHomeState.AddEventTransition(gameplayPlayState);
	}
	private void Start()
	{
		_gameplayFSM.Init();
	}

	private void Update()
	{
		_gameplayFSM.Update();
	}

	/*private void OnDestroy()
	{
		_gameplayFSM.Dispose();
	}*/

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.grey;
		Gizmos.DrawWireCube(startGameConfig.octreeBounds.center, startGameConfig.octreeBounds.size);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(startGameConfig.areaBounds.center, startGameConfig.areaBounds.size);
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(startGameConfig.asylumAreaBounds.center, startGameConfig.asylumAreaBounds.size);
		Gizmos.color = Color.white;
	}
#endif
}