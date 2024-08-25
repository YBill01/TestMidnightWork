using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile(CompileSynchronously = true)]
public class DestroyJob : IJob
{
	[ReadOnly]
	public NativeList<DestroyParams> inDestroyParams;
	[WriteOnly]
	public NativeQueue<int> freeIndices;

	public NativeArray<int> numUnits;

	[ReadOnly]
	public Factory inFactory;

	public NativeList<Unit> outUnits;
	[ReadOnly]
	public TransformAccessArray outTransformAccessArray;
	
	public void Execute()
	{
		for (int i = 0; i < inDestroyParams.Length; i++)
		{
			DestroyParams destroyParams = inDestroyParams[i];

			int index = destroyParams.index;

			Unit unit = outUnits[index];
			unit.isDestroyed = true;
			
			GameObject gameObject = outTransformAccessArray[index].gameObject;

			inFactory.Get(unit.unitType).Dispose(gameObject);

			numUnits[(int)unit.unitType] -= 1;

			outUnits[index] = unit;

			freeIndices.Enqueue(index);
		}
	}
}