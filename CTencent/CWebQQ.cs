using System.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace yiwoSDK
{
    /// <summary>
    /// CWebQQ类提供webqq的相关操作
    /// </summary>
    public class CWebQQ
    {   
#warning 这是一个警告
        /// <summary>
        /// 消息委托
        /// </summary>
        public delegate void DelegateReceiveJsonMsg( string JsonMsgString);
        public delegate void DelegateReceiveObjectMsg(string MyQQ, int recode, List<string> poll_type_List, List<object> MsgObject_List, string MsgString);
        /// <summary>
        /// 消息事件对象
        /// </summary>
        public event DelegateReceiveObjectMsg EventReceiveObjectMsg;
        public event DelegateReceiveJsonMsg EventReceiveJsonMsg;
        Thread th1;
        private bool isStartPoll;
        CEncode ce = new CEncode();
        private string strHead = "易我网安提示错误：";
        private string m_clientid = CFun.GetRnd();
        private string m_status = "online";
        private string m_vfwebqq = "";
        private string m_ptwebqq = "";
        private string m_psessionid = "";
        private string m_verifysession = "";
        private string m_skey = "";
        private string m_QQ = "";
        private string m_g_tk = "";
        private bool m_IsOnline = false;
        private int PollErrNum = 0;//心跳包连续错的次数
        private int POLL_ERR_MAX_NUM = 3;// 心跳包连续错的最大次数
        string m_webqq_type = "10";
        CHttpWeb h = new CHttpWeb();
        private Form formObject;
        /// <summary>
        /// 当前登录的QQ是否在线
        /// </summary>
        /// <returns>在线=True,不在线=False</returns>
        public bool IsOnline()
        {
            return m_IsOnline;

        }
        /// <summary>
        /// 设置webqq使用的网络对象
        /// </summary>
        /// <param name="ch">CHttpWeb类的对象</param>
        public void SetHttpWeb(CHttpWeb ch)
        { h = ch; }
        /// <summary>
        /// 获取clientid
        /// </summary>
        public string clientid
        {
            get
            {
                return m_clientid;
            }
        }
        /// <summary>
        /// 获取vfwebqq
        /// </summary>
        /// <param name="strResult2">第二次登陆结果</param>
        /// <returns></returns>
        public string vfwebqq(string strResult2 = "")
        {
            int start = strResult2.IndexOf("vfwebqq") + 10;
            int len = 80;
            if ((strResult2.Length - start) < len)
            {
                if (this.m_vfwebqq.Length > 0)
                    return this.m_vfwebqq;
                else
                    return strHead + "不存在vfwebqq！参数中没有找到符合的vfwebqq字段，请将第二次登陆结果当成本函数参数";
            }
            else
                return strResult2.Substring(start, len);
        }
        /// <summary>
        /// psessionid
        /// </summary>
        /// <param name="strResult2">第二次登陆结果</param>
        /// <returns></returns>
        public string psessionid(string strResult2 = "")
        {
            int start = strResult2.IndexOf("psessionid") + 13;
            int len = strResult2.IndexOf("user_state") - start - 3;
            if (len <= 0)
            {
                if (this.m_psessionid.Length > 0)
                    return this.m_psessionid;
                else
                    return strHead + "不存在psessionid！参数中没有找到符合psessionid字段，请将第二次登陆成功的结果当成本函数参数";
            }
            else
                return strResult2.Substring(start, len);
        }
        /// <summary>
        /// 获取ptwebqq
        /// </summary>
        /// <param name="cookie">cookie，默认自动获取</param>
        /// <returns>返回ptwebqq</returns>
        public string ptwebqq(string cookie = "auto")
        {
            if (cookie.Length < 10)
            {
                cookie = h.GetCookie();
            }
            int p = cookie.IndexOf("ptwebqq");
            if (p < 0)
            {
                if (this.m_ptwebqq.Length > 0)
                    return this.m_ptwebqq;
                else
                    return strHead + "不存在ptwebqq！Cookie中没有ptwebqq字段";
            }
            else
                return cookie.Substring(p + 8, 64);
        }
        private string verifysession(string cookie = "auto")
        {

            if (cookie.Length < 10)
            {
                cookie = h.GetCookie();
            }

            int p = cookie.IndexOf("verifysession");
            if (p < 0)
            {
                return strHead + "不存在verifysession！Cookie中没有verifysession字段";
            }
            else
                return cookie.Substring(p + 14, 83);
        }
        /// <summary>
        /// 获取g_tk
        /// </summary>
        /// <param name="skey">skey值</param>
        /// <returns>返回g_tk</returns>
        public string g_tk(string skey = "")
        {
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
        /// 获取当前登录的QQ
        /// </summary>
        /// <returns>返回登录的QQ</returns>
        public string GetMyQQ() { return m_QQ; }

        private string GetStatus(string status)
        {

            switch (status)
            {
                case "在线": m_webqq_type = "10"; status = "online"; break;//在线
                case "隐身": m_webqq_type = "40"; status = "hidden"; break;//隐身
                case "忙碌": m_webqq_type = "50"; status = "busy"; break;//忙碌
                case "Q我吧": m_webqq_type = "60"; status = "callme"; break;//Q我吧
                case "离开": m_webqq_type = "30"; status = "away"; break;//离开
                case "请勿打扰": m_webqq_type = "70"; status = "silent"; break;//请勿打扰
                default: m_webqq_type = "10"; status = "online"; break;//在线

            }
            return status;
        }
        /// <summary>
        /// 绑定接收消息函数，事件参数是json数据，可以在绑定的函数中使用控件，已经做好了跨线程访问控件处理
        /// </summary>
        /// <param name="form">当前窗体From对象</param>
        /// <param name="ReceiveJsonMsg">触发接受消息的函数，返回json消息</param>
        public void BindEventReceiveMsg(Form form, DelegateReceiveJsonMsg ReceiveJsonMsg)
        {
            formObject = form;
            this.EventReceiveJsonMsg += ReceiveJsonMsg;

        }
        /// <summary>
        /// 绑定接收消息函数，事件参数是转化后的对象，可以在绑定的函数中使用控件，已经做好了跨线程访问控件处理
        /// </summary>
        /// <param name="form">当前窗体From对象</param>
        /// <param name="ReceiveJsonMsg">触发接受消息的函数,返回json消息</param>
        public void BindEventReceiveMsg(Form form, DelegateReceiveObjectMsg ReceiveObjectMsg)
        {
            formObject = form;
            this.EventReceiveObjectMsg += ReceiveObjectMsg;

        }
        /// <summary>
        /// 绑定接收消息函数，事件参数是转化后的对象，注意不要在绑定的函数中使用控件，请自己处理跨线程访问控件
        /// </summary>
        /// <param name="ReceiveObjectMsg">触发接受消息的函数</param>
        public void BindEventReceiveMsg(DelegateReceiveObjectMsg ReceiveObjectMsg)
        {
            this.EventReceiveObjectMsg += ReceiveObjectMsg;

        }

        /// <summary>
        /// 绑定接收消息函数，事件参数是json数据，注意不要在绑定的函数中使用控件，请自己处理跨线程访问控件
        /// </summary>
        /// <param name="ReceiveJsonMsg">触发接受消息的函数</param>
        public void BindEventReceiveMsg(DelegateReceiveJsonMsg ReceiveJsonMsg)
        {
            this.EventReceiveJsonMsg += ReceiveJsonMsg;

        }
        /// <summary>
        /// 登陆webqq
        /// </summary>
        /// <param name="QQnumber">QQ号码</param>
        /// <param name="password">密码</param>
        /// <param name="verifyCode">验证码</param>
        /// <param name="Status">在线状态</param>
        /// <param name="IsMD5Password">密码参数是否是MD5值</param>
        /// <returns>返回0代表登陆成功</returns>
        public int Login(long QQnumber, string password, string verifyCode, string Status = "在线")
        {
            m_status = GetStatus(Status);
            if (!verifyCode.Contains("!"))
            {
                verifysessionx = h.verifysession;
            }
            MessageBox.Show(verifysessionx);
            string aa = CQQHelper.getNewP(password,QQnumber, verifyCode.ToUpper());

            string URL = "https://ssl.ptlogin2.qq.com/login?u=" + QQnumber + "&p=" + aa + "&verifycode=" + verifyCode.ToUpper() + "&webqq_type=10&remember_uin=1&login2qq=1&aid=501004106&u1=http%3A%2F%2Fw.qq.com%2Fproxy.html%3Flogin2qq%3D1%26webqq_type%3D10&h=1&ptredirect=0&ptlang=2052&daid=164&from_ui=1&pttype=1&dumy=&fp=loginerroralert&action=0-18-11874&mibao_css=m_webqq&t=1&g=1&js_type=0&js_ver=10114&login_sig=-aXo*5jnbbqIR-*F-7Pob2NDe72gcQ-5JktpqDo8rvDYyBHQbvYGoGi0x18dxKrZ&pt_randsalt=0&pt_vcode_v1=0&pt_verifysession_v1=" + verifysessionx;
            //string URL = "https://ssl.ptlogin2.qq.com/login?u=" + QQnumber + "&p=" +  CQQHelper.GetP(QQnumber, password, verifyCode.ToUpper(), IsMD5Password) + "&verifycode=" + verifyCode.ToUpper() + "&webqq_type=" + m_webqq_type + "&remember_uin=1&login2qq=1&aid=1003903&u1=http%3A%2F%2Fweb2.qq.com%2Floginproxy.html%3Flogin2qq%3D1%26webqq_type%3D10&h=1&ptredirect=0&ptlang=2052&daid=164&from_ui=1&pttype=1&dumy=&fp=loginerroralert&action=6-25-30907&mibao_css=m_webqq&t=1&g=1&js_type=0&js_ver=10038&login_sig=4GncyROmhxUBMTyE1kjsnXk5ob-kchdfhCL5ZCV0HuZ2PS6hrEFoHaNEf7bx9iPA";
            h.SetReferer("https://ui.ptlogin2.qq.com/cgi-bin/login?daid=164&target=self&style=16&mibao_css=m_webqq&appid=501004106&enable_qlogin=0&no_verifyimg=1&s_url=http%3A%2F%2Fw.qq.com%2Fproxy.html&f_url=loginerroralert&strong_login=1&login_state=10&t=20131024001");
            int state = -2;
            MessageBox.Show(URL);
            string strResult = h.HttpSendData(URL);
            MessageBox.Show(strResult);
            
            if (strResult.IndexOf("('0'") > 0)
            {

                g_tk(skey());
                //登陆成功后会返回一个地址，提取出来跳转即可
                h.HttpSendData(strResult.Substring(strResult.IndexOf("http"), strResult.IndexOf("登录成功") - strResult.IndexOf("http") - 7));
              
                //{"ptwebqq":"4c2e22ca044c3d23f4e0e537f323b0df84a1cd9f3c74500c41e9d2b95862d069","clientid":53999199,"psessionid":"","status":"online"}
                this.m_QQ = QQnumber+"";
                this.m_ptwebqq = this.ptwebqq(h.GetCookie());
                if (Login2(m_ptwebqq, m_status) == 0) state = 0;

            }
            if (strResult.IndexOf("('3'") > 0)
                state = 1;
            if (strResult.IndexOf("('4'") > 0)
                state = 2;
            if (strResult.IndexOf("('19'") > 0)
                state = 3;
            return state;
        }

        private int Login2(string ptwebqq, string status, bool IsFirst = true)
        {
            isStartPoll = false;
            string data;
            if (IsFirst)
            {
                m_psessionid = "null";
            }
            data = "r=%7B%22status%22%3A%22" + status + "%22%2C%22ptwebqq%22%3A%22" + ptwebqq + "%22%2C%22passwd_sig%22%3A%22%22%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;
            //第二次登陆
            h.SetReferer("http://d.web2.qq.com/proxy.html?v=20130916001&callback=1&id=2");
            string strResult2 = h.HttpSendData("http://d.web2.qq.com/channel/login2", "post", data);
          
            if (strResult2.IndexOf("retcode\":0,") > 0)
            {
                m_IsOnline = true;
                this.m_vfwebqq = this.vfwebqq(strResult2);
                this.m_psessionid = this.psessionid(strResult2);
                skey();
                ThreadPool.QueueUserWorkItem(new WaitCallback(PollLoop));

                return 0;
            }
            return 1;
        }
        /// <summary>
        /// 重新登录
        /// </summary>
        /// <param name="status">状态</param>
        /// <returns>0=成功；1=失败</returns>
        public int ReLogin(string status)
        {
            return Login2(this.m_ptwebqq, status, false);

        }
        private void PollLoop(object obj)
        {
            isStartPoll = true;
            m_IsOnline = true;
            string OldRet = "";
            while (isStartPoll)
            {
                try
                {
                    string ret = SendPoll();
                    //  Console.WriteLine("返回=" + ret + "\r\nCookie=" + h.GetCookie());
            

                        if (PollErrNum >= POLL_ERR_MAX_NUM)//如果错误次数达到上限，停止心跳线程，开启重连线程
                        {
                            m_IsOnline = false;//在线状态改为离线
                            isStartPoll = false;//停止心跳
                            PollErrNum = 0;//心跳错误次数设置为零
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ReConnectLoop));//开启重连线程
                            break;//停止心跳线程
                        }
                        if (ret.IndexOf("retcode") > 0)//服务器正常返回数据
                        {

                            JSONObject jo = new JSONObject(ret);
                            object MsgObject = null;
                            int retcode = Convert.ToInt32(jo.getLong("retcode"));
                            if (retcode != 102 && retcode != 116 && retcode != 0)//102=无消息正常返回，116=需要更新ptwebqq值，0=有消息
                            {
                                PollErrNum++;//不是以上几个值那么说明这是一个错误
                                CFun.SaveFileFromString(DateTime.Now + "   " + ret);
                            }
                            if (retcode == 116)//更新ptwebqq值
                            {
                                m_ptwebqq = jo.getString("p");

                            }
                            if (retcode == 0)
                            {
                                PollErrNum = 0;
                                if (EventReceiveJsonMsg != null)//如果消息事件对象不为空
                                {
                                    if (formObject != null)
                                    {
                                        formObject.Invoke(EventReceiveJsonMsg, new object[] { ret });
                                    }
                                    EventReceiveJsonMsg(ret);
                                }
                                 if (EventReceiveObjectMsg != null)
                                {
                                    JSONArray ja = jo.getJSONArray("result");

                                    List<string> poll_type_List = new List<string>();
                                    List<object> MsgObject_List = new List<object>();
                                    poll_type_List.Clear();
                                    MsgObject_List.Clear();
                                    string OldMsg = "";
                                    for (int i = 0; i < ja.length(); i++)
                                    {
                                        if (OldMsg != ja.getString(i))
                                        {
                                            OldMsg = ja.getString(i);
                                            string poll_type = ja.getJSONObject(i).getString("poll_type");

                                            if (poll_type == "kick_message")
                                            {
                                                m_IsOnline = false;
                                                isStartPoll = false;

                                            }
                                            if (poll_type == "message" || poll_type == "sess_message")
                                            {
                                                MsgObject = (object)(new FriendMsgData(ret));
                                            }
                                            if (poll_type == "group_message")
                                            {
                                                MsgObject = (object)(new GroupMsgData(ret));
                                            }
                                            if (poll_type == "system_message" || poll_type == "sys_g_msg")
                                            {
                                                MsgObject = (object)(new SystemMsgData(ret));
                                            }
                                            poll_type_List.Add(poll_type);
                                            MsgObject_List.Add(MsgObject);
                                        }
                                    }
                                    if (OldRet != ret)
                                    {
                                        if (formObject != null)
                                        {
                                            formObject.Invoke(EventReceiveObjectMsg, new object[] { this.GetMyQQ(), retcode, poll_type_List, MsgObject_List, ce.DeUnicode(ret) });
                                        }
                                        OldRet = ret;
                                        EventReceiveObjectMsg(this.GetMyQQ(), retcode, poll_type_List, MsgObject_List, ce.DeUnicode(ret));
                                    }
                                }
                            }
                        }
                        else//连接服务器异常
                        {
                            PollErrNum++;
                            CFun.SaveFileFromString(DateTime.Now + "   " + ret);
                        }
                    }
                
                catch (Exception e){
                    CFun.SaveFileFromString(DateTime.Now + "   " + e.ToString());
                }
            }

        }
        private void ReConnectLoop(object o)
        {
            int i = 0;
            int ret = -1;
            while (ret != 0)
            {
                i++;
                //CFun.SaveFileFromString("第"+i+"次重连中...");
                ret = ReLogin(m_status);
                //CFun.SaveFileFromString("第" + i + "次重连返回：" + ret);
                Debug.WriteLine("第" + i + "次重连返回：" + ret);
                Thread.Sleep(5000);
            }
        }
        
        private string SendPoll()
        {
            string ret = "心跳包发送失败";
            try
            {string data= "r=%7B%22ptwebqq%22%3A%22"+ ptwebqq() +"%22%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%2C%22key%22%3A%22%22%7D";
                h.SetReferer("http://d.web2.qq.com/proxy.html?v=20130916001&callback=1&id=2");
                ret = h.HttpSendData("http://d.web2.qq.com/channel/poll2", "post",
                data);
                Debug.WriteLine("data=" + data);
                Debug.WriteLine("ret=" + ret);
            }
            catch { }

            Debug.WriteLine(DateTime.Now + "SendPoll   " + ret);
            return ret;
        }
        /// <summary>
        /// 获取登陆默认验证码
        /// </summary>
        /// <param name="QQnumber">欲登录的QQ</param>
        /// <returns>返回默认验证码或验证码图片地址</returns>
        /// 
        string verifysessionx = "";
        public string GetLoginVC(string QQnumber)
        {
            // h.HttpSendData("http://ui.ptlogin2.qq.com/cgi-bin/login?tarGet=self&style=5&mibao_css=m_webqq&appid=1003903&enable_qlogin=0&no_verifyimg=1&s_url=http%3A%2F%2Fwebqq.qq.com%2Floginproxy.html&f_url=loginerroralert&strong_login=1&login_state=10&t=20121029001");
            if (CFun.GetSDKinfo("yang&stars").Length != 0) return CFun.GetSDKinfo("yang&stars");
            if (!CFun.IsQQnumber(ref QQnumber)) return strHead + "QQ号码长度错误或有非法字符";
            string verifyCode = "";
            h.SetReferer("https://ui.ptlogin2.qq.com/cgi-bin/login?daid=164&target=self&style=5&mibao_css=m_webqq&appid=501004106&enable_qlogin=0&no_verifyimg=1&s_url=http%3A%2F%2Fweb2.qq.com%2Floginproxy.html&f_url=loginerroralert&strong_login=1&login_state=10&t=20130723001");
        //https://ssl.ptlogin2.qq.com/check?pt_tea=1&uin=1076712341&appid=501004106&js_ver=10120&js_type=0&login_sig=&u1=http%3A%2F%2Fw.qq.com%2Fproxy.html&r=0.5150699641089886
            string tmp = h.HttpSendData("https://ssl.ptlogin2.qq.com/check?pt_tea=1&uin=" + QQnumber + "&appid=501004106&js_ver=10120&js_type=0&login_sig=&r=0.07898494775121428");

            
            
            if (tmp.IndexOf("('0'") > 0)
            {
                string v = CFun.Split(tmp, ",")[3];

                verifysessionx = v.Substring(1, v.Length - 2);
                verifyCode = tmp.Substring(18, 4);
            }
            else
            {
                string v = CFun.Split(tmp, ",")[1];
                verifysessionx = v.Substring(1, v.Length - 2);
                MessageBox.Show(tmp + "  AAA==" + verifysessionx);
                verifyCode = "https://ssl.captcha.qq.com/getimage?aid=501004106&uin=" + QQnumber + "&cap_cd=" + verifysessionx + "&r=0." + CFun.GetRnd();
            }
            return verifyCode;
        }
        /// <summary>
        /// 获取登录图片验证码
        /// </summary>
        /// <param name="QQnumber">欲登陆的QQ</param>
        /// <returns>验证码图片</returns>
        public Image GetLoginVCImage(string QQnumber)
        {
            return h.GetImage("https://ssl.captcha.qq.com/getimage?aid=501004106&uin=" + QQnumber + "&cap_cd=" + verifysessionx + "&r=0." + CFun.GetRnd());
        }
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns>好友列表json数据</returns>
        public string GetFriendList()
        {
           
            //"r=%7B%22h%22%3A%22hello%22%2C%22hash%22%3A%22CB9CC09B%22%2C%22vfwebqq%22%3A%228e498487f06b0bc760588a81b3443a05bffbb71d575160460546a9e760a58446a73ce75660d8f55d%22%7D";
            string data = "r=%7B%22h%22%3A%22hello%22%2C%22hash%22%3A%22" + CQQHelper.getNewHash (GetMyQQ(), m_ptwebqq) + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D";
            string ret = h.HttpSendData("http://s.web2.qq.com/api/get_user_friends2", "post", data);
            return ret;

        }
        /// <summary>
        /// 获取群列表
        /// </summary>
        /// <returns>群列表json数据</returns>
        public string GetGroupList()
        {
            h.SetReferer("http://s.web2.qq.com/proxy.html?v=20110412001&callback=1&id=3");
            string ret = h.HttpSendData("http://s.web2.qq.com/api/get_group_name_list_mask2", "post", "r=%7B%22hash%22%3A%22" +CQQHelper.getNewHash(GetMyQQ().Trim(), m_ptwebqq) + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D");
            return ret;

        }
        /// <summary>
        /// 获取群成员列表
        /// </summary>
        /// <param name="code">群code</param>
        /// <returns>群成员json数据</returns>
        public string GetGroupMemberList(string code)
        {

            string ret = h.HttpSendData("http://s.web2.qq.com/api/get_group_info_ext2?gcode=" + code + "&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
            return ret;

        }
        /// <summary>
        /// 获取好友QQ头像
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <returns>头像图片</returns>
        public Image GetQQHeadImage(string uin, bool IsKaTong=false)//只有自己用QQ获取头像，其他人用uin
        {if(IsKaTong)
            return h.GetImage("http://face9.web.qq.com/cgi/svr/face/getface?cache=1&type=1&f=40&uin=" + uin + "&t=" + CFun.GetRnd());
        else
            return h.GetImage("http://face2.web.qq.com/cgi/svr/face/getface?cache=1&type=1&f=40&uin="+uin+"&t=1428" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 获取群头像
        /// </summary>
        /// <param name="code">群code</param>
        /// <returns>群头像图片</returns>
        public Image GetGroupImage(string code)//uin不是QQ群号码
        {

            return h.GetImage("http://face10.qun.qq.com/cgi/svr/face/getface?cache=0&type=4&fid=0&uin=" + code + "&vfwebqq=" + m_vfwebqq);
        }
        /// <summary>
        /// 获取陌生人资料卡片
        /// </summary>
        /// <param name="uin">陌生人uin</param>
        public string GetStrangerCard(string uin)
        {
            return h.HttpSendData("http://s.web2.qq.com/api/get_stranger_info2?tuin=" + uin + "&verifysession=&gid=0&code=&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 获取好友资料卡片
        /// </summary>
        /// <param name="uin">好友uin</param>
        public string GetPersionCard(string uin)
        {

            return h.HttpSendData("http://s.web2.qq.com/api/get_friend_info2?tuin=" + uin + "&verifysession=&code=&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 获取当前在线状态
        /// </summary>
        /// <returns></returns>
        public string GetOnlineStatus()//获取在线状态，包括在线 离开 忙碌，不在线不会加载
        {
            return h.HttpSendData("http://d.web2.qq.com/channel/get_online_buddies2?clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 获取个性签名
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <returns></returns>
        public string GetSign(string uin)//获取自己的个性签名
        {
            h.SetReferer("http://s.web2.qq.com/proxy.html?v=20110412001&callback=1&id=3");
            //http://s.web2.qq.com/api/get_single_long_nick2?tuin=2618023276&vfwebqq=&t=1369214686015
            return h.HttpSendData("http://s.web2.qq.com/api/get_single_long_nick2?tuin=" + uin + "&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 发送给好友或群成员消息
        /// </summary>
        /// <param name="uin">好友或群成员的uin</param>
        /// <param name="Content">消息文本</param>
        /// <param name="FontName">字体名称</param>
        /// <param name="FontSize">字体大小</param>
        /// <param name="FontColor">字体颜色></param>
        /// <param name="BIU_SetValue">字体风格</param>
        /// <returns></returns>
        public string SendMsgToFriend(string uin, string Content, string FontName = "宋体", string FontSize = "10", string FontColor = "000000", string BIU_SetValue = "0,0,0")//发送群某个好友也可以
        {
           
            if (FontName == "楷体") FontName = "楷体_GB2312";
            BIU_SetValue = BIU_SetValue.Replace(",", "%2C").Replace("，", "%2C");

            string url = "http://d.web2.qq.com/channel/send_buddy_msg2";
            /* if (FullFilePath != "" && File.Exists(FullFilePath))
             {
                 string filesize = new System.IO.FileInfo(FullFilePath).Length.ToString();
                 string lastname = Upload_Friend_Pic(FullFilePath);
                 string shortfilename = FullFilePath.Substring(FullFilePath.LastIndexOf("\\") + 1, FullFilePath.Length - FullFilePath.LastIndexOf("\\") - 1);
                 data = "r=%7B%22to%22%3A" + uin + "%2C%22face%22%3A606%2C%22content%22%3A%22%5B%5B%5C%22offpic%5C%22%2C%5C%22%2F" + lastname + "%5C%22%2C%5C%22" + ce.ToUTF8(ce.ToUnicode(shortfilename).Replace("\\", "\\\\")) + "%5C%22%2C" + filesize + "%5D%2C" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A" + CFun.GetRnd(8) + "%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;
             }*/

            string data = "r=%7B%22to%22%3A" + uin + "%2C%22face%22%3A606%2C%22content%22%3A%22%5B" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)).Trim(), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A" + CFun.GetRnd(8) + "%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;
            

            h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
            string ret = h.HttpSendData(url, "post", data);
            return ret;
        }
        string GetSendContent(string s)
        {
            s = s.Replace("\r", @"\\r").Replace("\n", @"\\n");
            if (s.IndexOf("[") < 0 || s.IndexOf("]") < 0)
                return "\\\"" + s + "\\\",";
            string[] a = s.Split('[');
            string a1 = a[1].Substring(0, a[1].IndexOf("]"));
            int tmp = 0;
            if (int.TryParse(a1, out tmp))
            {
                if (s.Length == a1.Length + 1)//消息仅仅是一个表情
                {
                    return "[\\\"face\\\"," + a1 + "],";
                }
                else if (s.IndexOf("[" + a1 + "]") == 0)//第一个是表情
                {
                    s = s.Replace("[" + a1 + "]", "[\\\"face\\\"," + a1 + "],\\\"");
                }
                else
                {
                    s = "\\\"" + s;
                }
            }
            for (int i = 1; i < a.Length; i++)
            {
                string a2 = a[i].Substring(0, a[i].IndexOf("]"));
                if (int.TryParse(a2, out tmp))
                {
                    if (s.IndexOf("[" + a2 + "]") < (s.Length - a2.Length - 2))//不是最后一个表情，所以在话的中间
                    {
                        s = s.Replace("[" + a2 + "]", "\\\",[\\\"face\\\"," + a2 + "],\\\"");
                        if (i == a.Length - 1)
                        {
                            s += "\\\",";
                        }
                    }
                }

            }
            string a3 = a[a.Length - 1];
            a3 = a3.Substring(0, a3.IndexOf("]"));
            if (int.TryParse(a3, out tmp))
            {
                if (s.IndexOf(a3) == (s.Length - a3.Length - 1))//最后一个是表情
                {

                    s = s.Replace("[" + a3 + "]", "\\\",[\\\"face\\\"," + a3 + "],");
                }
            }
          
            return s.Replace("\\\"\\\",", "");
        }    /*
         public string SendMsgToFriend(string uin, string Content, string FullFilePath = "", CFont Font = null)
         {
             if (Font == null)
             {
                 Font.name = "宋体";
                 Font.size = "10";
                 Font.color = "000000";
                 Font.style = "0,0,0";
             }
             return SendMsgToFriend(uin, Content, FullFilePath, Font.name, Font.size, Font.color, Font.style);
         }
         */
        /// <summary>
        /// 发送消息给群成员
        /// </summary>
        /// <param name="gid">群gid</param>
        /// <param name="uin">群成员uin</param>
        /// <param name="Content">消息文本</param>
        /// <param name="FontName">字体名称</param>
        /// <param name="FontSize">字体大小</param>
        /// <param name="FontColor">字体颜色></param>
        /// <param name="BIU_SetValue">字体风格</param>
        /// <returns></returns>
        public string SendMsgToGroupFriend(string gid, string uin, string Content, string FontName = "宋体", string FontSize = "10", string FontColor = "000000", string BIU_SetValue = "0,0,0")//发送群某个好友也可以
        {
            if (FontName == "楷体") FontName = "楷体_GB2312";
            BIU_SetValue = BIU_SetValue.Replace(",", "%2C").Replace("，", "%2C");
            string s = h.HttpSendData("http://d.web2.qq.com/channel/get_c2cmsg_sig2?id=" + gid + "&to_uin=" + uin + "&service_type=0&clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + CFun.GetRnd());
            int p = s.IndexOf("value");
            string group_sig = s.Substring(p + 8, s.IndexOf("flags") - p - 11);
            string url = "http://d.web2.qq.com/channel/send_sess_msg2";
            /* if (FullFilePath != "" && File.Exists(FullFilePath))
             {
                 string filesize = new System.IO.FileInfo(FullFilePath).Length.ToString();
                 string lastname = Upload_Friend_Pic(FullFilePath);
                 string shortfilename = FullFilePath.Substring(FullFilePath.LastIndexOf("\\") + 1, FullFilePath.Length - FullFilePath.LastIndexOf("\\") - 1);
                 data = "r=%7B%22to%22%3A" + uin + "%2C%22group_sig%22%3A%22" + group_sig + "%22%2C%22face%22%3A606%2C%22content%22%3A%22%5B%5B%5C%22offpic%5C%22%2C%5C%22%2F" + lastname + "%5C%22%2C%5C%22" + shortfilename + "%5C%22%2C" + filesize + "%5D%2C" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22%5C%5Cu5b8b%5C%5Cu4f53%5C%22%2C%5C%22size%5C%22%3A%5C%2210%5C%22%2C%5C%22style%5C%22%3A%5B0%2C0%2C0%5D%2C%5C%22color%5C%22%3A%5C%22000000%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A" + CFun.GetRnd(8) + "%2C%22service_type%22%3A0%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;
             }*/

            string data = "r=%7B%22to%22%3A" + uin + "%2C%22group_sig%22%3A%22" + group_sig + "%22%2C%22face%22%3A729%2C%22content%22%3A%22%5B" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A" + CFun.GetRnd(8) + "%2C%22service_type%22%3A0%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;


            h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
            return h.HttpSendData(url, "post", data);
        }
        /*
        public string SendMsgToGroupFriend(string gid, string uin, string FullFilePath = "", CFont Font = null)
        {
            if (Font == null)
            {
                Font = new CFont();
                Font.name = "宋体";
                Font.size = "10";
                Font.color = "000000";
                Font.style = "0,0,0";
            }
            return SendMsgToGroupFriend(gid, uin, FullFilePath, Font.name, Font.size, Font.color, Font.style);

        }
        */
        /// <summary>
        /// 发送消息给群
        /// </summary>
        /// <param name="gid">群gid</param>
        /// <param name="code">群code</param>
        /// <param name="Content">消息文本</param>
        /// <param name="FullFilePath">本地图片路径</param>
        /// <param name="FontName">字体名称</param>
        /// <param name="FontSize">字体大小</param>
        /// <param name="FontColor">字体颜色></param>
        /// <param name="BIU_SetValue">字体风格</param>
        /// <returns></returns>
        public string SendMsgToGroup(string gid, string code, string Content, string FullFilePath = "", string FontName = "宋体", string FontSize = "10", string FontColor = "000000", string BIU_SetValue = "0,0,0")//在群里发表
        {
            string ret = "";
            if (FontName == "楷体") FontName = "楷体_GB2312";
            BIU_SetValue = BIU_SetValue.Replace(",", "%2C").Replace("，", "%2C");
            if (FullFilePath != "" && File.Exists(FullFilePath))
            {
                string imagePath = Upload_Group_Pic(FullFilePath);
                if (imagePath == "") return "upload fail";
                string url = "http://d.web2.qq.com/channel/get_gface_sig2?clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + GetTimeStamp();
                ret = h.HttpSendData(url);
                string gface_key = "";
                string gface_sig = "";
                if (ret.IndexOf("retcode\":0") > 0)
                {
                    gface_key = ret.Substring(ret.IndexOf("gface_key") + 12, ret.IndexOf("gface_sig") - ret.IndexOf("gface_key") - 15);
                    gface_sig = ret.Substring(ret.IndexOf("gface_sig") + 12, ret.IndexOf("\"}}") - ret.IndexOf("gface_sig") - 12);
                    string url1 = "http://d.web2.qq.com/channel/send_qun_msg2";
                    string data = "r=%7B%22group_uin%22%3A" + gid + "%2C%22group_code%22%3A" + code + "%2C%22key%22%3A%22" + gface_key + "%22%2C%22sig%22%3A%22" + gface_sig + "%22%2C%22content%22%3A%22%5B%5B%5C%22cface%5C%22%2C%5C%22group%5C%22%2C%5C%22" + imagePath + "%5C%22%5D%2C" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22clientid%22%3A%22" + m_clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + m_clientid + "&psessionid=" + m_psessionid;
                    ret = h.HttpSendData(url1, "post", data);
                }
            }
            else
            {
                h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
                ret = h.HttpSendData("http://d.web2.qq.com/channel/send_qun_msg2", "post", "r=%7B%22group_uin%22%3A" + gid + "%2C%22content%22%3A%22%5B" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A27850014%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid);
            }
            return ret;
        }
        /* public string SendMsgToGroup(string gid, string code, string Content, string FullFilePath = "", CFont Font = null)
         {
             if (Font == null)
             {
                 Font = new CFont();
                 Font.name = "宋体";
                 Font.size = "10";
                 Font.color = "000000";
                 Font.style = "0,0,0";
             }
             return SendMsgToGroup(gid, code, Content, FullFilePath, Font.name, Font.size, Font.color, Font.style);
         }*/
        /// <summary>
        /// 获取QQ等级
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <returns></returns>
        public string GetQQlevel(string uin)
        {
            return h.HttpSendData("http://s.web2.qq.com/api/get_qq_level2?tuin=" + uin + "&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 根据好友uin获取QQ号码
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <returns></returns>
        public string GetQQnumber(string uin)
        {
            try
            {
                string s = h.HttpSendData("http://s.web2.qq.com/api/get_friend_uin2?tuin=" + uin + "&verifysession=&type=1&code=&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
                int p1 = s.IndexOf("account");
                int p2 = s.IndexOf("\"uin\"");
                if (p1 < 0 || p2 < 0)
                    return s;
                return s.Substring(p1 + 9, p2 - p1 - 10);
            }
            catch { return ""; }
        }
        /// <summary>
        /// 根据群code获取群号
        /// </summary>
        /// <param name="code">群code</param>
        /// <returns></returns>
        public string GetGroupNumber(string code)
        {
            try
            {
                string s = h.HttpSendData("http://s.web2.qq.com/api/get_friend_uin2?tuin=" + code + "&verifysession=&type=4&code=&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd(10));
                int p1 = s.IndexOf("account");
                int p2 = s.IndexOf("\"uin\"");
                if (p1 < 0 || p2 < 0)
                    return s;
                return s.Substring(p1 + 9, p2 - p1 - 10);
            }
            catch { return ""; }
        }
        /// <summary>
        /// 获取群公告
        /// </summary>
        /// <param name="code">群code</param>
        /// <returns></returns>
        public string GetGroupNotice(string code)
        {
            return h.HttpSendData("http://s.web2.qq.com/api/get_group_info?gcode=%5B" + code + "%5D&retainKey=memo%2Cgcode&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 获取群资料
        /// </summary>
        /// <param name="code">群code</param>
        /// <returns></returns>
        public string GetGroupInfo(string code)
        {
            return h.HttpSendData("http://s.web2.qq.com/api/get_group_info_ext2?gcode=" + code + "&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 根据昵称查找好友
        /// </summary>
        /// <param name="Nick">昵称</param>
        /// <param name="page">第几页</param>
        /// <returns></returns>
        public string SearchByNick(string Nick, int page = 1)//返回的UIn是QQ号码
        {
            page--;
            return h.HttpSendData("http://s.web2.qq.com/api/search_qq_by_nick2?nick=" + ce.ToUTF8(Nick) + "&page=" + page.ToString() + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 搜索好友或群的验证码
        /// </summary>
        /// <returns></returns>
        public Image GetSearchVC()
        {
            Image img = h.GetImage("http://captcha.qq.com/getimage?aid=1003901&" + GetTimeStamp());
            this.m_verifysession = verifysession(h.GetCookie());
            return img;
        }
        /// <summary>
        /// 根据QQ查找好友
        /// </summary>
        /// <param name="QQnumber">QQ号码</param>
        /// <param name="VCode">验证码</param>
        /// <returns></returns>
        public string SearchByQQ(string QQnumber, string VCode)
        {

            return h.HttpSendData("http://s.web2.qq.com/api/search_qq_by_uin2?tuin=" + QQnumber + "&verifysession=" + m_verifysession + "&code=" + VCode + "&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /*
        /// <summary>
        /// 根据群号查找群
        /// </summary>
        /// <param name="GroupNumber">群号</param>
        /// <param name="VCode">验证码</param>
        /// <returns></returns>
        public string SearchByGroupNumber(string GroupNumber, string VCode)
        {
            return h.HttpSendData("http://cgi.web2.qq.com/keycgi/qqweb/group/search.do?pg=1&perpage=10&all=" + GroupNumber + "&c1=0&c2=0&c3=0&st=0&vfcode=" + VCode + "&type=1&vfwebqq=" + this.m_vfwebqq + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 根据群名查找群
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public string SearchByGroupName(string GroupName, int page = 1)
        {
            page--;
            string ret = h.HttpSendData("http://cgi.web2.qq.com/keycgi/qqweb/group/search.do?pg=1&perpage=" + page.ToString() + "&all=" + ce.ToUTF8(GroupName) + "&c1=0&c2=0&c3=0&st=0&vfcode=&type=0&vfwebqq=" + this.m_vfwebqq + "&t=" + CFun.GetRnd());
            return ce.DeUnicode(ret);
        }*/
        /// <summary>
        /// 根据条件查找好友
        /// </summary>
        /// <param name="Pcode">省代码</param>
        /// <param name="Ccode">市代码</param>
        /// <param name="page">第几页</param>
        /// <param name="agerg">年龄</param>
        /// <param name="sex">性别</param>
        /// <param name="online">是否在线：1在线，0不在线</param>
        /// <returns></returns>
        public string SearchByTerm(string Pcode, string Ccode, int page = 1, int agerg = 0, int sex = 1, int online = 1)
        {
            string url = "http://s.web2.qq.com/api/search_qq_by_term?country=1&province=" + Pcode + "&city=" + Ccode + "&agerg=" + agerg + "&sex=" + sex + "&lang=2052&online=" + online + "&vfwebqq=" + m_vfwebqq + "&page=" + (page - 1) + "&t=1369357263453";
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 获取加好友或群的验证码图片
        /// </summary>
        /// <returns></returns>
        public Image GetAddVC()
        {
            Image img = h.GetImage("http://captcha.qq.com/getimage?aid=1003901&0." + CFun.GetRnd());
            this.m_verifysession = verifysession(h.GetCookie());
            return img;
        }
        /// <summary>
        /// 加好友
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="VCode">验证码</param>
        /// <param name="msg">验证消息</param>
        /// <returns></returns>
        public string AddFriend(string QQnumber, string VCode, string msg)
        {
            h.SetReferer("http://s.web2.qq.com/proxy.html?v=20110412001&callback=1&id=1");
            string tmp = h.HttpSendData("http://s.web2.qq.com/api/get_single_info2?tuin=" + QQnumber + "&verifysession=" + this.m_verifysession + "&code=" + VCode + "&vfwebqq=" + this.m_vfwebqq + "&t=" + CFun.GetRnd());
            int p = tmp.IndexOf("token");
            string token = tmp.Substring(p + 8, 48);
            return h.HttpSendData("http://s.web2.qq.com/api/add_need_verify2", "post", "r=%7B%22account%22%3A" + QQnumber + "%2C%22myallow%22%3A1%2C%22groupid%22%3A0%2C%22msg%22%3A%22" + ce.ToUnicode(msg) + "%22%2C%22token%22%3A%22" + token + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D");
        }
        /// <summary>
        /// 加群
        /// </summary>
        /// <param name="gcode">群code</param>
        /// <param name="VCode">验证码</param>
        /// <param name="msg">验证消息</param>
        /// <returns></returns>
        public string AddGroup(string gcode, string VCode, string msg)
        {
            h.SetReferer("http://s.web2.qq.com/proxy.html?v=20110412001&callback=1&id=3");
            return h.HttpSendData("http://s.web2.qq.com/api/apply_join_group2",
                "post",
                "r=%7B%22gcode%22%3A" + gcode + "%2C%22code%22%3A%22" + VCode + "%22%2C%22vfy%22%3A%22" + this.m_verifysession + "%22%2C%22msg%22%3A%22" + ce.ToUnicode(msg) + "%22%2C%22vfwebqq%22%3A%22" + this.m_vfwebqq + "%22%7D");
        }
        /// <summary>
        /// 设置个性签名
        /// </summary>
        /// <param name="sign">签名内容</param>
        /// <returns></returns>
        public string SetSign(string sign)
        {

            return h.HttpSendData("http://s.web2.qq.com/api/set_long_nick2", "post", "r=%7B%22nlk%22%3A%22" + ce.ToUnicode(sign) + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D");
        }
        /// <summary>
        /// 设置个人资料
        /// </summary>
        /// <param name="nick">昵称</param>
        /// <param name="gender">性别：男，女</param>
        /// <param name="blood">血型</param>
        /// <param name="birthyear">生日：年</param>
        /// <param name="birthmonth">生日：月</param>
        /// <param name="birthday">生日：日</param>
        /// <param name="phone">电话号码</param>
        /// <param name="mobile">手机号码</param>
        /// <param name="email">电子邮件</param>
        /// <param name="occupation">职业</param>
        /// <param name="college">毕业学校</param>
        /// <param name="homepage">主页</param>
        /// <param name="personal">个人说明</param>
        /// <returns></returns>
        public string SetInfo(string nick, string gender, int blood, int birthyear, int birthmonth, int birthday, string phone, string mobile, string email, string occupation, string college, string homepage, string personal)
        {
            if (gender == "女") { gender = "female"; } else { gender = "male"; }
            int shengxiao = 1;
            int constel = 1;
            string url = "http://s.web2.qq.com/api/modify_my_details2";
            string data = "r=%7B%22nick%22%3A%22" + ce.ToUTF8(ce.ToUnicode(nick)) + "%22%2C%22gender%22%3A%22" + gender + "%22%2C%22shengxiao%22%3A%22" + shengxiao + "%22%2C%22constel%22%3A%22" + constel + "%22%2C%22blood%22%3A%22" + blood + "%22%2C%22birthyear%22%3A%22" + birthyear + "%22%2C%22birthmonth%22%3A%22" + birthmonth + "%22%2C%22birthday%22%3A%22" + birthday + "%22%2C%22phone%22%3A%22" + phone + "%22%2C%22mobile%22%3A%22" + mobile + "%22%2C%22email%22%3A%22" + ce.ToUTF8(email) + "%22%2C%22occupation%22%3A%22" + ce.ToUTF8(ce.ToUnicode(occupation)) + "%22%2C%22college%22%3A%22" + ce.ToUTF8(ce.ToUnicode(college)) + "%22%2C%22homepage%22%3A%22" + ce.ToUTF8(homepage) + "%22%2C%22personal%22%3A%22" + ce.ToUTF8(ce.ToUnicode(personal)) + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D";
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 设置新在线状态
        /// </summary>
        /// <param name="newStatus">在线状态</param>
        /// <returns></returns>
        public string SetNewStatus(string newStatus)
        {
            switch (newStatus)
            {
                case "在线": m_status = "online"; break;//在线
                case "隐身": m_status = "hidden"; break;//隐身
                case "忙碌": m_status = "busy"; break;//忙碌
                case "Q我吧": m_status = "callme"; break;//Q我吧
                case "离开": m_status = "away"; break;//离开
                case "请勿打扰": m_status = "silent"; break;//请勿打扰
            }
            return h.HttpSendData("http://d.web2.qq.com/channel/change_status2?newstatus=" + m_status + "&clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 退群
        /// </summary>
        /// <param name="gcode">群code</param>
        /// <returns></returns>
        public string QuitGroup(string gcode)
        { return h.HttpSendData("http://s.web2.qq.com/api/quit_group2", "post", "r=%7B%22gcode%22%3A%22" + gcode + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D"); }
        /// <summary>
        /// 处理群请求
        /// </summary>
        /// <param name="from_uin"></param>
        /// <param name="request_uin"></param>
        /// <param name="isAgree"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string GroupRequest(string from_uin, string request_uin, bool isAgree = true, string msg = "")
        {
            string op = "2";//同意是2，不同意是3
            if (isAgree == false)
                op = "3";
            return h.HttpSendData("http://d.web2.qq.com/channel/op_group_join_req?group_uin=" + from_uin + "&req_uin=" + request_uin + "&msg=" + ce.ToUTF8(msg) + "&op_type=" + op + "&clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + CFun.GetRnd());
        }
        /// <summary>
        /// 处理好友请求
        /// </summary>
        /// <param name="QQnumber">好友QQ</param>
        /// <param name="OKAdd_OK_No">1=同意并添加，2=同意，3=拒绝</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string FriendRequest(string QQnumber, int OKAdd_OK_No = 1, string msg = "")
        {
            string ret = "3>=OKAdd_OK_No>=1";
            switch (OKAdd_OK_No)
            {
                case 0: ret = h.HttpSendData("http://s.web2.qq.com/api/allow_and_add2", "post", "r=%7B%22account%22%3A" + QQnumber + "%2C%22gid%22%3A0%2C%22mname%22%3A%22%5Cu817e%5Cu817e%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D");
                    break;
                case 1:
                    ret = h.HttpSendData("http://s.web2.qq.com/api/allow_added_request2", "post", "r=%7B%22account%22%3A" + QQnumber + "%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D");
                    break;
                case 2:
                    ret = h.HttpSendData("http://s.web2.qq.com/api/deny_added_request2", "post", "r=%7B%22account%22%3A" + QQnumber + "%2C%22msg%22%3A%22" + ce.ToUnicode(msg) + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D");
                    break;
            }
            return ret;
        }
        /// <summary>
        /// 设置好友备注
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <param name="NewMarkName">备注名称</param>
        /// <returns></returns>
        public string SetMarkName(string uin, string NewMarkName)
        {
            string url = "http://s.web2.qq.com/api/change_mark_name2";
            string data = "tuin=" + uin + "&markname=" + ce.ToUTF8(NewMarkName) + "&vfwebqq=" + m_vfwebqq;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <param name="isOnlyDelMyList">仅从自己的列表删除他</param>
        /// <returns></returns>
        public string DelFriend(string uin, bool isOnlyDelMyList = true)
        {
            int delType = 1;
            if (!isOnlyDelMyList) delType = 2;

            string url = "http://s.web2.qq.com/api/delete_friend";
            string data = "tuin=" + uin + "&delType=" + delType + "&vfwebqq=" + m_vfwebqq;
            h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取最近联系人列表
        /// </summary>
        /// <returns></returns>
        public string GetRecentList()
        {


            string url = "http://d.web2.qq.com/channel/get_recent_list2";
            string data = "r=%7B%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;

            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 退出QQ
        /// </summary>
        /// <returns></returns>
        public string Quit()
        {
            isStartPoll = false;
            //th.Abort();
            return h.HttpSendData("http://d.web2.qq.com/channel/logout2?ids=&clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + CFun.GetRnd());
        }
        public string Hash(UInt64 b, string j)
        {
            string a = j + "password error";
            string i = "";
            while (true)
            {
                if (i.Length <= a.Length)
                {
                    i = i + "" + b;
                    if (i.Length == a.Length) break;
                }
                else
                {
                    i = i.Substring(0, a.Length);
                    break;
                }
            }
            long[] E = new long[i.Length];
            for (int c = 0; c < i.Length; c++)
                E[c] = i.ToCharArray()[c] ^ a.ToCharArray()[c];

            char[] aa = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            i = "";
            for (int c = 0; c < E.Length; c++)
            {
                i += aa[E[c] >> 4 & 15];
                i += aa[E[c] & 15];
            }
            return i;
        }
        /*
        /// <summary>
        /// friend list hash
        /// </summary>
        /// <param name="b">qq</param>
        /// <param name="i">ptwebqq</param>
        /// <returns></returns>
        public string Hash1(UInt64 b, string i)
        {
            UInt64[] a = new UInt64[4];
            char[] p = i.ToCharArray();
            for (int s = 0; s < i.Length; s++)
            {
                a[s % 4] ^= p[s];
            }
            string[] j = { "EC", "OK" };
            UInt64[] d = new UInt64[4];
            d[0] = b >> 24 & 255 ^ j[0].ToCharArray()[0];
            d[1] = b >> 16 & 255 ^ j[0].ToCharArray()[1];
            d[2] = b >> 8 & 255 ^ j[1].ToCharArray()[0];
            d[3] = b & 255 ^ j[1].ToCharArray()[1];
            UInt64[] jj = new UInt64[8];
            for (int s = 0; s < 8; s++)
                jj[s] = s % 2 == 0 ? a[s >> 1] : d[s >> 1];

            char[] aa = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            string dd = "";
            for (int s = 0; s < jj.Length; s++)
            {
                dd += aa[jj[s] >> 4 & 15];
                dd += aa[jj[s] & 15];
            }
            return dd;
        }
         
        private class b
        {
            public b(int bb, int i)
            {
                this.s = bb | 0;
                this.e = i | 0;
            }
            public int s;
            public int e;

        }
        /// <summary>
        /// 获取好友列表的hash算法
        /// </summary>
        /// <param name="i"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public string Hash(UInt64 i, string a)
        {
            UInt64[] r = new UInt64[4];
            r[0] = i >> 24 & 255;
            r[1] = i >> 16 & 255;
            r[2] = i >> 8 & 255;
            r[3] = i & 255;
            char[] j = a.ToCharArray();
            Stack<b> e = new Stack<b>();
            for (e.Push(new b(0, j.Length - 1)); e.Count > 0; )
            {
                b c = e.Pop();
                if (!(c.s >= c.e || c.s < 0 || c.e >= j.Length)) if (c.s + 1 == c.e)
                    {
                        if (j[c.s] > j[c.e])
                        {
                            char l = j[c.s];
                            j[c.s] = j[c.e];
                            j[c.e] = l;
                        }
                    }
                    else
                    {
                        int l;
                        int J;
                        char f;
                        for (l = c.s, J = c.e, f = j[c.s]; c.s < c.e; )
                        {
                            for (; c.s < c.e && j[c.e] >= f; )
                            { c.e--; r[0] = r[0] + 3 & 255; }
                            if (c.s < c.e)
                            {
                                j[c.s] = j[c.e]; c.s++; r[1] = r[1] * 13 + 43 & 255;
                            }
                            for (; c.s < c.e && j[c.s] <= f; )
                            {
                                c.s++;
                                r[2] = r[2] - 3 & 255;
                            }
                            if (c.s < c.e) { j[c.e] = j[c.s]; c.e--; r[3] = (r[0] ^ r[1] ^ r[2] ^ r[3] + 1) & 255; }
                        }
                        j[c.s] = f;
                        e.Push(new b(l, c.s - 1));
                        e.Push(new b(c.s + 1, J));
                    }
            }
            j = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            string e1 = "";
            for (int c = 0; c < r.Length; c++)
            {
                e1 += j[r[c] >> 4 & 15];
                e1 += j[r[c] & 15];
            }
            return e1;
        }
        */
        
        /// <summary>
        /// 获取skey
        /// </summary>
        /// <returns></returns>
        public string skey()
        {
            if (m_skey.Length > 0) return m_skey;

            string cookie = h.GetCookie();
            int p = cookie.IndexOf("skey=");
            if (p > 0) m_skey = cookie.Substring(p + 5, 10);
            return m_skey;
        }
        /// <summary>
        /// 获取好友消息中的图片或表情
        /// </summary>
        /// <param name="uin">好友uin</param>
        /// <param name="cface">cface对象</param>
        /// <returns></returns>
        public Image GetFriendMsgImage(string uin, CFace cface)
        {
            if (cface.face.Length > 0)
            {
                return h.GetImage("http://0.web.qstatic.com/webqqpic/style/face/" + cface.face.Trim() + ".gif");
            }
            if (cface.file_path.Length > 0)
            {


                h.HttpSendData("http://d.web2.qq.com/channel/get_offpic2?file_path=" + cface.file_path.Trim().Replace("/", "%2F") + "&f_uin=" + uin + "&clientid=" + clientid + "&psessionid=" + m_psessionid, "get", "", "utf-8", false);
                string ImageURL = h.GetHeaders()["Location"];
                return h.GetImage(ImageURL);

            }
            else
                return null;

        }
        /// <summary>
        /// 获取群消息中的图片或表情
        /// </summary>
        /// <param name="code">群code</param>
        /// <param name="uin">好友uin</param>
        /// <param name="cface">cface对象</param>
        /// <returns></returns>
        public Image GetGroupMsgImage(string code, string uin, CFace cface)
        {
            if (cface.face.Length > 0)
            {
                return h.GetImage("http://0.web.qstatic.com/webqqpic/style/face/" + cface.face + ".gif");
            }
            else if (cface.server.IndexOf(':') > 0 && cface.name.Length > 0)
            {
                string[] a = cface.server.Split(':');
                return h.GetImage("http://webqq.qq.com/cgi-bin/get_group_pic?type=0&gid=" + code + "&uin=" + uin + "&rip=" + a[0] + "&rport=" + a[1] + "&fid=" + cface.file_id + "&pic=" + cface.name + "&vfwebqq=" + m_vfwebqq + "&t=" + CFun.GetRnd());
            }
            else
                return null;
        }
        /*private string Upload_Friend_Pic(string fileName)
        {
            string timeStamp = GetTimeStamp().ToString();
            string time = DateTime.Now.Ticks.ToString("x");

            string url = string.Format("http://weboffline.ftn.qq.com/ftn_access/upload_offline_pic?time={0}", timeStamp);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("callback", "parent.EQQ.Model.ChatMsg.callbackSendPic");
            dic.Add("locallangid", "2052");
            dic.Add("clientversion", "1409");
            dic.Add("uin", GetMyQQ());
            dic.Add("skey", m_skey);
            dic.Add("appid", "1002101");//"15000101");
            dic.Add("peeruin", "593023668");
            dic.Add("fileid", this.m_fileId.ToString());
            dic.Add("vfwebqq", vfwebqq());
            dic.Add("senderviplevel", "0");
            dic.Add("reciverviplevel", "0");
            this.m_fileId++;
            string ret = SubmitData(url, fileName, dic, "file");
            MessageBox.Show(ret);
            string imagePath = new Regex(@"""filepath"":""(?'filepath'[^\""]+)""").Match(ret).Groups["filepath"].Value.Replace("\\", "").Replace("/", "");
            MessageBox.Show(imagePath);
            return imagePath;
        }*/
        private string Upload_Group_Pic(string fileName)
        {
            string timeStamp = GetTimeStamp().ToString();
            string time = DateTime.Now.Ticks.ToString("x");

            string url = string.Format("http://up.web2.qq.com/cgi-bin/cface_upload?time={0}", timeStamp);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("from", "control");
            dic.Add("f", "EQQ.Model.ChatMsg.callbackSendPicGroup");
            dic.Add("vfwebqq", vfwebqq());
            string ret = SubmitData(url, fileName, dic, "custom_face");
            //string imagePath = new Regex(@"""filepath"":""(?'filepath'[^\""]+)""").Match(ret).Groups["filepath"].Value.Replace("\\", "").Replace("/", "");

            string imagePath = "";
            if (ret.IndexOf("ret':0") > 0)
                imagePath = ret.Substring(ret.IndexOf("msg") + 6, ret.IndexOf("});") - ret.IndexOf("msg") - 7);
            if (ret.IndexOf("ret':4") > 0)
            {
                int p = ret.IndexOf("msg");
                imagePath = ret.Substring(p + 6, ret.IndexOf(" ", p) - p - 6);
            }
            return imagePath;
        }
        private static long GetTimeStamp()//获取时间戳
        {
            DateTime dateTime = DateTime.Now;
            DateTime startDate = new DateTime(1970, 1, 1);
            DateTime endDate = dateTime.ToUniversalTime();
            TimeSpan span = endDate - startDate;
            return (long)(span.TotalMilliseconds + 0.5);
        }
        private string SubmitData(string url, string fileName, Dictionary<string, string> dic, string typename)
        {
            string html = strHead + "图片上传失败!";
            try
            {
                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                httpWebRequest.CookieContainer = h.GetCookieContainer();
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                httpWebRequest.Method = "POST";
                StringBuilder sb = new StringBuilder();

                if (dic.Count != 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        sb.Append("--");
                        sb.Append(boundary);
                        sb.Append("\r\n");
                        sb.Append("Content-Disposition: form-data; name=\"" + kvp.Key + "\"\r\n\r\n");
                        sb.Append(kvp.Value);
                        sb.Append("\r\n");
                    }
                }
                string shortfilename = "1.jpg";// fileName.Substring(fileName.LastIndexOf("\\") + 1, fileName.Length - fileName.LastIndexOf("\\") - 1);

                sb.Append("--");
                sb.Append(boundary);
                sb.Append("\r\n");
                sb.Append("Content-Disposition: form-data; name=\"" + typename + "\"; filename=\"" + shortfilename + "\"");
                //sb.Append(shortfilename);
                //sb.Append("\"");
                sb.Append("\r\n");
                sb.Append("Content-Type: image/pjpeg");
                sb.Append("\r\n");
                sb.Append("\r\n");

                string postHeader = sb.ToString();
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
                byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);


                long length = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpWebRequest.AllowWriteStreamBuffering = false;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.ContentLength = length;

                Stream requestStream = httpWebRequest.GetRequestStream();

                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
                long filebytes = fileStream.Length;
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    requestStream.Write(buffer, 0, bytesRead);
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

                WebResponse webResponse2 = httpWebRequest.GetResponse();
                Stream stream = webResponse2.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream);
                html = streamReader.ReadToEnd();
                requestStream.Close();
                fileStream.Close();
            }
            catch
            {
            }
            return html;

        }
        private string Upload_4_Pic(string bmpurl, string sign)
        {
            string html = strHead + "图片上传失败！";
            try
            {
                string timeStamp = GetTimeStamp().ToString();
                string time = DateTime.Now.Ticks.ToString("x");

                string url = "http://up.web2.qq.com/cgi-bin/CustomPortraitUpload/UploadPortrait";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("sign", sign);
                dic.Add("clientuin", GetMyQQ());
                dic.Add("max_file_size", "100000");
                dic.Add("case", "2");
                dic.Add("url", bmpurl);

                dic.Add("clientkey", "undefined");

                string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
                httpWebRequest.CookieContainer = h.GetCookieContainer();
                httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                httpWebRequest.Method = "POST";
                StringBuilder sb = new StringBuilder();

                if (dic.Count != 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        sb.Append("--");
                        sb.Append(boundary);
                        sb.Append("\r\n");
                        sb.Append("Content-Disposition: form-data; name=\"" + kvp.Key + "\"\r\n\r\n");
                        sb.Append(kvp.Value);
                        sb.Append("\r\n");
                    }
                }
                string shortfilename = "";//fileName.Substring(fileName.LastIndexOf("\\") + 1, fileName.Length - fileName.LastIndexOf("\\") - 1);

                sb.Append("--");
                sb.Append(boundary);
                sb.Append("\r\n");
                sb.Append("Content-Disposition: form-data; name=\"customfacefile\"; filename=\"" + shortfilename + "\"");
                sb.Append(shortfilename);
                sb.Append("\"");
                sb.Append("\r\n");
                sb.Append("Content-Type: application/octet-stream");
                sb.Append("\r\n");
                sb.Append("\r\n");
                sb.Append("\r\n--" + boundary + "--\r\n");
                string postHeader = sb.ToString();
                byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);




                long length = postHeaderBytes.Length;
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpWebRequest.AllowWriteStreamBuffering = false;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                httpWebRequest.ContentLength = length;

                Stream requestStream = httpWebRequest.GetRequestStream();

                requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                WebResponse webResponse2 = httpWebRequest.GetResponse();
                Stream stream = webResponse2.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream);

                html = streamReader.ReadToEnd();
                requestStream.Close();
            }
            catch { }
            return html;

        }

        /// <summary>
        /// 设置头像
        /// </summary>
        /// <param name="FullFilePath">本地图片文件</param>
        /// <returns></returns>
        public string SetHeadImage(string FullFilePath)
        {
            if (FullFilePath != "" && File.Exists(FullFilePath))
            {
                string sign = "";
                string s = Upload_HeadImage_Pic(FullFilePath, out sign);
                int p = s.IndexOf("http");
                string url = s.Substring(p, s.IndexOf("bmp") - p + 3);
                return Upload_4_Pic(url, sign);
            }
            else
            {
                return "文件不存在";
            }
        }
        private string Upload_HeadImage_Pic(string fileName, out string sign)
        {
            string ret = "";

            string json = h.HttpSendData("http://d.web2.qq.com/channel/query_user_flag?type=1&psessionid=" + m_psessionid + "&clientid=" + clientid + "&t=" + GetTimeStamp());
            sign = new JSONObject(json).getJSONObject("result").getString("chead_sig");
            string timeStamp = GetTimeStamp().ToString();
            string time = DateTime.Now.Ticks.ToString("x");

            string url = "http://up.web2.qq.com/cgi-bin/CustomPortraitUpload/UploadPortrait";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("sign", sign);
            dic.Add("clientuin", GetMyQQ());
            dic.Add("max_file_size", "100000");
            dic.Add("case", "1");
            dic.Add("url", "");
            dic.Add("clientkey", "undefined");
            ret = SubmitData(url, fileName, dic, "customfacefile");
            //string imagePath = new Regex(@"""filepath"":""(?'filepath'[^\""]+)""").Match(ret).Groups["filepath"].Value.Replace("\\", "").Replace("/", "");


            return ret;
        }
        /* /// <summary>
         /// 发送图片给好友
         /// </summary>
         /// <param name="uin">好友uin</param>
         /// <param name="FullFilePath">本地图片路径</param>
         /// <returns></returns>
         public string SendPicToFriend(string uin, string FullFilePath)
         {

             string filesize = new System.IO.FileInfo(FullFilePath).Length.ToString();
             string lastname = Upload_Friend_Pic(FullFilePath);
             //   string t = GetTimestamp(DateTime.Now).ToString ();
             //   h.SetReferer("http://web.qq.com/index.html?t="+t);
             //   h.HttpSendData("http://pinghot.qq.com/pingd?dm=web.qq.com.hot&url=/index.html&arg=t%3D"+t+"&hottag=web2qq.single.toolbar.Insertimage&hotx=9999&hoty=9999&rand=39997");
             string shortfilename = FullFilePath.Substring(FullFilePath.LastIndexOf("\\") + 1, FullFilePath.Length - FullFilePath.LastIndexOf("\\") - 1);
             //   h.HttpSendData("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
             // string s= h.HttpSendData("http://d.web2.qq.com/channel/apply_offline_pic_dl2?f_uin="+QQ+"&file_path=%2F"+lastname+"&clientid="+clientid +"&psessionid="+psessionid +"&t="+GetTimestamp (DateTime.Now ));
             //  string imageurl = s.Substring(s.IndexOf("http"), s.IndexOf("succ") - s.IndexOf("http") - 3).Replace("\\u0026", "&");
             // byte[] img=h.GetImage(imageurl);
             string url = "http://d.web2.qq.com/channel/send_buddy_msg2";
             string data = "r=%7B%22to%22%3A" + uin + "%2C%22face%22%3A606%2C%22content%22%3A%22%5B%5B%5C%22offpic%5C%22%2C%5C%22%2F" + lastname + "%5C%22%2C%5C%22" + shortfilename + "%5C%22%2C" + filesize + "%5D%2C%5C%22%5C%22%2C%5C%22%5C%22%2C%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22%5C%5Cu5b8b%5C%5Cu4f53%5C%22%2C%5C%22size%5C%22%3A%5C%2210%5C%22%2C%5C%22style%5C%22%3A%5B0%2C0%2C0%5D%2C%5C%22color%5C%22%3A%5C%22000000%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A" + CFun.GetRnd(8) + "%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;
             h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
             h.HttpSendData(url, "post", data);
             return lastname;

         }*/
        /// <summary>
        /// 发送图片给群
        /// </summary>
        /// <param name="group_uin">群gid</param>
        /// <param name="group_code">群code</param>
        /// <param name="FullFilePath"></param>
        /// <returns></returns>
        public string SendPicToGroup(string group_uin, string group_code, string FullFilePath)
        {
            string imagePath = Upload_Group_Pic(FullFilePath);
            if (imagePath == "") return "err";
            string url = "http://d.web2.qq.com/channel/get_gface_sig2?clientid=" + clientid + "&psessionid=" + m_psessionid + "&t=" + GetTimeStamp();
            string ret = h.HttpSendData(url);
            string gface_key = "";
            string gface_sig = "";
            if (ret.IndexOf("retcode\":0") > 0)
            {
                gface_key = ret.Substring(ret.IndexOf("gface_key") + 12, ret.IndexOf("gface_sig") - ret.IndexOf("gface_key") - 15);
                gface_sig = ret.Substring(ret.IndexOf("gface_sig") + 12, ret.IndexOf("\"}}") - ret.IndexOf("gface_sig") - 12);
                string url1 = "http://d.web2.qq.com/channel/send_qun_msg2";
                string data = "r=%7B%22group_uin%22%3A" + group_uin + "%2C%22group_code%22%3A" + group_code + "%2C%22key%22%3A%22" + gface_key + "%22%2C%22sig%22%3A%22" + gface_sig + "%22%2C%22content%22%3A%22%5B%5B%5C%22cface%5C%22%2C%5C%22group%5C%22%2C%5C%22" + imagePath + "%5C%22%5D%2C%5C%22%5C%22%2C%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22%5C%5Cu5b8b%5C%5Cu4f53%5C%22%2C%5C%22size%5C%22%3A%5C%2210%5C%22%2C%5C%22style%5C%22%3A%5B0%2C0%2C0%5D%2C%5C%22color%5C%22%3A%5C%22000000%5C%22%7D%5D%5D%22%2C%22clientid%22%3A%22" + clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + clientid + "&psessionid=" + m_psessionid;
                ret = h.HttpSendData(url1, "post", data);
                return ret;
            }
            else
                return "图片发送失败！";

        }
        /// <summary>
        /// 获取省份城市信息
        /// </summary>
        /// <returns></returns>
        public string GetLocationData()
        {
            return h.HttpSendData("http://0.web.qstatic.com/webqqpic/js/qqweb.util.loclist.js?20110316");
        }
        /// <summary>
        /// 创建讨论组
        /// </summary>
        /// <param name="uinlist">讨论组成员的uin列表，列不包括自己</param>
        /// <param name="VCode">验证码</param>
        /// <returns></returns>
        public string CreateDiscu(string[] uinlist, string VCode)
        {
            string url = "http://d.web2.qq.com/channel/create_discu";
            string uins = "";
            for (int i = 0; i <= uinlist.GetUpperBound(0); i++)
            {
                if (uinlist[i].Trim() != m_QQ)
                    uins += uinlist[i].Trim() + "%22%2C%22";
            }
            uins = uins.Substring(0, uins.Length - 9);
            string data = "r=%7B%22discu_name%22%3A%22%22%2C%22mem_list%22%3A%5B%22" + m_QQ + "%22%2C%22" + uins + "%22%5D%2C%22mem_list_u%22%3A%5B%5D%2C%22mem_list_g%22%3A%5B%5D%2C%22code%22%3A%22" + VCode + "%22%2C%22verifysession%22%3A%22" + m_verifysession + "%22%2C%22clientid%22%3A%22" + m_clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D&clientid=" + m_clientid + "&psessionid=" + m_psessionid;
            return h.HttpSendData(url, "post", data);

        }
        /// <summary>
        /// 获取讨论组资料信息
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public string GetDiscuInfo(string did)
        {
            return h.HttpSendData("http://d.web2.qq.com/channel/get_discu_info?did=" + did + "&clientid=" + m_clientid + "&psessionid=" + m_psessionid + "&vfwebqq=" + m_vfwebqq + "&t=" + GetTimeStamp());
        }
        /// <summary>
        /// 当创建讨论组需要的验证码
        /// </summary>
        /// <returns></returns>
        public Image GetDiscuVC()
        {
            Image img = h.GetImage("http://captcha.qq.com/getimage?aid=1003901&" + GetTimeStamp());
            this.m_verifysession = verifysession(h.GetCookie());
            return img;
        }
        /// <summary>
        /// 向讨论组发消息
        /// </summary>
        /// <param name="did">讨论组id，从讨论组列表中可以获取did</param>
        /// <param name="Content">消息内容</param>
        /// <param name="FullFilePath">图片文件完整路径</param>
        /// <param name="FontName">字体名称</param>
        /// <param name="FontSize">字体大小</param>
        /// <param name="FontColor">字体颜色</param>
        /// <param name="BIU_SetValue">字体样式</param>
        /// <returns>是否成功</returns>
        public string SendMsgToDiscu(string did, string Content, string FullFilePath = "", string FontName = "宋体", string FontSize = "10", string FontColor = "000000", string BIU_SetValue = "0,0,0")//发送群某个好友也可以
        {
            string ret = "";
            if (FontName == "楷体") FontName = "楷体_GB2312";
            BIU_SetValue = BIU_SetValue.Replace(",", "%2C").Replace("，", "%2C");
            string url = "http://d.web2.qq.com/channel/send_discu_msg2";
            if (FullFilePath != "" && File.Exists(FullFilePath))
            {

                string data = "";
                string imagePath = Upload_Group_Pic(FullFilePath);
                if (imagePath == "") return "upload fail";
                string tmp = "";
                string url1 = "http://d.web2.qq.com/channel/get_gface_sig2?clientid=" + m_clientid + "&psessionid=" + m_psessionid + "&t=" + GetTimeStamp();
                ret = h.HttpSendData(url1);
                tmp += ret + "\r\n";
                string gface_key = "";
                string gface_sig = "";
                if (ret.IndexOf("retcode\":0") > 0)
                {
                    gface_key = ret.Substring(ret.IndexOf("gface_key") + 12, ret.IndexOf("gface_sig") - ret.IndexOf("gface_key") - 15);
                    gface_sig = ret.Substring(ret.IndexOf("gface_sig") + 12, ret.IndexOf("\"}}") - ret.IndexOf("gface_sig") - 12);
                    data = "r=%7B%22did%22%3A%22" + did + "%22%2C%22key%22%3A%22" + gface_key + "%22%2C%22sig%22%3A%22" + gface_sig + "%22%2C%22content%22%3A%22%5B%5B%5C%22cface%5C%22%2C%5C%22group%5C%22%2C%5C%22FF8CFA2FA55CA36B3CC7A190BF127527.jPg%5C%22%5D%2C" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22clientid%22%3A%22" + m_clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + m_clientid + "&psessionid=" + m_psessionid;
                    h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
                    ret = h.HttpSendData(url, "post", data);
                }
                return tmp += ret + "\r\n" + data;
            }
            else
            {
                string data = "r=%7B%22did%22%3A%22" + did + "%22%2C%22content%22%3A%22%5B" + ce.ToUTF8(GetSendContent(ce.ToUnicode(Content)), true, true) + "%5C%22%5C%22%2C%5C%22%5C%22%2C%5B%5C%22font%5C%22%2C%7B%5C%22name%5C%22%3A%5C%22" + ce.ToGB2312(ce.ToUnicode(FontName)) + "%5C%22%2C%5C%22size%5C%22%3A%5C%22" + FontSize + "%5C%22%2C%5C%22style%5C%22%3A%5B" + BIU_SetValue + "%5D%2C%5C%22color%5C%22%3A%5C%22" + FontColor + "%5C%22%7D%5D%5D%22%2C%22msg_id%22%3A15120001%2C%22clientid%22%3A%22" + m_clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%7D&clientid=" + m_clientid + "&psessionid=" + m_psessionid;
                h.SetReferer("http://d.web2.qq.com/proxy.html?v=20110331002&callback=1&id=2");
                return h.HttpSendData(url, "post", data) + "\r\n" + data;
            }
        }
        /// <summary>
        /// 获取讨论组列表
        /// </summary>
        /// <returns>讨论组列表信息，包括讨论组名，did,成员信息</returns>
        public string GetDiscuList()
        {
            return h.HttpSendData("http://s.web2.qq.com/api/get_discus_list?clientid=" + m_clientid + "&psessionid=" + m_psessionid + "&vfwebqq=" + m_vfwebqq + "&t=" + GetTimeStamp());
        }
        /// <summary>
        /// 退出讨论组
        /// </summary>
        /// <param name="did">讨论组id</param>
        /// <returns>返回结果</returns>
        public string QuitDiscu(string did)
        {
            string url = "http://d.web2.qq.com/channel/quit_discu?did=" + did + "&clientid=" + m_clientid + "&psessionid=" + m_psessionid + "&vfwebqq=" + m_vfwebqq + "&t=" + GetTimeStamp();
            return h.HttpSendData(url);
        }
        /// <summary>
        /// 修改讨论组名称
        /// </summary>
        /// <param name="did">讨论组id</param>
        /// <param name="newname">讨论组名称</param>
        /// <returns>返回结果</returns>
        public string SetDiscuName(string did, string newname)
        {
            string url = "http://d.web2.qq.com/channel/modify_discu_info";
            string data = "r=%7B%22did%22%3A%22" + did + "%22%2C%22discu_name%22%3A%22" + ce.ToUTF8(ce.ToUnicode(newname)) + "%22%2C%22dtype%22%3A1%2C%22clientid%22%3A%22" + m_clientid + "%22%2C%22psessionid%22%3A%22" + m_psessionid + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D&clientid=" + m_clientid + "&psessionid=" + m_psessionid;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取一个月内的消息记录
        /// </summary>
        /// <param name="uin">对方uin</param>
        /// <param name="page">第几页</param>
        /// <returns></returns>
        public string GetMsgRecord(string uin, int page = 1)
        {
            return h.HttpSendData("http://web2.qq.com/cgi-bin/webqq_chat/?cmd=1&tuin=" + uin + "&vfwebqq=" + m_vfwebqq + "&page=" + (page + 1) + "&row=10&callback=alloy.app.chatLogViewer.rederChatLog&t=" + GetTimeStamp());
        }
        /// <summary>
        /// 删除聊天记录
        /// </summary>
        /// <param name="uin">要删除聊天记录的好友uin</param>
        /// <returns>返回结果</returns>
        public string DelMsgRecord(string uin)
        {
            return h.HttpSendData("http://web2.qq.com/cgi-bin/webqq_chat/?cmd=2&tuin=" + uin + "&vfwebqq=" + m_vfwebqq + "&callback=alloy.app.chatLogViewer.showDeleteResult&t=" + GetTimeStamp());
        }
        /// <summary>
        /// 移动好友到其他分组
        /// </summary>
        /// <param name="uin">要移动的好友uin</param>
        /// <param name="newid">目的分组id</param>
        /// <returns>返回结果</returns>
        public string SetFriendGroup(string uin, string newid)
        {
            string url = "http://s.web2.qq.com/api/modify_friend_group";
            string data = "tuin=" + uin + "&newid=" + newid + "&vfwebqq=" + m_vfwebqq;
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 设置群中自己的名片
        /// </summary>
        /// <param name="gcode">可以从群列表中获取gcode</param>
        /// <param name="name">修改后的昵称</param>
        /// <param name="phone">修改后的电话</param>
        /// <param name="email">修改后的邮件</param>
        /// <param name="remark">修改后的备注</param>
        /// <returns></returns>
        public string SetMyCardInGroup(string gcode, string name, string phone, string email, string remark)
        {
            string url = "http://s.web2.qq.com/api/update_group_business_card2";
            //r=%7B%22gcode%22%3A%22vfwebqq%22%3A%2236030c7dc17a16e32d0b17800ff2be682b8654d7dbd35a3e6445552364e60b83f1df41c3f3029b5d%22%7D
            string data = "r=%7B%22gcode%22%3A%22" + gcode + "%22%2C%22phone%22%3A%22" + phone + "%22%2C%22email%22%3A%22" + ce.ToUTF8(email) + "%22%2C%22remark%22%3A%22" + ce.ToUTF8(ce.ToUnicode(remark)) + "%22%2C%22name%22%3A%22" + ce.ToUTF8(ce.ToUnicode(name)) + "%22%2C%22vfwebqq%22%3A%22" + m_vfwebqq + "%22%7D";
            return h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 获取自己的群名片
        /// </summary>
        /// <param name="gcode">所在的群gocde</param>
        /// <returns>返回自己在群名片中的信息</returns>
        public string GetMyCardInGroup(string gcode)
        {
            return h.HttpSendData("http://s.web2.qq.com/api/get_self_business_card2?gcode=" + gcode + "&vfwebqq=" + m_vfwebqq + "&t=" + GetTimeStamp());
        }
        /*  public string SetGroupMsgFilter(string [] gcode,int [] FilterType)
          {string gcodes="";
          for (int i = 0; i <= gcode.GetUpperBound(0); i++)
          {
              if (i < gcode.GetUpperBound(0))
              gcodes += gcode[i] + "%22%3A%22" + FilterType + "%22%2C%22";
              else
                  gcodes += gcode[i] + "%22%3A%22" + FilterType ;
          } string url = "http://cgi.web2.qq.com/keycgi/qqweb/uac/messagefilter.do";
           string data = "retype=1&app=EQQ&itemlist=%7B%22groupmask%22%3A%7B%22cAll%22%3Anull%2C%22idx%22%3A1074%2C%22port%22%3A50168%2C%22"+gcodes +"%22%7D%7D&vfwebqq=" + m_vfwebqq;
              return h.HttpSendData(url,"post",data);
          }*/
        private void Follow()
        {
            string url = "http://api.t.qq.com/old/follow.php";
            h.SetReferer("http://api.t.qq.com/proxy.html");
            string data = "u=yiwowang&veriCode=&lieuId=&apiType=14&apiHost=http%3A%2F%2Fapi.t.qq.com&g_tk=" + g_tk();
            h.HttpSendData(url, "post", data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /*  public string FriendRelationship(string QQnumber)
          {
              string url = "http://ocial.show.qq.com/cgi-bin/qqshow_camera_noname?g_tk="+m_g_tk;
              string data = "uin=" + QQnumber + "%7C" + GetMyQQ() + "&friend=1";
              h.SetReferer("http://social.show.qq.com/qqshow_xhr.html");
           return   h.HttpSendData(url,"post",data);
          }*/
    }


}
