using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Friday.Class
{
    public class HttpPostUntil
    {
        public static List<KeyValuePair<string, string>> GetBasicPostData()
        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            var data = new List<KeyValuePair<string, string>>();
            if (deviceInfo.SystemManufacturer != null)
            {
                data.Add(new KeyValuePair<string, string>("phoneBrand", deviceInfo.SystemManufacturer));
            }
            else
            {
                data.Add(new KeyValuePair<string, string>("phoneBrand", "DeskTop"));
            }
            data.Add(new KeyValuePair<string, string>("platform", "1"));
            data.Add(new KeyValuePair<string, string>("phoneVersion", "19"));
            data.Add(new KeyValuePair<string, string>("channel", "SoftwareUpdate"));
            if (deviceInfo.FriendlyName != null)
            {
                data.Add(new KeyValuePair<string, string>("phoneModel", deviceInfo.FriendlyName));
            }
            else
            {
                data.Add(new KeyValuePair<string, string>("phoneModel", "UnKnown"));
            }
            data.Add(new KeyValuePair<string, string>("versionNumber", "7.8.4"));
            return data;
        }
        public static bool isInternetAvailable
        {
            get
            {
                ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (InternetConnectionProfile == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public static string NowTime
        {
            get
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var time = Convert.ToInt64(ts.TotalMilliseconds).ToString();
                return time;
            }
        }
        public static async Task<string> HttpPost(string url, HttpFormUrlEncodedContent postdata=null,bool hascookie = true)
        {
            try
            {
                var httpclient = new HttpClient();
                if (postdata == null) postdata = new HttpFormUrlEncodedContent(GetBasicPostData());
                if (hascookie)
                {
                    var localSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
                    if (localSetting.Values.ContainsKey("usercookie"))
                    {
                        httpclient.DefaultRequestHeaders.Cookie.Add(new HttpCookiePairHeaderValue("JSESSIONID") { Value= (string)localSetting.Values["usercookie"] });
                    }
                    else
                    {
                        httpclient.DefaultRequestHeaders.Cookie.Add(new HttpCookiePairHeaderValue("JSESSIONID") { Value = "C689EDE891F83354DD5D895D0E564057-memcached1" });
                    }
                    if (localSetting.Values.ContainsKey("userserverid"))
                    {
                        httpclient.DefaultRequestHeaders.Cookie.Add(new HttpCookiePairHeaderValue("SERVERID") { Value = (string)localSetting.Values["userserverid"] });
                    }
                    else
                    {
                        httpclient.DefaultRequestHeaders.Cookie.Add(new HttpCookiePairHeaderValue("SERVERID") { Value = "b3cfa96ac8aa99e3eb8b1680a93c531c|1486744340|1486744288" });
                    }
                }
                var result = await httpclient.PostAsync(new Uri(url), postdata);
                var json = await result.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
