using System;

namespace PlatformerController2D.Runtime.Scripts.StateMachine
{
    public abstract class StateMachineFactory<TStates> where TStates : class, new()
    {
        protected TStates States { get; private set; }
        protected StateMachine stateMachine;

        public StateMachine CreateStateMachine()
        {
            stateMachine = new StateMachine();
        
            // Создаем состояния
            States = CreateStates();
        
            // Настраиваем переходы
            SetupTransitions(States);
        
            // Устанавливаем начальное состояние
            var initialState = GetInitialState(States);
            stateMachine.SetState(initialState);
        
            return stateMachine;
        }

        // Абстрактные методы, которые должны быть реализованы в наследниках
        protected abstract TStates CreateStates();
        protected abstract void SetupTransitions(TStates states);
        protected abstract IState GetInitialState(TStates states);

        // Утилитарный метод для добавления переходов
        protected void AddTransition(IState from, IState to, Func<bool> condition)
        {
            stateMachine.AddTransition(from, to, new FuncPredicate(condition));
        }
	
        protected void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        protected void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
    }
}