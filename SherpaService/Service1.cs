using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Sherpa.Classes;
using System.Configuration;
using Sherpa.Uploaders;
using SherpaService.CablecastWS;

namespace SherpaService
{
    public partial class Service1 : ServiceBase
    {

        bool m_run = true;
        Thread m_ProcessingThread;
        CCManager m_CCManager;
        string m_CCServer;
        string m_CCUsername;
        string m_CCPassword;
        int m_CCLocationID;
        string m_ArchiveKey;
        string m_ArchiveSecret;
        string m_ArchiveCollection;
        string m_ArchivePrefix;
        string m_ArchiveLicenseURL;
        string m_ContentPaths;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartService();
        }

        protected override void OnStop()
        {
            LogHelper.Logger.Info("Sherpa Media Service Stopping");
            m_run = false;
        }

        public void StartService()
        {
            var settingsFailed = false;
            try
            {
                m_CCServer = ConfigurationManager.AppSettings["CablecastServerURL"].ToString();
                m_CCUsername = ConfigurationManager.AppSettings["CablecastUsername"].ToString();
                m_CCPassword = ConfigurationManager.AppSettings["CablecastPassword"].ToString();
                m_CCLocationID = Int32.Parse(ConfigurationManager.AppSettings["CablecastLocationID"].ToString());
                m_ArchiveKey = ConfigurationManager.AppSettings["ArchiveKey"].ToString();
                m_ArchiveSecret = ConfigurationManager.AppSettings["ArchiveSecret"].ToString();
                m_ArchiveCollection = ConfigurationManager.AppSettings["ArchiveCollection"].ToString();
                m_ArchivePrefix = ConfigurationManager.AppSettings["ArchivePrefix"].ToString();
                m_ArchiveLicenseURL = ConfigurationManager.AppSettings["ArchiveLicenseURL"].ToString();
                m_ContentPaths = ConfigurationManager.AppSettings["ContentPath"].ToString();
            }
            catch(Exception ex)
            {
                settingsFailed = true;
                LogHelper.Logger.Error("Error loading settings from app config", ex);
            }
            
            if(settingsFailed == false)
            {
                m_CCServer = String.Format("http://{0}/CablecastWS/CablecastWS.asmx?WSDL", m_CCServer);
                m_CCManager = new CCManager(m_CCServer, m_CCUsername, m_CCPassword, m_CCLocationID);
                m_ProcessingThread = new Thread(RunLoop);
                m_ProcessingThread.IsBackground = true;
                m_ProcessingThread.Start();
            }
        }

        private void RunLoop()
        {
            while (m_run)
            {
                System.Threading.Thread.Sleep(1000 * 60 * 5); //Sleep five minute

                try
                {
                    ShowInfo showToUpload = null;
                    string pathToFile = String.Empty;

                    //This is specific for archive. Eventually pull it out into the Archive Class
                    var interestedFields = new Dictionary<string, List<string>>();
                    var yesValues = new List<string>(new string[] { "yes", "true", "x" });
                    interestedFields.Add("Archive-Upload", yesValues);
                    var shows = m_CCManager.GetUploads(interestedFields, "Archive-URL");

                    if (shows.Count == 0)
                        continue;

                    LogHelper.Logger.Debug(String.Format("Found {0} shows from cablecast that need uploading. ", shows.Count));

                    //TODO get from config
                    var paths = new List<string>();
                    foreach (var path in m_ContentPaths.Split('|'))
                    {
                        paths.Add(path);
                    }


                    //Try Uploading the first show with a file.
                    foreach (var show in shows)
                    {
                        pathToFile = CCFileFinder.FindFile(show.ShowID, paths);
                        if (String.IsNullOrWhiteSpace(pathToFile) == false)
                        {
                            showToUpload = show;
                            break;
                        }
                    }

                    if (showToUpload != null)
                    {
                        LogHelper.Logger.Debug(String.Format("Beginning Upload of {0}", pathToFile));
                        var uploader = new SherpaArchiveUploader()
                            {
                                FilePath = pathToFile,
                                Title = showToUpload.Title,
                                Description = showToUpload.Comments,
                                LicenseURL = m_ArchiveLicenseURL,
                                ArchiveBucket = m_ArchiveCollection,
                                IdentityToken = m_ArchivePrefix,
                                ArchiveAccessKey = m_ArchiveKey,
                                ArchiveSecret = m_ArchiveSecret,
                            };
                        try
                        {
                            uploader.StartUploadSync();
                            //This is where we should set showrecord.
                            LogHelper.Logger.Debug(String.Format("Saving to Cablecast: {0}", uploader.ItemURL));
                            var fields = new Dictionary<string, string>();
                            fields.Add("Archive-URL", uploader.ItemURL);
                            m_CCManager.UpdateShow(showToUpload.ShowID, fields);

                        }
                        catch (Exception ex)
                        {
                            LogHelper.Logger.Error(String.Format("Problem Uploading {0}", pathToFile), ex);
                        }
                    }
                    else
                    {
                        LogHelper.Logger.Debug("Could not find any files on disk for uploading");
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.Logger.Error("General exception in run loop", ex);
                }
            }
        }
    }
}
