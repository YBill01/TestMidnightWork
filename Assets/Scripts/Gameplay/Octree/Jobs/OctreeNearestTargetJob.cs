using NativeTrees;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct OctreeNearestTargetJob : IJob
{
	public NativeArray<Unit> units;
	public NativeArray<Target> targets;

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

			Target target = targets[i];

			if (target.isDirty && target.targetType == TargetType.Idle)
			{
				if (unit.unitType != UnitType.Interest)
				{
					Unit nearestUnit;
					if (unit.unitType == UnitType.Agent)
					{
						OctreeUnitNearestAgentVisitor visitorAgent = new OctreeUnitNearestAgentVisitor();
						octree.Nearest(unit.position, 8.0f, ref visitorAgent, default(OctreeUnitNearestDistanceSquaredProvider));

						if (visitorAgent.found)
						{
							nearestUnit = visitorAgent.nearest;

							target.targetIndex = nearestUnit.index;
							target.targetUniqueIndex = nearestUnit.uniqueIndex;
							target.targetType = TargetType.Pursuable;
							target.boundsSize = nearestUnit.boundsSize;

							target.isDirty = false;

							targets[i] = target;
						}
					}
					else if (unit.unitType == UnitType.Predator)
					{
						OctreeUnitNearestPredatorVisitor visitorPredator = new OctreeUnitNearestPredatorVisitor();
						octree.Nearest(unit.position, 16.0f, ref visitorPredator, default(OctreeUnitNearestDistanceSquaredProvider));

						if (visitorPredator.found)
						{
							nearestUnit = visitorPredator.nearest;

							target.targetIndex = nearestUnit.index;
							target.targetUniqueIndex = nearestUnit.uniqueIndex;
							target.targetType = TargetType.Destroyable;
							target.boundsSize = nearestUnit.boundsSize;

							target.isDirty = false;

							targets[i] = target;
						}
					}
				}
			}
		}
	}
}