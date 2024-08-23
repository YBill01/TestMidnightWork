using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameConfig", fileName = "GameStartConfig", order = int.MinValue)]
public class GameConfigData : ScriptableObject
{
	public Bounds octreeBounds;
	public Bounds areaBounds;
	public Bounds asylumAreaBounds;

	[Space(10)]
	public int startNumAgents;
	public int startNumInterests;
	public int startNumPredators;

	[Space(10)]
	public float speedAgents;
	public float speedPredators;

	[Space(10)]
	[Min(1)]
	public int spawnCountAgents;
	public float spawnRateInterests;
	public int maxNumInterests;

	[Space(10)]
	public bool nightMode;
}