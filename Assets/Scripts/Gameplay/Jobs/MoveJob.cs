using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct MoveJob : IJobParallelForTransform
{
	public NativeArray<Unit> units;

	[ReadOnly]
	public NativeArray<Target> targets;

	[ReadOnly]
	public NativeArray<float> velocities;

	[ReadOnly]
	public float deltaTime;

	public void Execute(int index, TransformAccess transform)
	{
		Unit unit = units[index];
		Target target = targets[index];

		float3 direction = target.outPosition - unit.position;

		float curLengthsq = math.lengthsq(direction);
		float allLengthsq = math.lengthsq(target.outPosition - target.inPosition);

		float3 targetDirection = math.normalize(direction);

		unit.direction = math.normalize(math.lerp(unit.direction, targetDirection, (4.0f + (12.0f * (1.0f - math.min(1.0f, curLengthsq / allLengthsq)))) * deltaTime));

		if (unit.isMove)
		{
			unit.position += unit.direction * (velocities[unit.velocityIndex] * deltaTime);
		}

		transform.SetPositionAndRotation(unit.position, quaternion.LookRotation(unit.direction, math.float3(0, 1, 0)));

		units[index] = unit;
	}
}