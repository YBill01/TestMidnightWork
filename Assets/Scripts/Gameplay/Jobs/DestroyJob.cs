using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile(CompileSynchronously = true)]
public class DestroyJob : IJob
{
	public NativeList<DestroyParams> inDestroyParams;

	public NativeArray<int> numUnits;

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
		SortJob<DestroyParams, DestroyParamsComparer> destroyParamsSortJobHandle = inDestroyParams.SortJob(new DestroyParamsComparer());
		destroyParamsSortJobHandle.Schedule().Complete();

		for (int i = 0; i < inDestroyParams.Length; i++)
		{
			DestroyParams destroyParams = inDestroyParams[i];

			int index = destroyParams.index;

			Unit unit = outUnits[index];
			GameObject gameObject = outTransformAccessArray[index].gameObject;

			inFactory.Get(unit.unitType).Dispose(gameObject);

			numUnits[(int)unit.unitType]--;
			outUnits.RemoveAtSwapBack(index);
			outTargets.RemoveAtSwapBack(index);
			outTransformAccessArray.RemoveAtSwapBack(index);
		}
	}

	struct DestroyParamsComparer : IComparer<DestroyParams>
	{
		public int Compare(DestroyParams x, DestroyParams y)
		{
			return y.index.CompareTo(x.index);
		}
	}
}