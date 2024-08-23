using Unity.Mathematics;

public struct Unit
{
	public int index;

	public int uniqueIndex;

	public UnitType unitType;

	public float3 position;
	public float3 direction;

	public int velocityIndex;

	public bool isMove;

	public float boundsSize;

	public int numIntersects;

	public bool isDestroyed;
}