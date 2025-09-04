namespace PlatformerController2D.Runtime.Scripts.StateMachine
{
	public interface IState 
	{
		void OnEnter();
		void Update();
		void FixedUpdate();
		void OnExit();
	}
}
