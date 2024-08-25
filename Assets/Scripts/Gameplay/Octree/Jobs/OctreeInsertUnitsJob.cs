using NativeTrees;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile(CompileSynchronously = true)]
public struct OctreeInsertUnitsJob : IJob
{
	[ReadOnly]
	public NativeArray<Unit> units;

	public NativeOctree<Unit> octree;

	public void Execute()
	{
		for (int i = 0; i < units.Length; i++)
		{
			Unit unit = units[i];

			if (unit.isDestroyed)
			{
				continue;
			}

			octree.InsertPoint(unit, unit.position);
		}
	}
}