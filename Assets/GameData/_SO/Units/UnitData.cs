using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unit", fileName = "Unit")]
public class UnitData : ScriptableObject
{
	public UnitType unitType;

	[Space(10)]
	public GameObject prefab;

	public float boundsSize;

	public float idleRadius;

	public int poolDefaultCapacity;
}