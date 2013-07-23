using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sherpa.Uploaders;

namespace SherpaArchiveUploaderTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void uxUploadBtn_Click(object sender, EventArgs e)
        {
            if (uxFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                uxUploadBtn.Enabled = false;
                uxStatus.Text = "Uploading";
                var uploader = new SherpaArchiveUploader()
                {
                    FilePath = uxFileDialog.FileName,
                    ArchiveBucket = "test_collection",
                    Title = "Archive Sherpa Test Upload",
                    ArchiveAccessKey = "Cm5Fwwwj68iaTVBF",
                    ArchiveSecret = "mSQqurxXNkrXX3Ux",
                    IdentityToken = "ray-tiley"
                };

                uploader.UploadProgressChanged += uploader_UploadProgressChanged;
                uploader.UploadComplete += uploader_UploadComplete;

                uploader.StartUploadAsync();
                
            }
        }

        void uploader_UploadComplete(object sender, UploadCompletedEventArgs e)
        {
            uxUploadProgress.Value = 0;
            uxUploadBtn.Enabled = true;
            if (e.Success == true)
            {
                uxStatus.Text = "";
            }
            else
            {
                if (e.Error != null)
                {
                    uxStatus.Text = String.Format("Upload failed: {0}", e.Error.Message);
                }
            }
        }

        void uploader_UploadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            uxUploadProgress.Value = e.ProgressPercentage;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
