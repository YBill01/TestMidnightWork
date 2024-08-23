using System;
using YB.HFSM;

public class GameplayStateMachine : StateMachine, IDisposable
{
	public GameplayStateMachine(params State[] states) : base(states)
	{
	}

	public void Dispose()
	{
		if (CurrentState is GameplayPlayState gameplayPlayState)
		{
			gameplayPlayState.Dispose();
		}
	}
}