namespace Rhythm_Player_Midi
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMidiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.trackSelect = new System.Windows.Forms.ComboBox();
            this.tempoBox = new System.Windows.Forms.TextBox();
            this.eventBox = new System.Windows.Forms.TextBox();
            this.difficultyBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.saveFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(858, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMidiToolStripMenuItem,
            this.saveFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openMidiToolStripMenuItem
            // 
            this.openMidiToolStripMenuItem.Name = "openMidiToolStripMenuItem";
            this.openMidiToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openMidiToolStripMenuItem.Text = "Open Midi...";
            this.openMidiToolStripMenuItem.Click += new System.EventHandler(this.openMidiToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "mid";
            this.openFileDialog1.Filter = "MIDI Files|*.mid";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Song Tempo:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(183, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Track:";
            // 
            // trackSelect
            // 
            this.trackSelect.Enabled = false;
            this.trackSelect.FormattingEnabled = true;
            this.trackSelect.Location = new System.Drawing.Point(227, 23);
            this.trackSelect.Name = "trackSelect";
            this.trackSelect.Size = new System.Drawing.Size(121, 21);
            this.trackSelect.TabIndex = 3;
            this.trackSelect.SelectedIndexChanged += new System.EventHandler(this.trackSelect_SelectedIndexChanged);
            // 
            // tempoBox
            // 
            this.tempoBox.Location = new System.Drawing.Point(79, 24);
            this.tempoBox.Name = "tempoBox";
            this.tempoBox.ReadOnly = true;
            this.tempoBox.Size = new System.Drawing.Size(98, 20);
            this.tempoBox.TabIndex = 4;
            // 
            // eventBox
            // 
            this.eventBox.Location = new System.Drawing.Point(12, 54);
            this.eventBox.Multiline = true;
            this.eventBox.Name = "eventBox";
            this.eventBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.eventBox.Size = new System.Drawing.Size(834, 489);
            this.eventBox.TabIndex = 5;
            // 
            // difficultyBox
            // 
            this.difficultyBox.FormattingEnabled = true;
            this.difficultyBox.Location = new System.Drawing.Point(465, 22);
            this.difficultyBox.Name = "difficultyBox";
            this.difficultyBox.Size = new System.Drawing.Size(121, 21);
            this.difficultyBox.TabIndex = 7;
            this.difficultyBox.SelectedIndexChanged += new System.EventHandler(this.difficultyBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(380, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Difficulty/Type:";
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveFileToolStripMenuItem.Text = "Save File";
            this.saveFileToolStripMenuItem.Click += new System.EventHandler(this.saveFileToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(602, 23);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(141, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Copy to Clipboard";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 555);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.difficultyBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.eventBox);
            this.Controls.Add(this.tempoBox);
            this.Controls.Add(this.trackSelect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Rhythm Reader Midi";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMidiToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox trackSelect;
        private System.Windows.Forms.TextBox tempoBox;
        private System.Windows.Forms.TextBox eventBox;
        private System.Windows.Forms.ComboBox difficultyBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripMenuItem saveFileToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

