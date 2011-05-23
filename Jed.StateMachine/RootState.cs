﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jed.StateMachine
{
	public class RootState : State
	{
		private StateMachine stateMachine;

		public RootState(StateMachine stateMachine) 
			: base("Root", null)
		{
			this.stateMachine = stateMachine;
		}

		/// <summary>
		/// By returning the current state of the fsm, we nullify this event because nobody responded.
		/// </summary>
		/// <param name="eventToProcess"></param>
		/// <returns></returns>
		public override State ProcessEvent(EventInstance eventToProcess)
		{
			Transition transition = transitions.MatchTransition(eventToProcess);
			if (transition != null)
				return Traverse(transition);

			return this.stateMachine.CurrentState;
		}

		/// <summary>
		/// Traversal that doesn't go to children of the root are for unknown states.
		/// </summary>
		/// <param name="transition"></param>
		/// <returns></returns>
		internal override State Traverse(Transition transition)
		{
			// Traverse down to children?
			foreach (State substate in Substates)
			{
				if (substate.ContainsState(transition.TargetState.Id))
				{
					Enter();
					return substate.Traverse(transition);
				}
			}

			throw new InvalidOperationException("Transition state [" + transition.TargetState.Id.ToString() + "] not found");
		}
	}
}
