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
            currentState = currentState.Next();
            Debug.Log($"Current {typeof(T).FullName} changed to {currentState}");
        }
    }
}
