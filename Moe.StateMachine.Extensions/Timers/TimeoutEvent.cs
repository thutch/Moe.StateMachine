﻿using System;
using Moe.StateMachine.States;

namespace Moe.StateMachine.Extensions.Timers
{
	public class TimeoutEvent : ITimer, IDisposable
	{
		public event EventHandler<EventArgs> ActiveChanged;

		private bool active;
		private DateTime nextTime;
		private int timeoutPeriod;
		private State state;
		private Action<State, object> eventPoster;
		private object transitionEvent;

		public TimeoutEvent(int timeoutInMilliseconds, object transitionEvent, State state, Action<State, object> eventPoster)
		{
			this.timeoutPeriod = timeoutInMilliseconds;
			this.eventPoster = eventPoster;
			this.transitionEvent = transitionEvent;
			this.state = state;
			this.state.Entered += OnStateEntered;
			this.state.Exited += OnStateExited;
		}

		public bool Active { get { return active; } }
		public DateTime NextTime { get { return nextTime; } }

		private void OnStateEntered(object sender, StateTransitionEventArgs args)
		{
			nextTime = DateTime.Now.AddMilliseconds(timeoutPeriod);
			SetActive(true);
		}

		private void OnStateExited(object sender, StateTransitionEventArgs args)
		{
			SetActive(false);
		}

		public void Execute()
		{
			SetActive(false);
			eventPoster(state, transitionEvent);
		}

		public void Dispose()
		{
			this.state.Entered -= OnStateEntered;
			this.state.Exited -= OnStateExited;
		}

		private void SetActive(bool activeFlag)
		{
			active = activeFlag;

			if (ActiveChanged != null)
				ActiveChanged(this, new EventArgs());
		}
	}
}
