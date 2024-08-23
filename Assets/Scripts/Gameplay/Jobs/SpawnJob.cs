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
	public NativeArray<float> inIdleRadiuses;
	[ReadOnly]
	public NativeArray<float> inUnitBoundsSizes;
	
	public NativeArray<int> numUnits;
	public NativeArray<int> uniqueIndex;

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
				uniqueIndex = uniqueIndex[0],
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

			numUnits[(int)spawnParams.unitType]++;
			uniqueIndex[0]++;
			outUnits.Add(unit);
			outTargets.Add(target);
			outTransformAccessArray.Add(inFactory.Get(spawnParams.unitType).Instantiate(spawnParams.position, Quaternion.LookRotation(spawnParams.direction)).transform);
		}
	}
}