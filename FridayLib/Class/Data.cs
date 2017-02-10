using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace FridayBgTask.Class
{
    public class Data
    {
        public class Json
        {
            public static string ToJsonData(object item)
            {
                DataContractJsonSerializer serialize = new DataContractJsonSerializer(item.GetType());
                string result = String.Empty;
                using (MemoryStream ms = new MemoryStream())
                {
                    serialize.WriteObject(ms, item);
                    ms.Position = 0;
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        result = reader.ReadToEnd();
                        reader.Dispose();
                    }
                }
                return result;
            }

            public static T DataContractJsonDeSerialize<T>(string json)
            {
                try
                {
                    var ds = new DataContractJsonSerializer(typeof(T));
                    var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                    T obj = (T)ds.ReadObject(ms);
                    ms.Dispose();
                    return obj;
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
        }
        public class Int_String
        {
            public static string NumberToChinese(string numberStr)
            {
                string numStr = "0123456789";
                string chineseStr = "零一二三四五六七八九";
                char[] c = numberStr.ToCharArray();
                for (int i = 0; i < c.Length; i++)
                {
                    int index = numStr.IndexOf(c[i]);
                    if (index != -1)
                        c[i] = chineseStr.ToCharArray()[index];
                }
                numStr = null;
                chineseStr = null;
                return new string(c);
            }
            public static string GetWeekString(DayOfWeek week)
            {
                if (week == (DayOfWeek)0)
                {
                    return "日";
                }
                else
                {
                    return NumberToChinese(((int)week).ToString());
                }
            }
        }
        public class Urls
        {
            public static string main = "http://120.55.151.61/";
            public class Social
            {
                public static string findToolbars = main + "Others/Update/V3/findToolbars.action";
                public static string getCampusNews = main + "Treehole/V4/Message/getCampusNewsV2.action";
                public static string Cave = main + "Treehole/V4/Cave/getList.action";
                public static string Flea = main + "Treehole/V4/Flea/getList.action";
            }
            public class Playground
            {
                public static string index = main + "Treehole/V4/Topic/getListV2.action";
                public static string GetAllTopicList = main + "Treehole/V4/Topic/getAllList.action";
                public static string FollowTopic = main + "Treehole/V4/Topic/follow.action";
                public static string UnFollowTopic = main + "Treehole/V4/Topic/cancelFollow.action";
                public static string getMessageByTopicId = main + "V2/Treehole/Message/getMessageByTopicIdV4.action";
                public static string getMessageDetail = main + "Treehole/V4/Message/getDetail.action";
                public static string sendcomment = main + "Treehole/V4/Comment/comment.action";
                public static string sendlike = main + "Treehole/V4/Like/likeNum.action";
                public static string getmorecomment = main + "Treehole/V4/Comment/showDialogue.action";
            }
            public class Course
            {
                public static string setNowWeek = main + "V2/Student/setNowWeek.action";
                public static string setMaxCount = main + "V2/StudentExt/setMaxCount.action";
                public static string getCourseListBySchoolTime = main + "V2/Course/getCourseListBySchoolTime.action";
                public static string getPopCourseGroups = main + "V2/Course/getPopCourseGroups.action";
                public static string getStudentsByCourseId = main + "V2/Student/getStudentsByCourseId.action";
                public static string getCourseListByPreciseName = main + "V2/Course/getCourseListByPreciseName2.action";
            }
            public class user
            {
                public static string login = main + "V2/StudentSkip/loginCheckV4.action";
            }
        }
    }
}
