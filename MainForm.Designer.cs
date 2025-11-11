namespace DocumentMoveApp
{
    partial class MainForm
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
            txtConnectionString = new System.Windows.Forms.TextBox();
            lblConnectionString = new System.Windows.Forms.Label();
            btnTestConnection = new System.Windows.Forms.Button();
            lblLibrary = new System.Windows.Forms.Label();
            cmbLibrary = new System.Windows.Forms.ComboBox();
            lblImportProfile = new System.Windows.Forms.Label();
            cmbImportProfile = new System.Windows.Forms.ComboBox();
            btnProcess = new System.Windows.Forms.Button();
            progressBar = new System.Windows.Forms.ProgressBar();
            lblStatus = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // txtConnectionString
            // 
            txtConnectionString.Location = new System.Drawing.Point(20, 54);
            txtConnectionString.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtConnectionString.Multiline = true;
            txtConnectionString.Name = "txtConnectionString";
            txtConnectionString.Size = new System.Drawing.Size(825, 90);
            txtConnectionString.TabIndex = 0;
            txtConnectionString.Text = "Server=(local);Database=Enadoc12688FF548A0000;Trusted_Connection=True;";
            // 
            // lblConnectionString
            // 
            lblConnectionString.AutoSize = true;
            lblConnectionString.Location = new System.Drawing.Point(20, 23);
            lblConnectionString.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblConnectionString.Name = "lblConnectionString";
            lblConnectionString.Size = new System.Drawing.Size(130, 20);
            lblConnectionString.TabIndex = 1;
            lblConnectionString.Text = "Connection String:";
            // 
            // btnTestConnection
            // 
            btnTestConnection.Location = new System.Drawing.Point(20, 155);
            btnTestConnection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new System.Drawing.Size(160, 46);
            btnTestConnection.TabIndex = 2;
            btnTestConnection.Text = "Test Connection";
            btnTestConnection.UseVisualStyleBackColor = true;
            btnTestConnection.Click += btnTestConnection_Click;
            // 
            // lblLibrary
            // 
            lblLibrary.AutoSize = true;
            lblLibrary.Location = new System.Drawing.Point(20, 38);
            lblLibrary.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLibrary.Name = "lblLibrary";
            lblLibrary.Size = new System.Drawing.Size(57, 20);
            lblLibrary.TabIndex = 3;
            lblLibrary.Text = "Library:";
            // 
            // cmbLibrary
            // 
            cmbLibrary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLibrary.FormattingEnabled = true;
            cmbLibrary.Location = new System.Drawing.Point(20, 69);
            cmbLibrary.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmbLibrary.Name = "cmbLibrary";
            cmbLibrary.Size = new System.Drawing.Size(399, 28);
            cmbLibrary.TabIndex = 4;
            cmbLibrary.SelectedIndexChanged += cmbLibrary_SelectedIndexChanged;
            // 
            // lblImportProfile
            // 
            lblImportProfile.AutoSize = true;
            lblImportProfile.Location = new System.Drawing.Point(447, 38);
            lblImportProfile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblImportProfile.Name = "lblImportProfile";
            lblImportProfile.Size = new System.Drawing.Size(104, 20);
            lblImportProfile.TabIndex = 5;
            lblImportProfile.Text = "Import Profile:";
            // 
            // cmbImportProfile
            // 
            cmbImportProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbImportProfile.FormattingEnabled = true;
            cmbImportProfile.Location = new System.Drawing.Point(447, 69);
            cmbImportProfile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            cmbImportProfile.Name = "cmbImportProfile";
            cmbImportProfile.Size = new System.Drawing.Size(399, 28);
            cmbImportProfile.TabIndex = 6;
            // 
            // btnProcess
            // 
            btnProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            btnProcess.Location = new System.Drawing.Point(20, 123);
            btnProcess.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new System.Drawing.Size(827, 62);
            btnProcess.TabIndex = 8;
            btnProcess.Text = "Process Documents";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += btnProcess_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new System.Drawing.Point(20, 38);
            progressBar.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(827, 35);
            progressBar.TabIndex = 9;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new System.Drawing.Point(20, 85);
            lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(50, 20);
            lblStatus.TabIndex = 10;
            lblStatus.Text = "Ready";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblConnectionString);
            groupBox1.Controls.Add(txtConnectionString);
            groupBox1.Controls.Add(btnTestConnection);
            groupBox1.Location = new System.Drawing.Point(16, 18);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox1.Size = new System.Drawing.Size(880, 231);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "Database Connection";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblLibrary);
            groupBox2.Controls.Add(cmbLibrary);
            groupBox2.Controls.Add(lblImportProfile);
            groupBox2.Controls.Add(cmbImportProfile);
            groupBox2.Controls.Add(btnProcess);
            groupBox2.Location = new System.Drawing.Point(16, 258);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox2.Size = new System.Drawing.Size(880, 215);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            groupBox2.Text = "Document Processing";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(progressBar);
            groupBox3.Controls.Add(lblStatus);
            groupBox3.Location = new System.Drawing.Point(16, 483);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBox3.Size = new System.Drawing.Size(880, 131);
            groupBox3.TabIndex = 13;
            groupBox3.TabStop = false;
            groupBox3.Text = "Status";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(912, 632);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Document Move - Folder Hierarchy";
            Load += MainForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label lblConnectionString;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Label lblLibrary;
        private System.Windows.Forms.ComboBox cmbLibrary;
        private System.Windows.Forms.Label lblImportProfile;
        private System.Windows.Forms.ComboBox cmbImportProfile;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}
