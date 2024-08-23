using YB.HFSM;

public class AppGameLoadState : State
{
	private readonly LoaderService _loader;
	private readonly EventsService _events;
	private readonly int _sceneId;

	public AppGameLoadState(LoaderService loader, EventsService events, int sceneId)
	{
		_loader = loader;
		_events = events;
		_sceneId = sceneId;
	}

	protected override void OnEnter()
	{
		_loader.LoadScene(_sceneId).OnComplete(() =>
		{
			_events.GameSceneLoaded?.Invoke();
		});
	}
}