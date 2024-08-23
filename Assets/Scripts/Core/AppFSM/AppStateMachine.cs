using YB.HFSM;

public class AppStateMachine : StateMachine
{
	public AppStateMachine(params State[] states) : base(states)
	{
	}
}