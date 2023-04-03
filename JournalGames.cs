using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DedicatedFTPServerValheim
{
    public partial class JournalGames : Form
    {
        public JournalGames(List<ServerStatus> list)
        {
            InitializeComponent();
            serverStatusBindingSource.DataSource = list;
            this.DoubleBuffered = true;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
