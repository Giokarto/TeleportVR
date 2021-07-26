using System.Collections.Generic;
using System;


namespace Training
{
    public class StateMachine<T> where T : Enum
    {
        public T State
        {
            get { return _state; }
            set
            {
                bool sameState = EqualityComparer<T>.Default.Equals(_state, value);
                if (onExit.ContainsKey(_state) && !sameState)
                {
                    onExit[_state](_state);
                }
                _state = value;
                if (onEnter.ContainsKey(_state) && !sameState)
                {
                    onEnter[_state](_state);
                }
            }
        }

        private T _state;
        public Dictionary<T, Action<T>> onEnter;
        public Dictionary<T, Action<T>> onExit;

        public StateMachine()
        {
            onEnter = new Dictionary<T, Action<T>>();
            onExit = new Dictionary<T, Action<T>>();
        }

        //public void ForceTrigger(T state)
        //{
        //    if (onExit.ContainsKey(_state))
        //    {
        //        onExit[_state](_state);
        //    }
        //    _state = state;
        //    if (onEnter.ContainsKey(_state))
        //    {
        //        onEnter[_state](_state);
        //    }
        //}
    }
}
