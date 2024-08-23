using System;
using System.Collections.Generic;

namespace YB.HFSM
{
	public abstract class StateMachine : State
	{
		public State DefaultState { get; set; }
		public State CurrentState { get; private set; }

		internal LinkedList<StateMachine> PathFromRoot
		{
			get
			{
				if (_pathFromRoot == null)
				{
					if (IsRoot)
					{
						_pathFromRoot = new LinkedList<StateMachine>();
					}
					else
					{
						_pathFromRoot = new LinkedList<StateMachine>(StateMachine.PathFromRoot);
					}

					_pathFromRoot.AddLast(this);
				}

				return _pathFromRoot;
			}
		}
		private LinkedList<StateMachine> _pathFromRoot;

		public StateMachine(params State[] states)
		{
			if (states.Length == 0)
			{
				throw new ArgumentException($"State machine of type '{GetType()}' does not have any states.");
			}

			DefaultState = states[0];
			foreach (State state in states)
			{
				state.StateMachine = this;
			}
		}

		public void Init()
		{
			if (IsRoot)
			{
				Enter();
			}
		}

		internal void SetState(State state)
		{
			CurrentState?.Exit();
			CurrentState = state;
			CurrentState.Enter();
		}

		internal override void Enter()
		{
			base.Enter();

			CurrentState ??= DefaultState;
			CurrentState.Enter();
		}

		internal override void Exit()
		{
			base.Exit();

			CurrentState.Exit();
			CurrentState = null;
		}

		public override void Update()
		{
			base.Update();

			if (!_stateChanged)
			{
				CurrentState.Update();
			}
		}

		protected override void OnLateUpdate()
		{
			if (!_stateChanged)
			{
				base.OnLateUpdate();
				CurrentState.LateUpdate();
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			CurrentState.FixedUpdate();
		}
	}
}