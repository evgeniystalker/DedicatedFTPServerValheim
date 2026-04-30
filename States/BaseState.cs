using System;
using System.Collections.Generic;
using System.Text;

namespace DedicatedFTPServerValheim.States
{
    internal abstract class BaseState : IFormState
    {
        protected StateContext StateContext { get; set; }
        protected BaseState(StateContext context) 
        {
            StateContext = context;
        }

        public abstract void EnterState();
        public abstract void ExistingState();
    }
}
