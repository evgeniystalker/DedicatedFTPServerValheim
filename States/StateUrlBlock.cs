using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DedicatedFTPServerValheim.States
{
    internal class StateUrlBlock : BaseState
    {
        StateUrlBlock(StateContext context) : base(context)
        { 
        }
        
        public override void EnterState()
        {
            var form = StateContext.MainForm;
            ButtonStart.Enabled = form.buttonUpload.Enabled = form.linkLabelJournal.Enabled = false;
        }

        public override void ExistingState()
        {
            ButtonStart.Enabled = buttonUpload.Enabled = linkLabelJournal.Enabled = true;
        }
    }
}
