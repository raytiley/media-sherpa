using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using SherpaService.CablecastWS;

namespace Sherpa.Classes
{
    public class CCManager
    {
        CablecastWS m_Server;
        int m_LocationID = 22;
        string m_Username = String.Empty;
        string m_Password = String.Empty;

        public CCManager(string serverURL, string username, string password, int locationID)
        {
            m_Server = new CablecastWS();
            m_Server.Url = serverURL;
            m_LocationID = locationID;
            m_Username = username;
            m_Password = password;
        }

        //TODO make this method more general
        public ICollection<ShowInfo> GetUploads(Dictionary<string, List<string>> interestedFieldsAndValues, string excludeIfPopulated)
        {
            var results = new List<ShowInfo>();
            var shows = m_Server.LastModifiedSearch(m_LocationID, DateTime.Now.AddHours(-240), ">");

            foreach (var show in shows)
            {
                foreach (var field in interestedFieldsAndValues)
                {
                    //We want to skip shows that are already populated with this field
                    var exludedField = show.CustomFields.FirstOrDefault(cf => cf.Name == excludeIfPopulated);
                    if (exludedField != null && !String.IsNullOrWhiteSpace(exludedField.Value))
                        continue;

                    //If we haven't already excluded it find out if we want to upload it.
                    var customField = show.CustomFields.FirstOrDefault(cf => cf.Name == field.Key);

                    if (customField != null && !String.IsNullOrWhiteSpace(customField.Value) && field.Value.Contains(customField.Value.ToLower()))
                        results.Add(show);
                }
            }

            return results;
        }

        public void UpdateShow(int showID, Dictionary<string, string> customFieldValues)
        {
            //First download show so we update the latest
            var show = m_Server.GetShowInformation(showID);
            var statusField = new CustomField();
            var customFields = new List<CustomField>();
            foreach (var field in customFieldValues)
            {
                var customField = new CustomField();
                customField.Name = field.Key;
                customField.Value = field.Value;

                customFields.Add(customField);
            }
            //Now update show
            m_Server.UpdateShowRecord(show.ShowID,
                show.LocalID,
                show.InternalTitle,
                show.Title,
                show.ProjectID,
                show.CGExempt,
                show.ProducerID,
                show.CategoryID,
                show.EventDate,
                show.Comments,
                customFields.ToArray(),
                show.OnDemand,
                show.OnDemandStatus,
                show.BugText,
                show.CrawlText,
                show.CrawlLengthInSeconds,
                m_Username,
                m_Password);
        }
    }
}

