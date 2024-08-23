using System;
using System.Collections.Generic;

namespace YB.HFSM
{
	public abstract class State
	{
		internal StateMachine StateMachine { get; set; }

		public bool IsActive { get; protected set; }

		public bool IsRoot
		{
			get => StateMachine == null;
		}

		protected List<StateTransition> _transitions;

		protected bool _stateChanged;

		public State()
		{
			_transitions = new List<StateTransition>();
		}

		public void AddTransition(State targetState, params Func<bool>[] conditions)
		{
			StateMachine commonStateMachine = GetCommonStateMachine(this, targetState);
			if (commonStateMachine != null)
			{
				StateTransition transition = new StateTransition(commonStateMachine, this, GetNearestTargetState(commonStateMachine, targetState), null, conditions);
				_transitions.Add(transition);
			}
		}
		public void AddTransition(State targetState, Action transitionApplied, params Func<bool>[] conditions)
		{
			StateMachine commonStateMachine = GetCommonStateMachine(this, targetState);
			if (commonStateMachine != null)
			{
				StateTransition transition = new StateTransition(commonStateMachine, this, GetNearestTargetState(commonStateMachine, targetState), transitionApplied, conditions);
				_transitions.Add(transition);
			}
		}

		public Action AddEventTransition(State targetState)
		{
			StateMachine commonStateMachine = GetCommonStateMachine(this, targetState);
			if (commonStateMachine != null)
			{
				StateEventTransition transition = new StateEventTransition(commonStateMachine, this, GetNearestTargetState(commonStateMachine, targetState));
				_transitions.Add(transition);

				return transition.ListenEvent;
			}

			return null;
		}
		public Action AddEventTransition(State targetState, Action transitionApplied)
		{
			StateMachine commonStateMachine = GetCommonStateMachine(this, targetState);
			if (commonStateMachine != null)
			{
				StateEventTransition transition = new StateEventTransition(commonStateMachine, this, GetNearestTargetState(commonStateMachine, targetState), transitionApplied);
				_transitions.Add(transition);

				return transition.ListenEvent;
			}

			return null;
		}

		internal StateMachine GetCommonStateMachine(State state1, State state2)
		{
			StateMachine stateMachineResult = null;

			StateMachine stateMachine1 = state1.StateMachine;
			StateMachine stateMachine2 = state2.StateMachine;

			if (stateMachine1 != null && stateMachine2 != null)
			{
				LinkedListNode<StateMachine> stateMachineNode1 = stateMachine1.PathFromRoot.First;
				LinkedListNode<StateMachine> stateMachineNode2 = stateMachine1.PathFromRoot.First;

				while (stateMachineNode1 != null && stateMachineNode2 != null && stateMachineNode1.Value.Equals(stateMachineNode2.Value))
				{
					stateMachineResult = stateMachineNode1.Value;

					stateMachineNode1 = stateMachineNode1.Next;
					stateMachineNode2 = stateMachineNode2.Next;
				}
			}

			return stateMachineResult;
		}

		internal State GetNearestTargetState(StateMachine stateMachine, State state)
		{
			State resultState = state;

			while (!resultState.IsRoot && !resultState.StateMachine.Equals(stateMachine))
			{
				resultState = resultState.StateMachine;
			}

			return resultState;
		}

		private bool TryChangeState()
		{
			foreach (StateTransition transition in _transitions)
			{
				if (transition.IsConditionsMet())
				{
					ChangeState(transition);

					return true;
				}
			}

			return false;
		}
		internal void ChangeState(StateTransition transition)
		{
			transition.InvokeTransitionAction();
			transition.CommonStateMachine.SetState(transition.TargetState);
		}

		protected virtual void OnEnter() { }
		protected virtual void OnExit() { }

		protected virtual void OnUpdate() { }

		protected virtual void OnFixedUpdate() { }
		protected virtual void OnLateUpdate() { }

		internal virtual void Enter()
		{
			IsActive = true;
			OnEnter();
		}

		internal virtual void Exit()
		{
			IsActive = false;
			OnExit();
		}

		public virtual void Update()
		{
			_stateChanged = TryChangeState();

			if (!_stateChanged)
			{
				OnUpdate();
			}
		}

		public virtual void FixedUpdate()
		{
			OnFixedUpdate();
		}

		public virtual void LateUpdate()
		{
			OnLateUpdate();
		}
	}
}