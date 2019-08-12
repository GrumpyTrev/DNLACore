namespace DLNACore
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
            this.CmdSSDP = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.CmdPlay = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CmdSSDP
            // 
            this.CmdSSDP.Location = new System.Drawing.Point(12, 3);
            this.CmdSSDP.Name = "CmdSSDP";
            this.CmdSSDP.Size = new System.Drawing.Size(75, 23);
            this.CmdSSDP.TabIndex = 0;
            this.CmdSSDP.Text = "SSDP";
            this.CmdSSDP.UseVisualStyleBackColor = true;
            this.CmdSSDP.Click += new System.EventHandler(this.CmdSSDP_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 32);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(429, 206);
            this.textBox1.TabIndex = 1;
            // 
            // CmdPlay
            // 
            this.CmdPlay.Enabled = false;
            this.CmdPlay.Location = new System.Drawing.Point(106, 3);
            this.CmdPlay.Name = "CmdPlay";
            this.CmdPlay.Size = new System.Drawing.Size(75, 23);
            this.CmdPlay.TabIndex = 2;
            this.CmdPlay.Text = "Play Music";
            this.CmdPlay.UseVisualStyleBackColor = true;
            this.CmdPlay.Click += new System.EventHandler(this.CmdPlay_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 261);
            this.Controls.Add(this.CmdPlay);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.CmdSSDP);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CmdSSDP;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button CmdPlay;
    }
}

