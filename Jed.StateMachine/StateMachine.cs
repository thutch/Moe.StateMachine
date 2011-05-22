﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jed.StateMachine
{
    public class StateMachine
    {
		internal readonly object DefaultEntryEvent = new object();

    	private State current;
    	private State root;
    	private StateBuilder rootBuilder;

		public StateMachine()
		{
			root = new State("Root", null);
			rootBuilder = new StateBuilder(this, root);
		}

    	public StateBuilder this[object idx] 
		{ 
			get { return new StateBuilder(this, root.GetState(idx)); }
		}

		public bool InState(object state)
		{
			bool result = false;
			if (current != null)
				current.VisitParentChain(s => result |= s.Id.Equals(state));
			return result;
		}

    	public virtual void Start()
    	{
    		current = root;
    		PostEvent(DefaultEntryEvent);
			if (current == root)
				throw new InvalidOperationException();
		}

		public virtual void PostEvent(object eventToPost)
		{
			PostEventFull(eventToPost);
		}

		private void PostEventShallow(object eventToPost)
		{
			TransitionInstance transition = current.EvaluateEvent(eventToPost);
			PerformTransition(transition);
		}

		private void PostEventFull(object eventToPost)
		{
			TransitionInstance transition = null;
			current.VisitParentChain(s =>
			{
				TransitionInstance found = s.EvaluateEvent(eventToPost);
				if (found != null && transition == null)
					transition = found;
			});

			PerformTransition(transition);
		}

		private void PerformTransition(TransitionInstance transition)
		{
			// Perform state transition
			if (transition != null)
			{
				current = transition.Transition();
				HandleSuperStateEntry();
			}
		}

		private void HandleSuperStateEntry()
		{
			State previous;
			do
			{
				previous = current;
				PostEventShallow(DefaultEntryEvent);
			} while (current != previous);
		}

		#region StateBuilder forwarding and help
		public StateBuilder AddState(object identifier)
		{
			return rootBuilder.AddState(identifier);
		}

		public StateBuilder DefaultTransition(object targetState)
		{
			return rootBuilder.DefaultTransition(targetState);
		}

		internal State GetState(object identifier)
		{
			return root.GetState(identifier);
		}
		#endregion
	}
}
