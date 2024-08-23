using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public struct DestroySortJob : IJobParallelFor
{



	public void Execute(int index)
	{
		
	}

	public struct DestroyParamsComparer : IComparer<DestroyParams>
	{
		public int Compare(DestroyParams x, DestroyParams y)
		{
			return y.index.CompareTo(x.index);
		}
	}
}