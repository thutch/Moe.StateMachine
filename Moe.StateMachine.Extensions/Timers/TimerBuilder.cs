using System;

namespace Moe.StateMachine.Extensions.Timers
{
	public static class TimerBuilder
	{
		public static readonly object DefaultTimeoutEvent = "Timeout";

		public static IStateBuilder Timeout(this IStateBuilder builder, int timeoutInMilliseconds, object targetState)
		{
			builder.TransitionOn(DefaultTimeoutEvent, targetState);
			AddTimeout(builder, DefaultTimeoutEvent, timeoutInMilliseconds);
			return builder;
		}
		
		public static IStateBuilder Timeout(this IStateBuilder builder, object transitiionEvent, int timeoutInMilliseconds, object targetState)
		{
			builder.TransitionOn(transitiionEvent, targetState);
			AddTimeout(builder,transitiionEvent, timeoutInMilliseconds);
			return builder;
		}

		public static IStateBuilder Timeout(this IStateBuilder builder, 
			int timeoutInMilliseconds, object targetState, Func<bool> guard)
		{
			builder.TransitionOn(DefaultTimeoutEvent).To(targetState).When(guard);
			AddTimeout(builder, DefaultTimeoutEvent, timeoutInMilliseconds);
			return builder;
		}
		
		public static IStateBuilder Timeout(this IStateBuilder builder, 
			object transitionEvent, int timeoutInMilliseconds, object targetState, Func<bool> guard)
		{
			builder.TransitionOn(transitionEvent).To(targetState).When(guard);
			AddTimeout(builder, transitionEvent, timeoutInMilliseconds);
			return builder;
		}
		
		private static void AddTimeout(IStateBuilder builder, object transitionEvent, int timeoutInMilliseconds)
		{
			if (!builder.Context.HasPlugIn<TimerPlugin>())
				builder.Context.AddPlugIn(new TimerPlugin());
			if (!builder.Context.HasPlugIn<TimeoutPlugin>())
				builder.Context.AddPlugIn(new TimeoutPlugin());

			builder.AddSecondPassAction(s =>
			                            	{
			                            		var timerPlugin = builder.Context.GetPlugIn<TimerPlugin>();
			                            		var timeoutPlugin = builder.Context.GetPlugIn<TimeoutPlugin>();
												var timeout = new TimeoutEvent(timeoutInMilliseconds, transitionEvent, s, timeoutPlugin.PostTimeout);

												timerPlugin.AddTimer(timeout);
											});
		}
	}
}