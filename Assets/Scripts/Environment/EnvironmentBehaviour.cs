using UnityEngine;

public class EnvironmentBehaviour : MonoBehaviour
{
	[Space(10)]
	[SerializeField]
	private Material daySkyboxMaterial;
	[SerializeField]
	private Material nightSkyboxMaterial;

	private void OnEnable()
	{
		App.Instance.Services.Get<EventsService>().GameplayInitialized += OnGameplayInitialized;
		App.Instance.Services.Get<EventsService>().UIUpdateConfigNightMode += OnUpdateNightMode;
	}
	private void OnDisable()
	{
		App.Instance.Services.Get<EventsService>().GameplayInitialized -= OnGameplayInitialized;
		App.Instance.Services.Get<EventsService>().UIUpdateConfigNightMode -= OnUpdateNightMode;
	}

	private void OnGameplayInitialized()
	{
		SetNightMode(App.Instance.Services.Get<GameConfigService>().nightMode);
	}
	private void OnUpdateNightMode(bool value)
	{
		SetNightMode(value);
	}

	public void SetNightMode(bool value)
	{
		RenderSettings.skybox = value ? nightSkyboxMaterial : daySkyboxMaterial;
		DynamicGI.UpdateEnvironment();
	}
}