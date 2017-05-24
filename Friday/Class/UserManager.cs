using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;

namespace Friday.Class
{
    public class UserManager
    {
        public static bool islogin
        {
            get
            {
                var localSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
                return localSetting.Values.ContainsKey("userdata");
            }
        }
        public static Model.User.Login_Result UserData
        {
            get
            {
                var localSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
                return Class.Data.Json.DataContractJsonDeSerialize<Class.Model.User.Login_Result>((string)localSetting.Values["userdata"]);
            }
        }
        public static async Task<Model.User.Login_Result> Login(string account,string password,bool relogin = false)
        {
            var auto_login = false;

            try
            {
                var localData = Windows.Storage.ApplicationData.Current.LocalSettings;
                if(account == "" && localData.Values.ContainsKey("prev_account") && !relogin)
                {
                    auto_login = true;

                    account = (string)localData.Values["prev_account"];
                    
                    if (localData.Values.ContainsKey("pw_" + account) && account!="" && account!=null)
                    {
                        
                        password = (string)localData.Values["pw_" + account];
                        account = (string)localData.Values["ac_" + account];
                    }
                    else
                    {
                        return UserManager.UserData;
                    }
                }
                else
                if (localData.Values.ContainsKey("ac_" + account)&&!relogin)
                {
                    password = (string)localData.Values["pw_" + account];
                    account = (string)localData.Values["ac_" + account];
                }else
                {
                    var service = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    localData.Values["prev_account"] = account;
                    localData.Values["ac_" + account] = await service.EncryptDataAsync(account);
                    localData.Values["pw_" + account] = await service.EncryptDataAsync(password);
                    account = await service.EncryptDataAsync(account);
                    password = await service.EncryptDataAsync(password);
                    await service.CloseAsync();
                }
                var postdata = HttpPostUntil.GetBasicPostData();
                postdata.Add(new KeyValuePair<string, string>("account", account));
                postdata.Add(new KeyValuePair<string, string>("password", password));
                var httpclient = new HttpClient();
                var result = await httpclient.PostAsync(new Uri(Data.Urls.user.login), new HttpFormUrlEncodedContent(postdata));
                var json = await result.Content.ReadAsStringAsync();
                json = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["student"].GetObject().ToString();
                var userdata = Data.Json.DataContractJsonDeSerialize<Model.User.Login_Result>(json);
                if (userdata != null&& json.Contains("studentId"))
                {
                    localData.Values["userdata"] = json;
                    var usercookie= result.Headers["Set-Cookie"];
                    var cookies = usercookie.Split(';');
                    foreach (var item in cookies)
                    {
                        if(item.Contains("JSESSIONID="))
                        {
                            localData.Values["usercookie"] = item.Split('=')[1];
                        }
                        if (item.Contains("SERVERID="))
                        {
                            localData.Values["userserverid"] = item.Split('=')[1];
                        }
                    }
                    try
                    {
                        await Class.Model.CourseManager.Async.GetCourseTableFromServer();

                        //将setTime转为本地时间
                        var setTimeVar = (new DateTime(1970, 1, 1, 0, 0, 0) + TimeSpan.FromMilliseconds(userdata.attachmentBO.nowWeekMsg.setTime)).ToLocalTime();

                        var dayOfWeek =(int)setTimeVar.DayOfWeek;
                        if (dayOfWeek == 0) dayOfWeek = 7;
                        var delta = (DateTime.Now - setTimeVar).TotalDays;


                        if (delta > 0)
                        {
                            userdata.attachmentBO.nowWeekMsg.nowWeek = userdata.attachmentBO.nowWeekMsg.nowWeek + int.Parse(Math.Floor(delta+dayOfWeek).ToString()) / 7;
                        }

                    }
                    catch (Exception)
                    {
                        
                    }
                    return userdata;
                }
                else
                {

                    return null;
                }
            }
            catch (Exception)
            {
                if (auto_login)
                {
                    if (UserData != null) return UserData;
                }
                return null;
            }

        }

        public static void Unlogin()
        {
            var localData = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                localData.Values.Remove("userdata");
                localData.Values.Remove("usercookie");
                localData.Values.Remove("userserverid");
                localData.Values.Remove("prev_account");
            }
            catch (Exception)
            {
                
            }
        }

        public static async Task<string> GetResetPasswordCaptcha(string m)
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    FridayCloudService.ServiceClient sc = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    var mm = await sc.EncryptDataByKeyAsync(m, m);
                    await sc.CloseAsync();
                    postdata.Add(new KeyValuePair<string, string>("m", m));
                    postdata.Add(new KeyValuePair<string, string>("mm", mm));
                    postdata.Add(new KeyValuePair<string, string>("areaCode", "null"));
                    postdata.Add(new KeyValuePair<string, string>("retry", "0"));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.getResetPasswordCaptcha, new HttpFormUrlEncodedContent(postdata), false);
                    var obj = JsonObject.Parse(json).GetNamedObject("data");
                    if (obj.GetNamedNumber("statusInt")!=1)
                    {
                        return obj.GetNamedString("errorStr");
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return "网络异常";
                }
            }
            else
            {
                return "网络异常";
            }
        }

        public static async Task<string> CheckResetPasswordCaptcha(string m,string captcha)
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("m", m));
                    postdata.Add(new KeyValuePair<string, string>("captcha", captcha));
                    postdata.Add(new KeyValuePair<string, string>("areaCode", "null"));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.checkResetPasswordCaptcha, new HttpFormUrlEncodedContent(postdata), false);
                    var obj = JsonObject.Parse(json).GetNamedObject("data");
                    if (obj.GetNamedNumber("statusInt") != 1)
                    {
                        return obj.GetNamedString("errorStr");
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return "网络异常";
                }
            }
            else
            {
                return "网络异常";
            }
        }

        public static async Task<string> ResetPassword(string m, string captcha, string pwd)
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    FridayCloudService.ServiceClient sc = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    var newPassword = await sc.EncryptDataAsync(pwd);
                    await sc.CloseAsync();
                    postdata.Add(new KeyValuePair<string, string>("mobileNumber", m));
                    postdata.Add(new KeyValuePair<string, string>("newPassword", newPassword));
                    postdata.Add(new KeyValuePair<string, string>("areaCode", "null"));
                    postdata.Add(new KeyValuePair<string, string>("captcha", captcha));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.resetPasswordByMobileV4, new HttpFormUrlEncodedContent(postdata),false);
                    var obj = JsonObject.Parse(json).GetNamedObject("data");
                    if (obj.GetNamedNumber("statusInt") != 1)
                    {
                        return obj.GetNamedString("errorStr");
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return "网络异常";
                }
            }
            else
            {
                return "网络异常";
            }
        }

        public static async Task<string> getRegisterCaptcha(string m, string pwd)
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    FridayCloudService.ServiceClient sc = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    var p = await sc.EncryptDataAsync(pwd);
                    var mm = await sc.EncryptDataByKeyAsync(m,m);
                    await sc.CloseAsync();
                    postdata.Add(new KeyValuePair<string, string>("m", m));
                    postdata.Add(new KeyValuePair<string, string>("p", p));
                    postdata.Add(new KeyValuePair<string, string>("areaCode", "null"));
                    postdata.Add(new KeyValuePair<string, string>("mm", mm));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.Register.getRegisterCaptcha, new HttpFormUrlEncodedContent(postdata), false);
                    var obj = JsonObject.Parse(json).GetNamedObject("data");
                    if (obj.GetNamedNumber("statusInt") != 1)
                    {
                        return obj.GetNamedString("errorStr");
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return "网络异常";
                }
            }
            else
            {
                return "网络异常";
            }
        }

        public static async Task<string> register(string user, string pwd,string captcha, string academyId,string grade,string schoolId)
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    FridayCloudService.ServiceClient sc = new FridayCloudService.ServiceClient(FridayCloudService.ServiceClient.EndpointConfiguration.BasicHttpBinding_IService);
                    var p = await sc.EncryptDataAsync(pwd);
                    var mm = await sc.EncryptDataByKeyAsync(user,user);
                    await sc.CloseAsync();
                    postdata.Add(new KeyValuePair<string, string>("account", mm));
                    postdata.Add(new KeyValuePair<string, string>("password", p));
                    postdata.Add(new KeyValuePair<string, string>("areaCode", "null"));
                    postdata.Add(new KeyValuePair<string, string>("identityStatus", "0"));
                    postdata.Add(new KeyValuePair<string, string>("deviceCode", HttpPostUntil.NowTime));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.Register.register, new HttpFormUrlEncodedContent(postdata), false);
                    var obj = JsonObject.Parse(json).GetNamedObject("data");
                    if (obj.GetNamedNumber("statusInt") != 1)
                    {
                        return obj.GetNamedString("errorStr");
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return "网络异常";
                }
            }
            else
            {
                return "网络异常";
            }
        }

        public static async Task<string> editNecessaryInfo(string nickName,string profession="",string organization = "")
        {
            if (HttpPostUntil.isInternetAvailable)
            {
                try
                {
                    var postdata = HttpPostUntil.GetBasicPostData();
                    postdata.Add(new KeyValuePair<string, string>("nickName", nickName));
                    postdata.Add(new KeyValuePair<string, string>("profession", profession));
                    postdata.Add(new KeyValuePair<string, string>("organization", organization));
                    var json = await HttpPostUntil.HttpPost(Data.Urls.user.editNecessaryInfo, new HttpFormUrlEncodedContent(postdata));
                    if (json.Contains("studentId"))
                    {
                        return "Success";
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
