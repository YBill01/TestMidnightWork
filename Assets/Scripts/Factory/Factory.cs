using System.Collections.Generic;
using UnityEngine;

public class Factory : MonoBehaviour
{
	[SerializeField]
	private UnitData[] unitsDataList;

	private Dictionary<UnitType, UnitFactory> _factories;
	
	private void Awake()
	{
		_factories = new Dictionary<UnitType, UnitFactory>();
		foreach (UnitData unitData in unitsDataList)
		{
			UnitFactory factory = new UnitFactory(unitData);
			_factories.Add(unitData.unitType, factory);
		}
	}

	public UnitFactory Get(UnitType unitType)
	{
		if (_factories.TryGetValue(unitType, out UnitFactory factory))
		{
			return factory;
		}
		else
		{
			throw new UnityException($"The requested factory: '{unitType}' is not registered");
		}
	}
}