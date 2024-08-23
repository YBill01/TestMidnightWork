using TMPro;
using UnityEngine;

public class UIGameSettingsStartParamsPanel : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField numAgentsText;
	[SerializeField]
	private TMP_InputField numInterestsText;
	[SerializeField]
	private TMP_InputField numPredatorsText;

	private void Awake()
	{
		numAgentsText.onValueChanged.AddListener(NumAgentsTextOnValueChanged);
		numInterestsText.onValueChanged.AddListener(NumInterestsTextOnValueChanged);
		numPredatorsText.onValueChanged.AddListener(NumPredatorsTextOnValueChanged);
	}

	private void NumAgentsTextOnValueChanged(string value)
	{
		App.Instance.Services.Get<EventsService>().UIUpdateConfigStartNumAgents?.Invoke(int.Parse(value != "" ? value : "0"));
	}
	private void NumInterestsTextOnValueChanged(string value)
	{
		App.Instance.Services.Get<EventsService>().UIUpdateConfigStartNumInterests?.Invoke(int.Parse(value != "" ? value : "0"));
	}
	private void NumPredatorsTextOnValueChanged(string value)
	{
		App.Instance.Services.Get<EventsService>().UIUpdateConfigStartNumPredators?.Invoke(int.Parse(value != "" ? value : "0"));
	}

	public void SetStartParams(GameConfigService gameConfig)
	{
		numAgentsText.SetTextWithoutNotify(gameConfig.startNumAgents.ToString());
		numInterestsText.SetTextWithoutNotify(gameConfig.startNumInterests.ToString());
		numPredatorsText.SetTextWithoutNotify(gameConfig.startNumPredators.ToString());
	}
}