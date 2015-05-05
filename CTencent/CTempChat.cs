using System;
using System.Collections.Generic;
using System.Text;
using yiwoSDK;
using System.Drawing;

namespace yiwoSDK
{
   public class CTempChat
    {
        CHttpWeb ch = new CHttpWeb();
        CEncode ce = new CEncode();
        string KEY = "";
        string QQ = "12345678";
        string ToQQ = "12345678";
        private static int num = 0;
        public string getMyQQ()
        {
            return QQ;
        }

        /// <summary>
        /// 初始化KEY
        /// </summary>
        /// <param name="ToQQ"></param>
        /// <returns></returns>
        private bool InitKEY(string ToQQ)
        {
            try
            {
                if (this.ToQQ != ToQQ)
                {
                    this.ToQQ = ToQQ;
                    string html = ch.HttpSendData("http://wpa.qq.com/msgrd?v=3&uin=" + ToQQ + "&site=qq&menu=yes");
                    int p = html.IndexOf("var tencentSeries");

                    if (p > 0)
                    {

                        KEY = html.Substring(html.IndexOf(@"\u0026sigT="), html.IndexOf("reportPool") - html.IndexOf(@"\u0026sigT=") - 3);

                        KEY = KEY.Substring(KEY.IndexOf("sigT")).Replace(@"\u0026", "&");
                        return true;
                    }
                    else
                    {
                        // MessageBox.Show("该好友发送失败，确认对方否开启临时会话 QQ：" + ToQQ);
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 获取默认验证码，如果存在默认验证码返回4位，不存在返回大于4位字符
        /// </summary>
        /// <param name="QQ">需要登陆的QQ</param>
        /// <returns>验证码或其他</returns>
        public string getVCode(string QQ)
        {
            ch.SetReferer("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=716027604&style=12&dummy=1&s_url=http%3A%2F%2Fconnect.qq.com%2Fwidget%2Fwpa%2Fchat.html%3Ftuin%3D" + ToQQ + "%26" + ce.ToUTF8(KEY));
            string ret = ch.HttpSendData("http://check.ptlogin2.qq.com/check?regmaster=&uin=" + QQ + "&appid=716027604&js_ver=10062&js_type=1&login_sig=GHOt4u*mIj4FlDh*iq34G4YX8Gmod6bwjGcRAr*N1g8mcuvjnXkOPrLGtkTZ2XXn&u1=http%3A%2F%2Fconnect.qq.com%2Fwidget%2Fwpa%2Fchat.html%3Ftuin%3D" + ToQQ + "%26" + ce.ToUTF8(KEY));
            if (ret.IndexOf("heckVC('0'") > 0)
            {

                return ret.Substring(18, 4);
            }
            else
            {

                // pictureBox1.Image = getVCImage(QQ);
                return ret;
            }
        }
        /// <summary>
        /// 登陆QQ，和传统登陆不一样，不在线、不掉线
        /// </summary>
        /// <param name="QQ">QQ</param>
        /// <param name="Pass">密码</param>
        /// <param name="vcode">验证码</param>
        /// <returns>登陆结果</returns>
        public string Login(string QQ, string Pass, string vcode)
        {
            
           InitKEY(ToQQ);
            this.QQ = QQ;
            ch.SetReferer("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=716027604&style=12&dummy=1&s_url=http%3A%2F%2Fconnect.qq.com%2Fwidget%2Fwpa%2Fchat.html%3Ftuin%3D" + ToQQ + "%26" + ce.ToUTF8(KEY));
            string p = CQQHelper.GetP(QQ, Pass, vcode);
            string url = "http://ptlogin2.qq.com/login?u=" + QQ + "&p=" + p + "&verifycode=" + vcode.ToLower() + "&aid=716027604&u1=http%3A%2F%2Fconnect.qq.com%2Fwidget%2Fwpa%2Fchat.html%3Ftuin%3D" + ToQQ + "%26" + ce.ToUTF8(KEY) + "&h=1&ptredirect=1&ptlang=2052&from_ui=1&dumy=&low_login_enable=0&regmaster=&fp=loginerroralert&action=21-71-1389013340181&mibao_css=&t=12&g=1&js_ver=10062&js_type=1&login_sig=Z-TLNhb-ucrX1GPX1Kk82LqEHYUMc0GdykuqEyJCLInzmRi1-*pVEBMN8xVhRAdT&pt_rsa=0";
            return ch.HttpSendData(url);

        }
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <param name="QQ">登陆的QQ</param>
        /// <returns>验证码图片</returns>
        public Image getVCImage(string QQ)
        {
            return ch.GetImage("http://captcha.qq.com/getimage?uin=" + QQ + "&aid=716027604&0.038797497590868035");
        }
        /// <summary>
        /// 发送临时会话
        /// </summary>
        /// <param name="msg">临时会话消息</param>
        /// <param name="ToQQ">对方QQ，必须支持临时会话</param>
        /// <returns>true=成功，false=失败</returns>
        public int SendMsg(string msg, string ToQQ)
        {
            num++;
            if (num <= 5)
            {
                if (InitKEY(ToQQ))
                {
                    string url = "https://d.connect.qq.com/webim/user/send";
                    ch.SetReferer("https://d.connect.qq.com/proxy/sslproxy.html");
                    //pkg_num=1&index=0&msg_0=%5B%7B%22t%22%3A0%2C%22text%22%3A%22%5Cu963f%5Cu76db%5Cu5927%5Cu7684%22%7D%5D&sigt=1c04aad0e33203534acf407e26cc9bd67ee171f2c1142c338439a0e464a96b4a13e0447bf24672890deb089b5010101e&sigui=84124643b44d89c01b5b2d9790bf50921dea9404d632a0f294bc2339f59a7eded19f86db089e574b&tuin=958796636&clientid=1390148089
                    string data = "pkg_num=1&index=0&msg_0=%5B%7B%22t%22%3A0%2C%22text%22%3A%22" + ce.ToUTF8(ce.ToUnicode(msg)) + "%22%7D%5D&" + KEY.ToLower().Replace("sigu", "sigui") + "&tuin=" + ToQQ + "&clientid=1390148089";
                    string ret = ch.HttpSendData(url, "post", data);
                    return ret.IndexOf("retcode\":0") >= 0?0:1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {

                throw new System.ArgumentException("未购买只能发送5次消息，购买后会去掉限制，QQ958796636"); 
            }
        }


    }
}
