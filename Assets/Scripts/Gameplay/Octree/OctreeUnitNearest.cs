using NativeTrees;
using Unity.Mathematics;

public struct OctreeUnitNearestDistanceSquaredProvider : IOctreeDistanceProvider<Unit>
{
	public float DistanceSquared(float3 point, Unit unit, AABB bounds) => bounds.DistanceSquared(point);
}

public struct OctreeUnitNearestAgentVisitor : IOctreeNearestVisitor<Unit>
{
	public Unit nearest;
	public bool found;

	private int visitCount;

	public bool OnVist(Unit unit)
	{
		if (unit.unitType == UnitType.Interest)
		{
			found = true;
			nearest = unit;

			return false;
		}

		found = false;
		nearest = unit;

		visitCount++;
		if (visitCount > 4)
		{
			return false;
		}

		return true;
	}
}
public struct OctreeUnitNearestPredatorVisitor : IOctreeNearestVisitor<Unit>
{
	public Unit nearest;
	public bool found;

	private int visitCount;

	public bool OnVist(Unit unit)
	{
		if (unit.unitType == UnitType.Agent)
		{
			found = true;
			nearest = unit;

			return false;
		}

		found = false;
		nearest = unit;

		visitCount++;
		if (visitCount > 4)
		{
			return false;
		}

		return true;
	}
}