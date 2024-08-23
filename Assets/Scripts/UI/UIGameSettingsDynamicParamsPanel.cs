using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameSettingsDynamicParamsPanel : MonoBehaviour
{
	[SerializeField]
	private Slider speedAgentsSlider;
	[SerializeField]
	private TMP_Text speedAgentsValueText;

	[Space(10)]
	[SerializeField]
	private Slider speedPredatorsSlider;
	[SerializeField]
	private TMP_Text speedPredatorsValueText;

	[Space(10)]
	[SerializeField]
	private Slider spawnCountAgentsSlider;
	[SerializeField]
	private TMP_Text spawnCountAgentsValueText;

	[Space(10)]
	[SerializeField]
	private Slider spawnRateInterestsSlider;
	[SerializeField]
	private TMP_Text spawnRateInterestsValueText;

	[Space(10)]
	[SerializeField]
	private Toggle nightModeToggle;

	private void Awake()
	{
		speedAgentsSlider.onValueChanged.AddListener(SpeedAgentsSliderOnValueChanged);
		speedPredatorsSlider.onValueChanged.AddListener(SpeedPredatorsSliderOnValueChanged);

		spawnCountAgentsSlider.onValueChanged.AddListener(SpawnRateAgentsSliderOnValueChanged);
		spawnRateInterestsSlider.onValueChanged.AddListener(SpawnRateInterestsSliderOnValueChanged);

		nightModeToggle.onValueChanged.AddListener(NightModeToggleOnValueChanged);
	}

	private void SpeedAgentsSliderOnValueChanged(float value)
	{
		speedAgentsValueText.text = value.ToString("F1");
		App.Instance.Services.Get<EventsService>().UIUpdateConfigSpeedAgents?.Invoke(value);
	}
	private void SpeedPredatorsSliderOnValueChanged(float value)
	{
		speedPredatorsValueText.text = value.ToString("F1");
		App.Instance.Services.Get<EventsService>().UIUpdateConfigSpeedPredators?.Invoke(value);
	}

	private void SpawnRateAgentsSliderOnValueChanged(float value)
	{
		spawnCountAgentsValueText.text = ((int)value).ToString("F0");
		App.Instance.Services.Get<EventsService>().UIUpdateConfigSpawnCountAgents?.Invoke((int)value);
	}
	private void SpawnRateInterestsSliderOnValueChanged(float value)
	{
		spawnRateInterestsValueText.text = value.ToString("F1");
		App.Instance.Services.Get<EventsService>().UIUpdateConfigSpawnRateInterests?.Invoke(value);
	}

	private void NightModeToggleOnValueChanged(bool value)
	{
		App.Instance.Services.Get<EventsService>().UIUpdateConfigNightMode?.Invoke(nightModeToggle.isOn);
	}

	public void SetStartParams(GameConfigService gameConfig)
	{
		speedAgentsSlider.SetValueWithoutNotify(gameConfig.speedAgents);
		speedAgentsValueText.text = gameConfig.speedAgents.ToString("F1");
		speedPredatorsSlider.SetValueWithoutNotify(gameConfig.speedPredators);
		speedPredatorsValueText.text = gameConfig.speedPredators.ToString("F1");

		spawnCountAgentsSlider.SetValueWithoutNotify(gameConfig.spawnCountAgents);
		spawnCountAgentsValueText.text = gameConfig.spawnCountAgents.ToString("F0");
		spawnRateInterestsSlider.SetValueWithoutNotify(gameConfig.spawnRateInterests);
		spawnRateInterestsValueText.text = gameConfig.spawnRateInterests.ToString("F1");

		nightModeToggle.SetIsOnWithoutNotify(gameConfig.nightMode);
	}
}