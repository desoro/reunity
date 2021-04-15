using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phuntasia.Fsm
{
    public abstract class Machine : BaseBehaviour
    {
        public bool IsRunning { get; private set; }
        public float TimeInState => Time.time - _timeLastChange;
        public MachineState LastState { get; private set; }
        public MachineState State { get; private set; }

        Dictionary<Type, MachineState> _stateMap;
        float _timeLastChange;

        protected override void Awake()
        {
            base.Awake();

            _stateMap = new Dictionary<Type, MachineState>();
        }

        protected virtual void Update()
        {
            if (State == null)
            {
                return;
            }

            State.Update();
        }

        protected virtual void FixedUpdate()
        {
            if (State == null)
            {
                return;
            }

            State.FixedUpdate();
        }

        public void StartMachine<TStartState>()
            where TStartState : MachineState, new()
        {
            IsRunning = true;

            ChangeState<TStartState>();
        }

        public void StopMachine(bool clearStates = false)
        {
            if (IsRunning)
            {
                IsRunning = false;

                State.Interupt();

                LastState = null;
                State = null;
            }

            if (clearStates)
            {
                _stateMap.Clear();
            }
        }

        T GetState<T>()
            where T : MachineState, new()
        {
            if (!_stateMap.TryGetValue(typeof(T), out var state))
            {
                state = new T
                {
                    Machine = this
                };

                _stateMap.Add(typeof(T), state);
            }

            return state as T;
        }

        public void ChangeState<T>(Action<T> initializer = null)
            where T : MachineState, new()
        {
            if (IsRunning == false)
            {
                throw new Exception("Machine is not running.");
            }

            _timeLastChange = Time.time;

            LastState = State;
            State = GetState<T>();

            if (LastState != null)
            {
                if (LastState.IsNotValidTransition<T>())
                {
                    LastState.Interupt();
                }

                LastState.Exit();
            }

            initializer?.Invoke(State as T);

            State.Enter();
        }

        public bool IsLastState<T>()
            where T : MachineState
        {
            return LastState is T;
        }

        public bool IsNotLastState<T>()
           where T : MachineState
        {
            return !IsLastState<T>();
        }

        public bool IsState<T>()
            where T : MachineState
        {
            return State is T;
        }

        public bool IsNotState<T>()
            where T : MachineState
        {
            return !IsState<T>();
        }
    }
}