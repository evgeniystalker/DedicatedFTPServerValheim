namespace DedicatedFTPServerValheim
{
    partial class SettingsBatForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsBatForm));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelName = new System.Windows.Forms.Label();
            this.checkBoxNoGraphics = new System.Windows.Forms.CheckBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.labelWorld = new System.Windows.Forms.Label();
            this.textBoxWorld = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.checkBoxBatchMode = new System.Windows.Forms.CheckBox();
            this.checkBoxCrossPlay = new System.Windows.Forms.CheckBox();
            this.checkBoxSaveBat = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(121, 129);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "ОК";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(211, 129);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Отмена";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(12, 15);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(42, 15);
            this.labelName.TabIndex = 2;
            this.labelName.Text = "-name";
            // 
            // checkBoxNoGraphics
            // 
            this.checkBoxNoGraphics.AutoSize = true;
            this.checkBoxNoGraphics.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxNoGraphics.Checked = true;
            this.checkBoxNoGraphics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNoGraphics.Location = new System.Drawing.Point(12, 99);
            this.checkBoxNoGraphics.Name = "checkBoxNoGraphics";
            this.checkBoxNoGraphics.Size = new System.Drawing.Size(90, 19);
            this.checkBoxNoGraphics.TabIndex = 3;
            this.checkBoxNoGraphics.Text = "-nographics";
            this.checkBoxNoGraphics.UseVisualStyleBackColor = true;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(60, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(136, 23);
            this.textBoxName.TabIndex = 4;
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(242, 12);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(44, 23);
            this.textBoxPort.TabIndex = 5;
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(202, 15);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(34, 15);
            this.labelPort.TabIndex = 6;
            this.labelPort.Text = "-port";
            // 
            // labelWorld
            // 
            this.labelWorld.AutoSize = true;
            this.labelWorld.Location = new System.Drawing.Point(12, 44);
            this.labelWorld.Name = "labelWorld";
            this.labelWorld.Size = new System.Drawing.Size(42, 15);
            this.labelWorld.TabIndex = 6;
            this.labelWorld.Text = "-world";
            // 
            // textBoxWorld
            // 
            this.textBoxWorld.Location = new System.Drawing.Point(60, 41);
            this.textBoxWorld.Name = "textBoxWorld";
            this.textBoxWorld.Size = new System.Drawing.Size(226, 23);
            this.textBoxWorld.TabIndex = 5;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(12, 73);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(62, 15);
            this.labelPassword.TabIndex = 6;
            this.labelPassword.Text = "-password";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(80, 70);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(206, 23);
            this.textBoxPassword.TabIndex = 5;
            this.textBoxPassword.UseSystemPasswordChar = true;
            this.textBoxPassword.Enter += new System.EventHandler(this.textBoxPassword_Enter);
            this.textBoxPassword.Leave += new System.EventHandler(this.textBoxPassword_Leave);
            // 
            // checkBoxBatchMode
            // 
            this.checkBoxBatchMode.AutoSize = true;
            this.checkBoxBatchMode.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxBatchMode.Checked = true;
            this.checkBoxBatchMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBatchMode.Location = new System.Drawing.Point(108, 99);
            this.checkBoxBatchMode.Name = "checkBoxBatchMode";
            this.checkBoxBatchMode.Size = new System.Drawing.Size(92, 19);
            this.checkBoxBatchMode.TabIndex = 7;
            this.checkBoxBatchMode.Text = "-batchmode";
            this.checkBoxBatchMode.UseVisualStyleBackColor = true;
            // 
            // checkBoxCrossPlay
            // 
            this.checkBoxCrossPlay.AutoSize = true;
            this.checkBoxCrossPlay.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxCrossPlay.Checked = true;
            this.checkBoxCrossPlay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCrossPlay.Location = new System.Drawing.Point(206, 99);
            this.checkBoxCrossPlay.Name = "checkBoxCrossPlay";
            this.checkBoxCrossPlay.Size = new System.Drawing.Size(80, 19);
            this.checkBoxCrossPlay.TabIndex = 8;
            this.checkBoxCrossPlay.Text = "-crossplay";
            this.checkBoxCrossPlay.UseVisualStyleBackColor = true;
            // 
            // checkBoxSaveBat
            // 
            this.checkBoxSaveBat.AutoSize = true;
            this.checkBoxSaveBat.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxSaveBat.Location = new System.Drawing.Point(18, 124);
            this.checkBoxSaveBat.Name = "checkBoxSaveBat";
            this.checkBoxSaveBat.Size = new System.Drawing.Size(84, 34);
            this.checkBoxSaveBat.TabIndex = 0;
            this.checkBoxSaveBat.Text = "Сохранять\r\nнастройки";
            this.checkBoxSaveBat.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetSettingsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(147, 26);
            // 
            // resetSettingsToolStripMenuItem
            // 
            this.resetSettingsToolStripMenuItem.Name = "resetSettingsToolStripMenuItem";
            this.resetSettingsToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.resetSettingsToolStripMenuItem.Text = "Reset settings";
            this.resetSettingsToolStripMenuItem.Click += new System.EventHandler(this.resetSettingsToolStripMenuItem_Click);
            // 
            // SettingsBatForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 166);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.ControlBox = false;
            this.Controls.Add(this.checkBoxSaveBat);
            this.Controls.Add(this.checkBoxCrossPlay);
            this.Controls.Add(this.checkBoxBatchMode);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.labelWorld);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxWorld);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.checkBoxNoGraphics);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsBatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройка .bat файла";
            this.Load += new System.EventHandler(this.SettingsBat_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button buttonOk;
        private Button buttonCancel;
        private Label labelName;
        private CheckBox checkBoxNoGraphics;
        private TextBox textBoxName;
        private TextBox textBoxPort;
        private Label labelPort;
        private Label labelWorld;
        private TextBox textBoxWorld;
        private Label labelPassword;
        private TextBox textBoxPassword;
        private CheckBox checkBoxBatchMode;
        private CheckBox checkBoxCrossPlay;
        private CheckBox checkBoxSaveBat;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem resetSettingsToolStripMenuItem;
    }
}