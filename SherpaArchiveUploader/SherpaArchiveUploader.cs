using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Sherpa.Uploaders
{
    public class UploadCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public Exception Error { get; set; }
    }

    public class ArchiveUploadFailedExeption : Exception
    {
        public HttpWebResponse Response { get; private set; }

        public ArchiveUploadFailedExeption(string message, HttpWebResponse response) : base(message)
        {
            this.Response = response;
        }
    }

    public class SherpaArchiveUploader
    {
        public event ProgressChangedEventHandler UploadProgressChanged;
        public event EventHandler<UploadCompletedEventArgs> UploadComplete;

        public string FilePath { get; set; }
        public string IdentityToken { get; set; }
        public string ArchiveAccessKey { get; set; }
        public string ArchiveSecret { get; set; }
        public string ArchiveBucket { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string LicenseURL { get; set; }

        public void StartUploadAsync()
        {
            var worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = false; // Probably change this later
            worker.WorkerReportsProgress = true;

            worker.DoWork += new DoWorkEventHandler(doUpload);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            worker.RunWorkerAsync();
        }

        public void StartUploadSync()
        {
            doUpload(null, null);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var args = new UploadCompletedEventArgs();
            if (e.Error == null)
            {
                args.Success = true;
            }
            else
            {
                args.Success = false;
                args.Error = e.Error;
            }

            OnUploadComplete(args);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnProgressChanged(e);
        }

        void doUpload(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker;
            var percent = 0;

            #region Archive Sample
            /****
             * 
             * Movie item (Will get video player on details page):
        curl --location --header 'x-amz-auto-make-bucket:1' \
         --header 'x-archive-meta01-collection:opensource_movies' \
         --header 'x-archive-meta-mediatype:movies' \
         --header 'x-archive-meta-title:Ben plays piano.' \
         --header "authorization: LOW $accesskey:$secret" \
         --upload-file ben-2009-05-09.avi \
         http://s3.us.archive.org/ben-plays-piano/ben-plays-piano.avi
              */
            #endregion
            
            // item name
            var destinationURL = this.ItemUploadURL;
            var fileInfo = new FileInfo(FilePath);

            var request = (HttpWebRequest)HttpWebRequest.Create(destinationURL);

            request.Headers.Add("x-amz-auto-make-bucket:1");
            request.Headers.Add(String.Format("authorization: LOW {0}:{1}", ArchiveAccessKey, ArchiveSecret));
            var collectionNumber = 1;
            foreach (var bucket in ArchiveBucket.Split(','))
            {
                request.Headers.Add(String.Format("x-archive-meta{0}-collection:{1}", collectionNumber.ToString("00"), bucket));
                collectionNumber++;
            }
            request.Headers.Add("x-archive-meta-mediatype:movies");
            request.Headers.Add(String.Format("x-archive-meta-title:{0}", Title.Replace("_", "--")));

            #region Optional Metadata
            if (String.IsNullOrWhiteSpace(Description) == false)
                request.Headers.Add(String.Format("x-archive-meta-description:{0}", Description.Replace("-", "--")));
            
            if(String.IsNullOrWhiteSpace(LicenseURL) == false)
                request.Headers.Add(String.Format("x-archive-meta-licenseurl:{0}", LicenseURL.Replace("_", "--")));
            #endregion

            request.Method = "PUT";
            request.ContentLength = fileInfo.Length;
            request.KeepAlive = false;
            request.Timeout = System.Threading.Timeout.Infinite;
            request.AllowWriteStreamBuffering = false;

            // The buffer size is set to 256 KB
            int buffLength = 524288;
            byte[] buff = new byte[buffLength];
            int contentLen;

            var fs = fileInfo.OpenRead();
            var strm = request.GetRequestStream();
            long totalBytesUploaded = 0;

            try
            {
                contentLen = fs.Read(buff, 0, buffLength);

                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the reqest stream
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                    totalBytesUploaded += contentLen;
                    var newPercent = (int)(((float)totalBytesUploaded / (float)fileInfo.Length) * 100);

                    if (percent != newPercent)
                    {
                        percent = newPercent;
                        Sherpa.LogHelper.LogHelper.Logger.Debug(String.Format("{0}% of {1} uploaded.", percent, FilePath));
                    }
                    // Only report progress if we have a background woker
                    // We won't have a background worker if this is being called syncronously
                    if (worker != null)
                        worker.ReportProgress(percent);
                }
            }
            finally
            {
                LogHelper.LogHelper.Logger.Debug(String.Format("Closing streams for file: {0}", FilePath));
                // Close the file stream and the Request Stream
                strm.Close();
                fs.Close();
            }
            // Check response and throw error if upload was not sucessful.
            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {   
                var message = String.Format("Upload for file {0} failed. Sever responded with status code: {1}",
                                            FilePath,
                                            response.StatusCode.ToString());

                throw new ArchiveUploadFailedExeption(message, response);
            }
        }

        private string ItemUploadURL
        {
            get
            {

                var fileName = String.Format("{0}{1}", ItemName, Path.GetExtension(FilePath));

                return String.Format("http://s3.us.archive.org/{0}/{1}",
                    ItemName,
                    fileName);
            }
        }

        public string ItemURL
        {
            get
            {
                return String.Format("http://archive.org/details/{0}",
                    ItemName);
            }
        }

        private string ItemName
        {
            get
            {
                Regex rgx = new Regex(@"[^a-zA-Z0-9-]");
                var itemName = Path.GetFileNameWithoutExtension(FilePath);
                itemName = itemName.Replace(' ', '-');
                itemName = rgx.Replace(itemName, String.Empty).ToLower();
                itemName = String.Format("{0}-{1}", IdentityToken, itemName);

                return itemName;
            }
        }

        private void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (UploadProgressChanged != null)
            {
                UploadProgressChanged(this, e);
            }
        }

        private void OnUploadComplete(UploadCompletedEventArgs e)
        {
            if(UploadComplete != null)
            {
                UploadComplete(this, e);
            }
        }

    }
}
