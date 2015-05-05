using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace yiwoSDK
{
    public class CWeiXin
    {
        public struct MsgType
        {
            public const int MSG_TYPE_LOGIN_ON = 0;
            public const int MSG_TYPE_CHAT_MSG = 1;
            public const int MSG_TYPE_LOGIN_OFF = 2;
        }
        public delegate void dMsg(int MsgType, string Msg);
        public event dMsg eMsg;
        private CHttpWeb ch = new CHttpWeb();
        private CEncode ce = new CEncode();
        private string m_uuid = "";
        private bool IsStart = true;
        private string skey = "";
        private Thread thLoopPoll;
        private Form formObject = null;
        private string wxsid = "";
        private string wxuin = "";
        private string username = "";
        public bool IsLoginSucc = false;
        private const string deviceid = "e417913674959579";
        private string SyncKeyString = "";//
        private string SyncKeyJson = "";//
        public void SetCHttpWeb(CHttpWeb ch)
        {
            this.ch = ch;
        }
        /// <summary>
        /// 绑定消息函数，可以在绑定的函数中使用控件
        /// </summary>
        /// <param name="form">当前窗体From对象</param>
        /// <param name="DReceiveMsg">触发接受消息的函数</param>
        public void BindEventReceiveMsg(Form form, CWeiXin.dMsg DReceiveMsg)
        {
            formObject = form;
            this.eMsg += DReceiveMsg;

        }
        /// <summary>
        /// 绑定消息函数，注意不要在绑定的函数中使用控件
        /// </summary>
        /// <param name="DReceiveMsg">触发接受消息的函数</param>
        public void BindEventReceiveMsg(CWeiXin.dMsg DReceiveMsg)
        {
            this.eMsg += DReceiveMsg;

        }
        private string Getuuid()
        {
            if (m_uuid.Length == 0)
            {

                string ret = ch.HttpSendData("https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb&redirect_uri=https%3A%2F%2Fwx.qq.com%2Fcgi-bin%2Fmmwebwx-bin%2Fwebwxnewloginpage&fun=new&lang=zh_CN&_=" + CFun.GetRnd());
                m_uuid = ret.Substring(ret.IndexOf("uuid") + 8, 14);
            }
            return m_uuid;
        }
        /// <summary>
        /// 获取头像
        /// </summary>
        /// <param name="UserName">用户的username</param>
        /// <returns></returns>
        public Image GetHeadImage(string UserName)
        {
            return ch.GetImage("http://wx.qq.com/cgi-bin/mmwebwx-bin/webwxgeticon?seq=620430187&username=" + UserName + "&skey=" + skey);
        }
        /// <summary>
        /// 获取登录二维码
        /// </summary>
        /// <returns></returns>
        public Image GetLoginQRCodeImage()
        {
            Image img = ch.GetImage("https://login.weixin.qq.com/qrcode/" + Getuuid() + "?t=webwx");
            thLoopPoll = new Thread(LoopPoll);
            thLoopPoll.Start();
            return img;

        }
        private string SendLoginPoll()
        {
            return ch.HttpSendData("https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?uuid=" + Getuuid() + "&tip=1&_=" + CFun.GetTimestamp());

        }
        private string SendCheckPoll()
        {
            ch.SetReferer("https://wx.qq.com/?&lang=zh_CN");
            long time = CFun.GetTimestamp();
            return ch.HttpSendData("https://webpush.weixin.qq.com/cgi-bin/mmwebwx-bin/synccheck?callback=jQuery1830261701937092988_" + time + "&uin=" + wxuin + "&deviceid=" + deviceid + "&synckey=" + SyncKeyString + "&sid=" + wxsid + "&r=" + time + "&skey=" + ce.ToUTF8(skey) + "&_=" + time);
        }
        private string SendGetMsgPoll()
        {
            string url = "https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxsync?&sid=" + wxsid + "&r=" + CFun.GetRnd() + "&skey=" + ce.ToUTF8(skey);
            string data = "{\"BaseRequest\":{\"Uin\":"+wxuin+",\"Sid\":\""+wxsid+"\"},"+SyncKeyJson+",\"rr\":"+CFun.GetTimestamp()+"}";
            return ch.HttpSendData(url,"post",data);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="ToUserName">对方username</param>
        /// <param name="Content">消息内容</param>
        /// <returns></returns>
        public string SendMsg(string ToUserName, string Content)
        {//{"BaseRequest":{"Uin":766342541,"Sid":"eNXAnHtD7Lr66N66","Skey":"@crypt_6071fcfe_631aecf5d9e3a14700fd0431d76e6d5d","DeviceID":"e592316104767913"},"Msg":{"FromUserName":"qq958796636","ToUserName":"wxid_naxpgcw673eh21","Type":1,"Content":"shuohua","ClientMsgId":1400153580714,"LocalID":1400153580714}}
            
            long time = CFun.GetTimestamp();
            string url = "https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxsendmsg?sid=" + ce.ToUTF8(wxsid, true) + "&r=" + time + "&skey=" + ce.ToUTF8(skey, true);
            string data = "{\"BaseRequest\":{\"Uin\":" + wxuin + ",\"Sid\":\"" + wxsid + "\",\"Skey\":\"" + skey + "\",\"DeviceID\":\"" + deviceid + "\"},\"Msg\":{\"FromUserName\":\"" + username + "\",\"ToUserName\":\"" + ToUserName + "\",\"Type\":1,\"Content\":\"" + Content.Replace("\"","\\\"") + "\",\"ClientMsgId\":" + time + ",\"LocalID\":" + time + "}}";
            ch.SetReferer("https://wx.qq.com/");
            return ch.HttpSendData(url, "post", data);
        }

        private void LoopPoll()
        {
            thLoopPoll.IsBackground = true;
            IsLoginSucc = false;
            IsStart = true;
            int MsgRecode = 0;
            int MsgSelector = 0;

            int iMsgType = 0;
            string MsgString = "";
            while (IsStart)
            {
                Application.DoEvents();

                if (IsLoginSucc)
                {
                    string msgRet = SendCheckPoll();

                    JSONObject jo = new JSONObject(msgRet.Replace("window.synccheck=", ""));
                    MsgRecode = Convert.ToInt32(jo.getString("retcode"));
                    MsgSelector = Convert.ToInt32(jo.getString("selector"));
                   // MessageBox.Show(msgRet+"==="+MsgRecode+"==="+MsgSelector);
                    if (MsgRecode == 0)
                    {
                        switch (MsgSelector)
                        {
                            case 0:
                                continue;
                            case 2:
                                MsgString = SendGetMsgPoll();
                                //MessageBox.Show(MsgString.Substring(MsgString.Length/2));
                                JSONObject joMsg = new JSONObject(MsgString);
                                long ret = joMsg.getJSONObject("BaseResponse").getLong("Ret");
                                GetSyncKeyFromJson(joMsg);
                                if (ret==1101)
                                {
                                    iMsgType = MsgType.MSG_TYPE_LOGIN_OFF;
                                    MsgString = "您已经下线，是手机端微信退出或者终止的";
                                    IsStart = false;
                                }
                                else if (ret==-1)
                                {//自己发消息就会返回
                                    continue;
                                }
                                else if (ret == 0)
                                {
                                    iMsgType = MsgType.MSG_TYPE_CHAT_MSG;
                                }
                                else
                                {//解析出错
                                    iMsgType = MsgType.MSG_TYPE_CHAT_MSG;
                                }
                                break;

                        }
                    }
                    else if (MsgRecode== 1101)
                    {
                        iMsgType = MsgType.MSG_TYPE_LOGIN_OFF;
                        //MsgString = "您已经下线，是手机端微信退出或者终止的";
                        IsStart = false;
                    }
                    if (eMsg != null)
                    {
                        //JSONObject(msgRet);

                        if (formObject != null)
                        {
                            formObject.Invoke(eMsg, new object[] { iMsgType, MsgString });
                        }
                        else
                        {
                            eMsg(iMsgType, MsgString);
                        }
                    }

                }
                else
                {
                    string ret = SendLoginPoll();
                    if (ret.IndexOf("=201") > 0)
                    {
                        IsLoginSucc = true;
                        if (eMsg != null)
                        {
                            this.LoadCookie();
                            wxsid = ch.GetCookieByKey("wxsid");
                            wxuin = ch.GetCookieByKey("wxuin");
                            if (username.Length == 0)
                            {
                                string json = GetRecentList();
                                JSONObject jo = new JSONObject(json);
                                JSONObject jo1 = jo.getJSONObject("User");
                                username = jo1.getString("UserName");
                                SyncKeyString = GetSyncKeyFromJson(jo);
                            }
                            if (formObject != null)
                            {
                                formObject.Invoke(eMsg, new object[] { MsgType.MSG_TYPE_LOGIN_ON, "登陆成功！" });
                            }
                            else
                            {

                                eMsg(MsgType.MSG_TYPE_LOGIN_ON, "登陆成功！");
                            }

                        }

                    }
                }
            }
        }
        private string GetSyncKeyFromJson(JSONObject jo)
        {
            JSONObject jo2 = jo.getJSONObject("SyncKey");
            JSONArray ja = jo2.getJSONArray("List");
            if (jo2 != null && ja != null)
            {
                SyncKeyJson = "\"SyncKey\":{" + jo2.ToString();
                SyncKeyString = "";
                for (int i = 0; i < ja.length(); i++)
                {
                    JSONObject jo3 = ja.getJSONObject(i);

                    SyncKeyString += jo3.getLong("Key") + "_" + jo3.getLong("Val") + "%7C";

                }
                SyncKeyString = SyncKeyString.Substring(0, SyncKeyString.Length - 3);
            }
            return SyncKeyString;
        }
        private bool LoadCookie()
        {
            try
            {
                string ret = ch.HttpSendData("https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?uuid=" + Getuuid() + "&tip=0&_=1385468657332");

                string url = ret.Substring(ret.IndexOf("uri=") + 5, ret.Length - ret.IndexOf("uri=") - 7);
                ch.SetReferer("https://wx.qq.com/?&lang=zh_CN");
                ret = ch.HttpSendData(url, "get", "", "UTF-8", false);
                skey = ret.Substring(ret.IndexOf("<skey>") + 6, ret.IndexOf("</skey>") - ret.IndexOf("<skey>") - 6);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 获取最近聊天用户
        /// </summary>
        /// <returns></returns>
        public string GetRecentList()
        {
            return ch.HttpSendData("https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxinit?r=" + CFun.GetRnd() + "&skey=" + ce.ToUTF8(skey));
        }
        /// <summary>
        /// 获取微信好友列表
        /// </summary>
        /// <returns></returns>
        public string GetFriendList()
        {
            return ch.HttpSendData("https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxgetcontact?r=" + CFun.GetRnd() + "&skey=" + ce.ToUTF8(skey));
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        public void LoginOut()
        {
            IsStart = false;
            if (IsLoginSucc)
            {
                string url = "https://wx.qq.com/cgi-bin/mmwebwx-bin/webwxlogout?redirect=1&type=0&skey=" + ce.ToUTF8(skey);
                string data = "sid=" + wxsid + "&uin=" + wxuin;
                ch.HttpSendData(url, "post", data);
            }
        }
    }

}
