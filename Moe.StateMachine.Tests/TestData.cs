﻿namespace Moe.StateMachine.Tests
{
	public enum States
	{
		Green,
		GreenParent,
		GreenGrandParent,
		GreenChild,
		GreenChild2,
		GreenGrandChild,
		Red,
		RedParent,
		RedChild,
		Yellow,
		YellowChild,
		Gold
	}

	public enum Events
	{
		Change,
		Panic,
		OutATime
	}
}
