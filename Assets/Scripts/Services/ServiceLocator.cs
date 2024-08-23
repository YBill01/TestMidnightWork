using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator
{
	private Dictionary<Type, IService> _services;

	public ServiceLocator()
	{
		_services = new Dictionary<Type, IService>();
	}

	public T Add<T>(T service) where T : IService
	{
		if (_services.TryAdd(typeof(T), service))
		{
			return service;
		}
		else
		{
			throw new UnityException($"The requested service: '{typeof(T)}' is already registered");
		}
	}
	public void Remove<T>() where T : IService
	{
		_services.Remove(typeof(T));
	}

	public T Get<T>() where T : IService
	{
		if (_services.TryGetValue(typeof(T), out IService service))
		{
			return (T)service;
		}
		else
		{
			throw new UnityException($"The requested service: '{typeof(T)}' is not registered");
		}
	}

	public bool Has<T>() where T : IService
	{
		return _services.ContainsKey(typeof(T));
	}
}