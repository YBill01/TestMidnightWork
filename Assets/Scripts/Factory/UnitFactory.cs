using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[Serializable]
public class UnitFactory
{
	/// <summary>
	/// optimization for parallel jobs
	/// </summary>
	private const int _MAX_ITEMS_IN_CONTAINER = 256;
	private static List<Transform> _subContainers = new List<Transform>();
	private static int _counter = 0;

	private static bool _active = true;
	public static bool Active
	{
		get => _active;
		set
		{
			_active = value;

			foreach (Transform t in _subContainers)
			{
				t.gameObject.SetActive(_active);
			}
		}
	}

	private UnitData _unitData;
	public UnitData UnitData => _unitData;

	private IObjectPool<GameObject> _objectPool;

	public UnitFactory(UnitData unitData)
	{
		_unitData = unitData;

		_objectPool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false, unitData.poolDefaultCapacity, int.MaxValue);
	}

	private GameObject CreatePooledItem()
	{
		_counter++;

		int index = _counter / _MAX_ITEMS_IN_CONTAINER;
		if (index >= _subContainers.Count)
		{
			GameObject newGO = new GameObject($"T{index}");
			newGO.SetActive(_active);
			_subContainers.Add(newGO.transform);
		}

		return UnityEngine.Object.Instantiate(_unitData.prefab, _subContainers[index]);
	}
	private void OnTakeFromPool(GameObject unit)
	{
		unit.SetActive(true);
	}
	private void OnReturnedToPool(GameObject unit)
	{
		unit.SetActive(false);
	}
	private void OnDestroyPoolObject(GameObject unit)
	{
		UnityEngine.Object.Destroy(unit);
	}

	public GameObject Instantiate(Vector3 position, Quaternion rotation)
	{
		GameObject unit = _objectPool.Get();

		unit.transform.SetPositionAndRotation(position, rotation);

		return unit;
	}
	public void Dispose(GameObject unit)
	{
		_objectPool.Release(unit);
	}
}