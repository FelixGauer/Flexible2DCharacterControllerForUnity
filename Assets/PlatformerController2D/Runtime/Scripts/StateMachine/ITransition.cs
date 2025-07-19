using System;

public interface ITransition
{
	IState To { get; } // Куда
	IPredicate Condition { get; } // Условие
}
