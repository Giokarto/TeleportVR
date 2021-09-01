using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training
{
    public class Automaton<T> : MonoBehaviour where T : System.Enum
    {
        internal StateMachine<T> stateMachine = new StateMachine<T>();
        public T currentState
        {
            get { return stateMachine.State; }
            set { stateMachine.State = value; }
        }

        public void Next()
        {
            ChangeState(currentState.Next());
        }

        public void Prev()
        {
            ChangeState(currentState.Prev());
        }

        private void ChangeState(T next)
        {
            Debug.Log($"Current {typeof(T).FullName} transition {currentState} -> {next}");
            currentState = next;
        }

        public void GoToNoEnterCallback(T state)
        {
            stateMachine.GoNoEnterCallback(state);
        }
    }
}
