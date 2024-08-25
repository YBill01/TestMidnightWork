using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile(CompileSynchronously = true)]
public class SpawnJob : IJob
{
	[ReadOnly]
	public NativeList<SpawnParams> inSpawnParams;
	[ReadOnly]
	public NativeQueue<int> freeIndices;

	[ReadOnly]
	public NativeArray<float> inIdleRadiuses;
	[ReadOnly]
	public NativeArray<float> inUnitBoundsSizes;
	
	public NativeArray<int> numUnits;
	public NativeReference<int> uniqueIndex;

	[ReadOnly]
	public Factory inFactory;

	[WriteOnly]
	public NativeList<Unit> outUnits;
	[WriteOnly]
	public NativeList<Target> outTargets;
	[WriteOnly]
	public TransformAccessArray outTransformAccessArray;
	
	public void Execute()
	{
		for (int i = 0; i < inSpawnParams.Length; i++)
		{
			SpawnParams spawnParams = inSpawnParams[i];

			Unit unit = new Unit
			{
				uniqueIndex = uniqueIndex.Value,
				unitType = spawnParams.unitType,
				position = spawnParams.position,
				direction = spawnParams.direction,
				velocityIndex = (int)spawnParams.unitType,
				isMove = true,
				boundsSize = inUnitBoundsSizes[(int)spawnParams.unitType]
			};

			Target target = new Target
			{
				isDirty = true,
				inPosition = spawnParams.position,
				outPosition = spawnParams.position,
				idleRadius = inIdleRadiuses[(int)spawnParams.unitType]
			};

			Transform transform = inFactory.Get(spawnParams.unitType).Instantiate(spawnParams.position, Quaternion.LookRotation(spawnParams.direction)).transform;

			numUnits[(int)spawnParams.unitType] += 1;
			uniqueIndex.Value += 1;

			if (!freeIndices.IsEmpty())
			{
				int index = freeIndices.Dequeue();

				unit.index = index;

				outUnits[index] = unit;
				outTargets[index] = target;
				outTransformAccessArray[index] = transform;
			}
			else
			{
				unit.index = outUnits.Length;

				outUnits.Add(unit);
				outTargets.Add(target);
				outTransformAccessArray.Add(transform);
			}
		}
	}
}