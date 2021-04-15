using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phuntasia.Fsm
{
    public abstract class MachineState
    {
        public Machine Machine { get; internal set; }
        public double TimeInState => Machine.TimeInState;
        protected GameObject gameObject => Machine.gameObject;
        protected Transform transform => Machine.transform;

        readonly HashSet<Type> _transitions;

        public MachineState()
        {
            _transitions = new HashSet<Type>();
        }

        protected T GetMachine<T>()
            where T : Machine
        {
            return Machine as T;
        }

        protected void AddTransition<T>()
            where T : MachineState
        {
            _transitions.Add(typeof(T));
        }

        public bool IsValidTransition<T>()
            where T : MachineState
        {
            return _transitions.Contains(typeof(T));
        }

        public bool IsNotValidTransition<T>()
            where T : MachineState
        {
            return !IsValidTransition<T>();
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Interupt() { }
        public virtual void Exit() { }
    }
}