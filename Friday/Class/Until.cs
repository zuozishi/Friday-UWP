using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friday.Class
{
    public class Until
    {
        public class Friday
        {
            public class Playground
            {
                public static async Task<bool> FollowTopic(string topicId)
                {
                    if (Class.HttpPostUntil.isInternetAvailable)
                    {
                        try
                        {
                            var postdata = HttpPostUntil.GetBasicPostData();
                            postdata.Add(new KeyValuePair<string, string>("topicId", topicId));
                            var json = await Class.HttpPostUntil.HttpPost(Data.Urls.Playground.FollowTopic, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                            var result = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["flag"].GetBoolean();
                            return result;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                public static async Task<bool> UnFollowTopic(string topicId)
                {
                    if (Class.HttpPostUntil.isInternetAvailable)
                    {
                        try
                        {
                            var postdata = HttpPostUntil.GetBasicPostData();
                            postdata.Add(new KeyValuePair<string, string>("topicId", topicId));
                            var json = await Class.HttpPostUntil.HttpPost(Data.Urls.Playground.UnFollowTopic, new Windows.Web.Http.HttpFormUrlEncodedContent(postdata));
                            var result = Windows.Data.Json.JsonObject.Parse(json)["data"].GetObject()["flag"].GetBoolean();
                            return result;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
