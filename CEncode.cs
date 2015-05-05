using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
namespace yiwoSDK
{
    public class CEncode
    {
        public string MD5(byte[] md5_bytes, int md5Len=32)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] bytes1 = md5.ComputeHash(md5_bytes);
            System.Text.StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in bytes1)
            {
                stringBuilder.Append(item.ToString("x").PadLeft(2, '0'));
            }
            string strmd5 = "";
            if (md5Len == 32)
                strmd5 = stringBuilder.ToString().ToUpper();
            else
                strmd5 = stringBuilder.ToString().ToUpper().Substring(8, md5Len);
            return strmd5;

        }
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="md5_str">欲加密的字符串</param>
        /// <param name="md5Len">长度</param>
        /// <returns>返回加密结果</returns>
        public  string MD5(string md5_str, int md5Len=32)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(md5_str);
            byte[] bytes1 = md5.ComputeHash(bytes);

            System.Text.StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in bytes1)
            {
                stringBuilder.Append(item.ToString("x").PadLeft(2, '0'));
            }
            string strmd5 = "";
            if (md5Len == 32)
                strmd5 = stringBuilder.ToString().ToUpper();
            else
                strmd5 = stringBuilder.ToString().ToUpper().Substring(8, md5Len);
            return strmd5;
        }
        /// <summary>
        /// sha1加密
        /// </summary>
        /// <param name="sha1_str">欲加密的字符串</param>
        /// <returns>返回加密结果</returns>
        public  string sha1( string sha1_str)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();

            //将mystr转换成byte[]
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] dataToHash = enc.GetBytes(sha1_str);

            //Hash运算
            byte[] dataHashed = sha.ComputeHash(dataToHash);

            //将运算结果转换成string
            string hash = BitConverter.ToString(dataHashed).Replace("-", "");

            return hash;
        }
        /// <summary>
        /// UTF-8编码
        /// </summary>
        /// <param name="str">欲进行UTF-8编码的字符串</param>
        /// <param name="isAll">是否全部编码</param>
        /// <param name="isUpper">是否大写</param>
        /// <returns>返回编码的字符串</returns>
        public string ToUTF8(string str,bool isAll=false,bool isUpper=false)
        {
             byte[] bs = Encoding.GetEncoding("UTF-8").GetBytes(str);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bs.Length; i++)
            {
                if (bs[i] < 128)
                {
                    bool isHave = false;
                    char[] c = " +/\\?%#&=".ToCharArray();
                    for (int xx = 0; xx <= c.GetUpperBound(0); xx++)
                        if (c[xx] == bs[i])
                            isHave = true;
                    if (isHave || isAll)
                        sb.Append("%" + toUpper(isUpper, bs[i].ToString("x")));
                    else
                        sb.Append(Convert.ToChar(bs[i]).ToString());
                }
                else
                {
                    sb.Append("%" + toUpper(isUpper, bs[i++].ToString("x").PadLeft(2, '0')));

                    if (bs.Length > i)
                        sb.Append("%" + toUpper(isUpper,bs[i].ToString("x").PadLeft(2, '0')));
                }
            }
            return sb.ToString();
        }
        private string toUpper(bool isToUpper,string str)
        {
            if (isToUpper)
                return str.ToUpper();
            else
                return str;
        }
        /// <summary>
        /// GB2132编码
        /// </summary>
        /// <param name="str">欲进行GB2132编码的字符串</param>
        /// <param name="isAll">是否全部编码</param>
        /// <param name="isUpper">是否大写</param>
        /// <returns>返回编码后的字符串</returns>
        public string ToGB2312(string str,bool isAll=false ,bool isUpper=false)
        {
            byte[] bs = Encoding.GetEncoding("GB2312").GetBytes(str);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bs.Length; i++)
            {
                if (bs[i] < 128)
                {
                    bool isHave = false;
                    char[] c = " +/\\?%#&=".ToCharArray();
                    for (int xx = 0; xx <= c.GetUpperBound(0); xx++)
                        if (c[xx] == bs[i])
                            isHave = true;
                    if (isHave || isAll)
                        sb.Append("%" + toUpper(isUpper, bs[i].ToString("x")));
                    else
                        sb.Append(Convert.ToChar(bs[i]).ToString());
                }
                else
                {
                    sb.Append("%" + toUpper(isUpper, bs[i++].ToString("x").PadLeft(2, '0')));

                    if (bs.Length > i)
                        sb.Append("%" + toUpper(isUpper, bs[i].ToString("x").PadLeft(2, '0')));
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Unicode编码
        /// </summary>
        /// <param name="str">欲编码的Unicode字符串</param>
        /// <param name="isAll">是否全部编码，False指仅对汉字编码</param>
        /// <returns></returns>
        public string ToUnicode(string str,bool isAll=false)
        {
            string dst = "";
            char[] src = str.ToCharArray();
            for (int i = 0; i < src.Length; i++)
            {
                string tmp = "";
                if (!(src[i] > 0 && src[i] < 226) || isAll)
                {
                    byte[] bytes = Encoding.Unicode.GetBytes(src[i].ToString());
                    tmp = @"\u" + bytes[1].ToString("X2") + bytes[0].ToString("X2");
                    dst += tmp;
                }
                else
                { dst += src[i]; }
               
            }
            return dst;
        }
        /// <summary>
        /// Unicode解码
        /// </summary>
        /// <param name="str">欲解码的Unicode字符串</param>
        /// <returns>解码后的字符串</returns>
       public string DeUnicode(String str)
        {
            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            return reg.Replace(str, delegate(Match m) { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });


        }
       public string ToBase64(string str)
       {
         byte[] myByte;
           //先把字符串按照utf-8的编码转换成byte[]
           Encoding myEncoding = Encoding.GetEncoding("utf-8");
           //myByte中获得这样的字节数组：228,184,173,229,141,142,228,186,186,230,176,145,229,133,177,229,146,140,229,155,189
           myByte = myEncoding.GetBytes(str);
           //把byte[]转成base64编码,这个例子形成的base64编码的unicode等价字符串为:"5Lit5Y2O5Lq65rCR5YWx5ZKM5Zu9"
          return  Convert.ToBase64String(myByte);           
       }
        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="str">欲解码的Base64字符串</param>
        /// <returns>解码后的字符串</returns>
       public string DeBase64(string str)
       {          //再从base64编码转成byte[]，又恢复为字节数组：228,184,173,229,141,142,228,186,186,230,176,145,229,133,177,229,146,140,229,155,189
           byte[] myByte;
           myByte = Convert.FromBase64String(str);
           Encoding myEncoding = Encoding.GetEncoding("utf-8");
           //用同一个Encoding对象把byte[]转成字符串："中华人民共和国"

          return myEncoding.GetString(myByte);
       }
    }
}
