using Unity.Mathematics;

public struct Target
{
	public int targetIndex;
	public int targetUniqueIndex;
	public TargetType targetType;
	
	public bool isDirty;

	public float3 inPosition;
	public float3 outPosition;

	public float idleRadius;

	public float boundsSize;
}