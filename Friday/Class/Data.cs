using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Friday.Class
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
            public static string GetWeekString (string weekIntStr)
            {
                if (!int.TryParse(weekIntStr, out int week)) return "";

                if (week == 0 || week>6 )
                {
                    return "日";
                }
                else
                {
                    return NumberToChinese(((int)week).ToString());
                }
            }
            public static string GetMonthString (int month)
            {
                if (month < 1 || month > 12) return "";
                string[] monthString = { "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月" };
                return monthString[month - 1];
            }
        }
        public class Urls
        {
            public static string main = "http://120.55.151.61/";
            public class Social
            {
                public static string findToolbars = main + "Others/Update/V3/findToolbars.action";
                public static string getCampusNews = main + "Treehole/V4/Message/getCampusNewsV2.action";
                public static string getNewsList = main + "Treehole/V4/News/getList.action";
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
                public static string getCourseTableBySuperId = main + "V2/Course/getCourseTableBySuperId.action";
                public static string createNewTerm = main + "V2/StudentExt/createNewSyllabus.action";
                public static string switchTerm = main + "V2/StudentExt/switchSyllabus.action";
                public static string delTerm = main + "V2/StudentExt/deleteSyllabus.action";
                public static string getSyllabusTheme = main + "V3/Theme/getSyllabusTheme.action";
                public class Async
                {
                    public static string deleteCourse = main + "V2/Course/deleteCourse.action";
                    public static string addCourse = main + "V2/Course/addCourse3.action";
                    public static string getCourseTableFromServer = main + "V2/Course/getCourseTableFromServer.action";
                }
            }
            public class user
            {
                public static string login = main + "V2/StudentSkip/loginCheckV4.action";
                public static string myobs = main + "Treehole/V4/Message/getMe.action";
                public static string mycollect = main + "Treehole/V4/Collect/getMe.action";
                public static string getTopInfoById = main + "V2/Student/getTopInfoById.action";
                public static string showMovement = main + "Treehole/V4/Message/showMovement.action";
                public static string getResetPasswordCaptcha = main + "V2/SMS/getResetPasswordCaptcha.action";
                public static string checkResetPasswordCaptcha = main + "V2/SMS/checkResetPasswordCaptcha.action";
                public static string resetPasswordByMobileV4 = main + "V2/StudentSkip/resetPasswordByMobileV4.action";
                public static string editNecessaryInfo = main + "V2/Student/editNecessaryInfoV2.action";
                public class Register
                {
                    public static string getUpdateSchoolList = main + "V2/School/getUpdateSchoolList.action";
                    public static string getRecommendSchoolList = main + "V2/School/getRecommendSchoolList.action";
                    public static string findAcademysBySchoolId = main + "V2/Academy/findAcademysBySchoolIdV2.action";
                    public static string getRegisterCaptcha = main + "V2/SMS/getRegisterCaptchaV2.action";
                    public static string register = main + "V2/StudentSkip/registerV5.action";
                }
            }
        }
    }
}
