﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jed.StateMachine
{
	public class TimerManager
	{
		private List<StateTimePair> timers;
		
		public TimerManager()
		{
			timers = new List<StateTimePair>();
		}

		public void SetTimer(State state, DateTime timeout)
		{
			lock (timers)
			{
				timers.Add(new StateTimePair(state, timeout));
				timers.Sort((a, b) => a.Time.CompareTo(b.Time));
			}
		}

		public void ClearTimer(State state)
		{
			lock (timers)
			{
				timers.RemoveAll(stp => stp.State.Equals(state));
			}
		}

		public State GetNextStateTimeout()
		{
			lock (timers)
			{
				var timer = timers.FirstOrDefault();
				if (timer != null && timer.Time < DateTime.Now)
				{
					timers.Remove(timer);
					return timer.State;
				}

				return null;
			}
		}

		public TimeSpan GetTimeToNextTimeout()
		{
			lock (timers)
			{
				var timer = timers.FirstOrDefault();
				if (timer != null)
					return timer.Time - DateTime.Now;

				return TimeSpan.FromDays(1);
			}
		}

		private class StateTimePair
		{
			public State State { get; private set; }
			public DateTime Time { get; private set; }

			public StateTimePair(State state, DateTime time)
			{
				this.State = state;
				this.Time = time;
			}
		}
	}
}
