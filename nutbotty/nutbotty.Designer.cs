namespace nutbotty
{
    partial class NutBotty
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
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.chatTab = new System.Windows.Forms.TabPage();
            this.sendMessageTextBox = new System.Windows.Forms.TextBox();
            this.chatTextBox = new System.Windows.Forms.RichTextBox();
            this.consoleTab = new System.Windows.Forms.TabPage();
            this.consoleTextBox = new System.Windows.Forms.RichTextBox();
            this.mainTabControl.SuspendLayout();
            this.chatTab.SuspendLayout();
            this.consoleTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabControl
            // 
            this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabControl.Controls.Add(this.chatTab);
            this.mainTabControl.Controls.Add(this.consoleTab);
            this.mainTabControl.Location = new System.Drawing.Point(12, 12);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(576, 505);
            this.mainTabControl.TabIndex = 0;
            // 
            // chatTab
            // 
            this.chatTab.Controls.Add(this.sendMessageTextBox);
            this.chatTab.Controls.Add(this.chatTextBox);
            this.chatTab.Location = new System.Drawing.Point(4, 22);
            this.chatTab.Name = "chatTab";
            this.chatTab.Padding = new System.Windows.Forms.Padding(3);
            this.chatTab.Size = new System.Drawing.Size(568, 479);
            this.chatTab.TabIndex = 0;
            this.chatTab.Text = "Chat";
            this.chatTab.UseVisualStyleBackColor = true;
            // 
            // sendMessageTextBox
            // 
            this.sendMessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sendMessageTextBox.Location = new System.Drawing.Point(7, 453);
            this.sendMessageTextBox.Name = "sendMessageTextBox";
            this.sendMessageTextBox.Size = new System.Drawing.Size(555, 20);
            this.sendMessageTextBox.TabIndex = 1;
            // 
            // chatTextBox
            // 
            this.chatTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chatTextBox.Location = new System.Drawing.Point(7, 7);
            this.chatTextBox.Name = "chatTextBox";
            this.chatTextBox.ReadOnly = true;
            this.chatTextBox.Size = new System.Drawing.Size(555, 440);
            this.chatTextBox.TabIndex = 0;
            this.chatTextBox.Text = "";
            this.chatTextBox.TextChanged += new System.EventHandler(this.chatTextBox_TextChanged);
            // 
            // consoleTab
            // 
            this.consoleTab.Controls.Add(this.consoleTextBox);
            this.consoleTab.Location = new System.Drawing.Point(4, 22);
            this.consoleTab.Name = "consoleTab";
            this.consoleTab.Padding = new System.Windows.Forms.Padding(3);
            this.consoleTab.Size = new System.Drawing.Size(754, 171);
            this.consoleTab.TabIndex = 1;
            this.consoleTab.Text = "Console";
            this.consoleTab.UseVisualStyleBackColor = true;
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.consoleTextBox.Location = new System.Drawing.Point(7, 7);
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.ReadOnly = true;
            this.consoleTextBox.Size = new System.Drawing.Size(741, 162);
            this.consoleTextBox.TabIndex = 0;
            this.consoleTextBox.Text = "";
            this.consoleTextBox.TextChanged += new System.EventHandler(this.consoleTextBox_TextChanged);
            // 
            // NutBotty
            // 
            this.ClientSize = new System.Drawing.Size(600, 529);
            this.Controls.Add(this.mainTabControl);
            this.Name = "NutBotty";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NutBotty_FormClosing);
            this.mainTabControl.ResumeLayout(false);
            this.chatTab.ResumeLayout(false);
            this.chatTab.PerformLayout();
            this.consoleTab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.TabPage chatTab;
        private System.Windows.Forms.TextBox sendMessageTextBox;
        private System.Windows.Forms.RichTextBox chatTextBox;
        private System.Windows.Forms.TabPage consoleTab;
        private System.Windows.Forms.RichTextBox consoleTextBox;
    }
}

