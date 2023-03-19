using System;

namespace DedicatedFTPServerValheim
{
    partial class DedicatedFTPServerValheim
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DedicatedFTPServerValheim));
            label1 = new Label();
            pathBat = new TextBox();
            label2 = new Label();
            label3 = new Label();
            pathFTP = new TextBox();
            pathTemp = new TextBox();
            buttonPathBat = new Button();
            buttonPathTemp = new Button();
            ButtonStart = new Button();
            ButtonStop = new Button();
            LabelCheck = new LinkLabel();
            progressBarFtp = new CustomControls.CustomProgressBar();
            linkLabelSettings = new LinkLabel();
            contextMenuStrip1 = new ContextMenuStrip(components);
            resetSettingAplicationToolStripMenuItem = new ToolStripMenuItem();
            fileInfoDateTime = new Label();
            buttonUpload = new Button();
            toolTip = new ToolTip(components);
            cleanFilesCheckBox = new CheckBox();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 20);
            label1.Name = "label1";
            label1.Size = new Size(155, 15);
            label1.TabIndex = 8;
            label1.Text = "Путь к *.bat файлу сервера";
            // 
            // pathBat
            // 
            pathBat.Location = new Point(181, 17);
            pathBat.Name = "pathBat";
            pathBat.Size = new Size(350, 23);
            pathBat.TabIndex = 5;
            pathBat.TextChanged += pathBat_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 60);
            label2.Name = "label2";
            label2.Size = new Size(111, 15);
            label2.TabIndex = 9;
            label2.Text = "Путь к FTP серверу";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 100);
            label3.Name = "label3";
            label3.Size = new Size(142, 15);
            label3.TabIndex = 10;
            label3.Text = "Путь к временной папке";
            // 
            // pathFTP
            // 
            pathFTP.Location = new Point(181, 57);
            pathFTP.Name = "pathFTP";
            pathFTP.Size = new Size(350, 23);
            pathFTP.TabIndex = 6;
            pathFTP.TextChanged += pathFTP_TextChanged;
            // 
            // pathTemp
            // 
            pathTemp.Location = new Point(181, 97);
            pathTemp.Name = "pathTemp";
            pathTemp.Size = new Size(350, 23);
            pathTemp.TabIndex = 7;
            pathTemp.TextChanged += pathTemp_TextChanged;
            // 
            // buttonPathBat
            // 
            buttonPathBat.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonPathBat.Location = new Point(537, 17);
            buttonPathBat.Name = "buttonPathBat";
            buttonPathBat.RightToLeft = RightToLeft.No;
            buttonPathBat.Size = new Size(26, 25);
            buttonPathBat.TabIndex = 2;
            buttonPathBat.Text = "...";
            buttonPathBat.UseVisualStyleBackColor = true;
            buttonPathBat.Click += buttonPathBat_Click;
            // 
            // buttonPathTemp
            // 
            buttonPathTemp.Location = new Point(537, 97);
            buttonPathTemp.Name = "buttonPathTemp";
            buttonPathTemp.Size = new Size(25, 25);
            buttonPathTemp.TabIndex = 4;
            buttonPathTemp.Text = "...";
            buttonPathTemp.UseVisualStyleBackColor = true;
            buttonPathTemp.Click += buttonPathTemp_Click;
            // 
            // ButtonStart
            // 
            ButtonStart.Location = new Point(150, 146);
            ButtonStart.Name = "ButtonStart";
            ButtonStart.Size = new Size(75, 23);
            ButtonStart.TabIndex = 0;
            ButtonStart.Text = "Start";
            toolTip.SetToolTip(ButtonStart, "Выгрузка файлов из FTP и запуск сервера.");
            ButtonStart.UseVisualStyleBackColor = true;
            ButtonStart.Click += ButtonStart_Click;
            // 
            // ButtonStop
            // 
            ButtonStop.Location = new Point(365, 146);
            ButtonStop.Name = "ButtonStop";
            ButtonStop.Size = new Size(75, 23);
            ButtonStop.TabIndex = 1;
            ButtonStop.Text = "Stop";
            toolTip.SetToolTip(ButtonStop, "Остановить сервер и загрузить файлы на FTP.");
            ButtonStop.UseVisualStyleBackColor = true;
            ButtonStop.Click += ButtonStop_Click;
            // 
            // LabelCheck
            // 
            LabelCheck.AutoSize = true;
            LabelCheck.LinkColor = Color.Black;
            LabelCheck.Location = new Point(537, 60);
            LabelCheck.Name = "LabelCheck";
            LabelCheck.Size = new Size(40, 15);
            LabelCheck.TabIndex = 12;
            LabelCheck.TabStop = true;
            LabelCheck.Text = "Check";
            LabelCheck.LinkClicked += LabelCheck_LinkClicked;
            // 
            // progressBarFtp
            // 
            progressBarFtp.CustomText = "";
            progressBarFtp.Location = new Point(181, 57);
            progressBarFtp.Maximum = 1000;
            progressBarFtp.Name = "progressBarFtp";
            progressBarFtp.ProgressColor = Color.LightGreen;
            progressBarFtp.Size = new Size(350, 23);
            progressBarFtp.Step = 1;
            progressBarFtp.Style = ProgressBarStyle.Continuous;
            progressBarFtp.TabIndex = 13;
            progressBarFtp.TextColor = Color.Black;
            progressBarFtp.TextFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            progressBarFtp.Visible = false;
            // 
            // linkLabelSettings
            // 
            linkLabelSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            linkLabelSettings.AutoEllipsis = true;
            linkLabelSettings.AutoSize = true;
            linkLabelSettings.Enabled = false;
            linkLabelSettings.Location = new Point(473, 146);
            linkLabelSettings.Margin = new Padding(0);
            linkLabelSettings.Name = "linkLabelSettings";
            linkLabelSettings.Size = new Size(90, 15);
            linkLabelSettings.TabIndex = 14;
            linkLabelSettings.TabStop = true;
            linkLabelSettings.Text = "Настройки .bat";
            linkLabelSettings.LinkClicked += linkLabelSettings_LinkClicked;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { resetSettingAplicationToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(197, 26);
            // 
            // resetSettingAplicationToolStripMenuItem
            // 
            resetSettingAplicationToolStripMenuItem.Name = "resetSettingAplicationToolStripMenuItem";
            resetSettingAplicationToolStripMenuItem.Size = new Size(196, 22);
            resetSettingAplicationToolStripMenuItem.Text = "Reset setting aplication";
            resetSettingAplicationToolStripMenuItem.Click += resetSettingAplicationToolStripMenuItem_Click;
            // 
            // fileInfoDateTime
            // 
            fileInfoDateTime.AutoSize = true;
            fileInfoDateTime.Location = new Point(20, 143);
            fileInfoDateTime.Name = "fileInfoDateTime";
            fileInfoDateTime.Size = new Size(95, 30);
            fileInfoDateTime.TabIndex = 15;
            fileInfoDateTime.Text = "Дата изменения\r\nфайлов на FTP:\r\n";
            fileInfoDateTime.Visible = false;
            // 
            // buttonUpload
            // 
            buttonUpload.Location = new Point(331, 146);
            buttonUpload.Name = "buttonUpload";
            buttonUpload.Size = new Size(28, 23);
            buttonUpload.TabIndex = 16;
            buttonUpload.Text = "↺";
            toolTip.SetToolTip(buttonUpload, "Загрузить файлы обратно на FTP.");
            buttonUpload.UseVisualStyleBackColor = true;
            buttonUpload.Click += buttonUpload_Click;
            // 
            // toolTip
            // 
            toolTip.AutoPopDelay = 15000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 100;
            // 
            // cleanFilesCheckBox
            // 
            cleanFilesCheckBox.AutoSize = true;
            cleanFilesCheckBox.Location = new Point(331, 175);
            cleanFilesCheckBox.Name = "cleanFilesCheckBox";
            cleanFilesCheckBox.Size = new Size(117, 19);
            cleanFilesCheckBox.TabIndex = 17;
            cleanFilesCheckBox.Text = "Очистка файлов";
            toolTip.SetToolTip(cleanFilesCheckBox, "Очистка файлов и папок перед загрузкой или выгрзукой файлов в целевой папке.");
            cleanFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // DedicatedFTPServerValheim
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(585, 197);
            ContextMenuStrip = contextMenuStrip1;
            Controls.Add(cleanFilesCheckBox);
            Controls.Add(buttonUpload);
            Controls.Add(fileInfoDateTime);
            Controls.Add(linkLabelSettings);
            Controls.Add(LabelCheck);
            Controls.Add(ButtonStop);
            Controls.Add(ButtonStart);
            Controls.Add(buttonPathTemp);
            Controls.Add(buttonPathBat);
            Controls.Add(pathTemp);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(pathBat);
            Controls.Add(label1);
            Controls.Add(progressBarFtp);
            Controls.Add(pathFTP);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Location = new Point(20, 20);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DedicatedFTPServerValheim";
            StartPosition = FormStartPosition.Manual;
            Text = "DedicatedFTPServerValheim";
            LocationChanged += DedicatedFTPServerValheim_LocationChanged;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox pathBat;
        private Label label2;
        private Label label3;
        private TextBox pathFTP;
        private TextBox pathTemp;
        private Button buttonPathBat;
        private Button buttonPathTemp;
        private Button ButtonStart;
        private Button ButtonStop;
        private LinkLabel LabelCheck;
        private CustomControls.CustomProgressBar progressBarFtp;
        private LinkLabel linkLabelSettings;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem resetSettingAplicationToolStripMenuItem;
        private Label fileInfoDateTime;
        private Button buttonUpload;
        private ToolTip toolTip;
        private CheckBox cleanFilesCheckBox;
    }
}