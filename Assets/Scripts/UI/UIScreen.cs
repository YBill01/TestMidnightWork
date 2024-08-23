using System;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIScreen : MonoBehaviour
{
	public Action<UIScreen> onPreShow;
	public Action<UIScreen> onPreHide;
	public Action<UIScreen> onShow;
	public Action<UIScreen> onHide;

	public int layerIndex;

	public bool IsShowing => state == ScreenState.Show || state == ScreenState.ShowProcess;

	public ScreenState state { get; private set; }
	public enum ScreenState
	{
		None,
		ShowProcess,
		Show,
		HideProcess,
		Hide
	}

	protected UIScreenController _controller;

	internal void Init(UIScreenController controller)
	{
		state = ScreenState.Hide;

		_controller = controller;

		gameObject.SetActive(false);

		OnInit();
	}

	internal void Activate(Action action)
	{
		OnActivate(action);
	}
	internal void DeActivate(Action action)
	{
		OnDeActivate(action);
	}

	public UIScreen SelfShow()
	{
		if (_controller != null)
		{
			return _controller.Show(this.GetType());
		}

		return this;
	}
	public UIScreen SelfHide()
	{
		if (_controller != null)
		{
			return _controller.Hide(this.GetType());
		}

		return this;
	}

	protected void OnPreActivate(Action action = null)
	{
		state = ScreenState.ShowProcess;

		gameObject.SetActive(true);

		action?.Invoke();

		onPreShow?.Invoke(this);
		onPreShow = null;
		
		OnPreShow();
	}
	protected void OnPreDeActivate(Action action = null)
	{
		state = ScreenState.HideProcess;

		action?.Invoke();

		onPreHide?.Invoke(this);
		onPreHide = null;
		
		OnPreHide();
	}

	protected void OnPostActivate(Action action = null)
	{
		state = ScreenState.Show;

		action?.Invoke();

		onShow?.Invoke(this);
		onShow = null;

		OnShow();
	}
	protected void OnPostDeActivate(Action action = null)
	{
		state = ScreenState.Hide;

		gameObject.SetActive(false);

		action?.Invoke();

		onHide?.Invoke(this);
		onHide = null;

		OnHide();
	}

	protected virtual void OnActivate(Action action)
	{
		OnPreActivate(action);
		OnPostActivate();
	}
	protected virtual void OnDeActivate(Action action)
	{
		OnPreDeActivate();
		OnPostDeActivate(action);
	}
	
	protected virtual void OnInit()
	{
		//Do Nothing...
	}

	protected virtual void OnPreShow()
	{
		//Do Nothing...
	}
	protected virtual void OnPreHide()
	{
		//Do Nothing...
	}

	protected virtual void OnShow()
	{
		//Do Nothing...
	}
	protected virtual void OnHide()
	{
		//Do Nothing...
	}
}

public static class UIScreenExtensions
{
	public static T SelfShow<T>(this T t) where T : UIScreen
	{
		return (T)t.SelfShow();
	}
	public static T SelfHide<T>(this T t) where T : UIScreen
	{
		return (T)t.SelfHide();
	}

	public static T OnPreShow<T>(this T t, Action<UIScreen> action) where T : UIScreen
	{
		t.onPreShow = action;

		return t;
	}
	public static T OnPreHide<T>(this T t, Action<UIScreen> action) where T : UIScreen
	{
		t.onPreHide = action;

		return t;
	}

	public static T OnShow<T>(this T t, Action<UIScreen> action) where T : UIScreen
	{
		t.onShow = action;

		return t;
	}
	public static T OnHide<T>(this T t, Action<UIScreen> action) where T : UIScreen
	{
		t.onHide = action;

		return t;
	}
}