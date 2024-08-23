using System;

namespace YB.HFSM
{
	public class StateEventTransition : StateTransition
	{
		private bool _forced;
		private bool _eventListened;

		public StateEventTransition(StateMachine commonStateMachine, State originState, State targetState, Action transitionApplied = null, bool forced = false) : base(commonStateMachine, originState, targetState, transitionApplied)
		{
			_forced = forced;
		}

		internal override bool IsConditionsMet()
		{
			bool eventListenedResult = _eventListened;

			_eventListened = false;

			return eventListenedResult;
		}

		public void ListenEvent()
		{
			if (OriginState.IsActive)
			{
				if (_forced)
				{
					OriginState.ChangeState(this);
				}
				else
				{
					_eventListened = true;
				}
			}
		}
	}
}