using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SRPGDemo.Utility
{
    public interface IStateMachineState
    {
        void EnterState();
        void LeaveState();
    }

    public class StateMachineMonoBehaviour<TState> : MonoBehaviour where TState : class, IStateMachineState
    {
        protected TState currentState = null;
        private bool stateLocked = false;

        protected virtual void EnterCurrentState()
        {
            if (currentState == null)
                Debug.Log("State machine entering null.");
            else
            {
                Debug.Log("State machine entering " + currentState.GetType().Name + ".");
                currentState.EnterState();
            }
        }

        protected virtual void LeaveCurrentState()
        {
            if (currentState == null)
                Debug.Log("State machine exiting null.");
            else
            {
                Debug.Log("State machine exiting " + currentState.GetType().Name + ".");
                currentState.LeaveState();
            }
        }

        protected virtual void StateTransition(TState newState)
        {
            currentState = newState;
        }

        public void ChangeState(TState newState)
        {
            if (stateLocked)
            {
                throw new Exception("Trying to change state while in a state transition!");
            }
            else if (newState == currentState)
            {
                Debug.Log("Neutral state change from/to " + newState.GetType().Name + ".");
            }
            else
            {
                stateLocked = true;

                LeaveCurrentState();

                StateTransition(newState);

                EnterCurrentState();

                stateLocked = false;
            }
        }
    }
}
