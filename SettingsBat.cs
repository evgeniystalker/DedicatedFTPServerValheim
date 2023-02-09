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
    public partial class SettingsBatForm : Form
    {
        public SettingModel SettingModel { get; set; }

        public SettingsBatForm()
        {
            InitializeComponent();
            SettingModel = new SettingModel();
        }
        public SettingsBatForm(SettingModel setting)
        {
            InitializeComponent();
            SettingModel = setting;
        }

        private void SettingsBat_Load(object sender, EventArgs e)
        {
            textBoxName.Text = SettingModel.Name;
            textBoxPassword.Text = SettingModel.Password;
            textBoxPort.Text = SettingModel.Port.ToString();
            textBoxWorld.Text = SettingModel.World;
            checkBoxNoGraphics.Checked = SettingModel.Nographics;
            checkBoxBatchMode.Checked = SettingModel.Batchmode;
            checkBoxCrossPlay.Checked = SettingModel.Crossplay;
            checkBoxSaveBat.Checked = Properties.Settings.Default.SaveParamFlag;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Int32.TryParse(textBoxPort.Text, out int port);
            SettingModel = new SettingModel(textBoxName.Text, port, textBoxWorld.Text, textBoxPassword.Text, checkBoxNoGraphics.Checked, checkBoxBatchMode.Checked, checkBoxCrossPlay.Checked, (Owner as DedicatedFTPServerValheim).Bat.savedirParameter);
            (Owner as DedicatedFTPServerValheim).Bat.SettingBatModel = SettingModel;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBoxPassword_Enter(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = false;
        }

        private void textBoxPassword_Leave(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = true;
        }

        private void checkBoxSaveBat_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveParamFlag = checkBoxSaveBat.Checked;
        }

        private void resetSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingModel = new SettingModel();
            this.OnLoad(e);
        }

    }

}
