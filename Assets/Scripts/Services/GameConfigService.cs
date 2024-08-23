using UnityEngine;

public class GameConfigService : IService
{
	public Bounds octreeBounds;
	public Bounds areaBounds;
	public Bounds asylumAreaBounds;

	public int startNumAgents;
	public int startNumInterests;
	public int startNumPredators;

	public float speedAgents;
	public float speedPredators;

	public int spawnCountAgents;
	public float spawnRateInterests;
	public int maxNumInterests;

	public bool nightMode;
}