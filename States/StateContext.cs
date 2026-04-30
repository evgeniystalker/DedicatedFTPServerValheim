using System;
using System.Collections.Generic;
using System.Text;

namespace DedicatedFTPServerValheim.States
{
    internal abstract partial class StateContext : DedicatedFTPServerValheim
    {
        IFormState _state;
        public StateContext(IFormState initialState)
        {
            _state = initialState;
            initialState.EnterState();
        }

        public void SetState(IFormState state)
        {
            _state.ExistingState();
            _state = state;
            state.EnterState();
        }
    }
}
