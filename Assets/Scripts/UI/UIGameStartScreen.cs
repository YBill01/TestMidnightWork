using UnityEngine;
using UnityEngine.UI;

public class UIGameStartScreen : UIScreen
{
	[Space(10)]
	[SerializeField]
	private Button startButton;

	private void Awake()
	{
		startButton.onClick.AddListener(StartButtonOnClick);
	}

	private void StartButtonOnClick()
	{
		App.Instance.Services.Get<EventsService>().UIGameStart?.Invoke();
	}
}