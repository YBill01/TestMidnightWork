using System;

public class EventsService : IService
{
	public Action AppInitialized;

	public Action GameSceneLoaded;

	public Action GameplayInitialized;
	public Action GameplayStart;

	public Action<GameInfo> GameplayUpdateInfo;
	public Action GameplayTakeInfo;
	
	public Action UIGameStart;
	public Action UIGameBack;

	public Action<int> UIUpdateConfigStartNumAgents;
	public Action<int> UIUpdateConfigStartNumInterests;
	public Action<int> UIUpdateConfigStartNumPredators;

	public Action<float> UIUpdateConfigSpeedAgents;
	public Action<float> UIUpdateConfigSpeedPredators;
	public Action<int> UIUpdateConfigSpawnCountAgents;
	public Action<float> UIUpdateConfigSpawnRateInterests;

	public Action<bool> UIUpdateConfigNightMode;

	public Action<int> UISpawnAgents;
	public Action<int> UISpawnPredators;
	public Action<int> UISpawnInterests;
}