using YB.HFSM;

public class GameStateMachine : StateMachine
{
	public GameStateMachine(params State[] states) : base(states)
	{
	}
}