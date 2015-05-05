
namespace yiwoSDK
{
    public class C3GQQ
    {
        CHttpWeb h = new CHttpWeb();
        /// <summary>
        /// 设置3gqq使用的CHttpWeb网络对象
        /// </summary>
        /// <param name="ch">CHttpWeb网络对象</param>
        public void SetHttpWeb(CHttpWeb ch)
        { h = ch; }
        CEncode ce = new CEncode();
        string rsid;
        string MyQQ;
        string Password;
        string LoginType;
        string SID = "";
        /// <summary>
        /// 获取sid
        /// </summary>
        /// <returns></returns>
        public string GetSID() { return SID; }
        /// <summary>
        /// 获取当前登录的QQ
        /// </summary>
        /// <returns></returns>
        public string GetMyQQ() { return MyQQ; }
        /// <summary>
        /// 登录3GQQ
        /// </summary>
        /// <param name="QQnumber">QQ</param>
        /// <param name="Password">密码</param>
        /// <param name="LoginType">登录状态</param>
        /// <returns></returns>
        public string Login(string QQnumber, string Password, string LoginType = "1")
        {
            this.MyQQ = QQnumber;
            this.Password = Password;
            this.LoginType = LoginType;
            string result = h.HttpSendData("http://pt.3g.qq.com/login?act=json", "POST",
                "pmd5=" + ce.MD5(Password.Trim()).ToUpper() + "&qq=" + QQnumber.Trim() + "&login_url=%2Fs%3Ft%3Dqzone%26bid_code%3DqzoneLogin%26fid%3D572%26aid%3DnLoginqz%26KqqWap_Act%3D3%26g_ut%3D3%26go_url%3D%2Fqzone%2F&go_url=%2Fqzone%2F&bid_code=qzoneLogin&loginType=" + LoginType);
            if (result.IndexOf("code\":\"0") >= 0)
            {
                this.SID = result.Substring(result.IndexOf("sid") + 6, 24);
                return this.SID;
            }
            if (result.IndexOf("gtimg") >= 0)
            {
                string[] a = result.Split(',');
                rsid = a[1].Substring(8, a[1].Length - 9);
                string vurl = a[2].Substring(8, a[2].Length - 9) + ".gif";
                return vurl;
            }
            return result;
        }
        /// <summary>
        /// 出现验证码时登录3GQQ
        /// </summary>
        /// <param name="verifycode"></param>
        /// <returns></returns>
        public string LoginByVC(string verifycode)
        {
            string result = h.HttpSendData("http://pt.3g.qq.com/login?act=json", "post",
                "pmd5=" + ce.MD5(this.Password.Trim()).ToUpper() + "&qq=" + MyQQ + "&login_url=%2Fs%3Ft%3Dqzone%26bid_code%3DqzoneLogin%26fid%3D572%26aid%3DnLoginqz%26KqqWap_Act%3D3%26g_ut%3D3%26go_url%3D%2Fqzone%2F&go_url=%2Fqzone%2F&bid_code=qzoneLogin&loginType=" + LoginType + "&r_sid=" + this.rsid + "&verify=" + verifycode + "&u_token=" + MyQQ);
            if (result.IndexOf("code\":\"0") >= 0)
            {
                this.SID = result.Substring(result.IndexOf("sid") + 6, 24);
                return this.SID;
            }
            return result;

        }
        /// <summary>
        /// 通过sid登录
        /// </summary>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string LoginBySID(string SID)
        {
            return h.HttpSendData("http://pt.3g.qq.com/s?aid=nLogin3gqqbysid&3gqqsid=" + SID);

        }
        /// <summary>
        /// 发送消息给好友
        /// </summary>
        /// <param name="SID">sid</param>
        /// <param name="ToQQ">好友QQ</param>
        /// <param name="Content">消息内容</param>
        /// <returns></returns>
        public string SendMsg(string SID, string ToQQ, string Content)
        {
            return h.HttpSendData("http://q32.3g.qq.com/g/s", "post", "sid=" + SID + "&aid=sendmsg&tfor=qq&referer=&msg=" + ce.ToUTF8(Content) + "&u=" + ToQQ + "&saveURL=0&do=send&on=1&saveURL=0");
        }
        /// <summary>
        /// 返回主界面
        /// </summary>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetQQMain(string SID)
        {
            return h.HttpSendData("http://q16.3g.qq.com/g/s?sid=" + SID + "&aid=nqqchatMain&on=1&g_f=1657");
        }
        /// <summary>
        /// 发送家信
        /// </summary>
        /// <param name="SID">sid</param>
        /// <param name="ToQQ">好友QQ</param>
        /// <param name="Content">家信内容</param>
        /// <returns></returns>
        public string HouseSend(string SID, string ToQQ, string Content)
        {
            return h.HttpSendData("http://house60.3g.qq.com/g/msgbox/send2.jsp?sid=" + SID + "&saveURL=0&to=" + ToQQ + "&chid=0&content=" + ce.ToUTF8(Content));
        }
        /// <summary>
        /// 发表说说
        /// </summary>
        /// <param name="SID">sid</param>
        /// <param name="Content">说说内容</param>
        /// <param name="PhoneType">移动设备类型：iphone ipad 3gqq</param>
        /// <param name="Adress">地理位置</param>
        /// <returns></returns>
        public string SendShuo(string SID, string Content, string PhoneType = "", string Adress = "")
        {
            string UserPhone = "";
            if (PhoneType.ToLower().Trim() == "iphone" || PhoneType.ToLower().Trim() == "ipad")
            {
                UserPhone = "Mozilla/5.0 (" + PhoneType + "; U; CPU " + PhoneType + " OS 3_0 like Mac OS X; en-us) AppleWebKit/528.18 (KHTML, like Gecko) Version/4.0 Mobile/7A341 Safari/528.16";
                h.SetUserAgent(UserPhone);

                if (Adress.Trim().Length != 0)
                    return h.HttpSendData("http://m.z.qq.com/" + PhoneType + "/vaction.jsp?sid=" + SID, "post", "ac=3&mst=2&buid=" + MyQQ + "&content=" + ce.ToUTF8(Content) + "&latitude=90&longitude=90&address=" + ce.ToUTF8(Adress));
                else
                    return h.HttpSendData("http://m.z.qq.com/" + PhoneType + "/vaction.jsp?sid=" + SID, "post", "ac=3&mst=2&content=" + ce.ToUTF8(Content));


            }
            else
            {

                return h.HttpSendData("http://blog60.z.qq.com/mood/mood_add_exe.jsp?sid=" + SID, "post", "content=" + ce.ToUTF8(Content) + "&B_UID=" + GetMyQQ() + "&come_from=mood_add_from_infocenter&action=1&btnSubmit=%E5%8F%91%E8%A1%A8");
            }

        }
        
        private int DoSID(ref string sid)
        {
            if (sid.Length != 24 && this.SID.Length != 24)
            {
                sid = "sid无效，请输入正确的sid，或者重新登录";
                return 1;
            }
            else if (SID.Length == 24)
            {
                sid = this.SID;
                return 0;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 空间资料
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetQzoneInfo(string QQnumber, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            return h.HttpSendData("http://blog60.z.qq.com/user_info.jsp?B_UID=" + QQnumber + "&sid=" + SID);
        }
        /// <summary>
        /// QQ资料
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetQQInfo(string QQnumber, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=findnqqUserInfo";
            string data = "u=" + QQnumber;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 设置空间资料
        /// </summary>
        /// <param name="spaceName">空间名称</param>
        /// <param name="desc">空间资料</param>
        /// <param name="sign">空间签名</param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string SetQzoneInfo(string spaceName, string desc, string sign, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;

            string url = "http://blog60.z.qq.com/user_info_update_text.jsp?sid=" + SID + "&B_UID=" + MyQQ;
            string data = "spaceName=" + ce.ToUTF8(spaceName) + "&desc=" + ce.ToUTF8(desc) + "&sign=" + ce.ToUTF8(sign);
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 设置QQ资料
        /// </summary>
        /// <param name="nick">昵称</param>
        /// <param name="signname">签名</param>
        /// <param name="gender">性别</param>
        /// <param name="age">年龄</param>
        /// <param name="phoneNo">电话</param>
        /// <param name="adress">地址</param>
        /// <param name="email">邮件</param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string SetQQInfo(string nick, string signname, string gender, string age, string phoneNo, string adress, string email, string SID = "")//0女，1男，2不公开 
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=chgQqSetting";
            string data = "nick=" + ce.ToUTF8(nick) + "&signname=" + ce.ToUTF8(signname) + "&gender=" + gender + "&age=" + age + "&phoneNo=" + phoneNo + "&address=" + ce.ToUTF8(adress) + "&email=" + ce.ToUTF8(email);
            return h.HttpSendData(url, "post", data);

        }
        /*
        /// <summary>
        /// 加好友
        /// </summary>
        /// <param name="QQnumber">对方QQ</param>
        /// <param name="verifyInfo">验证信息</param>
        /// <param name="SID">SID</param>
        /// <returns></returns>
        public string AddFriend(string QQnumber, string verifyInfo, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;

            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=addFriendVerify";
            string data = "verify=" + ce.ToUTF8(verifyInfo) + "&uin=" + QQnumber;
            return h.HttpSendData(url, "post", data) + SID;


        }
         * */
        /// <summary>
        /// 退出3GQQ
        /// </summary>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string Quit(string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://pt.3g.qq.com/s?sid=" + SID + "&aid=nLogout";
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 获取群
        /// </summary>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetFriendGroup(string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=nqqGroup";
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 获取在线好友
        /// </summary>
        /// <param name="page"></param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetFriendByOnline(int page = 1, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=nqqchatMain&p=" + page;
            return h.HttpSendData(url);

        }
        /// <summary>
        /// 获取群好友
        /// </summary>
        /// <param name="GroupID"></param>
        /// <param name="page"></param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetFriendByGroup(int GroupID = 1, int page = 1, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=nqqGrpF&name=&id=" + (GroupID + 1) + "&gindex=" + (GroupID + 1) + "&pid=" + page;
            return h.HttpSendData(url);

        }
        /// <summary>
        /// 获取最近联系好友
        /// </summary>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetFriendByRecent(string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&aid=nqqRecent";
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 获取QQ消息
        /// </summary>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string GetQQMsg(string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://q32.3g.qq.com/g/s?sid=" + SID + "&3G_UIN=" + MyQQ + "&saveURL=0&aid=nqqChat";
            return h.HttpSendData(url);
        }

        /// <summary>
        /// 访问对方空间
        /// </summary>
        /// <param name="QQnumber"></param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string QzoneVisitTo(string QQnumber, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            return h.HttpSendData("http://blog30.z.qq.com/blog_wap.jsp?sid=" + SID + "&B_UID=" + QQnumber);
        }
        /// <summary>
        /// 发表微博
        /// </summary>
        /// <param name="Content">微博内容</param>
        /// <param name="SID">sid</param>
        /// <returns></returns>
        public string WeiBoSend(string Content, string SID = "")
        {
            if (DoSID(ref SID) == 1)
                return SID;
            string url = "http://ti50.3g.qq.com/g/s?sid=" + SID + "&r=484592&aid=amsg&bid=h%23q" + MyQQ + "%230%230%230%230&rof=true&ifh=1&ngpd=false";
            string data = "msg=" + ce.ToUTF8(Content) + "&ac=51&confirm=%E5%B9%BF%E6%92%AD";
            return h.HttpSendData(url, "post", data);
        }

    }

}
