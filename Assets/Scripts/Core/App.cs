using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class App : MonoBehaviour
{
	public static App Instance { get; private set; }

	public ServiceLocator Services { get; private set; }

	private AppStateMachine _appFSM;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			throw new Exception("An instance of this singleton already exists.");
		}

		Instance = this;

		DontDestroyOnLoad(this);
	}

	private async UniTaskVoid Start()
	{
		await UniTask.NextFrame();

		// some initialize services...
		Services = new ServiceLocator();

		var events = Services.Add(new EventsService());
		var loader = Services.Add(new LoaderService());
		var ui = Services.Add(new UIService());

		AppInitializeState appInitializeState = new AppInitializeState(events);
		AppGameLoadState appGameLoadState = new AppGameLoadState(loader, events, 1);
		AppGameplayState appGameplayState = new AppGameplayState();

		_appFSM = new AppStateMachine(appInitializeState, appGameLoadState, appGameplayState);

		events.AppInitialized += appInitializeState.AddEventTransition(appGameLoadState);
		events.GameSceneLoaded += appGameLoadState.AddEventTransition(appGameplayState);

		_appFSM.Init();
	}

	private void Update()
	{
		_appFSM?.Update();
	}

	public void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
		System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
	}
}