using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIScreenController : MonoBehaviour
{
	[SerializeField]
	protected RectTransform allScreens;
	[SerializeField]
	protected RectTransform screenContainer;

	[Space(10)]
	[SerializeField]
	protected UIScreen defaultScreen;
	[SerializeField]
	protected List<UIScreen> screens;

	private Dictionary<Type, UIScreen> _screensDic;
	private List<UIScreen> _showingScreens;

	private void Awake()
	{
		_screensDic = new Dictionary<Type, UIScreen>();
		_showingScreens = new List<UIScreen>();

		foreach (UIScreen screen in screens)
		{
			_screensDic.Add(screen.GetType(), screen);
			screen.Init(this);
		}

		if (allScreens != null) allScreens.gameObject.SetActive(false);
		if (screenContainer != null) screenContainer.gameObject.SetActive(true);

		ShowDefault();
	}

	private async UniTaskVoid ShowProcess(UIScreen screen)
	{
		await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

		screen.Activate(() => {
			_showingScreens.Add(screen);
			screen.transform.SetParent(screenContainer);

			SortScreens();
		});
		
	}
	private async UniTaskVoid HideProcess(UIScreen screen)
	{
		await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

		screen.DeActivate(() => {
			_showingScreens.Remove(screen);
			screen.transform.SetParent(allScreens);
		});
	}

	public void ShowDefault()
	{
		if (defaultScreen != null)
		{
			Show(defaultScreen.GetType());
		}
	}

	public UIScreen Show(Type type)
	{
		if (_screensDic.TryGetValue(type, out UIScreen screen))
		{
			if (!screen.IsShowing)
			{
				UIScreen currentScreen = GetCurrents(screen.layerIndex)
					.LastOrDefault();

				if (currentScreen != null)
				{
					currentScreen.SelfHide().OnHide(s =>
					{
						ShowProcess(screen).Forget();
					});
				}
				else
				{
					ShowProcess(screen).Forget();
				}
			}

			return screen;
		}

		return null;
	}
	public UIScreen Hide(Type type)
	{
		if (_screensDic.TryGetValue(type, out UIScreen screen))
		{
			if (screen.IsShowing)
			{
				HideProcess(screen).Forget();
			}

			return screen;
		}

		return null;
	}

	public T Show<T>() where T : UIScreen
	{
		return (T)Show(typeof(T));
	}
	public T Hide<T>() where T : UIScreen
	{
		return (T)Hide(typeof(T));
	}

	private void SortScreens()
	{
		List<UIScreen> list = GetCurrents();
		list.Sort((a, b) => a.layerIndex.CompareTo(b.layerIndex));

		for (int i = 0; i < list.Count; i++)
		{
			list[i].transform.SetSiblingIndex(i);
		}
	}

	public bool Has<T>() where T : UIScreen
	{
		return _screensDic.ContainsKey(typeof(T));
	}

	public T Get<T>() where T : UIScreen
	{
		if (_screensDic.TryGetValue(typeof(T), out UIScreen screen))
		{
			return (T)screen;
		}

		return null;
	}
	public UIScreen Get(Type type)
	{
		if (_screensDic.TryGetValue(type, out UIScreen screen))
		{
			return screen;
		}

		return null;
	}
	public UIScreen Get(string name)
	{
		return _screensDic.FirstOrDefault(x => x.Value.name == name).Value;
	}

	public UIScreen GetCurrent()
	{
		return _showingScreens.LastOrDefault();
	}

	public List<UIScreen> GetCurrents()
	{
		return _showingScreens.ToList();
	}
	public List<UIScreen> GetCurrents(int layerIndex)
	{
		return _showingScreens.Where(x => x.layerIndex == layerIndex).ToList();
	}
}