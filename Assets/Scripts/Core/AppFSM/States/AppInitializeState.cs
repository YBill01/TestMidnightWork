using YB.HFSM;

public class AppInitializeState : State
{
	private readonly EventsService _events;

	public AppInitializeState(EventsService events)
	{
		_events = events;
	}

	protected override void OnEnter()
	{
		_events.AppInitialized?.Invoke();
	}
}