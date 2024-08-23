using UnityEngine;

public class UIGameCanvas : UIComponent
{
	[Space(10)]
	[SerializeField]
	private UIScreenController screens;

	private void OnEnable()
	{
		App.Instance.Services.Get<EventsService>().GameplayInitialized += OnGameplayInitialized;
		App.Instance.Services.Get<EventsService>().GameplayStart += OnGameplayStart;
	}
	private void OnDisable()
	{
		App.Instance.Services.Get<EventsService>().GameplayInitialized -= OnGameplayInitialized;
		App.Instance.Services.Get<EventsService>().GameplayStart -= OnGameplayStart;
	}

	private void OnGameplayInitialized()
	{
		ShowStartState();

		screens.Get<UIGameSettingsScreen>().SetStartParams(App.Instance.Services.Get<GameConfigService>());
	}
	private void OnGameplayStart()
	{
		ShowPlayState();
	}

	public void ShowStartState()
	{
		screens.Show<UIGameStartScreen>();
		screens.Show<UIGameSettingsScreen>()
			.ShowStartState();
		screens.Hide<UIGameBackScreen>();
	}
	public void ShowPlayState()
	{
		screens.Hide<UIGameStartScreen>();
		screens.Show<UIGameSettingsScreen>()
			.ShowPlayState();
		screens.Show<UIGameBackScreen>();
	}
}