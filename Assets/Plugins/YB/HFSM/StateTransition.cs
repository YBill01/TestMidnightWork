using System;

namespace YB.HFSM
{
	public class StateTransition
	{
		private event Action TransitionApplied;

		internal StateMachine CommonStateMachine { get; private set; }

		internal State OriginState { get; private set; }
		internal State TargetState { get; private set; }

		private readonly Func<bool>[] _conditions;

		public StateTransition(StateMachine commonStateMachine, State originState, State targetState, Action transitionApplied = null, params Func<bool>[] conditions)
		{
			CommonStateMachine = commonStateMachine;

			OriginState = originState;
			TargetState = targetState;

			TransitionApplied = transitionApplied;

			_conditions = conditions;
		}

		internal virtual bool IsConditionsMet()
		{
			foreach (Func<bool> condition in _conditions)
			{
				if (!condition())
				{
					return false;
				}
			}

			return true;
		}

		internal void InvokeTransitionAction()
		{
			TransitionApplied?.Invoke();
		}
	}
}