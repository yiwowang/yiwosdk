using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace yiwoSDK
{
    public static class  CQQHelper
    {
        public static String byte2HexString(byte[] b)
        {
    
      string ss="";
        char[] hex = new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8',
                '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        if (b == null)
            return "null";

        int offset = 0;
        int len = b.Length;

        // 检查索引范围
        int end = offset + len;
        if (end > b.Length)
            end = b.Length;

       
        for (int i = offset; i < end; i++) {
            ss+=""+hex[(b[i] & 0xF0) >> 4] +""+hex[b[i] & 0xF];
        }
        return ss;
    }
 public static byte[] long2bytes(long i)
 {
     byte[] b = new byte[8];
     for (int m = 0; m < 8; m++, i >>= 8)
     {
         b[7 - m] = (byte)(i & 0x000000FF); 

     }
     return b;
 }
        public static byte[] hashchar2bin(string str)
        {
            string HEXSTRING = "0123456789ABCDEF";
            byte[] ab = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i = i + 2)
            {
                ab[i / 2] = (byte)(HEXSTRING.IndexOf(str[i] + "") << 4 | HEXSTRING
                        .IndexOf(str[i + 1]));
            }
            return ab;
        }
        public static string GetP(string QQnumber, string password, string verifyCode, bool IsMD5Password = false)
        {
            long uin;
            uin = long.Parse(QQnumber);
            ByteBuffer buffer = new ByteBuffer();
            if (IsMD5Password)
                buffer.Put(hashchar2bin(password));
            else
                buffer.Put(MD5_getBytes(password));
            buffer.PutLong(0);
            buffer.PutLong((ulong)uin);
            byte[] bytes = buffer.ToByteArray();
            CEncode encode = new CEncode();
            string md5_1 = encode.MD5(bytes, 32);//将混合后的字节流进行一次md5加密
            string result = encode.MD5(md5_1 + verifyCode.ToUpper(), 32);//再用加密后的结果与大写的验证码一起加密一次
            return result;

        }

        public static string getNewP(string password,long uin, string vcode)
        {
            string hexString = byte2HexString(long2bytes(uin)).ToLower();

          
            string fun = string.Format(@"getPassword('{0}','{1}','{2}')", password, hexString, vcode);
            return CFun. ExecuteScript(fun);
        }
        public static string getNewHash(string uin,string ptwebqq)
        {
            string fun = string.Format(@"hash('{0}','{1}')", uin, ptwebqq);
            return CFun.ExecuteScript(fun);
        }
        private static byte[] MD5_getBytes(string md5_str)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(md5_str);
            return md5.ComputeHash(bytes);


        }
        private static string Encrypt_1(string md5_str)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(md5_str);
            bytes = md5.ComputeHash(bytes);
            System.Text.StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in bytes)
            {
                stringBuilder.Append(@"\x");
                stringBuilder.Append(item.ToString("x2"));
            }
            return stringBuilder.ToString();
        }
       
    }
    class ByteBuffer
    {
        private byte[] _buffer;
        /// <summary>
        /// 获取同后备存储区连接的基础流
        /// </summary>
        public Stream BaseStream;
        public ByteBuffer()
        {
            this.BaseStream = new MemoryStream();
            this._buffer = new byte[0x10];
        }
        /// <summary>
        /// 设置当前流中的位置
        /// </summary>
        /// <param name="offSet">相对于origin参数字节偏移量</param>
        /// <param name="origin">System.IO.SeekOrigin类型值,指示用于获取新位置的参考点</param>
        /// <returns></returns>
        private long Seek(long offSet, SeekOrigin origin)
        {
            return this.BaseStream.Seek((long)offSet, origin);
        }
        /// <summary>
        /// 检测是否还有可用字节
        /// </summary>
        /// <returns></returns>
        private bool Peek()
        {
            return BaseStream.Position >= BaseStream.Length ? false : true;
        }
        /// <summary>
        /// 将整个流内容写入字节数组，而与 Position 属性无关。
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            long org = BaseStream.Position;
            BaseStream.Position = 0;
            byte[] ret = new byte[BaseStream.Length];
            BaseStream.Read(ret, 0, ret.Length);
            BaseStream.Position = org;
            return ret;
        }
        /// <summary>
        /// 压入一个布尔值,并将流中当前位置提升1
        /// </summary>
        /// <param name="value"></param>
        public void Put(bool value)
        {
            this._buffer[0] = value ? (byte)1 : (byte)0;
            this.BaseStream.Write(_buffer, 0, 1);
        }
        /// <summary>
        /// 压入一个Byte,并将流中当前位置提升1
        /// </summary>
        /// <param name="value"></param>
        public void Put(Byte value)
        {
            this.BaseStream.WriteByte(value);
        }
        /// <summary>
        /// 压入Byte数组,并将流中当前位置提升数组长度
        /// </summary>
        /// <param name="value">字节数组</param>
        public void Put(Byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.BaseStream.Write(value, 0, value.Length);
        }
        /// <summary>
        /// Puts the int.
        /// </summary>
        /// <param name="value">The value.</param>
        public void PutLong(long value)
        {
            PutLong((ulong)value);
        }
        /// <summary>
        /// 压入一个int,并将流中当前位置提升4
        /// </summary>
        /// <param name="value"></param>
        public void PutLong(ulong value)
        {
            this._buffer[0] = (byte)(value >> 0x18);
            this._buffer[1] = (byte)(value >> 0x10);
            this._buffer[2] = (byte)(value >> 8);
            this._buffer[3] = (byte)value;
            this.BaseStream.Write(this._buffer, 0, 4);
        }
        /// <summary>
        /// Puts the int.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void PutLong(int index, ulong value)
        {
            long pos = (long)this.BaseStream.Position;
            Seek(index, SeekOrigin.Begin);
            PutLong(value);
            Seek(pos, SeekOrigin.Begin);
        }
        /// <summary>
        /// 读取Byte值,并将流中当前位置提升1
        /// </summary>
        /// <returns></returns>
        public byte Get()
        {
            return (byte)BaseStream.ReadByte();
        }
    }

}
