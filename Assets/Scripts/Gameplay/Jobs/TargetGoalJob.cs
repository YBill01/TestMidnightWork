using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct TargetGoalJob : IJobParallelFor
{
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<Unit> units;
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<Target> targets;

	[WriteOnly]
	public NativeList<SpawnParams>.ParallelWriter outSpawnParams;

	[ReadOnly]
	public int inSpawnCountAgents;

	[WriteOnly]
	public NativeList<DestroyParams>.ParallelWriter outDestroyParams;

	[WriteOnly]
	[NativeDisableContainerSafetyRestriction]
	public NativeReference<bool> outOctreeIsRebuild;

	public void Execute(int index)
	{
		Unit unit = units[index];

		if (unit.isDestroyed)
		{
			return;
		}

		Target target = targets[index];

		float len = target.boundsSize + unit.boundsSize;
		bool isIntersect = math.lengthsq(target.outPosition - unit.position) <= (len * len);

		if (target.targetType == TargetType.Idle)
		{
			if (isIntersect)
			{
				target.isDirty = true;
				targets[index] = target;

				if (unit.unitType != UnitType.Interest)
				{
					outOctreeIsRebuild.Value = true;
				}
			}
		}
		else if (target.targetType == TargetType.Pursuable)
		{

			if (target.targetIndex > -1)
			{
				if (unit.isMove == isIntersect)
				{
					unit.isMove = !isIntersect;
					units[index] = unit;

					Unit targetUnit = units[target.targetIndex];
					targetUnit.numIntersects += isIntersect ? 1 : -1;

					if (targetUnit.numIntersects >= 2)
					{
						if (!targetUnit.isDestroyed)
						{
							outDestroyParams.AddNoResize(new DestroyParams
							{
								index = target.targetIndex
							});

							targetUnit.isDestroyed = true;

							for (int i = 0; i < inSpawnCountAgents; i++)
							{
								outSpawnParams.AddNoResize(new SpawnParams
								{
									unitType = UnitType.Agent,
									position = targetUnit.position,
									direction = targetUnit.direction
								});
							}
						}
					}

					units[target.targetIndex] = targetUnit;
				}
			}
		}
		else if (target.targetType == TargetType.Destroyable)
		{
			if (target.targetIndex > -1)
			{
				if (isIntersect)
				{
					Unit targetUnit = units[target.targetIndex];
					if (!targetUnit.isDestroyed)
					{
						outDestroyParams.AddNoResize(new DestroyParams
						{
							index = target.targetIndex
						});

						targetUnit.isDestroyed = true;
						units[target.targetIndex] = targetUnit;
					}
				}
			}
		}
	}
}