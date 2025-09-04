namespace PlatformerController2D.Runtime.Scripts.StateMachine
{
	public interface ITransition
	{
		IState To { get; } // Куда
		IPredicate Condition { get; } // Условие
	}
}
