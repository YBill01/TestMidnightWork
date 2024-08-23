using System;
using System.Collections.Generic;

public class UIService : IService
{
	private Dictionary<Type, UIComponent> _dic;

	public UIService()
	{
		_dic = new Dictionary<Type, UIComponent>();

		UIComponent.Initialized += OnInizializeComponent;
		UIComponent.Disposed += OnDisposeComponent;
	}

	private void OnInizializeComponent(UIComponent component)
	{
		TryAddComponent(component);
	}
	private void OnDisposeComponent(UIComponent component)
	{
		TryRemoveComponent(component);
	}

	private bool TryAddComponent(UIComponent component)
	{
		return _dic.TryAdd(component.GetType(), component);
	}
	private bool TryRemoveComponent(UIComponent component)
	{
		return _dic.Remove(component.GetType());
	}

	public bool Has<T>() where T : UIComponent
	{
		return _dic.ContainsKey(typeof(T));
	}
	public T Get<T>() where T : UIComponent
	{
		if (_dic.TryGetValue(typeof(T), out UIComponent value))
		{
			return (T)value;
		}
		else
		{
			throw new ArgumentException($"<color=red>UI Component: '{typeof(T).Name}' is not found.</color>");
		}
	}
}