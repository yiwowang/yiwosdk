using System;
using System.Text;
using System.IO;
using System.Net;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
namespace yiwoSDK
{
    internal class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        public AcceptAllCertificatePolicy()
        {
        }

        public bool CheckValidationResult(ServicePoint sPoint,
           X509Certificate cert, WebRequest wRequest, int certProb)
        {
            // Always accept
            return true;
        }
    }
   
    #region 网络类
    public class CHttpWeb
    {
        WebHeaderCollection Headers;
        public  CookieContainer cookies = new CookieContainer();
        private  String m_Referer = "";
        private String m_strUserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
        private  String m_Accept = "*/*";
        private  String m_ContentType = "application/x-www-form-urlencoded";
        private  string currUrl="";
        public bool IsRecordLog = false;
        private string m_Log = "";
        private WebProxy m_myproxy;
        /// <summary>
        /// 设置Referer
        /// </summary>
        /// <param name="Referer">要设置的Referer</param>
        public void SetReferer(string Referer) { m_Referer = Referer; }
        /// <summary>
        /// 设置UserAgent
        /// </summary>
        /// <param name="UserAgent">要设置UserAgent</param>
        public void SetUserAgent(string UserAgent) { m_strUserAgent = UserAgent; }
        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="IP">IP</param>
        /// <param name="Proxy">端口</param>
        /// <param name="UserName">账号</param>
        /// <param name="UserPass">密码</param>
        public void SetWebProxy(string IP, int Proxy, string UserName = "", string UserPass = "")
        {
            IP = IP.Trim();
            UserName = UserName.Trim();
            UserPass = UserPass.Trim();
            if (IP.Length != 0 && Proxy != 0)
            {
                m_myproxy = new WebProxy(IP, Proxy);
                if (UserName.Length != 0 && UserPass.Length != 0)
                    m_myproxy.Credentials = new NetworkCredential(UserName, UserPass);
            }

        }
        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="myproxy">WebProxy对象</param>
        public void SetWebProxy(WebProxy myproxy) { this.m_myproxy = myproxy; }
        /// <summary>
        /// 发送HTTP数据包
        /// </summary>
        /// <param name="URL">URL</param>
        /// <param name="Method">方式：GET或POST</param>
        /// <param name="Data">POST数据部分，GET方式保持空</param>
        /// <param name="Encode">编码方式：GB2132或UTF-8</param>
        /// <param name="IsAllowAutoRedirect">是否允许自动重定向</param>
        /// <returns>返回服务器响应的数据</returns>
        public string HttpSendData(string URL, string Method = "Get", string Data = "", string Encode = "UTF-8", bool IsAllowAutoRedirect =true)
        {
            if (CFun.Init( URL))
            {
                throw new Exception("发生了错误，错误代码：-1，请联系QQ:958796636");
            }
            System.GC.Collect();
            if (URL.IndexOf("https") == 0)
                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            HttpWebResponse response;
            HttpWebRequest request; if (CFun.GetSDKinfo("yang&stars").Length != 0)
            {
                throw new Exception(CFun.GetSDKinfo("yang&stars")+" 或者付费注册yiwoSDK,QQ:958796636");
            }
            try
            {
                URL = URL.Trim() ; Method = Method.Trim(); Data = Data.Trim(); Encode = Encode.Trim();
      
                m_ContentType = "application/x-www-form-urlencoded; charSet=" + Encode.ToLower();

                Uri uri = new Uri(URL.Trim());
                request = (HttpWebRequest)WebRequest.Create(uri);
                if (m_myproxy != null)
                    request.Proxy = m_myproxy;
                request.UserAgent = m_strUserAgent;
                request.Accept = m_Accept;
                request.ContentType = m_ContentType;
                request.Method = Method;
                request.Referer = m_Referer;
                request.CookieContainer = cookies;//用上次的COOKIE
                request.AllowAutoRedirect = IsAllowAutoRedirect;
                request.KeepAlive = true;
                if (Method.ToUpper() == "POST")
                {
                    byte[] byteRequest;
                    if (Encode.ToLower() == "utf-8")
                    {
                         byteRequest = Encoding.UTF8.GetBytes(Data);
                    }
                    else
                    {
                       byteRequest = Encoding.Default.GetBytes(Data);
                    }
                    Stream rs = request.GetRequestStream();
                    rs.Write(byteRequest, 0, byteRequest.Length);
                    rs.Close();
                }
                response = (HttpWebResponse)request.GetResponse();
                
                Headers = response.Headers;
                cookies.Add(response.Cookies);//储存收到的Cookie
                //cookies.SetCookies(uri, response.Cookies.ToString());
                          Stream resultStream = response.GetResponseStream();
                StreamReader sr;
                if (Encode.ToLower() == "utf-8")
                    sr = new StreamReader(resultStream, Encoding.GetEncoding("utf-8"));
                else
                    sr = new StreamReader(resultStream, Encoding.GetEncoding("gb2312"));
                string html = sr.ReadToEnd();
                sr.Close();
                resultStream.Close();
                request.Abort();
                response.Close();
                if (IsRecordLog == true)
                {
                    DateTime dt = DateTime.Now;
                    m_Log += "【" + dt.ToLongTimeString() + "】\r\n" + html + "\r\n";
                    
                }
                currUrl = URL;
                return html;
            }
            catch (Exception e) { 
                return "yiwoSDK发起http请求出现错误:"+e.ToString(); } 
        }
        /// <summary>
        /// 获取Cookie
        /// </summary>
        /// <param name="URL">URL</param>
        /// <returns>返回Cookie</returns>
        public  string GetCookie(string URL="")
        {
            string currCookies = "";
            
            try
            {
                if (URL.Length >4)
                    currUrl = URL;
                Uri u = new Uri(currUrl);

                foreach (Cookie cook in cookies.GetCookies(u))
                {
                    currCookies = currCookies + cook.ToString() + "\n";
                }
            }
            catch  { }
            return currCookies;
        }
        /// <summary>
        /// 根据cookie 的Key 获取Cookie的value
        /// </summary>
        /// <param name="key">字段名</param>
        /// <returns>返回Cookie</returns>
        public string GetCookieByKey(string key)
        {
            string currCookies = "";

            try
            {
                Uri u = new Uri(currUrl);

                foreach (Cookie cook in cookies.GetCookies(u))
                {
                    if (cook.ToString().IndexOf(key + "=") == 0 && cook.ToString().Length>key.Length+1)
                    {
                        currCookies = cook.ToString().Substring(key.Length + 1);
                    }
                }
            }
            catch { }
            return currCookies;
        }
        /// <summary>
        /// 获取数据包头信息
        /// </summary>
        /// <returns>返回数据包头信息</returns>
        public WebHeaderCollection GetHeaders()
        {
            return Headers;
        }
        /// <summary>
        /// 获取网络数据日志，需要先开始记录
        /// </summary>
        /// <returns>返回网络数据日志</returns>
        public string GetLog()
        { return m_Log; }
        /// <summary>
        /// 获取网络图片
        /// </summary>
        /// <param name="URL">图像的URL</param>
        /// <returns>返回Image对象</returns>
       public string verifysession = "";
        public Image GetImage(String URL)
        {
            try
            {
                Uri uri = new Uri(URL);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = m_strUserAgent;
                request.Accept = "*/*";
                request.ContentType = m_ContentType;
                request.Method = "Get";
                request.Referer = m_Referer;
                request.CookieContainer = cookies;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                foreach (Cookie cookie in response.Cookies)
                {
                    if (cookie.ToString().Contains("verifysession="))
                    {
                }
                    verifysession = cookie.ToString().Replace("verifysession=", "");

                    cookies.Add(cookie);
                }
                
                Stream s = response.GetResponseStream();
                Image Img = Image.FromStream(s);
                currUrl=URL;
                //return Img;
                return Img;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取CookieContainer
        /// </summary>
        /// <returns>返回CookieContainer</returns>
        public CookieContainer GetCookieContainer()
        {
            return cookies;
        }
  
    #endregion
    }
}