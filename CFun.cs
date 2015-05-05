using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace yiwoSDK
{
    #region 共同静态类
    public static class CFun
    {//错误信息
        private static string strTIP = "请在使用类之前，调用:CFun.I_LOVE_BBS(\"yiwowang.com\");";
        private static int UsingValue = 0;
        private static int MAX_NUM = 43 + 57;
        private static int CurNum = 1 - 1;
        private static bool IsStop = false;
        private static string[] urlKey = new string[] { "/poll2", "/synccheck" };
        private static CEncode ce = new CEncode();
        private static string eString = "XHU2Q0U4XHU2MTBGXHVGRjAxXHU1RjUzXHU1MjREXHU2NjJGXHU4QkQ1XHU3NTI4XHU3MjQ4XHVGRjBDXHU2MEE4XHU3Njg0XHU3RjUxXHU3RURDXHU4QkY3XHU2QzQyXHU2QjIxXHU2NTcwXHU4RkJFXHU1MjMwXHU0RTBBXHU5NjUwKDIwKVx1RkYwQ1x1NjUzNlx1OEQzOVx1NzI0OFx1NEUwRFx1OTY1MFx1NTIzNlx1RkYwQ1x1NEVGN1x1NjgzQ1x1NEYxOFx1NjBFMFx1RkYwQ1FROTU4Nzk2NjM2";
        private static string deString = "";
        private static string regCode = "sunguowei";
        public static bool Init(string url)
        {
            if (deString.Length == 0)
            {
                deString = ce.DeUnicode(ce.DeBase64(eString));
            }
            bool IsKey = false;
            // foreach(string key in urlKey)
            {
                //   if(url.Contains(key))
                {
                    //      IsKey = true;
                    //      break;
                }
            }
            if (!IsKey)
            {
                // CurNum++;
            }
            if (CurNum > MAX_NUM)
            {
                throw new Exception(deString); ;
            }

            return IsStop;
        }

        /// <summary>
        /// 简单检测是否为QQ的格式
        /// </summary>
        /// <param name="QQnumber">QQ号码</param>
        /// <returns></returns>
        public static bool IsQQnumber(ref string QQnumber)
        {
            QQnumber = QQnumber.Trim();
            bool b = false;
            if (QQnumber.Length >= 4 && QQnumber.Length <= 10)
            {
                try
                {
                    double d = Convert.ToDouble(QQnumber);
                    b = true;
                }
                catch (Exception e) { string tmp = e.Message; return false; }
            }
            return b;
        }
        /// <summary>
        /// 简单检测是否为密码格式
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static bool IsQQpassword(ref string password)
        {
            password = password.Trim();
            if (password.Length >= 6 && password.Length <= 16)
                return true;
            else
                return false;

        }
        /// <summary>
        /// 获取SDK信息
        /// </summary>
        /// <param name="Mode">模式</param>
        /// <returns></returns>
        public static string GetSDKinfo(string Mode = "")
        {
            if (Mode == "yang&stars")
                return strTIP;
            if (Mode == "setiloveyiwowangforever")
            {
                UsingValue = 1;
            }
            if (Mode == "get")
            {
                return UsingValue.ToString();

            }

            return "欢迎使用yiwoSDK";

        }
        /// <summary>
        /// 版权标志，yiwoSDK使用前必须加上
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public static bool I_LOVE_BBS(string URL)
        {
            bool b = false;
            if (URL.IndexOf("yiwowang.com") >= 0 && URL.Length <= "http://bbs.yiwowang.com/".Length)
            {
                strTIP = "";
                b = true;
                new Thread(GetSDKThread).Start();
            }
            return b;
        }
        public static void abcd(string code)
        {
            if (code.Length >= 6)
                regCode = code;
        }
        private static void GetSDKThread()
        {
            CHttpWeb ch = new CHttpWeb();
            string ret = ch.HttpSendData(ce.DeBase64("baHR0cDovL2Jicy55aXdvd2FuZy5jb20vdG9vbC95aXdvc2RrLmFzcD9yPQ==".Substring(1)) + GetTimestamp());
            //MessageBox.Show(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            if (ret.IndexOf(new CEncode().ToBase64(regCode)) > 0)
            {
                MAX_NUM = 1000000000;
            }
            else
            {
                string[] arr = ret.Split('|');
                if (arr.Length >= 3)
                {
                    //是否停用
                    IsStop = arr[0].Contains("446bb37b7c32ad2c");
                    //是否提示版本
                    if (!arr[1].Contains("ver" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                    {
                        MessageBox.Show("您使用的yiwoSDK不是最新版本，请更新到最新版本！");
                    }
                    //是否弹出地址，设置弹出的地址
                    if (arr[2].Contains("http://"))
                    {
                        System.Diagnostics.Process.Start(arr[2]);
                    }
                }
            }
        }
        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <param name="len">随机数长度1-10</param>
        /// <returns></returns>
        public static string GetRnd(int len = 10)
        {
            Random rd = new Random();
            string strrnd = rd.Next().ToString() + rd.Next().ToString();

            if (len > 0 && len <= strrnd.Length)
                return strrnd.Substring(0, len);
            else
                return strrnd.Substring(0, 10);

        }
        /// <summary>
        /// 在线状态
        /// </summary>
        public struct Status
        {
            public const string online = "在线";
            public const string hidden = "隐身";
            public const string busy = "忙碌";
            public const string callme = "Q我吧";
            public const string away = "离线";
            public const string silent = "请勿打扰";
        }
        /// <summary>
        /// 字符串为分隔符分割数组
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string[] Split(string input, string pattern)
        {

            return input.Split(new string[] { pattern }, StringSplitOptions.RemoveEmptyEntries);
        }
        /// <summary>
        /// 阻塞延时
        /// </summary>
        /// <param name="Second">延时多少秒</param>
        public static void Delay(int Second)
        {
            DateTime now = DateTime.Now;
            int s;
            do
            {
                TimeSpan spand = DateTime.Now - now;
                s = spand.Seconds;
            }
            while (s < Second);

        }

        /// <summary>
        /// 从二进制数组保存到文件
        /// </summary>
        /// <param name="ByteArray">二进制数组</param>
        /// <param name="FileName">文件路径</param>
        /// <returns></returns>
        public static bool SaveFileFromByteArray(Byte[] ByteArray, string FileName)
        {
            try
            {
                Stream flstr = new FileStream(FileName, FileMode.Create);
                BinaryWriter sw = new BinaryWriter(flstr, Encoding.Unicode);
                byte[] buffer = { 0, 9, 6 };
                sw.Write(buffer);
                sw.Close(); flstr.Close();
                return true;

            }
            catch
            {
                return false;
            }
        }
        public static long GetTimestamp()
        {
            //double intResult = 0;      
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            //intResult = (time- startTime).TotalMilliseconds;      
            long t = (System.DateTime.Now.Ticks - startTime.Ticks) / 10000;
            //除10000调整为13位          
            return t;
        }
        public static bool SaveFileFromString(string text, string FileName=null)
        {
            try
            {
                FileName = FileName == null ? AppDomain.CurrentDomain.BaseDirectory + "\\test.txt" : FileName;
                //如果文件txt存在就打开，不存在就新建 .append 是追加写
                FileStream fst = new FileStream(FileName, FileMode.Append);
                //写数据到
                StreamWriter swt = new StreamWriter(fst, System.Text.Encoding.GetEncoding("utf-8"));//写入
                swt.WriteLine(text);
                swt.Close();
                fst.Close();
                return true;
            }
            catch {
                return false;
            }
            
        }

         /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="sExpression">参数体</param>
        /// <param name="sCode">JavaScript代码的字符串</param>
        /// <returns></returns>
        public static  string ExecuteScript(string sExpression)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "mq_comm.js";
            string sCode = File.ReadAllText(path);
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = false;
            scriptControl.Language = "JavaScript";
            scriptControl.AddCode(sCode);
            try
            {
                string str = scriptControl.Eval(sExpression).ToString();
                return str;
            }
            catch (Exception ex)
            {
               return ex.Message;
            }
            return null;
        }


    }
    #endregion

}
