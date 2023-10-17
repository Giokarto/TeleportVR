using System;
using System.Collections.Generic;
using InputDevices;
using UnityEngine;

namespace Tutorial
{
    public class Tutorial : Automaton
    {
        private void Awake()
        {
            Steps = new List<Step>()
            {
                new Welcome(),
                new MoveHead(),
            };
        }

        private new void Start()
        {
            base.Start();
        }

        private void OnEnable()
        {
            //InputSystem.OnRightPrimaryButton += StartAutomaton;
        }

        private void OnDisable()
        {
            //InputSystem.OnRightPrimaryButton -= StartAutomaton;
        }

    }
}