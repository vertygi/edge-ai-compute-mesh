namespace EdgeMesh.Core
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox listBoxAgents;
        private System.Windows.Forms.TextBox textBoxCommand;
        private System.Windows.Forms.TextBox textBoxResponses;
        private System.Windows.Forms.Button buttonCompileAgent;
        private System.Windows.Forms.Button buttonUpdateAgent;
        private System.Windows.Forms.Button buttonSelfInstall;
        private System.Windows.Forms.Button buttonSelfUninstall;
        // private System.Windows.Forms.DataGridView dataGridViewAgents;
        private System.Windows.Forms.GroupBox groupBoxCompiler;
        private System.Windows.Forms.TextBox textBoxAgentName;
        private System.Windows.Forms.ComboBox comboBoxAgentType;
        private System.Windows.Forms.Button buttonBuildAgent;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.GroupBox groupBoxCommands;
        private System.Windows.Forms.TextBox textBoxCommandInput;
        private System.Windows.Forms.Button buttonSendCommand;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.listBoxAgents = new System.Windows.Forms.ListBox();
            this.textBoxCommand = new System.Windows.Forms.TextBox();
            this.buttonSendCommand = new System.Windows.Forms.Button();
            this.textBoxResponses = new System.Windows.Forms.TextBox();
            this.buttonCompileAgent = new System.Windows.Forms.Button();
            this.buttonUpdateAgent = new System.Windows.Forms.Button();
            this.buttonSelfInstall = new System.Windows.Forms.Button();
            this.buttonSelfUninstall = new System.Windows.Forms.Button();
            // this.dataGridViewAgents = new System.Windows.Forms.DataGridView();
            this.groupBoxCompiler = new System.Windows.Forms.GroupBox();
            this.textBoxAgentName = new System.Windows.Forms.TextBox();
            this.comboBoxAgentType = new System.Windows.Forms.ComboBox();
            this.buttonBuildAgent = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.groupBoxCommands = new System.Windows.Forms.GroupBox();
            this.textBoxCommandInput = new System.Windows.Forms.TextBox();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            // ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAgents)).BeginInit();
            this.groupBoxCompiler.SuspendLayout();
            this.groupBoxCommands.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 2;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowCount = 3;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(800, 500);
            this.tableLayoutPanelMain.TabIndex = 100;
            // 
            // listBoxAgents
            // 
            this.listBoxAgents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Controls.Add(this.listBoxAgents, 0, 0);
            this.tableLayoutPanelMain.SetRowSpan(this.listBoxAgents, 3);
            // 
            // groupBoxCompiler
            // 
            this.groupBoxCompiler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Controls.Add(this.groupBoxCompiler, 1, 0);
            // 
            // groupBoxCommands
            // 
            this.groupBoxCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Controls.Add(this.groupBoxCommands, 1, 1);
            // 
            // textBoxResponses
            // 
            this.textBoxResponses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Controls.Add(this.textBoxResponses, 1, 2);
            // 
            // groupBoxCompiler (внутри)
            // 
            this.groupBoxCompiler.Controls.Clear();
            this.groupBoxCompiler.Controls.Add(this.textBoxAgentName);
            this.groupBoxCompiler.Controls.Add(this.comboBoxAgentType);
            this.groupBoxCompiler.Controls.Add(this.buttonBuildAgent);
            this.groupBoxCompiler.Controls.Add(this.textBoxLog);
            this.groupBoxCompiler.Controls.Add(this.buttonCompileAgent);
            this.groupBoxCompiler.Controls.Add(this.buttonUpdateAgent);
            this.groupBoxCompiler.Controls.Add(this.buttonSelfInstall);
            this.groupBoxCompiler.Controls.Add(this.buttonSelfUninstall);
            // 
            // Расположение элементов внутри groupBoxCompiler
            // 
            this.textBoxAgentName.Location = new System.Drawing.Point(10, 20);
            this.textBoxAgentName.Size = new System.Drawing.Size(100, 20);
            this.textBoxAgentName.TabIndex = 0;
            this.textBoxAgentName.Text = "AgentName";
            // 
            // comboBoxAgentType
            // 
            this.comboBoxAgentType.FormattingEnabled = true;
            this.comboBoxAgentType.Location = new System.Drawing.Point(120, 20);
            this.comboBoxAgentType.Size = new System.Drawing.Size(120, 21);
            this.comboBoxAgentType.TabIndex = 1;
            this.comboBoxAgentType.Text = "Select Agent Type";
            // 
            // buttonBuildAgent
            // 
            this.buttonBuildAgent.Location = new System.Drawing.Point(10, 50);
            this.buttonBuildAgent.Size = new System.Drawing.Size(90, 23);
            this.buttonBuildAgent.TabIndex = 2;
            this.buttonBuildAgent.Text = "Build Agent";
            this.buttonBuildAgent.UseVisualStyleBackColor = true;
            this.buttonBuildAgent.Click += new System.EventHandler(this.buttonBuildAgent_Click);
            // 
            // buttonCompileAgent
            // 
            this.buttonCompileAgent.Location = new System.Drawing.Point(110, 50);
            this.buttonCompileAgent.Size = new System.Drawing.Size(90, 23);
            this.buttonCompileAgent.TabIndex = 4;
            this.buttonCompileAgent.Text = "Compile Agent";
            this.buttonCompileAgent.UseVisualStyleBackColor = true;
            this.buttonCompileAgent.Click += new System.EventHandler(this.buttonCompileAgent_Click);
            // 
            // buttonUpdateAgent
            // 
            this.buttonUpdateAgent.Location = new System.Drawing.Point(210, 50);
            this.buttonUpdateAgent.Size = new System.Drawing.Size(90, 23);
            this.buttonUpdateAgent.TabIndex = 5;
            this.buttonUpdateAgent.Text = "Update Agent";
            this.buttonUpdateAgent.UseVisualStyleBackColor = true;
            this.buttonUpdateAgent.Click += new System.EventHandler(this.buttonUpdateAgent_Click);
            // 
            // buttonSelfInstall
            // 
            this.buttonSelfInstall.Location = new System.Drawing.Point(10, 80);
            this.buttonSelfInstall.Size = new System.Drawing.Size(90, 23);
            this.buttonSelfInstall.TabIndex = 6;
            this.buttonSelfInstall.Text = "Self Install";
            this.buttonSelfInstall.UseVisualStyleBackColor = true;
            this.buttonSelfInstall.Click += new System.EventHandler(this.buttonSelfInstall_Click);
            // 
            // buttonSelfUninstall
            // 
            this.buttonSelfUninstall.Location = new System.Drawing.Point(110, 80);
            this.buttonSelfUninstall.Size = new System.Drawing.Size(90, 23);
            this.buttonSelfUninstall.TabIndex = 7;
            this.buttonSelfUninstall.Text = "Self Uninstall";
            this.buttonSelfUninstall.UseVisualStyleBackColor = true;
            this.buttonSelfUninstall.Click += new System.EventHandler(this.buttonSelfUninstall_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Location = new System.Drawing.Point(10, 110);
            this.textBoxLog.Size = new System.Drawing.Size(290, 60);
            this.textBoxLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            // 
            // groupBoxCommands (внутри)
            // 
            this.groupBoxCommands.Controls.Clear();
            this.groupBoxCommands.Controls.Add(this.textBoxCommand);
            this.groupBoxCommands.Controls.Add(this.buttonSendCommand);
            this.textBoxCommand.Location = new System.Drawing.Point(10, 20);
            this.textBoxCommand.Size = new System.Drawing.Size(300, 20);
            this.buttonSendCommand.Location = new System.Drawing.Point(320, 18);
            this.buttonSendCommand.Size = new System.Drawing.Size(120, 23);
            // 
            // textBoxResponses
            // 
            this.textBoxResponses.Multiline = true;
            this.textBoxResponses.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "MainForm";
            this.Text = "Secure Remote Control System";
            // ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAgents)).EndInit();
            this.groupBoxCompiler.ResumeLayout(false);
            this.groupBoxCompiler.PerformLayout();
            this.groupBoxCommands.ResumeLayout(false);
            this.groupBoxCommands.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}