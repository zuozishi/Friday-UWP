using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FridayBgTask.Class
{
    public class Model
    {
        public class CourseManager
        {
            public class CourseModel:INotifyPropertyChanged
            {
                public bool autoEntry { get; set; }
                public int beginYear { get; set; }
                public string classroom { get; set; }
                public long courseId { get; set; }
                public int courseMark { get; set; }
                public int courseType { get; set; }
                public int day { get; set; }
                public int endYear { get; set; }
                public string id { get; set; }
                public string name { get; set; }
                public int nonsupportAmount { get; set; }
                public string period { get; set; }
                public int provinceId { get; set; }
                public string schoolId { get; set; }
                public long schooltime { get; set; }
                public int sectionEnd { get; set; }
                public int sectionStart { get; set; }
                public string smartPeriod { get; set; }
                public int studentCount { get; set; }
                public int supportAmount { get; set; }
                public string teacher { get; set; }
                public int term { get; set; }
                public int verifyStatus { get; set; }
                public string time
                {
                    get
                    {
                        string result = "";
                        if (period!="")
                        {
                            string[] weeks;
                            if (smartPeriod != null) weeks = smartPeriod.Split(' '); else weeks = period.Split(' ', ',');

                            var firstWeekValid = int.TryParse(weeks[0], out int firstWeek);
                            var lastWeekValid = int.TryParse(weeks[weeks.Count() - 1], out int lastWeek);

                            if (firstWeekValid && lastWeekValid && (lastWeek - firstWeek == weeks.Count() - 1))
                            {
                                result = weeks[0] + "-" + weeks[weeks.Count() - 1] + "周";
                            }
                            else
                            {
                                result = period + "周";
                            }
                            result = result + " 周" + Data.Int_String.NumberToChinese(day.ToString());
                            result = result + " ";
                            if (sectionStart == sectionEnd)
                            {
                                result = result + "第" + sectionStart + "节";
                            }
                            else
                            {
                                result = result + sectionStart + "-" + sectionEnd + "节";
                            }
                        }
                        return result;
                    }
                }
                public bool isadd { get; set; }
                public string btntext
                {
                    get
                    {
                        if (isadd)
                        {
                            return "从课表移除";
                        }
                        else
                        {
                            return "添加到课表";
                        }
                    }
                }
                public string btncolor
                {
                    get
                    {
                        if (isadd)
                        {
                            return "#FFC7C7C7";
                        }
                        else
                        {
                            return "#FF65E271";
                        }
                    }
                }
                public Windows.UI.Xaml.Controls.TextBlock BtnContent {
                    get
                    {
                        var textblock = new Windows.UI.Xaml.Controls.TextBlock();
                        textblock.Text = name + "@" + classroom;
                        textblock.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(CourseButton.ForegroundColor);
                        textblock.TextWrapping = Windows.UI.Xaml.TextWrapping.WrapWholeWords;
                        textblock.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                        textblock.Margin = new Windows.UI.Xaml.Thickness(5);
                        textblock.FontSize = 12;
                        textblock.DataContext = id;
                        return textblock;
                    }
                }
                public event PropertyChangedEventHandler PropertyChanged;
                public void RaisePropertyChanged(string name)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
                public CourseBtnStyle CourseButton { get; set; }
                public class CourseBtnStyle
                {
                    public Windows.UI.Color ForegroundColor { get; set; }
                    public Windows.UI.Color BackgroundColor { get; set; }
                    public CourseBtnStyle()
                    {
                        var ColorList = new List<Windows.UI.Color>();
                        ColorList.Add(Windows.UI.Color.FromArgb(255,238,88,88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 231, 238, 88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 163, 238, 88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 108, 238, 88));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 108));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 170));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 101));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 88, 238, 0));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 226, 23, 60));
                        ColorList.Add(Windows.UI.Color.FromArgb(255, 79, 168, 147));
                        int num = new Random().Next(0, ColorList.Count);
                        ForegroundColor = Windows.UI.Colors.White;
                        BackgroundColor = ColorList[num];
                    }
                }
                public CourseModel()
                {
                    CourseButton = new CourseBtnStyle();
                }
            }
            public class StudentModel
            {
                public string academyName { get; set; }
                public string fullAvatarUrl { get; set; }
                public int grade { get; set; }
                public int gender { get; set; }
                public string nickName { get; set; }
                public string signature { get; set; }
                public string studentId { get; set; }
                public string sex
                {
                    get
                    {
                        if (gender == 1)
                        {
                            return "ms-appx:///Assets/images/ic_th_sex_boy.png";
                        }
                        else
                        {
                            return "ms-appx:///Assets/images/ic_th_sex_girl.png";
                        }
                    }
                }
                public string hassignature
                {
                    get
                    {
                        if (signature== null||signature=="")
                        {
                            return "Collapsed";
                        }
                        else
                        {
                            return "Visible";
                        }
                    }
                }
            }
            public static async Task<bool> Add(CourseModel course,string yaer=null,string term=null)
            {
                var courselist =await GetCourse(yaer, term);
                if (CanAdd(course, courselist))
                {
                    courselist.Add(course);
                    await SaveCourse(courselist, yaer, term);
                    return true;
                }
                else
                {
                    //Tools.ShowMsgAtFrame("有课程冲突");
                    return false;
                }
            }
            public static async Task Remove(CourseModel course, string yaer = null, string term = null)
            {
                var courselist = await GetCourse(yaer, term);
                for (int i = 0; i < courselist.Count; i++)
                {
                    if (courselist[i].id == course.id)
                    {
                        courselist.RemoveAt(i);
                        await SaveCourse(courselist, yaer, term);
                    }
                }
            }
            public static async Task Remove(string id, string yaer = null, string term = null)
            {
                var courselist = await GetCourse(yaer, term);
                for (int i = 0; i < courselist.Count; i++)
                {
                    if (courselist[i].id == id)
                    {
                        courselist.RemoveAt(i);
                        await SaveCourse(courselist, yaer, term);
                    }
                }
            }
            public static bool CanAdd(CourseModel Course, ObservableCollection<CourseModel> CourseList)
            {
                var CourseDir = new Dictionary<int, List<int>>();
                bool can = true;
                for (int i = 1; i < 8; i++)
                {
                    CourseDir.Add(i, new List<int>());
                }
                foreach (var item in CourseList)
                {
                    if (item.sectionStart == item.sectionEnd)
                    {
                        CourseDir[item.day].Add(item.sectionStart);
                    }else
                    {
                        for (int i = 0; i < item.sectionEnd- item.sectionStart; i++)
                        {
                            CourseDir[item.day].Add(item.sectionStart+i);
                        }
                    }
                }
                if (Course.sectionStart == Course.sectionEnd)
                {
                    return !CourseDir[Course.day].Contains(Course.sectionStart);
                }
                else
                {
                    for (int i = 0; i < Course.sectionEnd - Course.sectionStart; i++)
                    {
                        if (CourseDir[Course.day].Contains(Course.sectionStart + i))
                        {
                            can = false;
                        }
                    }
                    return can;
                }

            }
            public static async Task<ObservableCollection<CourseModel>> GetCourse(string yaer = null, string term = null)
            {
                string filename = "course_";
                if (yaer == null)
                {
                    filename = filename + MD5.GetMd5String(UserManager.UserData.beginYear.ToString() + UserManager.UserData.term.ToString());
                }
                else
                {
                    filename = filename + MD5.GetMd5String(yaer + term);
                }
                var CourseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Course",CreationCollisionOption.OpenIfExists);
                var CourseFile = await CourseFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                var json = await FileIO.ReadTextAsync(CourseFile);
                ObservableCollection<CourseModel> CourseList;
                if (json == "")
                {
                    CourseList = null;
                }
                else
                {
                    CourseList = Data.Json.DataContractJsonDeSerialize<ObservableCollection<CourseModel>>(json);
                }
                if (CourseList == null)
                {
                    CourseList = new ObservableCollection<CourseModel>();
                }
                return CourseList;
            }
            public static async Task<CourseModel> GetCourse(int day, int section, string yaer = null, string term = null, string week = null)
            {
                string filename = "course_";
                if (yaer == null)
                {
                    filename = filename + MD5.GetMd5String(UserManager.UserData.beginYear.ToString() + UserManager.UserData.term.ToString());
                }
                else
                {
                    filename = filename + MD5.GetMd5String(yaer + term);
                }


                var CourseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Course", CreationCollisionOption.OpenIfExists);
                var CourseFile = await CourseFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                var json = await FileIO.ReadTextAsync(CourseFile);

                ObservableCollection<CourseModel> CourseList;
                if (json == "")
                {
                    CourseList = null;
                }
                else
                {
                    CourseList = Data.Json.DataContractJsonDeSerialize<ObservableCollection<CourseModel>>(json);
                }
                if (CourseList == null)
                {
                    return null;
                }
                else
                {
                    CourseModel result = null;

                    var resultList = new List<CourseModel>();
                    foreach (var item in CourseList)
                    {
                        if (item.day == day && (item.sectionStart <= section && section <= item.sectionEnd))
                        {
                            resultList.Add(item);
                            result = item;
                        }
                    }

                    var isWeekValid = int.TryParse(week, out int _week);


                    if (resultList.Count > 1 && isWeekValid)
                    {
                        foreach (var item in resultList)
                        {
                            var weeks = item.smartPeriod.Split(' ');

                            if (Array.IndexOf(weeks, week) >= 0)
                            {
                                result = item;
                                break;
                            }
                        }

                    }



                    return result;
                }
            }
            public static async Task SaveCourse(ObservableCollection<CourseModel> CourseList, string yaer = null, string term = null)
            {
                string filename = "course_";
                if (yaer == null)
                {
                    filename = filename + MD5.GetMd5String(UserManager.UserData.beginYear.ToString() + UserManager.UserData.term.ToString());
                }
                else
                {
                    filename = filename + MD5.GetMd5String(yaer + term);
                }
                var CourseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Course", CreationCollisionOption.OpenIfExists);
                var CourseFile = await CourseFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(CourseFile, Data.Json.ToJsonData(CourseList));
            }
        }
        public class User
        {
            public class Login_Result
            {
                /// <summary>
                /// 
                /// </summary>
                public long academyId { get; set; }
                /// <summary>
                /// 计算机技术与工程学院
                /// </summary>
                public string academyName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long addTime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public AttachmentBO attachmentBO { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string avatarUrl { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int beginYear { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string bigAvatarUrl { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long bornCityId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long bornDate { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long bornProvinceId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string fullAvatarUrl { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long gender { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long grade { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long highSchoolId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string identity { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long lastLoglongime { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long loveState { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int maxCount { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string mobileNumber { get; set; }
                /// <summary>
                /// 王文斌
                /// </summary>
                public string nickName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public List<PhotoBOItem> photoBO { get; set; }
                /// <summary>
                /// 王文斌
                /// </summary>
                public string realName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long schoolId { get; set; }
                /// <summary>
                /// 长春工程学院
                /// </summary>
                public string schoolName { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long schoolRoll { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long studentId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string superId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public string supportAuto { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public int term { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long versionId { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long weiboAccount { get; set; }
                /// <summary>
                /// 
                /// </summary>
                public long weiboExpiresIn { get; set; }
                public class GopushBO
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public string aliasName { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long mid { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long pmid { get; set; }
                }
                public class MyTermListItem
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public long addTime { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long beginYear { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long id { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long maxCount { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long studentId { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long term { get; set; }
                }
                public class NowWeekMsg
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public int nowWeek { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long setTime { get; set; }
                }
                public class AttachmentBO
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public long contactStatus { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long courseRemind { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long courseRemindTime { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long dayOfWeek { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long defaultOpen { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public GopushBO gopushBO { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string hasTermList { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string hasVerCode { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string identity { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public List<MyTermListItem> myTermList { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long needSASL { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public NowWeekMsg nowWeekMsg { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string openGopush { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string openJpush { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long openRubLessonlong { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long purviewValue { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long rate { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long realNameMsgNum { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string rubLessonTips { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string showRate { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string supportAuto { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string type { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long vipLevel { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string xmppDomain { get; set; }
                }
                public class PhotoBOItem
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public string avatar { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long id { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long photoId { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string photoUrl { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public long studentId { get; set; }
                    /// <summary>
                    /// 
                    /// </summary>
                    public string thumUrl { get; set; }
                }
            }
        }
    }
}
