﻿using System;
using Moe.StateMachine.Actions;
using Moe.StateMachine.Events;
using Moe.StateMachine.States;

namespace Moe.StateMachine
{
    public class StateMachine : IDisposable
    {
		public static readonly object DefaultEntryEvent = "DefaultEntry";
		public static readonly object TimeoutEvent = "Timeout";
    	public static readonly object PulseEvent = "Pulse";

    	protected State current;
		protected RootState root;
		protected StateBuilder rootBuilder;
		protected EventProcessor eventHandler;
		protected TimerManager timers;

		public StateMachine()
		{
			root = new RootState(this);
			rootBuilder = this.CreateStateBuilder(root);
			eventHandler = new EventProcessor();
			timers = new TimerManager();
		}

		/// <summary>
		/// Short circuit indexer.  This will fetch ANY state by id if it exists.
		/// If it doesn't exist, it will be created off the root state.  
		/// Statebuilder does NOT work the same way.
		/// </summary>
		/// <param name="idx">State ID</param>
		/// <returns></returns>
    	public StateBuilder this[object idx] 
		{ 
			get
			{
				if (root.GetState(idx) != null)
					return CreateStateBuilder(root.GetState(idx));
				return rootBuilder[idx];
			}
		}

		public RootState RootNode { get { return root; } }
		public State CurrentState { get { return current; } }

		/// <summary>
		/// Extension point for creating own builders
		/// </summary>
		/// <param name="stateId"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public virtual StateBuilder CreateStateBuilder(object stateId, State parent)
		{
			return new StateBuilder(this, parent);
		}

		/// <summary>
		/// Extension point for creating own builders
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public virtual StateBuilder CreateStateBuilder(State state)
		{
			return new StateBuilder(this, state);
		}

		/// <summary>
		/// Returns bool indicating if the machine is in the given state (at any level)
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public virtual bool InState(object state)
		{
			bool result = false;
			if (current != null)
				current.VisitParentChain(s => result |= s.Id.Equals(state));
			return result;
		}

		/// <summary>
		/// Starts the finite state machine
		/// </summary>
    	public virtual void Start()
    	{
    		current = root.ProcessEvent(root, new SingleStateEventInstance(root, DefaultEntryEvent));
			if (current == null || current == root)
				throw new InvalidOperationException("No initial state found.");
		}

		/// <summary>
		/// Sends a pulse event.  Primarily useful in a synchronous state machine with timers.
		/// </summary>
		public virtual void Pulse()
		{
			PostEvent(PulseEvent);
		}

		/// <summary>
		/// Post an event to the state machine
		/// </summary>
		/// <param name="eventToPost"></param>
		public virtual void PostEvent(object eventToPost)
		{
			PostEvent(new EventInstance(eventToPost));
		}

		/// <summary>
		/// Post an event to the state machine
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="eventToPost"></param>
		/// <param name="context"></param>
		public virtual void PostEvent<T>(object eventToPost, T context)
		{
			PostEvent(new EventInstance(eventToPost, context));
		}

		protected virtual void PostEvent(EventInstance eventToPost)
		{
			UpdateTimers();

			eventHandler.AddEvent(eventToPost);

			while (eventHandler.CanProcess)
				current = eventHandler.ProcessNextEvent(current);
		}

		public virtual void RegisterTimer(State s, DateTime timeout)
		{
			timers.SetTimer(s, timeout);
		}

		public virtual void RemoveTimer(State s)
		{
			timers.ClearTimer(s);
		}

		protected virtual void UpdateTimers()
		{
			// Only grab one timeout at a time, a flurry of timeouts isn't helpful
			State timeout = timers.GetNextStateTimeout();
			if (timeout != null)
				eventHandler.AddEvent(new SingleStateEventInstance(timeout, TimeoutEvent));
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

		public StateBuilder DefaultTransition(object targetState, Func<bool> guard)
		{
			return rootBuilder.DefaultTransition(targetState, guard);
		}

		internal State GetState(object identifier)
		{
			return root.GetState(identifier);
		}
		#endregion

		public virtual void Dispose()
		{
		}
	}
}