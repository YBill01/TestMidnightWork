using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using RandomMath = Unity.Mathematics.Random;

[BurstCompile]
public struct TargetJob : IJobParallelFor
{
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<Unit> units;

	public NativeArray<Target> targets;

	[NativeDisableContainerSafetyRestriction]
	public NativeArray<RandomMath> randomGenerators;
	[NativeSetThreadIndex]
	private int _threadId;

	[ReadOnly]
	public bool reGenerate;

	[ReadOnly]
	public Bounds areaBounds;
	[ReadOnly]
	public Bounds asylumAreaBounds;

	private int _unitsCount => units.Length;

	public void Execute(int index)
	{
		Unit unit = units[index];
		Target target = targets[index];
		RandomMath random = randomGenerators[_threadId];

		if (reGenerate)
		{
			if (unit.unitType == UnitType.Agent)
			{
				target.outPosition = random.NextFloat3(asylumAreaBounds.min, asylumAreaBounds.max);
			}
			else if (unit.unitType == UnitType.Interest)
			{
				target.outPosition = math.min(math.max(areaBounds.min, target.inPosition + (random.NextFloat3Direction() * target.idleRadius)), areaBounds.max);
			}
			else
			{
				target.outPosition = random.NextFloat3(areaBounds.min, areaBounds.max);
			}

			unit.isMove = true;
			units[index] = unit;

			target.targetIndex = -1;
			target.targetUniqueIndex = -1;
			target.targetType = TargetType.None;
			target.inPosition = target.outPosition;
			target.boundsSize = 0.0f;
			target.isDirty = false;
			targets[index] = target;

			randomGenerators[_threadId] = random;
		}
		else if (target.isDirty)
		{
			if (unit.unitType == UnitType.Agent)
			{
				target.outPosition = math.min(math.max(asylumAreaBounds.min, target.inPosition + (random.NextFloat3Direction() * target.idleRadius)), asylumAreaBounds.max);
			}
			else
			{
				target.outPosition = math.min(math.max(areaBounds.min, target.inPosition + (random.NextFloat3Direction() * target.idleRadius)), areaBounds.max);
			}

			unit.isMove = true;
			units[index] = unit;
			
			target.targetIndex = -1;
			target.targetUniqueIndex = -1;
			target.targetType = TargetType.None;
			target.boundsSize = 0.0f;
			target.isDirty = false;
			targets[index] = target;

			randomGenerators[_threadId] = random;
		}
		else
		{
			if (target.targetIndex > -1)
			{
				if (target.targetIndex < _unitsCount && target.targetUniqueIndex == units[target.targetIndex].uniqueIndex)
				{
					target.outPosition = units[target.targetIndex].position;
					targets[index] = target;
				}
				else
				{
					target.targetIndex = -1;
					target.targetUniqueIndex = -1;
					target.targetType = TargetType.None;
					target.boundsSize = 0.0f;
					target.isDirty = true;
					targets[index] = target;
				}
			}
		}
	}
}