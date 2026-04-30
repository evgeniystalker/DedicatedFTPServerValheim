using System;
using System.Collections.Generic;
using System.Text;

namespace DedicatedFTPServerValheim.States
{
    internal interface IFormState
    {
        public void ExistingState();

        public void EnterState();
    }
}
