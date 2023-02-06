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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DedicatedFTPServerValheim));
            this.label1 = new System.Windows.Forms.Label();
            this.pathBat = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pathFTP = new System.Windows.Forms.TextBox();
            this.pathTemp = new System.Windows.Forms.TextBox();
            this.buttonPathBat = new System.Windows.Forms.Button();
            this.buttonPathTemp = new System.Windows.Forms.Button();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.LabelCheck = new System.Windows.Forms.LinkLabel();
            this.progressBarFtp = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "Путь к *.bat файлу сервера";
            // 
            // pathBat
            // 
            this.pathBat.Location = new System.Drawing.Point(181, 17);
            this.pathBat.Name = "pathBat";
            this.pathBat.Size = new System.Drawing.Size(350, 23);
            this.pathBat.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Путь к FTP серверу";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Путь к временной папке";
            // 
            // pathFTP
            // 
            this.pathFTP.Location = new System.Drawing.Point(181, 57);
            this.pathFTP.Name = "pathFTP";
            this.pathFTP.Size = new System.Drawing.Size(350, 23);
            this.pathFTP.TabIndex = 6;
            this.pathFTP.TextChanged += new System.EventHandler(this.pathFTP_TextChanged);
            // 
            // pathTemp
            // 
            this.pathTemp.Location = new System.Drawing.Point(181, 97);
            this.pathTemp.Name = "pathTemp";
            this.pathTemp.Size = new System.Drawing.Size(350, 23);
            this.pathTemp.TabIndex = 7;
            // 
            // buttonPathBat
            // 
            this.buttonPathBat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPathBat.Location = new System.Drawing.Point(537, 17);
            this.buttonPathBat.Name = "buttonPathBat";
            this.buttonPathBat.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.buttonPathBat.Size = new System.Drawing.Size(26, 25);
            this.buttonPathBat.TabIndex = 2;
            this.buttonPathBat.Text = "...";
            this.buttonPathBat.UseVisualStyleBackColor = true;
            this.buttonPathBat.Click += new System.EventHandler(this.buttonPathBat_Click);
            // 
            // buttonPathTemp
            // 
            this.buttonPathTemp.Location = new System.Drawing.Point(537, 97);
            this.buttonPathTemp.Name = "buttonPathTemp";
            this.buttonPathTemp.Size = new System.Drawing.Size(25, 25);
            this.buttonPathTemp.TabIndex = 4;
            this.buttonPathTemp.Text = "...";
            this.buttonPathTemp.UseVisualStyleBackColor = true;
            this.buttonPathTemp.Click += new System.EventHandler(this.buttonPathTemp_Click);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(150, 146);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 0;
            this.ButtonStart.Text = "Start";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ButtonStop
            // 
            this.ButtonStop.Location = new System.Drawing.Point(365, 146);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(75, 23);
            this.ButtonStop.TabIndex = 1;
            this.ButtonStop.Text = "Stop";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(420, 172);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "Василий понятное дело ...";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // LabelCheck
            // 
            this.LabelCheck.AutoSize = true;
            this.LabelCheck.LinkColor = System.Drawing.Color.Black;
            this.LabelCheck.Location = new System.Drawing.Point(537, 60);
            this.LabelCheck.Name = "LabelCheck";
            this.LabelCheck.Size = new System.Drawing.Size(40, 15);
            this.LabelCheck.TabIndex = 12;
            this.LabelCheck.TabStop = true;
            this.LabelCheck.Text = "Check";
            this.LabelCheck.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LabelCheck_LinkClicked);
            // 
            // progressBarFtp
            // 
            this.progressBarFtp.Location = new System.Drawing.Point(181, 57);
            this.progressBarFtp.Name = "progressBarFtp";
            this.progressBarFtp.Size = new System.Drawing.Size(350, 23);
            this.progressBarFtp.Step = 1;
            this.progressBarFtp.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarFtp.TabIndex = 13;
            this.progressBarFtp.Visible = false;
            // 
            // DedicatedFTPServerValheim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 197);
            this.Controls.Add(this.LabelCheck);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ButtonStop);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.buttonPathTemp);
            this.Controls.Add(this.buttonPathBat);
            this.Controls.Add(this.pathTemp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pathBat);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBarFtp);
            this.Controls.Add(this.pathFTP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DedicatedFTPServerValheim";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DedicatedFTPServerValheim";
            this.LocationChanged += new System.EventHandler(this.DedicatedFTPServerValheim_LocationChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private Label label4;
        private LinkLabel LabelCheck;
        private ProgressBar progressBarFtp;
    }
}