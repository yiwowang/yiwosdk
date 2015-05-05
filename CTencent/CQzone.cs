using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
namespace yiwoSDK
 {
#region QQ空间
    public  class CQzone
    {
        CEncode ce = new CEncode();
        private string strErrHead = "易我论坛提示错误：";
        private string m_g_tk="";
        private string m_skey = "";
        
         CHttpWeb h=new CHttpWeb ();
        string QQ="12345678";
        struct BlogRoute
        {
           public  string QQnumber;
            public string route;

        };
        struct PhotoRouteData
        {
            public string QQnumber;
            public string route;

        };
        BlogRoute br = new BlogRoute();
        PhotoRouteData pr = new PhotoRouteData();
        //装扮代码结构
        struct Items
        {
            public string type, itemno, posx, posy, posz, height, width, flag;
        };
        //已经装扮的模块id结构
        public struct Windows
        {
            public string appid, mode, posx, posy, posz, height, width, wndid;
        };
        //模块数据结构
        struct ModeData
        {
            public string id;
            public string data;

        }
        string strDressCurQQ ="";
        int[] ModeIndex;//需要装扮的模块数据数组的下标
        ModeData[] modedata;//全部模块数据
        string styleid;//风格颜色
        Items[] items;
        public Windows[] windows;
        /// <summary>
        /// 当前登录的QQ
        /// </summary>
        /// <returns>返回登录的QQ</returns>
     public string GetMyQQ()
        {return QQ;}
     /// <summary>
     /// 设置webqq使用的网络对象
     /// </summary>
     /// <param name="ch">CHttpWeb类的对象</param>
        public void SetHttpWeb(CHttpWeb ch)
        { h = ch; }


        /// <summary>
        /// 登陆QQ空间
        /// </summary>
        /// <param name="QQnumber">QQ号码</param>
        /// <param name="password">密码</param>
        /// <param name="VCode">验证码</param>
        /// <returns></returns>
        string verifysessionx = "";
        public string  Login(long QQnumber,string password,string VCode)
        {

            verifysessionx = h.verifysession;
            string p = CQQHelper.getNewP(password, QQnumber, VCode.ToUpper());
            string URL = "http://ptlogin2.qq.com/login?u=" + QQnumber + "&verifycode=" + VCode + "&pt_vcode_v1=1&pt_verifysession_v1=" + verifysessionx + "&p=" + p + "&pt_randsalt=0&u1=http%3A%2F%2Fqzs.qq.com%2Fqzone%2Fv5%2Floginsucc.html%3Fpara%3Dizone&ptredirect=0&h=1&t=1&g=1&from_ui=1&ptlang=2052&action=4-18-1428934975515&js_ver=10119&js_type=1&login_sig=5SBaSJHNkALXSHqjacv8nXdmlt5GGeFbThJ2Qv25wY7-hotr2Bb8pWE7dC1gTrdY&pt_uistyle=32&aid=549000912&daid=5&pt_qzone_sig=1";
            string url = "";
            Debug.WriteLine("URL=" + URL);
            h.SetReferer("http://xui.ptlogin2.qq.com/cgi-bin/xlogin?proxy_url=http%3A//qzs.qq.com/qzone/v6/portal/proxy.html&daid=5&pt_qzone_sig=1&hide_title_bar=1&low_login=0&qlogin_auto_login=1&no_verifyimg=1&link_target=blank&appid=549000912&style=22&target=self&s_url=http%3A//qzs.qq.com/qzone/v5/loginsucc.html?para=izone&pt_qr_app=鎵嬫満QQ绌洪棿&pt_qr_link=http%3A//z.qzone.com/download.html&self_regurl=http%3A//qzs.qq.com/qzone/v6/reg/index.html&pt_qr_help_link=http%3A//z.qzone.com/download.html");
            string strResult= h.HttpSendData(URL);
            Debug.WriteLine("strResult=" + strResult);
            if (strResult.IndexOf("('0'") >=0)
            {
                QQ = QQnumber+"";
                this.m_skey = this.skey();
                this.m_g_tk = this.g_tk(this.m_skey); 
            }
            return strResult;
        }
        /// <summary>
        /// 获取登录验证码图片
        /// </summary>
        /// <param name="QQnumber">欲登陆的QQ</param>
        /// <returns></returns>
        public Image GetLoginVCImage(string QQnumber)
        {
            return h.GetImage("http://captcha.qq.com/getimgbysig?aid=549000912&uin="+QQnumber+"&sig="+sig);
        }
        /// <summary>
        /// 获取默认验证码或验证码图片
        /// </summary>
        /// <param name="QQnumber">欲登陆的QQ</param>
        /// <returns></returns>
        /// string 
        
        string sig = "";
        public string GetLoginVC(string QQnumber)
        {
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");
            if (!CFun.IsQQnumber(ref QQnumber)) return strErrHead + "QQ号码长度错误或有非法字符";
            string verifyCode = "";
            string tmp = h.HttpSendData("http://check.ptlogin2.qq.com/check?regmaster=&pt_tea=1&pt_vcode=1&uin="+QQnumber+"&appid=549000912&js_ver=10119&js_type=1&login_sig=iu6R1CKp6RJKpNFqewpOaRE1VPb*t6F*TqnelJ2oVwdU-x1Iyys0k3mg0jpweR7L&u1=http%3A%2F%2Fqzs.qq.com%2Fqzone%2Fv5%2Floginsucc.html%3Fpara%3Dizone&r=0.7945340680074419");
            string cap_cd  = tmp.Substring(tmp.IndexOf("','") + 3, tmp.IndexOf(",'\\") - tmp.IndexOf("','") - 4);
            //
            Debug.WriteLine("tmp=" + tmp);
            string tmp1 = h.HttpSendData("http://captcha.qq.com/cap_union_show?clientype=2&uin="+QQnumber+"&aid=549000912&cap_cd="+cap_cd+"&0.32104462615761137");
            Debug.WriteLine("tmp1=" + tmp1);

            sig = tmp1.Substring(tmp1.IndexOf("g_click_cap_sig") +17, tmp1.IndexOf("\";") - tmp1.IndexOf("g_click_cap_sig") - 16);
            Debug.WriteLine("sig=" + sig);
            if (tmp.IndexOf("('0'") > 0)
              verifyCode = tmp.Substring(18, 4);
          else
                verifyCode = "http://captcha.qq.com/getimgbysig?aid=549000912&uin=" + QQnumber + "&sig=" + sig;
          return verifyCode;
       }
        /// <summary>
        /// 获取g_tk
        /// </summary>
        /// <param name="skey">skey值</param>
        /// <returns>返回g_tk</returns>
        public  string g_tk(string skey="")
        {
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");
            
            if (this.m_g_tk.Length>0&&skey.Length==0)return this.m_g_tk; 

            int hash = 5381;
            char[]c=skey.ToCharArray();
            for (int i = 0, len = c.Length; i < len; ++i)
            {
                hash += (hash << 5) + c[i];
            }
            hash = hash & 0x7fffffff;
            this.m_g_tk = hash.ToString();
            return this.m_g_tk;
        }
        /// <summary>
        /// 从cookie中获取skey
        /// </summary>
        /// <param name="cookie">cookie，获取默认自动</param>
        /// <returns>返回skey</returns>
        public string skey(string cookie="")
        {
            if (this.m_skey.Length > 0 && cookie.Length <10) return this.m_skey;
                cookie = h.GetCookie();
            int p=cookie.IndexOf("skey");
            if (p >= 0)
            {
                this.m_skey = cookie.Substring(p + 5, 10);
            }
            return this.m_skey;
        }
        /// <summary>
        /// 计算数据包中的p字段
        /// </summary>
        /// <param name="QQnumber">QQ号码</param>
        /// <param name="password">密码</param>
        /// <param name="VCode">验证码</param>
        /// <returns></returns>
        public  string GetP(string QQnumber, string password, string VCode)
        {
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");

            return CQQHelper.GetP(QQnumber,password,VCode);

        }
        /// <summary>
        /// 获取好友信息
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public string GetUserInfo(string QQnumber)
        { return h.HttpSendData("http://base.s6.qzone.qq.com/cgi-bin/user/cgi_userinfo_get_all?uin=" + QQnumber + "&vuin=" + QQ + "&rd=0." + CFun.GetRnd() + "&g_tk=" + m_g_tk); }
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        public string GetFriendList()
        {
         
                string ret = h.HttpSendData("http://r.cnc.qzone.qq.com/cgi-bin/tfriend/friend_mngfrd_get.cgi?uin=" + QQ + "&rd=0." + CFun.GetRnd() + "&g_tk=" + m_g_tk);
                return ret.Replace("_Callback(", "").Replace (");","");
     
        }
        /// <summary>
        /// 获取好友资料卡片
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>

        public string GetPersionCard(string QQnumber)
        { return h.HttpSendData("http://r.cnc.qzone.qq.com/cgi-bin/tfriend/friend_mngfrd_get.cgi?uin=" + QQnumber + "&rd=0." + CFun.GetRnd() + "&g_tk="+m_g_tk); }
        /// <summary>
        /// 获取空间访客数量
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public string VisitCount(string QQnumber)
        { return h.HttpSendData("http://g.cnc.qzone.qq.com/fcg-bin/cgi_emotion_list.fcg?uin="+QQnumber+"&loginUin=" + QQ + "&s=250195&num=3&g_tk="+ m_g_tk); }
        /// <summary>
        /// 获取访客列表
        /// </summary>
        /// <param name="QQnumber"></param>
        /// <returns></returns>
        
        public string VisitorList(string QQnumber)
        { return h.HttpSendData("http://g.qzone.qq.com/cgi-bin/friendshow/cgi_get_visitor_simple?uin="+QQnumber +"&mask=7&clear=1&sd=872534&g_tk=" + m_g_tk); }
        public string VisitorDel(string QQnumber,string src)
        {
            return h.HttpSendData("http://w.qzone.qq.com/cgi-bin/tfriend/friendshow_hide_visitor_onelogin?g_tk="+m_g_tk, "post", "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQ+"%3Fptsig%3DiHPndmXYOXPk7RvMEd57Xis8t6twC6aDMHRx3MDbmL4_&vuin="+QQnumber+"&huin="+QQ+"&type=0&src="+src);
        }
        /// <summary>
        /// 获取QQ头像
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public Image GetQQHeadImage(string QQnumber)
        { return h.GetImage("http://face7.qun.qq.com/cgi/svr/face/getface?cache=0&type=1&fid=0&uin="+QQnumber); }
        /// <summary>
        /// 获取QQ秀图片
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public Image GetQQShowImage(string QQnumber)
        {
            return h.GetImage("http://qqshow-user.tencent.com/"+QQnumber+"/10/0");
        }
        /// <summary>
        /// 获取QQ在线状态图片
        /// </summary>
        /// <param name="QQnumber">好友</param>
        /// <param name="style">显示风格</param>
        /// <returns></returns>
        public Image GetQQOnlineImage(string QQnumber,int style=1)
        {
            if (style < 1 || style > 53)
                style = 1;
            return h.GetImage("http://wpa.qq.com/pa?p=1:"+QQnumber+":"+style);
        }
        /// <summary>
        /// 获取空间头像
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="size">头像大小</param>
        /// <returns></returns>
        public Image GetQzoneHeadImage(string QQnumber,int size=1)
        {
            string strSize = "";
            if (size == 1) strSize = "100"; else strSize = "50";
            return h.GetImage("http://qlogo1.store.qq.com/qzone/" + QQnumber + "/" + QQnumber + "/"+strSize); }
        /// <summary>
        /// 给好友留言
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="Content">留言内容</param>
        /// <returns></returns>
        public string WordsAdd( string QQnumber, string Content)
        {
            return h.HttpSendData("http://m.qzone.qq.com/cgi-bin/new/add_msgb?ref=qzone&g_tk=" + m_g_tk, "post",
            "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fmsgboard%2Fmsgbcanvas.html%23page%3D1&content="+ce.ToGB2312 (Content)+"&hostUin="+QQnumber+"&uin="+QQ+"&format=fs&g_tk="+m_g_tk+"&ref=qzone&json=1&inCharset=gbk&outCharset=gbk&iNotice=1" , "GB2312");
        }//Anwser 改为Words
        /// <summary>
        /// 主人回复留言者
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="WordsID">留言ID</param>
        /// <param name="content">回复内容</param>
        /// <returns></returns>
        public string WordsReply(string QQnumber,string WordsID, string content)
        {//空间主人回复留言者
            string url = "http://m.qzone.qq.com/cgi-bin/new/add_reply?ref=qzone&g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fmsgboard%2Fmsgbcanvas.html%23uin%3D"+QQnumber+"%26pfid%3D2%26qz_ver%3D6%26appcanvas%3D0%26qz_style%3Dv6%2F1%26params%3D%26entertime%3D1355890053139%26canvastype%3D&hostUin="+QQnumber+"&msgId=281&format=fs&content="+ce.ToGB2312(content)+"&uin="+QQ+"&g_tk="+m_g_tk+"&ref=qzone&json=1&inCharSet=gbk&outCharSet=gbk&iNotice=1";
            return h.HttpSendData(url,"post",data,"gb2312");
        }
        /// <summary>
        /// 留言者回复主人
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="WordsID">留言ID</param>
        /// <param name="content">回复内容</param>
        /// <returns></returns>
        public string WordsComment(string QQnumber, string WordsID, string content)
        {//留言者回复空间主人
            string url = "http://m.qzone.qq.com/cgi-bin/new/msgb_comment_answer.cgi?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQ+"%2Finfocenter&ouin="+QQ+"&param="+QQnumber+"%2C"+WordsID+"%2C0&reqref=feeds&feedversion=1&content="+ce.ToGB2312(content)+"&g_tk="+m_g_tk+"&postURL=http%3A%2F%2Fm.qzone.qq.com%2Fcgi-bin%2Fnew%2Fmsgb_comment_answer.cgi&appId=334&btnstyle=0";
            return h.HttpSendData(url, "post", data, "gb2312");
        }
        /// <summary>
        /// 删除留言
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="WordsID">留言ID</param>
        /// <returns></returns>
        public string WordsDel(string QQnumber,string WordsID)
        {
            string url = "http://m.qzone.qq.com/cgi-bin/new/del_msgb?ref=qzone&g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fmsgboard%2Fmsgbcanvas.html%23uin%3D"+QQ+"%26pfid%3D2%26qz_ver%3D6%26appcanvas%3D0%26qz_style%3Dv6%2F1%26params%3D%26entertime%3D1355890767323%26canvastype%3D&hostUin="+QQ+"&idList="+WordsID+"&uinList="+QQnumber+"&format=fs&g_tk=966182619&ref=qzone&json=1&inCharSet=gbk&outCharSet=gbk&iNotice=1";
            return h.HttpSendData(url,"post",data,"gb2312");
        }
        /// <summary>
        /// 留言列表
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="page">第几页</param>
        /// <returns></returns>
        public string WordsList(string QQnumber,int page=1)
        {
          string url= "http://m.qzone.qq.com/cgi-bin/new/get_msgb?uin="+QQ+"&hostUin="+QQnumber+"&start="+(page-1)*10+"&s=0.5270251905953441&g_tk="+m_g_tk+"&format=jsonp&num=10";
          return h.HttpSendData(url,"Get","","gb2312");
        }
        /// <summary>
        /// 获取好友动态
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns></returns>
        public string GetFriendFeeds(int count)
        {
            string ret = h.HttpSendData("http://ic2.s11.qzone.qq.com/cgi-bin/feeds/feeds2_html_more?uin=" + GetMyQQ() + "&scope=0&view=1&daylist=&uinlist=&gid=&flag=1&filter=all&applist=all&refresh=0&firstGetGroup=0&icServerTime=0&mixnocache=0&scene=0&begintime=0&count=" + count.ToString() + "&dayspac=0&sidomain=ctc.qzonestyle.gtimg.cn&g_tk=" + g_tk() + "&grz=0.23415492590650777&useutf8=1&outputhtmlfeed=1");
            return ret.Replace("\\x22", "\"").Replace("\\/", "/").Replace("\\x3C", "<");
        }
        /// <summary>
        /// 获取说说列表
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="page">第几页</param>
        /// <returns></returns>
        public string ShuoList(string QQnumber,int page=1)
        {
           string  url = "http://taotao.qq.com/cgi-bin/emotion_cgi_msglist_v6?uin="+QQnumber+"&ftype=0&sort=0&pos="+(page-1)*20+"&num=20&replynum=100&g_tk="+m_g_tk+"&callback=_preloadCallback&code_version=1&format=jsonp";
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 发表说说
        /// </summary>
        /// <param name="Content">说说内容</param>
        /// <returns></returns>
        public string ShuoAdd(string Content)
        { return h.HttpSendData ("http://taotao.qq.com/cgi-bin/emotion_cgi_publish_v6?g_tk="+m_g_tk,"post",
            "who=1&con="+ce.ToUTF8(Content).ToUpper() +"&feedversion=1&ver=1&hostuin="+QQ +"&qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQ);}
        /// <summary>
        /// 说说评论者回复主人
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="shuoid">说说ID</param>
        /// <param name="content">回复内容</param>
        /// <returns></returns>
        public string ShuoComment(string QQnumber,string shuoid,string content)
        {
            string url = "http://taotao.qq.com/cgi-bin/emotion_cgi_comment_v6?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fapp%2Fmood_v6%2Fhtml%2Findex.html%3Fmood%23uin%3D"+QQnumber+"%26pfid%3D2%26qz_ver%3D6%26appcanvas%3D0%26qz_style%3Dv6%2F81%26params%3D%26entertime%3D1355732719150%26canvastype%3D&otype=json&puin="+QQnumber+"&ptid="+shuoid+"&con="+ce.ToUTF8 (content)+"&code_version=1&format=fs&out_charSet=UTF-8&hostuin="+QQ;
            return h.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 说说主人回复评论者
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="shuoid">说说ID</param>
        /// <param name="content">回复内容</param>
        /// <param name="QQpos">好友的QQ位置</param>
        /// <returns></returns>
        public string ShuoReply(string QQnumber,string shuoid,string content,int QQpos=1)
        {
            string url = "http://taotao.qq.com/cgi-bin/emotion_cgi_addreply_v6?g_tk=" + m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fapp%2Fmood_v6%2Fhtml%2Findex.html%3Ftab%3Dmine&otype=json&puin="+QQ+"&ptid="+shuoid+"&cuin="+QQnumber+"&ctid="+QQpos+"&con="+ce.ToUTF8(content)+"&code_version=1&format=fs&out_charSet=UTF-8&hostuin="+QQ;
            h.SetReferer("http://ctc.qzs.qq.com/qzone/app/mood_v6/html/index.html?tab=mine");
            return h.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 赞说说
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="shuoid">说说ID</param>
        /// <param name="isLike">赞/取消赞</param>
        /// <returns></returns>
        public string ShuoLike(string QQnumber,string shuoid,bool isLike=true)
        {
            string url;
            if (isLike == true)
                url = "http://w.qzone.qq.com/cgi-bin/likes/internal_dolike_app?g_tk=" + m_g_tk;
            else
                url = "http://w.qzone.qq.com/cgi-bin/likes/internal_unlike_app?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQ+"%2Finfocenter%23%21app%3D311%26url%3Dhttp%253A%252F%252Fctc.qzs.qq.com%252Fqzone%252Fapp%252Fmood_v6%252Fhtml%252Findex.html%253Fmood%2523uin%253D"+QQ+"%2526pfid%253D2%2526qz_ver%253D6%2526appcanvas%253D0%2526qz_style%253Dv6%252F1%2526params%253D%2526entertime%253D1355796534279%2526canvastype%253D&opuin="+QQ+"&unikey=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQnumber+"%2Fmood%2F"+shuoid+".1&curkey=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQnumber+"%2Fmood%2F"+shuoid+".1&from=-100&fupdate=1";

           //string ss= "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F799101989%2Fmyhome&opuin=799101989&unikey=http%3A%2F%2Fuser.qzone.qq.com%2F799101989%2Fblog%2F1415022493&curkey=http%3A%2F%2Fuser.qzone.qq.com%2F799101989%2Fblog%2F1415022493&from=1&appid=2&typeid=5&abstime=1415022493&fid=""&active=0&fupdate=1";

            return h.HttpSendData(url,"post",data);
        }

        public string Like(string QQnumber, string id,string appid,string typeid,string abstime, bool isLike = true)
        {
            string url;
            if (isLike == true)
                url = "http://w.qzone.qq.com/cgi-bin/likes/internal_dolike_app?g_tk=" + m_g_tk;
            else
                url = "http://w.qzone.qq.com/cgi-bin/likes/internal_unlike_app?g_tk=" + m_g_tk;
            string data = "";
            
            if (appid == "311")//说说
            {
             data=   "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQ + "%2Fmyhome&opuin=" + QQ + "&unikey=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQnumber + "%2Fmood%2F" + id + "&curkey=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQnumber + "%2Fmood%2F" + id + "&from=1&appid=" + appid + "&typeid=" + typeid + "&abstime=" + abstime  + "&fid=" + id + "&active=0&fupdate=1";
            }
            else if (appid == "2")//日志
            {
                data = "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQ + "%2Fmyhome&opuin=" + QQ + "&unikey=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQnumber + "%2Fblog%2F" + abstime + "&curkey=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQnumber + "%2Fblog%2F" + abstime + "&from=1&appid=" + appid + "&typeid=" + typeid + "&abstime=" + abstime+ "&fid=" + id + "&active=0&fupdate=1";
            }
   
            //string ss= "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F799101989%2Fmyhome&opuin=799101989&unikey=http%3A%2F%2Fuser.qzone.qq.com%2F799101989%2Fblog%2F1415022493&curkey=http%3A%2F%2Fuser.qzone.qq.com%2F799101989%2Fblog%2F1415022493&from=1&appid=2&typeid=5&abstime=1415022493&fid=""&active=0&fupdate=1";

            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 转载说说
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="shuoid">说说ID</param>
        /// <param name="content">说说内容</param>
        /// <returns></returns>
        public string ShuoForward(string QQnumber,string shuoid,string content)
        {
            string url = "http://taotao.qq.com/cgi-bin/emotion_cgi_forward_v6?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F"+QQ+"%2Finfocenter%23%21app%3D311%26url%3Dhttp%253A%252F%252Fctc.qzs.qq.com%252Fqzone%252Fapp%252Fmood_v6%252Fhtml%252Findex.html%253Fmood%2523uin%253D"+QQ+"%2526pfid%253D2%2526qz_ver%253D6%2526appcanvas%253D0%2526qz_style%253Dv6%252F1%2526params%253D%2526entertime%253D1355797421437%2526canvastype%253D&tid="+shuoid+"&t1_source=1&t1_uin="+QQnumber+"&signin=0&con="+ce.ToUTF8 (content)+"&with_cmt=0&fwdToWeibo=0&code_version=1&format=fs&out_charSet=UTF-8&hostuin="+QQ;
            return h.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 删除说说
        /// </summary>
        /// <param name="shuoid">说说ID</param>
        /// <returns></returns>
        public string ShuoDel(string shuoid)
        {
            string url = "http://taotao.qq.com/cgi-bin/emotion_cgi_delete_v6?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fapp%2Fmood_v6%2Fhtml%2Findex.html%3Ftab%3Dmine&hostuin="+QQ+"&tid="+shuoid+"&t1_source=1&code_version=1&format=fs&out_charSet=UTF-8";
            return h.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 发表日志
        /// </summary>
        /// <param name="Category">日志分类</param>
        /// <param name="Title">标题</param>
        /// <param name="Content">内容</param>
        /// <returns></returns>
        public string BlogAdd(string Category,string Title,string Content)
        {

                return h.HttpSendData("http://" + BlogRouteInit(QQ) + ".qzone.qq.com/cgi-bin/blognew/add_blog?g_tk=" + m_g_tk, "post",
                    "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fnewblog%2Fv5%2Feditor.html%23uin%3D" + QQ + "%26pfid%3D2%26qz_ver%3D6%26appcanvas%3D0%26qz_style%3Dv6%2F1%26entertime%3D1382612185106&cate=" + ce.ToGB2312(Category) + "&title=" + ce.ToGB2312(Title) + "&html=" + ce.ToGB2312(Content) + "&blogType=0&autograph=1&topFlag=0&feeds=1&tweetFlag=0&rightType=1&uin="+QQ+"&hostUin="+QQ+"&iNotice=1&inCharset=gbk&outCharset=gbk&format=fs&ref=qzone&json=1&g_tk=" + m_g_tk + "&secverifykey=28Q1206" + QQ + "_" + QQ
                    //  "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fnewblog%2Fv5%2Feditor.html%23opener%3Drefererurl%26source%3D1%26refererurl%3Dhttp%253A%252F%252Fctc.qzs.qq.com%252Fqzone%252Fapp%252Fblog%252Fv6%252Fbloglist.html%2523nojump%253D1%2526page%253D1%2526catalog%253Dlist&uin=" + QQ + "&category=" + ce.ToGB2312(Category) + "&title=" + ce.ToGB2312(Title) + "&html=" + ce.ToGB2312(Content) + "&tweetflag=0&cb_autograph=1&topflag=0&needfeed=0&source=1&lp_id=81177&lp_style=16843520&lp_flag=0&lp_type=0&g_tk=" + m_g_tk + "&ref=qzone&secverifykey=28Q1206" + QQ + "_" + QQ
                 , "gb2312");
 
        }
        /// <summary>
        /// 日志分类
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
    
        /// <summary>
        /// 获取日志域名
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public string BlogRouteInit(string QQnumber)
        {
            if (br.QQnumber== QQnumber)
            {
                return br.route ;
            }
            else
            {
                string s = h.HttpSendData("http://br.cnc.qzone.qq.com/cgi-bin/blognew/blog_get_route?fupdate=1&g_tk="+g_tk()+"&uinlist="+QQnumber+","+QQnumber);
                string[] a = s.Split(':');
                br.QQnumber = QQnumber;
               br.route = a[a.GetUpperBound(0)].Substring(1, a[a.GetUpperBound(0)].IndexOf("}") - 2);
               return br.route;
            }
        }
        /// <summary>
        /// 日志列表
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="page">第几页</param>
        /// <param name="isJson">是否为json数据</param>
        /// <returns></returns>
        public string BlogList(string QQnumber,int page=1)
        {
            string  url = "http://" + BlogRouteInit(QQnumber) + ".cnc.qzone.qq.com/cgi-bin/blognew/get_abs?hostUin=" + QQnumber + "&uin=" + GetMyQQ() + "&blogType=0&cateName=&cateHex=&statYear=2014&reqInfo=7&pos=" + (page - 1) * 100 + "&num=100&sortType=0&source=0&rand=0.4807406110103369&ref=qzone&g_tk=" + g_tk() + "&verbose=1";
          return  h.HttpSendData(url, "Get", "", "GB2312");
           
        }
        /// <summary>
        /// 日志内容
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <returns></returns>
         public string BlogContent(string QQnumber, string blogid)
        { return h.HttpSendData("http://" + BlogRouteInit(QQnumber) + ".cnc.qzone.qq.com/cgi-bin/blognew/blog_output_data?uin=" + QQnumber + "&blogid=" + blogid + "&styledm=cnc.qzonestyle.gtimg.cn&imgdm=cnc.qzs.qq.com&bdm=b.cnc.qzone.qq.com", "Get", "", "gb2312"); }
        /// <summary>
        /// 删除日志
        /// </summary>
        /// <param name="blogid">日志ID</param>
        /// <returns></returns>
       public string BlogDel(string blogid)
       {
           string url = "http://" + BlogRouteInit(QQ) + ".qzone.qq.com/cgi-bin/blognew/del_blog?ref=qzone&g_tk=" + m_g_tk;
           string data = "qzreferrer=http%3A%2F%2Fb11.qzone.qq.com%2Fcgi-bin%2Fblognew%2Fblog_output_data%3Fuin%3D" + blogid + "%26blogid%3D" + blogid + "%26styledm%3Dctc.qzonestyle.gtimg.cn%26imgdm%3Dctc.qzs.qq.com%26bdm%3Db.qzone.qq.com%26mode%3D2%26numperpage%3D15%26blogseed%3D0.8042467648592322%26property%3DGoRE%26timestamp%3D1355811180%26dprefix%3D%26g_tk%3D" + m_g_tk + "%26page%3D1%26refererurl%3Dhttp%253A%252F%252Fctc.qzs.qq.com%252Fqzone%252Fapp%252Fblog%252Fv6%252Fbloglist.html%2523nojump%253D1%2526page%253D1%2526catalog%253Dlist%26ref%3Dqzone%26v6%3D1&uin=" + QQ + "&hostUin=" + QQ + "&idList=" + blogid + "&blogType=0&g_tk=" + m_g_tk + "&ref=qzone&json=1&inCharSet=gbk&outCharSet=gbk&format=fs&iNotice=1";
           return h.HttpSendData(url, "post", data, "gb2312");
       }
        /// <summary>
        /// 日志主人回复评论者
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public string BlogComment(string QQnumber, string blogid, string content)
        {
            string url = "http://" + BlogRouteInit(QQnumber) + ".qzone.qq.com/cgi-bin/blognew/add_comment?ref=qzone&g_tk=" + m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fb11.qzone.qq.com%2Fcgi-bin%2Fblognew%2Fblog_output_data%3Fuin%3D" + QQnumber + "%26blogid%3D" + blogid + "%26styledm%3Dctc.qzonestyle.gtimg.cn%26imgdm%3Dctc.qzs.qq.com%26bdm%3Db.qzone.qq.com%26mode%3D2%26numperpage%3D15%26blogseed%3D0.7633681135395410%26property%3DGoRE%26timestamp%3D1355732615%26dprefix%3D%26g_tk%3D" + m_g_tk + "%26page%3D1%26refererurl%3Dhttp%253A%252F%252Fctc.qzs.qq.com%252Fqzone%252Fapp%252Fblog%252Fv6%252Fbloglist.html%2523nojump%253D1%2526page%253D1%2526catalog%253Dlist%26ref%3Dqzone%26v6%3D1&uin=" + QQ + "&content=" + ce.ToGB2312(content) + "&topicId=" + QQnumber + "_" + blogid + "&yscProp=0&property=GoRE&source=45&g_tk=" + m_g_tk + "&ref=qzone&json=1&inCharSet=gbk&outCharSet=gbk&format=fs&iNotice=1&secverifykey=28Q1206" + QQ + "_" + QQnumber;
            return h.HttpSendData(url,"post",data,"gb2312");
        }
        /// <summary>
        /// 日志评论者回复主人
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <param name="content">回复内容</param>
        /// <param name="QQid"></param>
        /// <returns></returns>
        public string BlogReply(string QQnumber,string blogid,string content,string QQid)
        {
            string url = "http://" + BlogRouteInit(QQnumber) + ".qzone.qq.com/cgi-bin/blognew/add_reply?ref=qzone&g_tk=" + m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fb11.qzone.qq.com%2Fcgi-bin%2Fblognew%2Fblog_output_data%3Fuin%3D" + QQnumber + "%26blogid%3D" + blogid + "%26styledm%3Dctc.qzonestyle.gtimg.cn%26imgdm%3Dctc.qzs.qq.com%26bdm%3Db.qzone.qq.com%26mode%3D2%26numperpage%3D15%26blogseed%3D0.7838272935914745%26property%3DGoRE%26timestamp%3D1355731091%26dprefix%3D%26g_tk%3D" + m_g_tk + "%26page%3D1%26refererurl%3Dhttp%253A%252F%252Fctc.qzs.qq.com%252Fqzone%252Fapp%252Fblog%252Fv6%252Fbloglist.html%2523nojump%253D1%2526page%253D1%2526catalog%253Dlist%26ref%3Dqzone%26v6%3D1&uin=" + QQ + "&topicId=" + QQnumber + "_" + blogid + "&commentId=" + QQid + "&content=" + ce.ToGB2312(content) + "&g_tk=" + m_g_tk + "&ref=qzone&json=1&inCharSet=gbk&outCharSet=gbk&format=fs&iNotice=1";
            return h.HttpSendData(url, "post", data, "gb2312");
        }
        /// <summary>
        /// 赞日志
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <param name="isLike">赞/取消赞</param>
        /// <returns></returns>
        public string BlogLike(string QQnumber,string blogid ,bool isLike=true)
        {string url ;
            if (isLike==true)
                url = "http://w.qzone.qq.com/cgi-bin/likes/internal_dolike_app?g_tk=" + m_g_tk;
        else
              url = "http://w.qzone.qq.com/cgi-bin/likes/internal_unlike_app?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQnumber + "%23%21app%3D2%26via%3DQHashRefresh%26pos%3D" + blogid + "&opuin=" + QQ + "&unikey=http%3A%2F%2Fuser.qzone.qq.com%2F" + QQnumber + "%2Fblog%2F" + blogid + "&curkey=http%3A%2F%2Fuser.qzone.qq.com%2F958796636%2Fblog%2F" + blogid + "&from=2&fupdate=1";
            return h.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 日志访客
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <returns></returns>
        public string BlogVisitor(string QQnumber,string blogid)
        {
            string url = "http://g.qzone.qq.com/cgi-bin/friendshow/cgi_get_visitor_simple?uin="+QQnumber+"&mask=2&mod=1&contentid="+blogid+"&fupdate=1&g_tk="+m_g_tk;
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 转载日志
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <param name="MyCategory">我的日志分类</param>
        /// <returns></returns>
        public string BlogQuote(string QQnumber, string blogid, string MyCategory = "个人日记")
        {//转载
            string url = "http://" + BlogRouteInit(QQnumber) + ".qzone.qq.com/cgi-bin/blognew/quote_blog?ref=qzone&g_tk=" + m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fb1.qzone.qq.com%2Fcgi-bin%2Fblognew%2Fblog_output_data%3Fuin%3D"+QQnumber+"%26blogid%3D"+blogid+"%26styledm%3Dctc.qzonestyle.gtimg.cn%26imgdm%3Dctc.qzs.qq.com%26bdm%3Db.qzone.qq.com%26mode%3D2%26numperpage%3D15%26blogseed%3D0.5354686784442171%26property%3DGoRE%26timestamp%3D1355805015%26dprefix%3D%26g_tk%3D"+m_g_tk+"%26ref%3Dqzone%26v6%3D1%26entertime%3D1355805014533%26via%3DQHashRefresh%26pos%3D"+blogid+"&uin="+QQ+"&hostUin="+QQnumber+"&blogId="+blogid+"&cateName="+ce.ToGB2312 (MyCategory)+"&rightType=1&force=0&source=34&g_tk="+m_g_tk+"&ref=qzone&json=1&inCharSet=gbk&outCharSet=gbk&format=fs&iNotice=1&secverifykey=28Q1206"+QQ+"_"+QQnumber;
        return h.HttpSendData (url,"post",data,"gb2312");
        }
        /// <summary>
        /// 分享日志
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="blogid">日志ID</param>
        /// <param name="content">分享理由</param>
        /// <returns></returns>
        public string BlogShare(string QQnumber,string blogid,string content)
        {
            string url = "http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshare_save?g_tk="+m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fapp%2Fqzshare%2Fpopup.html&notice=1&fupdate=1&platform=qzone&token="+m_g_tk+"&auto=0&type=blog&description="+ce.ToUTF8(content)+"&share2weibo=0&onekey=0&comment=0&entryuin="+QQ+"&spaceuin="+QQnumber+"&id="+blogid+"&sendparam=";
            return h.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 相册访客
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="Albumid">相册ID</param>
        /// <returns></returns>
        public string PhotoVisitor(string QQnumber,string Albumid)
        {
            return h.HttpSendData("http://g.qzone.qq.com/cgi-bin/friendshow/cgi_get_visitor_simple?uin="+QQnumber+"&mask=2&mod=2&contentid="+Albumid+"&fupdate=1&g_tk=" + m_g_tk);

        }
        /// <summary>
        /// 获取相册列表信息
        /// </summary>
        /// <param name="QQnumber"></param>
        /// <param name="isJson"></param>
        /// <returns></returns>
        public string PhotoAlbum(string QQnumber,bool isJson=true)
        {
            string url;
            if (isJson == true)
                url = "http://" + PhotoRoute(QQnumber) + "/fcgi-bin/fcg_list_album_v2?inCharSet=gbk&outCharSet=gbk&hostUin=" + QQnumber + "&notice=0&callbackFun=&format=jsonp&plat=qzone&source=qzone&appid=4&uin=" + QQ + "&t=0.9521921327099081&g_tk=" + m_g_tk;
            else
                url = "http://alist.photo.qq.com/fcgi-bin/fcg_list_album?uin=" + QQnumber + "&g_tk=" + m_g_tk;
                return h.HttpSendData(url, "Get", "", "gb2312");
         
                
        }
      /// <summary>
        /// 获取某一相册的照片列表信息
      /// </summary>
      /// <param name="QQnumber">对方</param>
      /// <param name="AlbumID">相册ID</param>
      /// <param name="pageIndex">第几页</param>
      /// <param name="pageSize">每页显示照片数量</param>
      /// <returns></returns>
        public string PhotoList(string QQnumber, string AlbumID,int pageIndex=1,int pageSize=12)
        {  
            return  h.HttpSendData("http://" + PhotoRoute(QQnumber) + "/fcgi-bin/cgi_list_photo?t=196273242&mode=0&idcNum=3&hostUin=" + GetMyQQ() + "&topicId=" + AlbumID + "&noTopic=0&uin=" + QQnumber + "&pageStart=" + (pageIndex - 1) * pageSize + "&pageNum=" + pageSize + "&skipCmtCount=0&singleurl=1&batchId=&notice=0&appid=4&inCharset=utf-8&outCharset=utf-8&source=qzone&plat=qzone&outstyle=json&format=jsonp&json_esc=1&callbackFun=shine0&g_tk=" + m_g_tk, "Get");
        }
        /// <summary>
        /// 回答问题进入某相册
        /// </summary>
        /// <param name="QQnumber">相册QQ</param>
        /// <param name="albumid">相册ID</param>
        /// <param name="question">相册问题</param>
        /// <param name="answer">相册答案</param>
        /// <returns></returns>
        public string PhotoPass(string QQnumber,string albumid,string question,string answer)
        {
            string url = "http://photo.qq.com/cgi-bin/common/cgi_view_album_v2?inCharSet=gbk&outCharSet=gbk&hostUin=" + QQnumber + "&notice=0&callbackFun=&format=jsonp&plat=qzone&source=qzone&appid=4&uin=" + QQ + "&albumId=" + albumid + "&singleUrl=1&t=0.6463017842229011&verifycode=&question=" + ce.ToUTF8(question) + "&answer=" + ce.MD5(ce.ToUTF8 (answer,false,true)) + "&g_tk=" + m_g_tk;
            
            return h.HttpSendData(url,"Get","","gb2312");
        }
       /* /// <summary>
        /// 相册
        /// </summary>
        /// <param name="albumid">相册ID</param>
        /// <returns></returns>
        public string PhotoAnswer(string albumid)
        {
            string url = "http://photo.qq.com/cgi-bin/common/cgi_get_albumqa_v2?inCharSet=gbk&outCharSet=gbk&hostUin=" + QQ + "&notice=0&callbackFun=&format=jsonp&plat=qzone&source=qzone&appid=4&uin=" + QQ + "&albumid=" + albumid + "&g_tk=" + m_g_tk + "&t=0." + CFun.GetRnd();
        return h.HttpSendData (url,"Get","","gb2312");
        }
        */
        /// <summary>
        /// 相册域名
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public string PhotoRoute(string QQnumber)
        {
            if (pr.QQnumber == QQnumber)
                return pr.route;
            string jsondata = h.HttpSendData("http://route.store.qq.com/GetRoute?UIN="+QQnumber +"&type=json&version=2&json_esc=1&g_tk="+this.m_g_tk );
            CData.json json = new CData.json(jsondata);
            pr.QQnumber = QQnumber;
          pr.route = json.GetValue(json.GetValue("default") + ".s");
          return pr.route;
        }
        /// <summary>
        /// 获取装扮验证码图片
        /// </summary>
        /// <returns></returns>
        public Image GetDressVC()
        {
            return h.GetImage("http://captcha.qq.com/getimage?aid=8000102&r=" + CFun.GetRnd(10));
        }

        private bool DressLoad(string QQNumber,bool AllowReTry=false)
      {
          if (QQNumber == strDressCurQQ&&AllowReTry==false) return true;
            try
            {
                string html = h.HttpSendData("http://user.qzone.qq.com/" + QQNumber );
                //风格颜色
                int pt1 = html.IndexOf("g_StyleID=");
                int pt2 = html.IndexOf("g_fullMode=");
                string len = html.Length.ToString ();
                styleid = html.Substring(pt1 + 14, pt2 - pt1 - 16);
 
                 
                int p1 = html.IndexOf("g_Dressup");
                int p2 = html.IndexOf(",iv");
                string strCodeData = html.Substring(p1 + 10, p2 - p1 - 13);

                int ip1 = strCodeData.IndexOf("items");
                int wp2 = strCodeData.IndexOf("windows");
                string strItems = strCodeData.Substring(ip1, wp2 - ip1 - 4);
                string[] aItems = CFun.Split(strItems, "},{");

                string strWindows = strCodeData.Substring(wp2 + 10);
                string[] aWindows = CFun.Split(strWindows, "},{");

                items = new Items[aItems.GetUpperBound(0) + 1];
                windows = new Windows[aWindows.GetUpperBound(0) + 1];
                for (int i = 0; i <= aItems.GetUpperBound(0); i++)
                {
                    //Application.DoEvents();
                    string[] tmp = aItems[i].Split(',');

                    items[i].type = tmp[0].Substring(tmp[0].IndexOf("type:") + 5);
                    items[i].itemno = tmp[1].Substring(tmp[1].IndexOf("itemno:") + 7);
                    items[i].posx = tmp[2].Substring(tmp[2].IndexOf("posx:") + 5);
                    items[i].posy = tmp[3].Substring(tmp[3].IndexOf("posy:") + 5);
                    items[i].posz = tmp[4].Substring(tmp[4].IndexOf("posz:") + 5);
                    items[i].height = tmp[5].Substring(tmp[5].IndexOf("height:") + 7);
                    items[i].width = tmp[6].Substring(tmp[6].IndexOf("width:") + 6);
                    items[i].flag = tmp[7].Substring(tmp[7].IndexOf("flag:") + 5);
                    //  MessageBox.Show(items[i].type);

                }

                for (int i = 0; i <= aWindows.GetUpperBound(0); i++)
                {
                    //Application.DoEvents();
                    string[] tmp = aWindows[i].Split(',');
                    windows[i].appid = tmp[0].Substring(tmp[0].IndexOf("appid:") + 6);
                    windows[i].mode = tmp[1].Substring(tmp[1].IndexOf("mode:") + 5);
                    windows[i].posx = tmp[2].Substring(tmp[2].IndexOf("posx:") + 5);
                    windows[i].posy = tmp[3].Substring(tmp[3].IndexOf("posy:") + 5);
                    windows[i].posz = tmp[4].Substring(tmp[4].IndexOf("posz:") + 5);
                    windows[i].height = tmp[5].Substring(tmp[5].IndexOf("height:") + 7);
                    windows[i].width = tmp[6].Substring(tmp[6].IndexOf("width:") + 6);
                    windows[i].wndid = tmp[7].Substring(tmp[7].IndexOf("wndid:") + 6);
                    //MessageBox.Show(windows[i].appid );
                }
                DressModeData(QQNumber);
                strDressCurQQ = QQNumber;
            }
            catch { return false; }
            return items.Length > 0 || windows.Length>0;
       }
       /// <summary>
       /// 模块数量
       /// </summary>
       /// <param name="QQNumber">好友QQ</param>
       /// <returns></returns>
        public int DressModeCount(string QQNumber)
        {
            if (QQNumber!=strDressCurQQ)
            DressLoad(QQNumber); 
           return ModeIndex.Length;
        }
        /// <summary>
        /// 装扮数据
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public string DressModeData(string QQnumber)
        {//加载模块数据，将已经装扮的id和模块数据对应，将模块数据下标储存在ModeIndex[]
            string html = h.HttpSendData("http://b.cnc.qzone.qq.com/cgi-bin/custom/get_custom_window.cgi?uin=" + QQnumber + "&type=qzml&&t=0.9199468369090699&g_tk=" + g_tk(),"Get","","gb2312");
            string[] aMode = CFun.Split(html, "],");//大块数据
            //
            modedata = new ModeData[aMode.Length];//最终数据结构 id data
            for (int i = 0; i <= aMode.GetUpperBound(0); i++)
            {
                //Application.DoEvents();
                int p1, p2;
                if (aMode[i].Substring(0, 4) == "call")
                {
                    p1 = 11;
                    p2 = aMode[i].IndexOf(":") - 11;

                }
                else
                {
                    p1 = 0;
                    p2 = aMode[i].IndexOf(":");
                }

                modedata[i].id = aMode[i].Substring(p1, p2).Trim ().Replace("\r", "").Replace("\n", "");//加载模块id
                int pd1, pd2;
                pd1 = aMode[i].IndexOf("'") + 1;
                pd2 = aMode[i].IndexOf("'", aMode[i].IndexOf("'") + 1);
                modedata[i].data = aMode[i].Substring(pd1, pd2 - pd1).Replace("\\n","");//加载data模块数据
                


            }
            ModeIndex = new int[windows.Length];//用于储存模块数据下标
            for (int i = 0; i <= windows.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= modedata.GetUpperBound(0); j++)
                {
                    //Application.DoEvents();
                    if (windows[i].wndid == modedata[j].id)
                    {
                        ModeIndex[i] = j;
                    }
                }
            }
            return html;
        }
       /// <summary>
       /// 增加装扮模块
       /// </summary>
       /// <param name="Index"></param>
       /// <param name="VCode"></param>
       /// <returns></returns>
        public string DressAddMode(int Index,string VCode)
       { 
           return h.HttpSendData("http://b.qzone.qq.com/cgi-bin/custom/add_custom_window.cgi?g_tk=" + g_tk(), "post", "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F" + GetMyQQ() + "%2Fmain%2Fcustom%2Fcommon&qzml=" + ce.ToGB2312(modedata[ModeIndex[Index]].data) + "&uin=" + GetMyQQ() + "&verifycode="+VCode +"&wndid=&g_tk=" + g_tk(),"gb2312"); 
       }
        /// <summary>
        /// 克隆装扮代码
        /// </summary>
        /// <param name="QQNumber"></param>
        /// <returns></returns>
       private string DressCopyCode(string QQNumber)
       {
           bool isLoad = DressLoad(QQNumber);
           if (!isLoad) return "对方的装扮加载失败！请确认该QQ空间权限是否为对所有人可见";
           return DressSave();
       }
        /// <summary>
        /// 装扮保存
        /// </summary>
        /// <returns></returns>
        public string DressSave()
        { 
            string ItemsData = "";
            string ItemnoData = "";
            string newitems = "";
            string winData="";
            for (int i = 0; i <= items.GetUpperBound(0); i++)
            {
                int j = 0;
                j = items.GetUpperBound(0) - i;
                //height width反过来
                ItemsData += items[j].type + "_" + items[j].itemno + "_" + items[j].posx + "_" + items[j].posy + "_" + items[j].posz + "_" + items[j].width + "_" + items[j].height + "_" + items[j].flag + "%7C";
                ItemnoData += items[i].itemno + "_";
                newitems += items[j].type + "_" + items[j].itemno + "%2C";
            }
            //“保存数据包”中的windlist=
            for (int k=0;k<=windows.GetUpperBound (0);k++)
            {//height width反过来
                winData += windows[k].appid + "_" + windows[k].mode + "_" + windows[k].posx + "_" + windows[k].posy + "_" + windows[k].posz + "_" + windows[k].width + "_" + windows[k].height +"_"+(k+1) +"%7C";
            }
            ItemsData = ItemsData.Substring(0, ItemsData.Length - 3);
            newitems = newitems.Substring(0, newitems.Length - 3);
            ItemnoData = ItemnoData.Substring(0, ItemnoData.Length - 1);
            winData = winData.Substring(0, winData.Length - 3);
            string AllData = "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F" + GetMyQQ() + "%2Fmain%2Fmall%3FtarGet%3Dhome%26style%3D0%26tryOnItems%3D" + ItemnoData + "&uin=" + GetMyQQ() + "&bstyle=0&styleid=" + styleid + "&scene=9&framestyle=0&mode=5&icstyle=0&ishidetitle=0&xpos=0&ypos=0&diystyle=0&inshop=1&transparence=0&diytitle=&isBGScroll=0&iweekday=0&neatstyle=0&needfeeds=1&np=1&itemlist=" + ItemsData + "&newitems=" + newitems + "&windlist="+winData+"&diyitemlist=&fupdate=1";
             return h.HttpSendData("http://w.qzone.qq.com/cgi-bin/scenario/save_scenario_v6?g_tk=" + g_tk(), "post", AllData, "gb2312");
        }
        /// <summary>
        /// 删除装扮模块
        /// </summary>
        /// <param name="ModeCount"></param>
        public void DressDelMode(int ModeCount)
        {
            for (int i = 1; i <= ModeCount; i++)
            {
                //Application.DoEvents();
                h.HttpSendData("http://b.qzone.qq.com/cgi-bin/custom/del_custom_window.cgi?g_tk=" + m_g_tk, "post", "qzreferrer=http%3A%2F%2Fuser.qzone.qq.com%2F" + GetMyQQ() + "%2Fmain%2Fcustom%2Fcommon%23home&uin=" + GetMyQQ() + "&wndid=" + i + "&g_tk=" + m_g_tk); 
            }
        }
        /// <summary>
        /// 获取音乐列表
        /// </summary>
        /// <param name="QQnumber"></param>
        /// <returns></returns>
        public string MusicGet(string QQnumber)
        {
            return h.HttpSendData("http://qzone-music.qq.com/fcg-bin/cgi_playlist_xml.fcg?json=2&uin=" + QQnumber + "&g_tk=" + g_tk(),"Get","","gb2312");
        }
        /// <summary>
        /// 添加音乐
        /// </summary>
        /// <param name="SongName">歌曲名</param>
        /// <param name="Songer">歌手名</param>
        /// <param name="URL">歌曲URL</param>
        /// <returns>是否添加成功</returns>
        public bool MusicAdd(string SongName, string Songer, string URL)
        {
            string ret = h.HttpSendData("http://qzone-music.qq.com/fcg-bin/favnetsongtobgm.fcg?g_tk=" + g_tk(), "post", "formsender=1&source=102&url=" + ce.ToGB2312(URL) + "&songtitle=" + ce.ToGB2312(SongName) + "&singer=" + ce.ToGB2312(Songer) + "&desc=", "gb2312");
            if (ret.IndexOf("code:0") > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 删除音乐
        /// </summary>
        /// <param name="id">要删除的音乐ID</param>
        /// <returns></returns>
        public bool MusicDel(string id)
        {
            string ret = h.HttpSendData("http://qzone-music.qq.com/fcg-bin/fcg_playlist_delsongs.fcg?g_tk=" + g_tk(), "post", "formsender=1&source=102&uin=" + GetMyQQ() + "&ids=" + id + "&types=1", "gb2312");
            if (ret.IndexOf("code:0") > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 是否随机播放
        /// </summary>
        /// <param name="isRandom">是否随机播放</param>
        /// <returns>返回是否设置成功</returns>
        public bool MusicIsRandom(bool isRandom)
        {
            int i;
            if (isRandom)
                i = 1;
            else
                i = 0;
            string ret = h.HttpSendData("http://qzone-music.qq.com/cgi-bin/v5/cgi_music_randomplay?uin="+GetMyQQ()+"&ranplay=" + i + "&out=1&g_tk=" + g_tk(),"Get","","gb2312");
            if (ret.IndexOf("code:0") > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 发送礼物
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="GiftID">礼物ID</param>
        /// <param name="Title">礼物标题</param>
        /// <param name="Content">祝福语</param>
        /// <returns></returns>
        public string GiftSend(string QQnumber,string GiftID,string Title,string Content)
        {
            string isPrivate="0";
            string SendTime="";
            string url = "http://drift.qzone.qq.com/cgi-bin/sendagift?g_tk="+m_g_tk ;
            string data = "qzreferrer=http%3A%2F%2Fctc.qzs.qq.com%2Fqzone%2Fgift%2Fsend_list.html%3Fuin%3D%26type%3D%26birthday%3D%26birthdaytab%3D0%26lunarFlag%3D0%26source%3D%26nick%3D%26giveback%3D%23html%3Dsend_list&fupdate=1&random=0.3457676704044410&charSet=utf-8&uin=" + QQ + "&tarGetuin=" + QQnumber + "&source=0&nick=&giftid=" + GiftID + "&private=" + isPrivate + "&giveback=&qzoneflag=1&time=" + SendTime + "&timeflag=0&giftmessage=" + Content + "&gifttype=0&gifttitle=" + Title + "&newadmin=1&birthdaytab=0&answerid=&arch=0&clicksrc=&verifycode=%5Bobject%5D";
            
            return h.HttpSendData(url,"post",data,"gb2132");
        }
        /// <summary>
        /// 获取礼物列表
        /// </summary>
        /// <returns></returns>
        public string GiftList()
        {
            string url = "http://ctc.qzonestyle.gtimg.cn/qzone/mall/static/giftitem/gift_556.js";
            return h.HttpSendData(url,"Get","","gb2132");
        }
        /// <summary>
        /// 获取礼物图片
        /// </summary>
        /// <param name="GiftID">礼物ID</param>
        /// <param name="isPNG">是否为PNG/GIF</param>
        /// <returns></returns>
        public Image GiftImage(string GiftID,bool isPNG=true)
        {
        string url1 = "http://ctc.qzonestyle.gtimg.cn/qzone/space_item/pre/" + Convert.ToInt32(GiftID) % 16 + "/" + GiftID + "_1.gif";
        string url2 = "http://ctc.qzonestyle.gtimg.cn/qzone/space_item/pre/" + Convert.ToInt32(GiftID) % 16 + "/" + GiftID + "_1.png";
        if (isPNG )
            return h.GetImage(url2);
        else
            return h.GetImage(url1);
        }
        /// <summary>
        /// 获取高清QQ头像
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <returns></returns>
        public Image GetHeadImageHD(string QQnumber)
        {
            return h.GetImage("http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin="+QQnumber+"src_uin="+QQnumber+"&fid=190807956&spec=100&url_enc=0&referer=bu_interface&term_type=pc");
        }
        public string SetQzoneName(string name)
        { 
            string url = "http://w.qzone.qq.com/cgi-bin/user/cgi_apply_updateuserinfo_new?g_tk=" + m_g_tk;
            string data = "qzreferrer=http%3A%2F%2Frc.qzone.qq.com%2Fblog%2Fadd%23%21app%3D2%26via%3DQZ.HashRefresh%26pos%3D1382612232&uin=" + QQ + "&spacename=" + ce.ToUTF8(name) + "&desc=undefined&mb=2048&pageindex=3&fupdate=1";
            return h.HttpSendData(url, "post", data);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string FriendRelationship(string QQnumber)
        {
            string url = "http://ocial.show.qq.com/cgi-bin/qqshow_camera_noname?g_tk=" + m_g_tk;
            string data = "uin=" + QQnumber + "%7C" + GetMyQQ() + "&friend=1";
            h.SetReferer("http://social.show.qq.com/qqshow_xhr.html");
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 分享网页
        /// </summary>
        /// <param name="shareURL">要分享的网页http开头的</param>
        /// <param name="description">分享的描述信息</param>
        /// <param name="title">分享的标题，默认自动获取</param>
        /// <param name="summary">分享的简介，默认自动获取</param>
        /// <returns></returns>
        public string shareURL(string shareURL, string description, bool share2weibo=true,string title="", string summary="")
        {
            if (title.Length == 0)
            {
                string html = h.HttpSendData(shareURL);
                int p1 = html.IndexOf("<title>")+7, p2 = html.IndexOf("</title>")-7;
                if (p2 > p1)
                {
                    title = html.Substring(p1, p2-p1);
                }
            }
            if (summary.Length == 0)
            {
                string html = h.HttpSendData("http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshareget_urlinfo?fupdate=1&random=0&url="+ce.ToUTF8(shareURL)+"&g_tk="+m_g_tk);
                int p1 = html.IndexOf("summary\":") + 10, p2 = html.IndexOf("\"type") - 12;
                if (p2 > p1)
                {
                    summary = html.Substring(p1, p2 - p1);
                }
            }
        
            string url = "http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshareadd_url?g_tk=" + m_g_tk;
            string data = "where=0&entryuin=" + GetMyQQ() + "&spaceuin=" + GetMyQQ() + "&title=" + ce.ToUTF8(title) + "&summary=" + ce.ToUTF8(summary) + "&token=" + m_g_tk + "&sendparam=&description=" + ce.ToUTF8(description) + "&type=4&url=" + ce.ToUTF8(shareURL) + "&site=&to=&share2weibo=" + Convert.ToInt32(share2weibo) + "&pics=http://www.baidu.com/img/baidu_jgylogo3.gif&fupdate=1&notice=1";
            h.SetReferer("http://sns.qzone.qq.com/cgi-bin/qzshare/cgi_qzshare_onekey?url="+ ce.ToUTF8(shareURL)+"&title=&desc=&summary=&site=");
            return summary+h.HttpSendData(url, "post", data);
        }
    }
#endregion

}



