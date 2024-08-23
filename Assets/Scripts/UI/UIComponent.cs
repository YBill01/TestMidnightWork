using System;
using UnityEngine;

[DefaultExecutionOrder(-9)]
public abstract class UIComponent : MonoBehaviour
{
	public static event Action<UIComponent> Initialized;
	public static event Action<UIComponent> Disposed;

	private void Awake()
	{
		Initialized?.Invoke(this);

		Initialize();
	}
	private void OnDestroy()
	{
		Disposed?.Invoke(this);

		Dispose();
	}

	protected virtual void Initialize() { }
	protected virtual void Dispose() { }
}