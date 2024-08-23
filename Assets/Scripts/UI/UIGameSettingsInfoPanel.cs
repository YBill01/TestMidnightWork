using TMPro;
using UnityEngine;

public class UIGameSettingsInfoPanel : MonoBehaviour
{
	[SerializeField]
	private TMP_Text countAgentsText;
	[SerializeField]
	private TMP_Text countInterestsText;
	[SerializeField]
	private TMP_Text countPredatorsText;

	private void OnEnable()
	{
		App.Instance.Services.Get<EventsService>().GameplayUpdateInfo += OnGameplayUpdateInfo;
	}
	private void OnDisable()
	{
		App.Instance.Services.Get<EventsService>().GameplayUpdateInfo -= OnGameplayUpdateInfo;
	}

	private void OnGameplayUpdateInfo(GameInfo info)
	{
		countAgentsText.text = info.countAgents.ToString();
		countInterestsText.text = info.countInterests.ToString();
		countPredatorsText.text = info.countPredators.ToString();
	}
}