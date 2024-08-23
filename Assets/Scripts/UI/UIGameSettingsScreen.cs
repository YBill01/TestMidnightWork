using UnityEngine;
using UnityEngine.UI;

public class UIGameSettingsScreen : UIScreen
{
	[Space(10)]
	[SerializeField]
	private Toggle showToggle;
	[SerializeField]
	private RectTransform panel;

	[Space(10)]
	[SerializeField]
	private UIGameSettingsInfoPanel infoPanel;
	[SerializeField]
	private UIGameSettingsStartParamsPanel startParamsPanel;
	[SerializeField]
	private UIGameSettingsDynamicParamsPanel dynamicParamsPanel;
	[SerializeField]
	private UIGameSettingsSpawnerPanel spawnerPanel;

	private void Awake()
	{
		showToggle.onValueChanged.AddListener(ShowToggleOnValueChanged);
	}

	protected override void OnShow()
	{
		App.Instance.Services.Get<EventsService>().GameplayTakeInfo?.Invoke();
	}

	private void ShowToggleOnValueChanged(bool value)
	{
		panel.gameObject.SetActive(value);

		if (value)
		{
			App.Instance.Services.Get<EventsService>().GameplayTakeInfo?.Invoke();
		}
	}

	public void SetStartParams(GameConfigService gameConfig)
	{
		startParamsPanel.SetStartParams(gameConfig);
		dynamicParamsPanel.SetStartParams(gameConfig);
	}

	public void ShowStartState()
	{
		infoPanel.gameObject.SetActive(false);
		startParamsPanel.gameObject.SetActive(true);
		dynamicParamsPanel.gameObject.SetActive(true);
		spawnerPanel.gameObject.SetActive(false);
	}
	public void ShowPlayState()
	{
		infoPanel.gameObject.SetActive(true);
		startParamsPanel.gameObject.SetActive(false);
		dynamicParamsPanel.gameObject.SetActive(true);
		spawnerPanel.gameObject.SetActive(true);
	}
}