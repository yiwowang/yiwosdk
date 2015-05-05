using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
namespace yiwoSDK
{
    public class CQQData
    {   
        private string strErrHead = "易我论坛提示错误：";
        private string m_g_tk = "";
        private string m_skey = "";
        private string m_basekey = "";
        CHttpWeb h = new CHttpWeb();
        CEncode ce = new CEncode();
        string QQ = "";
        public string GetMyQQ()
        { return QQ; }
        /// <summary>
        /// 设置使用的网络对象
        /// </summary>
        /// <param name="ch">CHttpWeb类的对象</param>
        public void SetHttpWeb(CHttpWeb ch)
        { h = ch; }
        /// <summary>
        /// 登陆QQ中心
        /// </summary>
        /// <param name="QQnumber">QQ账号</param>
        /// <param name="password">QQ密码</param>
        /// <param name="VCode">验证码</param>
        /// <returns></returns>
        public string Login(string QQnumber, string password, string VCode)
        {
            if (!CFun.IsQQnumber(ref QQnumber)) return strErrHead + "QQ号码长度错误或有非法字符";
            if (!CFun.IsQQpassword(ref password)) return strErrHead + "QQ密码长度错误";
            if (!(VCode.Length == 4)) return strErrHead + "QQ验证码长度错误";
            string URL = "http://ptlogin2.qq.com/login?u=" + QQnumber +
                 "&verifycode=" + VCode.ToUpper() +
                 "&p=" + this.GetP(QQnumber, password, VCode.ToUpper()) +
                 "&pt_rsa=0&ptredirect=1&u1=http%3A%2F%2Fid.qq.com%2Findex.html&h=1&t=1&g=1&from_ui=1&ptlang=2052&action=5-46-1397972090023&js_ver=10076&js_type=1&login_sig=7nBn3Y0FzH8UwcMKLO5nQ9ee9tXwlLrn4LcmsYXEtdItSky*kGU8y*vrX98bjULd&aid=1006102&daid=1&";
            /* string URL = "http://ptlogin2.qq.com/login?ptlang=2052&u=" + QQnumber +
                 "&p=" + this.GetP(QQnumber, password, VCode.ToUpper()) +
                 "&verifycode=" + VCode.ToUpper() +
                 "&css=http://imgcache.qq.com/ptcss/b2/qzone/15000101/style.css&mibao_css=m_qzone&aid=15000101&u1=http%3A%2F%2Fimgcache.qq.com%2Fqzone%2Fv5%2Floginsucc.html%3Fpara%3Dizone&ptredirect=1&h=1&from_ui=1&dumy=&fp=loginerroralert&action=2-10-44469&g=1&t=1&dummy=";
             */
            h.SetReferer("http://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=1006102&daid=1&style=23&hide_border=1&proxy_url=http%3A%2F%2Fid.qq.com%2Flogin%2Fproxy.html&s_url=http://id.qq.com/index.html");
            string strResult = h.HttpSendData(URL);
            if (strResult.IndexOf("('0'") >= 0)
            {
                QQ = QQnumber;
                this.m_skey = this.skey();
                this.m_g_tk = this.g_tk(this.m_skey);

                h.HttpSendData(strResult.Substring(strResult.IndexOf("http"), strResult.IndexOf("登录成功") - strResult.IndexOf("http") - 7));

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
            return h.GetImage("http://captcha.qq.com/getimage?aid=1006102&uin=" + QQnumber + "&r=0." + CFun.GetRnd());
        }
        /// <summary>
        /// 获取默认验证码或验证码图片
        /// </summary>
        /// <param name="QQnumber">欲登陆的QQ</param>
        /// <returns></returns>
        public string GetLoginVC(string QQnumber)
        {
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");
            if (!CFun.IsQQnumber(ref QQnumber)) return strErrHead + "QQ号码长度错误或有非法字符";
            string verifyCode = "";

            string tmp = h.HttpSendData("http://check.ptlogin2.qq.com/check?regmaster=&uin=" + QQnumber + "&appid=1006102&js_ver=10076&js_type=1&login_sig=7nBn3Y0FzH8UwcMKLO5nQ9ee9tXwlLrn4LcmsYXEtdItSky*kGU8y*vrX98bjULd&u1=http%3A%2F%2Fid.qq.com%2Findex.html&r=0." + CFun.GetRnd());
            if (tmp.IndexOf("('0'") > 0)
                verifyCode = tmp.Substring(18, 4);
            else
                verifyCode = "http://captcha.qq.com/getimage?aid=1006102&uin=" + QQnumber + "&r=0." + CFun.GetRnd();
            return verifyCode;
        }
        /// <summary>
        /// 获取g_tk
        /// </summary>
        /// <param name="skey">skey值</param>
        /// <returns>返回g_tk</returns>
        public string g_tk(string skey = "")
        {
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");

            if (this.m_g_tk.Length > 0 && skey.Length == 0) return this.m_g_tk;

            int hash = 5381;
            char[] c = skey.ToCharArray();
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
        public string skey(string cookie = "")
        {
            if (this.m_skey.Length > 0 && cookie.Length < 10) return this.m_skey;
            cookie = h.GetCookie();
            int p = cookie.IndexOf("skey");
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
        public string GetP(string QQnumber, string password, string VCode)
        {
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");

            return CQQHelper.GetP(QQnumber, password, VCode);

        }
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        public string GetFriendList()
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/friends_home";
            string data = "ver=20100914&from=mars" + GetMyQQ() + "&ldw=" + m_g_tk;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取带备注的好友列表
        /// </summary>
        /// <returns></returns>
        public string GetFriendRemarkList()
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/remark";
            string data = "k=&ldw=" + m_g_tk + "&t=" + m_g_tk;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取QQ好友上线时间和与自己沟通时间
        /// </summary>
        /// <param name="QQArr">要查询的好友的QQ数组</param>
        /// <returns>c=最后一次联系是多少天内，o=最近多少天内上线</returns>
        public string GetFriendOnlineList(string[] QQArr)
        {
            if (QQArr == null) return "";
            if (QQArr.Length == 0) return "";
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/online";
            string strQQ = "";
            for (int i = 0; i <= QQArr.GetUpperBound(0); i++)
            {
                strQQ += QQArr[i] + "%2D";
            }
            strQQ = strQQ.Substring(0, strQQ.Length - 3);
            string data = "u=" + strQQ + "&k=&ldw=" + m_g_tk + "&t=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 是否开通了QQ空间
        /// </summary>
        /// <param name="QQArr">要查询的好友的QQ数组</param>
        /// <returns>zq=1开通了，zq=0没开通</returns>
        public string GetIsRegQzoneList(string[] QQArr)
        {
            if (QQArr == null) return "";
            if (QQArr.Length == 0) return "";
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/richflag";
            string strQQ = "";
            for (int i = 0; i <= QQArr.GetUpperBound(0); i++)
            {
                strQQ += QQArr[i] + "%2D";
            }
            strQQ = strQQ.Substring(0, strQQ.Length - 3);
            string data = "u=" + strQQ + "&k=&ldw=" + m_g_tk + "&t=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 获取可能认识人的资料
        /// </summary>
        /// <param name="QQArr"></param>
        /// <returns></returns>
        public string GetCanKnowRichInfo(string[] QQArr)
        {
            if (QQArr == null) return "";
            if (QQArr.Length == 0) return "";
            h.SetReferer("http://id.qq.com/possible_w/possible.html?ver=10044&mod=possible");
            string url = "http://id.qq.com/cgi-bin/richinfov2";
            string strQQ = "";
            for (int i = 0; i <= QQArr.GetUpperBound(0); i++)
            {
                strQQ += QQArr[i] + "-";
            }
            strQQ = strQQ.Substring(0, strQQ.Length - 1);
            string data = "&ldw=" + GetBaseKey() + "&t=4&xy=1&u=" + strQQ;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 建议备注
        /// </summary>
        /// <returns></returns>
        public string GetFriendRemarkBatList()
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/remark_bat";
            string data = "k=&ldw=" + m_g_tk + "&t=" + m_g_tk;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取好友头像
        /// </summary>
        /// <param name="QQNumber"></param>
        /// <returns></returns>
        public Image GetQQFace(string QQNumber)
        {
            return h.GetImage("http://id.qq.com/cgi-bin/face?u=" + QQNumber + "&k=104");
        }
        /// <summary>
        /// 详细资料
        /// </summary>
        /// <param name="QQNumber"></param>
        /// <returns>overlap=共同好友数量</returns>
        public string GetQQRichInfo(string QQNumber)
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/richinfo";
            string data = "u=" + QQNumber + "&ldw=" + m_g_tk + "&t=1";
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 获取群卡片信息
        /// </summary>
        /// <param name="QQNumber"></param>
        /// <returns>c=备注，n=所在的群</returns>
        public string GetGroupCard(string QQNumber)
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/groups_card";
            string data = "u=" + QQNumber + "&ldw=" + m_g_tk + "&k=";
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 设置是否对某好友隐身可见，以及设置是否在线对其隐身
        /// </summary>
        /// <param name="QQNumber"></param>
        /// <param name="IsHiddenCanSee">是否隐身对其可见</param>
        /// <param name="IsOnlineCanNotSee">是否在线对其隐身</param>
        /// <returns></returns>
        public string SetHiddenVisible(string QQNumber, bool IsHiddenCanSee, bool IsOnlineCanNotSee)
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/visible";
            string data = "ldw=" + m_g_tk + "&s=" + (Convert.ToUInt32(IsHiddenCanSee) + 1) + "&fl=" + (Convert.ToUInt32(IsOnlineCanNotSee) + 1) + "&f=" + QQNumber + "&k=";
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 获取单项好友列表，对方有我，我没他
        /// </summary>
        /// <returns>t=具体删除的时间戳，u=单向好友QQ</returns>
        public string GetSingleList()
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/odd";
            string data = "k=&ldw=" + m_g_tk + "&t=" + m_g_tk;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取需要恢复的好友列表，是删除的好友列表
        /// </summary>
        /// <returns></returns>
        public string GetBaseKey()
        {
            if (m_basekey.Length == 0)
            {
                h.SetReferer("http://id.qq.com/");
                h.HttpSendData("http://id.qq.com/cgi-bin/get_base_key?r=0." + CFun.GetRnd());
                m_basekey = h.GetCookie().Substring(h.GetCookie().IndexOf("ldw=") + 4, 48);
            }
            return m_basekey;

        }
        /// <summary>
        /// 好友分组
        /// </summary>
        /// <returns></returns>
        public string GetFirendCategory()
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/groups";
            string data = "ldw=" + GetBaseKey();
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 获取个人信息
        /// </summary>
        /// <returns></returns>
        public string GetMySummaryInfo()
        {
            h.SetReferer("http://id.qq.com/home/home.html?ver=10049&");
            return h.HttpSendData("http://id.qq.com/cgi-bin/summary?ldw=" + GetBaseKey() + "&r=0." + CFun.GetRnd());

        }
        /// <summary>
        /// 获取个人信息
        /// </summary>
        /// <returns></returns>
        public string GetMyInfo()
        {
            h.SetReferer("http://id.qq.com/home/home.html?ver=10049&");
            return h.HttpSendData("http://id.qq.com/cgi-bin/userinfo?ldw=" + GetBaseKey() + "&r=0." + CFun.GetRnd());

        }

        /// <summary>
        /// 可能认识的好友
        /// </summary>
        /// <returns>c=共同好友数量</returns>
        public string GetCanKnow()
        {
            h.SetReferer("http://id.qq.com/home/home.html?ver=10049&");
            return h.HttpSendData("http://id.qq.com/cgi-bin/possiblev2?ldw=" + GetBaseKey() + "&r=0." + CFun.GetRnd());

        }
        /// <summary>
        /// 安全信息
        /// </summary>
        /// <returns></returns>
        public string GetSafeInfo()
        {
            h.SetReferer("http://id.qq.com/info/index.html?source=id&from=id");
            return h.HttpSendData("http://id.qq.com/cgi-bin/safe_info?ldw=" + GetBaseKey() + "&r=0." + CFun.GetRnd());
        }
        /// <summary>
        /// 好友动态
        /// </summary>
        /// <returns></returns>
        public string GetFeed(int num = 100)
        {
            h.SetReferer("http://id.qq.com/info/index.html?source=id&from=id");
            return h.HttpSendData("http://id.qq.com/cgi-bin/feed?v=0." + CFun.GetRnd() + "&ut=0&ft=0&fc=" + num + "&ldw=" + GetBaseKey() + "&ntime=0&nuin=0");
        }
        bool isget = false, iscreate = false;
        private void InitGetQunRquest()
        {
            if (!isget)
            {
                isget = true;
                h.HttpSendData("http://ptlogin2.qq.com/pt4_web_jump?pt4_token=c-GbMzuXY8Nh7xkXbCSM0A__&daid=73&appid=715021405&succ_url=http%3A%2F%2Fadmin.qun.qq.com%2Fdismiss%2Ftransfer.html%3Fmod%3Dpossible");

            }
        }
        private void InitCreateQunRquest()
        {
            if (!iscreate)
            {
                isget = true;
                h.HttpSendData("http://ptlogin2.qq.com/pt4_web_jump?pt4_token=c-GbMzuXY8Nh7xkXbCSM0A__&daid=73&appid=715021405&succ_url=http%3A%2F%2Fadmin.qun.qq.com%2Fcreate%2Findex.html%3Fmod%3Dpossible");

            }
        }
        /// <summary>
        /// 获取群列表
        /// </summary>
        /// <returns></returns>
        public string GetQunList()
        {
            InitGetQunRquest();
            h.SetReferer("http://admin.qun.qq.com/dismiss/transfer.html?mod=possible");
            return h.HttpSendData("http://admin.qun.qq.com/cgi-bin/qun_admin/get_group_list");
        }
        /// <summary>
        /// 获取群成员列表
        /// </summary>
        /// <param name="QunNumber">群号</param>
        /// <returns></returns>
        public string GetQunMemberList(string QunNumber)
        {
            InitGetQunRquest();
            h.SetReferer("http://admin.qun.qq.com/dismiss/transfer.html?mod=possible");
            string url = "http://admin.qun.qq.com/cgi-bin/qun_admin/get_group_members";
            string data = "gc=" + QunNumber + "&bkn=" + m_g_tk;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取简洁的好友列表
        /// </summary>
        /// <returns></returns>
        public string GetFriendListSample()
        {
            InitCreateQunRquest();
            h.SetReferer("http://admin.qun.qq.com/create/index.html?mod=possible");
            string url = "http://admin.qun.qq.com/cgi-bin/qun_admin/get_friends_list";
            string data = "&bkn=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 获取圈子列表
        /// </summary>
        /// <returns></returns>
        public string GetQuanZi()
        {
            InitCreateQunRquest();
            h.SetReferer("http://admin.qun.qq.com/create/index.html?mod=possible");
            string url = "http://admin.qun.qq.com/cgi-bin/qun_admin/quan_download";
            string data = "&bkn=" + m_g_tk;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="QunName"></param>
        /// <param name="QunText"></param>
        /// <returns>gc=群号</returns>
        public string CreateQun(string QunName, string QunText)
        {
            InitCreateQunRquest();
            h.SetReferer("http://admin.qun.qq.com/create/index.html?mod=possible");
            string url = "http://admin.qun.qq.com/cgi-bin/qun_admin/create_group";
            string data = "&gCls=10018&gn=" + ce.ToUTF8(QunName) + "&t=1&v=2&gClsTxt=" + ce.ToUTF8(QunText) + "&mn=0&m=&bkn=" + m_g_tk + "&s=1&open=0&speak=0&key=value";
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 修改群信息
        /// </summary>
        /// <param name="QunNumber">群号</param>
        /// <param name="QunName">群名</param>
        /// <param name="QunText">群介绍</param>
        /// <returns></returns>
        public string SetQunInfo(string QunNumber, string QunName, string QunText)
        {
            InitCreateQunRquest();
            h.SetReferer("http://admin.qun.qq.com/create/index.html?mod=possible");
            string url = "http://admin.qun.qq.com/cgi-bin/qun_admin/store_extra_grp_info";
            string data = "&gn=" + ce.ToUTF8(QunName) + "&v=2&gid=" + QunNumber + "&i=" + ce.ToUTF8(QunText) + "&t=&bkn=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 修改备注
        /// </summary>
        /// <param name="QQNumber"></param>
        /// <param name="RemarkName"></param>
        /// <returns></returns>
        public string SetRemark(string QQNumber, string RemarkName)
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/remark_mod";
            string data = "r=" + ce.ToUTF8(RemarkName) + "&k=&f=" + QQNumber + "&ldw=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="QQArr">好友QQ数组</param>
        /// <param name="IsDelMeFromHisList">是否从对方列表中删除自己</param>
        /// <returns></returns>
        public string DelFriend(string[] QQArr, bool IsDelMeFromHisList = true)
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/friends_del";
            string strQQ = "";
            for (int i = 0; i <= QQArr.GetUpperBound(0); i++)
            {
                strQQ += QQArr[i] + "%2D";
            }
            strQQ = strQQ.Substring(0, strQQ.Length - 3);
            string data = "u=" + strQQ + "&k=&vc=&sppkey=&t=" + (Convert.ToInt32(IsDelMeFromHisList) + 1) + "&ldw=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 移动好友到指定分组
        /// </summary>
        /// <param name="QQArr">要移动的好友</param>
        /// <param name="gid">分组ID</param>
        /// <returns></returns>
        public string MoveFriend(string[] QQArr, string gid)
        {
            h.SetReferer("http://1.url.cn/id/flash/img/friends_mgr.swf?v=10029");
            string url = "http://id.qq.com/cgi-bin/friends_move";
            string strQQ = "";
            for (int i = 0; i <= QQArr.GetUpperBound(0); i++)
            {
                strQQ += QQArr[i] + "%2D";
            }
            strQQ = strQQ.Substring(0, strQQ.Length - 3);
            string data = "u=" + strQQ + "&k=&gid=" + gid + "&ldw=" + m_g_tk;
            return h.HttpSendData(url, "post", data);

        }
    }

}
