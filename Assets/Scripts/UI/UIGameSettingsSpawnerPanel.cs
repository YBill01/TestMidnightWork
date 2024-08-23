using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameSettingsSpawnerPanel : MonoBehaviour
{
	[SerializeField]
	private Button spawnAgentsButton;
	[SerializeField]
	private TMP_InputField agentsText;

	[Space(10)]
	[SerializeField]
	private Button spawnInterestsButton;
	[SerializeField]
	private TMP_InputField interestsText;

	[Space(10)]
	[SerializeField]
	private Button spawnPredatorsButton;
	[SerializeField]
	private TMP_InputField predatorsText;

	private void Awake()
	{
		spawnAgentsButton.onClick.AddListener(SpawnAgentsButtonOnClick);
		spawnInterestsButton.onClick.AddListener(SpawnInterestsButtonButtonOnClick);
		spawnPredatorsButton.onClick.AddListener(SpawnPredatorsButtonButtonOnClick);
	}

	private void SpawnAgentsButtonOnClick()
	{
		App.Instance.Services.Get<EventsService>().UISpawnAgents?.Invoke(int.Parse(agentsText.text));
	}
	private void SpawnInterestsButtonButtonOnClick()
	{
		App.Instance.Services.Get<EventsService>().UISpawnInterests?.Invoke(int.Parse(interestsText.text));
	}
	private void SpawnPredatorsButtonButtonOnClick()
	{
		App.Instance.Services.Get<EventsService>().UISpawnPredators?.Invoke(int.Parse(predatorsText.text));
	}
}