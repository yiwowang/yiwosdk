using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using yiwoSDK;
namespace yiwoSDK.Sina
{ public class BlogList
{
    public string blog_id;
    public string title;
    public string time;
}
public class BlogContent
{public string title;
public string content;
public string author;
public string time;
}
   public  class CBlog
    {
        CHttpWeb h = new CHttpWeb();
        CEncode ce = new CEncode();
        
        string vtoken = "";
        string uid = "";
        public void SetHttpWeb(CHttpWeb ch)
        { h = ch; }
        public int Login(string UserName, string Password, string VCode = "")
        {

          

            //entry=blog&gateway=1&from=referer%3Ahttp%3A%2F%2Fi.blog.sina.com.cn&savestate=30&useticket=0&pagerefer=&door=nmgyk&su=OTU4Nzk2NjM2JTQwcXEuY29t&service=blog&sp=123456sgw%23&encoding=UTF-8&prelt=0&callback=parent.sinaSSOController.loginCallBack&returntype=IFRAME&setdomain=1
            string data = "entry=account&gateway=1&from=null&savestate=30&useticket=0&pagerefer=http%3A%2F%2Fblog.sina.com.cn%2F&door=" + VCode + "&vsnf=1&su=" + ce.ToBase64(UserName) + "&service=account&sp=" + ce.ToGB2312(Password) + "&encoding=UTF-8&prelt=0&callback=parent.sinaSSOController.loginCallBack&returntype=IFRAME&setdomain=1";
            string ret = h.HttpSendData("https://login.sina.com.cn/sso/login.php?client=ssologin.js(v1.4.8)", "post", data);
            if (ret.IndexOf("retcode=4049") > 0)
            {
                //ret = h.HttpSendData("http://login.sina.com.cn/crossdomain2.php?action=logincallback&retcode=4049&reason=%CE%AA%C1%CB%C4%FA%B5%C4%D5%CA%BA%C5%B0%B2%C8%AB%A3%AC%C7%EB%CA%E4%C8%EB%D1%E9%D6%A4%C2%EB&callback=parent.sinaSSOController.loginCallBack&setdomain=1");
                return 2;
            }
            else if (ret.IndexOf("retcode=0") > 0)
            {
              
                string s ;
                s=h.HttpSendData("http://login.sina.com.cn/crossdomain2.php?action=logincallback&retcode=0&reason=&callback=parent.sinaSSOController.loginCallBack&setdomain=1");
                int puid = s.IndexOf("uid");
               uid= s.Substring(puid + 6, s.IndexOf("});;") - puid - 7);
                return 0;
            }
            else if (ret.IndexOf("retcode=2070") > 0)
                return 3;
            else if (ret.IndexOf("retcode=101") > 0)
                return 1;
            else
                return -1;
            //int p = s.IndexOf("http:");
            //string url = s.Substring(p, s.IndexOf("]})") - p - 1).Replace("\\", "");
            //return  h.HttpSendData(url);

        }
      public string GetUid()
        {
            return uid;
        }
        public Image GetLoginVC()
        {
            //h.SetReferer("http://login.sina.com.cn/signup/signin.php?entry=blog&r=http%3A%2F%2Fi.blog.sina.com.cn&from=referer:http://i.blog.sina.com.cn");
            return h.GetImage("http://login.sina.com.cn/cgi/pin.php?r=" + CFun.GetRnd(8) + "&s=0");
        }
        public string AddBlog(string Title,string Content,int BlogClassFlag=0,int CommentFlag=0,bool IsOnlyICanSee=false,bool IsForbidQoute=false)
        {
            Content = Content.Trim();
            Title = Title.Trim();
            if (vtoken.Length != 32)
            {//获取vtoken
                string s = h.HttpSendData("http://control.blog.sina.com.cn/admin/article/article_add.php");
                vtoken = s.Substring(s.IndexOf("vtoken") + 15, 32);
            }
            //发表文章 
            /*x_cms_flag 0允许所有人评论  1不允许匿名评论  2不允许评论
             * x_rank=1文章仅自己可见 x_quote_flag=1禁止转载
            */
            DateTime dt = DateTime.Now;
            string d=dt.ToLocalTime().ToString().Split(' ')[0];
            string t = dt.ToLocalTime().ToString().Split(' ')[1];
            h.SetReferer("http://control.blog.sina.com.cn/admin/article/article_add.php");
            string url = "http://control.blog.sina.com.cn/admin/article/article_post.php";
            string data = "ptype=&teams=&worldcuptags=&album=&album_cite=&blog_id=&is_album=0&old365=0&stag=&sno=&book_worksid=&channel_id=&url=&channel=&newsid=&fromuid=&wid=&articletj=&vtoken=" + vtoken + "&is_media=0&is_stock=0&is_tpl=0&assoc_article=&assoc_style=1&assoc_article_data=&article_BGM=&xRankStatus=&commentGlobalSwitch=&commenthideGlobalSwitch=&articleStatus_preview=1&source=&topic_id=0&topic_channel=0&topic_more=&utf8=1&conlen=" 
                + Content.Length + "&date_pub=" + d + "&time=" + ce.ToUTF8(t) + "&isTimed=0&immediatepub=0&tbh_click_item_url=&tbh_title=&tbh_pic_url=&tbh_price=&tbh_item_type=&tbh_from=&blog_title=" + ce.ToUTF8(Title) + "&blog_body=" + ce.ToUTF8(Content)
                + "&blog_class="+BlogClassFlag+"&tag=&x_cms_flag=" + CommentFlag + "&x_rank=" + Convert.ToInt32(IsOnlyICanSee) + "&x_quote_flag="+Convert.ToInt32(IsForbidQoute)+"&join_circle=1";
            return h.HttpSendData(url,"post",data);
        }
        public string GetBlogClass(string uid="")
        {
            if (uid.Length == 0)
                uid = this.uid;
            return h.HttpSendData("http://hits.blog.sina.com.cn/acate?varname=x&uid="+uid);
        }
        public BlogList[]  GetBlogList(string uid="",int page=1,int classid=0)
        {
            if (uid.Length == 0)
                uid = this.uid;

            string s = h.HttpSendData("http://blog.sina.com.cn/s/articlelist_"+uid+"_"+classid+"_"+page+".html"); ;
                        string[] a = CFun.Split(s, "atc_title");
            BlogList[] bloglist = new BlogList[a.GetLength(0) - 1];
            for (int i = 1; i <= a.GetUpperBound(0); i++)
            {
                int p_title = a[i].IndexOf("title");
                int p_blog_id = a[i].IndexOf("blog_");
                string blog_id = a[i].Substring(p_blog_id + 5, a[i].IndexOf(".html") - p_blog_id-5);
                string title = a[i].Substring(p_title + 7, a[i].IndexOf("target") - p_title - 9);
                string time = a[i].Substring(a[i].IndexOf("atc_tm SG_txtc") + 16, 16);
                bloglist[i - 1] = new BlogList();
                bloglist[i - 1].blog_id = blog_id;
                bloglist[i - 1].title = title.Split ('_')[0];
                bloglist[i - 1].time = time;
                
            }
            return bloglist;

        }
       public BlogContent  GetBlogContent(string BlogId)
        {
            BlogContent bc = new BlogContent();
           string s=h.HttpSendData ("http://blog.sina.com.cn/s/blog_"+BlogId+".html");
           int p_title = s.IndexOf("<title>");
           string title = s.Substring(p_title + 7, s.IndexOf("</title>") - p_title - 7);
           bc.title = title.Split('_')[0];
           bc.author = title.Split('_')[1];
           int p_content = s.IndexOf("<!-- 正文开始 -->");
           bc.content = s.Substring(p_content + 77, s.IndexOf("<!-- 正文结束 -->") - p_content - 94).Trim ();
           bc.time = s.Substring(s.IndexOf("time SG_txtc")+15,19);
           return bc;
       }
        public string EditBlog(string BlogId,string Title, string Content, int BlogClassFlag = 0, int CommentFlag = 0, bool IsOnlyICanSee = false, bool IsForbidQoute = false)
        {
            Content = Content.Trim();
            Title = Title.Trim();
            if (vtoken.Length != 32)
            {//获取vtoken
                string s = h.HttpSendData("http://control.blog.sina.com.cn/admin/article/article_add.php");
                vtoken = s.Substring(s.IndexOf("vtoken") + 15, 32);
            }
            DateTime dt = DateTime.Now;
            string d = dt.ToLocalTime().ToString().Split(' ')[0];
            string t = dt.ToLocalTime().ToString().Split(' ')[1];
            string url = "http://control.blog.sina.com.cn/admin/article/article_edit_post.php";
            string data = "ptype=&teams=&worldcuptags=&album=&album_cite=&blog_id="+BlogId+"&is_album=0&old365=0&stag=&sno=&book_worksid=&channel_id=&url=&channel=&newsid=&fromuid=&wid=&articletj=0&vtoken="
               +vtoken+"&is_media=0&is_stock=0&is_tpl=0&assoc_article=&assoc_style=1&assoc_article_data=&article_BGM=&xRankStatus=0&commentGlobalSwitch=&commenthideGlobalSwitch=&articleStatus_preview=2&source=&topic_id=0&topic_channel=0&topic_more=&utf8=1&conlen="
                + Content.Length + "&date_pub=" + d + "&time=" + ce.ToUTF8(t) + "&isTimed=0&immediatepub=0&tbh_click_item_url=&tbh_title=&tbh_pic_url=&tbh_price=&tbh_item_type=&tbh_from=&blog_title=" 
                + ce.ToUTF8(Title) + "&blog_body=" + ce.ToUTF8(Content) + "&blog_class=" + BlogClassFlag + "&tag=&x_cms_flag=" + CommentFlag + "&x_rank=" + Convert.ToInt32(IsOnlyICanSee) + "&x_quote_flag=" + Convert.ToInt32(IsForbidQoute) + "&join_circle=1";
            h.SetReferer("http://control.blog.sina.com.cn/admin/article/article_edit.php?blog_id="+BlogId );
            return h.HttpSendData(url,"post",data);
        }
        public Image CommentVC()
        {
            return h.GetImage("http://interface.blog.sina.com.cn/riaapi/checkwd_image.php?"+CFun.GetRnd());
        }
        public string CommentAdd(string BlogId, string Content, string VCode, bool IsAnonymity = false)
    {
        string url="http://control.blog.sina.com.cn/admin/comment/comment_post.php?version=7&domain=1";
        string data = "comment=" + ce.ToUTF8(Content) + "&login_name=&login_pass=&check=" + VCode + "&comment_anonyous=&anonymity=" + IsAnonymity.ToString().ToLower() + "&is_mobile=0&is_t=1&login_remember=true&article_id=" + BlogId + "&uid=" + uid + "&fromtype=commentadd";
        return h.HttpSendData(url,"post",data);
    }
    }
   
}
