using UnityEngine;
using UnityEngine.UI;

public class UIGameBackScreen : UIScreen
{
	[Space(10)]
	[SerializeField]
	private Button backButton;

	private void Awake()
	{
		backButton.onClick.AddListener(BackButtonOnClick);
	}

	private void BackButtonOnClick()
	{
		App.Instance.Services.Get<EventsService>().UIGameBack?.Invoke();
	}
}