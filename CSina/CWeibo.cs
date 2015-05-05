using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using yiwoSDK;
namespace yiwoSDK.Sina
{
    public class CWeibo
    {
        CHttpWeb h = new CHttpWeb();
        CEncode ce = new CEncode();
        string uid="";
        string uName="";
        string uDomain = "";
        /// <summary>
        /// 设置CHttpWeb对象，不同的对象Cookie不一样，为了防止Cookie干扰可以选择设置
        /// </summary>
        /// <param name="ch"></param>
        public void SetHttpWeb(CHttpWeb ch)
        { h = ch; }
        public string GetUserDomain()
        {
            return uDomain;
        }
        public Image GetLoginVC()
        {
            //h.SetReferer("http://login.sina.com.cn/signup/signin.php?entry=blog&r=http%3A%2F%2Fi.blog.sina.com.cn&from=referer:http://i.blog.sina.com.cn");
            return h.GetImage("http://login.sina.com.cn/cgi/pin.php?r=" + CFun.GetRnd(8) + "&s=0");
        }
        public int Login(string UserName,string Password,string VCode="")
        {

            string data = "entry=account&gateway=1&from=null&savestate=30&useticket=0&pagerefer=http%3A%2F%2Fblog.sina.com.cn%2F&door=" + VCode + "&vsnf=1&su=" + ce.ToBase64(UserName) + "&service=account&sp=" + ce.ToGB2312(Password) + "&encoding=UTF-8&prelt=0&callback=parent.sinaSSOController.loginCallBack&returntype=IFRAME&setdomain=1";
            string ret = h.HttpSendData("https://login.sina.com.cn/sso/login.php?client=ssologin.js(v1.4.8)", "post", data);
            if (ret.IndexOf("retcode=4049") > 0)
            {
                
                return 2;
            }
            else if (ret.IndexOf("retcode=0") > 0)
            {

                string s;
                s = h.HttpSendData("http://login.sina.com.cn/crossdomain2.php?action=logincallback&retcode=0&reason=&callback=parent.sinaSSOController.loginCallBack&setdomain=1");
                int p = s.IndexOf("http:");
                string url = s.Substring(p, s.IndexOf("]})") - p - 1).Replace("\\", "");
                s = h.HttpSendData(url);
                
                int p1 = s.IndexOf("uniqueid");
                int p2 = s.IndexOf("userid");
                int p4 = s.IndexOf("userdomain");
                uid=s.Substring(p1 + 11, p2 - p1 - 14);
                uName=s.Substring(p2 + 9, s.IndexOf("displayname") - p2 - 12);
                uDomain = s.Substring(p4+13, s.IndexOf("}}")-p4-14);
                return 0;
            }
            else if (ret.IndexOf("retcode=2070") > 0)
                return 3;
            else if (ret.IndexOf("retcode=101") > 0)
                return 1;
            else
                return -1;
        }
        public string GetUid()
        {
            return uid;
        }
        public string AddWeibo(string Content)
        {
            h.SetReferer("http://weibo.com/u/"+uid +"?topnav=1&wvr=5");
           return h.HttpSendData("http://weibo.com/aj/mblog/add?_wv=5&__rnd="+CFun .GetRnd (), "post", "text="+ce.ToUTF8 (Content)+"&pic_id=&rank=0&rankid=&_surl=&hottopicid=&location=home&module=stissue&_t=0");
       
        }
    }
}
