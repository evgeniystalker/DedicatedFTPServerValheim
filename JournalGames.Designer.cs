namespace DedicatedFTPServerValheim
{
    partial class JournalGames
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JournalGames));
            dataGridView1 = new DataGridView();
            serverStatusBindingSource = new BindingSource(components);
            buttonSave = new Button();
            buttonClose = new Button();
            dateTimeStartServerDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            worldDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            statusCodeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            ipDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            nameServerDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            nameUserDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)serverStatusBindingSource).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = SystemColors.GradientInactiveCaption;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { dateTimeStartServerDataGridViewTextBoxColumn, worldDataGridViewTextBoxColumn, statusCodeDataGridViewTextBoxColumn, ipDataGridViewTextBoxColumn, nameServerDataGridViewTextBoxColumn, nameUserDataGridViewTextBoxColumn });
            dataGridView1.DataSource = serverStatusBindingSource;
            dataGridView1.Location = new Point(12, 12);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 30;
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(906, 374);
            dataGridView1.TabIndex = 0;
            // 
            // serverStatusBindingSource
            // 
            serverStatusBindingSource.DataSource = typeof(ServerStatus);
            // 
            // buttonSave
            // 
            buttonSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSave.DialogResult = DialogResult.OK;
            buttonSave.Location = new Point(843, 392);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 1;
            buttonSave.Text = "Сохранить";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.DialogResult = DialogResult.Cancel;
            buttonClose.Location = new Point(762, 392);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(75, 23);
            buttonClose.TabIndex = 2;
            buttonClose.Text = "Закрыть";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            // 
            // dateTimeStartServerDataGridViewTextBoxColumn
            // 
            dateTimeStartServerDataGridViewTextBoxColumn.DataPropertyName = "DateTimeStartServer";
            dateTimeStartServerDataGridViewTextBoxColumn.HeaderText = "DateTimeStartServer";
            dateTimeStartServerDataGridViewTextBoxColumn.Name = "dateTimeStartServerDataGridViewTextBoxColumn";
            dateTimeStartServerDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // worldDataGridViewTextBoxColumn
            // 
            worldDataGridViewTextBoxColumn.DataPropertyName = "World";
            worldDataGridViewTextBoxColumn.HeaderText = "World";
            worldDataGridViewTextBoxColumn.Name = "worldDataGridViewTextBoxColumn";
            worldDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // statusCodeDataGridViewTextBoxColumn
            // 
            statusCodeDataGridViewTextBoxColumn.DataPropertyName = "StatusCode";
            statusCodeDataGridViewTextBoxColumn.HeaderText = "StatusCode";
            statusCodeDataGridViewTextBoxColumn.Name = "statusCodeDataGridViewTextBoxColumn";
            statusCodeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // ipDataGridViewTextBoxColumn
            // 
            ipDataGridViewTextBoxColumn.DataPropertyName = "Ip";
            ipDataGridViewTextBoxColumn.HeaderText = "Ip";
            ipDataGridViewTextBoxColumn.Name = "ipDataGridViewTextBoxColumn";
            ipDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameServerDataGridViewTextBoxColumn
            // 
            nameServerDataGridViewTextBoxColumn.DataPropertyName = "NameServer";
            nameServerDataGridViewTextBoxColumn.HeaderText = "NameServer";
            nameServerDataGridViewTextBoxColumn.Name = "nameServerDataGridViewTextBoxColumn";
            nameServerDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // nameUserDataGridViewTextBoxColumn
            // 
            nameUserDataGridViewTextBoxColumn.DataPropertyName = "NameUser";
            nameUserDataGridViewTextBoxColumn.HeaderText = "NameUser";
            nameUserDataGridViewTextBoxColumn.Name = "nameUserDataGridViewTextBoxColumn";
            nameUserDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // JournalGames
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(930, 427);
            Controls.Add(buttonClose);
            Controls.Add(buttonSave);
            Controls.Add(dataGridView1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "JournalGames";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Journal";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)serverStatusBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button buttonSave;
        private Button buttonClose;
        private DataGridView dataGridView1;
        public BindingSource serverStatusBindingSource;
        private DataGridViewTextBoxColumn dateTimeStartServerDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn worldDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusCodeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn ipDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nameServerDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nameUserDataGridViewTextBoxColumn;
    }
}