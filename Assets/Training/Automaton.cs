using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training
{
    public class Automaton<T> : MonoBehaviour where T : System.Enum
    {
        internal StateMachine<T> stateMachine = new StateMachine<T>();
        internal T currentState
        {
            get { return stateMachine.State; }
            set { stateMachine.State = value; }
        }
    }
}
