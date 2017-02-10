using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace FridayBgTask.Class
{
    public class UserManager
    {
        public static Model.User.Login_Result UserData
        {
            get
            {
                var localSetting = Windows.Storage.ApplicationData.Current.LocalSettings;
                return Class.Data.Json.DataContractJsonDeSerialize<Class.Model.User.Login_Result>((string)localSetting.Values["userdata"]);
            }
        }
    }
}
