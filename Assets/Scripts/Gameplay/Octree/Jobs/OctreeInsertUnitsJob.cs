using NativeTrees;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile(CompileSynchronously = true)]
public struct OctreeInsertUnitsJob : IJob
{
	public NativeArray<Unit> units;

	public NativeOctree<Unit> octree;

	public void Execute()
	{
		for (int i = 0; i < units.Length; i++)
		{
			Unit unit = units[i];
			unit.index = i;
			units[i] = unit;

			octree.InsertPoint(units[i], units[i].position);
		}
	}
}