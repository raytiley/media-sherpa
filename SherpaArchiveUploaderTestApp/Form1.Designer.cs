namespace SherpaArchiveUploaderTestApp
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
            this.uxUploadBtn = new System.Windows.Forms.Button();
            this.uxFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.uxUploadProgress = new System.Windows.Forms.ProgressBar();
            this.uxStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // uxUploadBtn
            // 
            this.uxUploadBtn.Location = new System.Drawing.Point(101, 66);
            this.uxUploadBtn.Name = "uxUploadBtn";
            this.uxUploadBtn.Size = new System.Drawing.Size(75, 23);
            this.uxUploadBtn.TabIndex = 0;
            this.uxUploadBtn.Text = "Choose File";
            this.uxUploadBtn.UseVisualStyleBackColor = true;
            this.uxUploadBtn.Click += new System.EventHandler(this.uxUploadBtn_Click);
            // 
            // uxFileDialog
            // 
            this.uxFileDialog.FileName = "openFileDialog1";
            // 
            // uxUploadProgress
            // 
            this.uxUploadProgress.Location = new System.Drawing.Point(12, 123);
            this.uxUploadProgress.Name = "uxUploadProgress";
            this.uxUploadProgress.Size = new System.Drawing.Size(268, 23);
            this.uxUploadProgress.TabIndex = 1;
            // 
            // uxStatus
            // 
            this.uxStatus.AutoSize = true;
            this.uxStatus.Location = new System.Drawing.Point(121, 149);
            this.uxStatus.Name = "uxStatus";
            this.uxStatus.Size = new System.Drawing.Size(0, 13);
            this.uxStatus.TabIndex = 2;
            this.uxStatus.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.uxStatus);
            this.Controls.Add(this.uxUploadProgress);
            this.Controls.Add(this.uxUploadBtn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button uxUploadBtn;
        private System.Windows.Forms.OpenFileDialog uxFileDialog;
        private System.Windows.Forms.ProgressBar uxUploadProgress;
        private System.Windows.Forms.Label uxStatus;
    }
}

