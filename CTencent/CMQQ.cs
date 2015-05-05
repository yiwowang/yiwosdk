using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
namespace yiwoSDK
{
    public class CMQQ
    {
        private TcpClient tcp;
        private NetworkStream networkStream;
        private BinaryReader br;
        private BinaryWriter bw;
        public delegate void DMsg(string msg);
        public event DMsg RevMsg;
        public delegate void IsNotConnection();
        private Thread th;
        private bool isLoop = false;
        private string ret;
        private bool isRev=false;//是否消息到达
        private bool isReturn = false;//是否是返回模式 这个是设置的
        private bool isRequest = false;//请求函数执行，说明结果应该是作为函数的返回值，不应该触发消息事件
        public void SetMode(bool isReturn=true) {
            this.isReturn = isReturn;
            ret = "";
        }
        private int Connect(string IP, int Port)
        {
            try
            {
                tcp = new TcpClient(IP, Port);

            }
            catch
            {

                return 1;
            }
            networkStream = tcp.GetStream();
            br = new BinaryReader(networkStream, Encoding.UTF8);
            bw = new BinaryWriter(networkStream, Encoding.UTF8);

            th = new Thread(new ThreadStart(GetData));
            th.IsBackground = true;
            th.Start();
            return 0;
        }

        private void GetData()
        {
            isLoop = true;
            while (isLoop)
            {
                byte[] message = new byte[100000];
                try
                {
                    int i = br.Read(message, 0, message.Length);
                    ret = Encoding.UTF8.GetString(message, 0, message.Length);
                    isRev = true;
                    if (!isReturn || isReturn && !isRequest)//不是返回模式都触发消息，或者是返回模式并且请求函数已经没执行的时候触发
                    {

                        RevMsg(ret);
                    }
                }
                catch
                {

                }

            }
        }

        public void Send(string data)
        {
            ret = "yiwoSDK提示请在在消息事件取函数返回值！";
            byte[] bdata = Encoding.UTF8.GetBytes(data);
            try
            {
                bw.Write(bdata);
                bw.Flush();
            }
            catch
            {
            }
        }
        public string MyQQ;
        int iSEQ = 0;
        int SEQ()
        { return ++iSEQ; }
        public string GetMyQQ() { return MyQQ; }
        public int Login(string QQNumber, string QQPassword)
        {
           
            int ret = Connect("211.136.236.88", 14000);
            if (ret == 1) { return 1; }
            CEncode ce = new CEncode();
            string str = string.Format("VER=1.4&CON=1&CMD=Login&SEQ={0}&UIN={1}&PS={2}&M5=1&LG=40&LC=812822641C978097&GD=TW00QOJ9KUVD753S&CKE=\n", SEQ(), QQNumber, ce.MD5(QQPassword, 32).ToUpper());
            Send(str);
            MyQQ = QQNumber;
            return 0;
        }
        public void GetFriendList(string UN)
        {
            string str = string.Format("VER=1.4&CON=1&CMD=SimpleInfo2&SEQ={0}&UIN={1}&SID=&XP=C4CA4238A0B92382&UN={2}&TO=0\n", SEQ(), MyQQ, UN);
            Send(str);
      
        }
        public Image HexToImage(string HexString)
        {
            HexString = Filter(HexString);//只要数字和字母
            if (HexString.Length == 0)
            {
                return new Bitmap(0, 0);
            }

            if (HexString.Length % 2 == 1)
            {
                HexString = "0" + HexString;
            }

            byte[] result = new byte[HexString.Length / 2];

            for (int i = 0; i < HexString.Length / 2; i++)
            {
                result[i] = byte.Parse(HexString.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            Bitmap bitmap = new Bitmap(new System.IO.MemoryStream(result));
            return bitmap;
        }
        public void SendVCode(string VCode)
        {
            string data = "VER=1.4&CON=1&CMD=VERIFYCODE&SEQ=" + SEQ() + "&UIN=" + MyQQ + "&SID=&XP=C4CA4238A0B92382&SC=2&VC=" + VCode + "\n";
            Send(data);
        }
        private string Filter(string data)
        {
            char[] c = data.ToCharArray();
            string tmp = "";
            for (int i = 0; i < c.Length - 1; i++)
            {
                if (c[i] >= 48 && c[i] <= 57 || c[i] >= 65 && c[i] <= 90 || c[i] >= 97 && c[i] <= 122)
                    tmp = tmp + c[i];

            }
            return tmp;
        }
        public string SendMsgToFriend(string QQ, string strContent)
        {
            string str = string.Format("VER=1.4&CON=1&CMD=CLTMSG&SEQ={0}&UIN={1}&SID=&XP=C4CA4238A0B92382&UN={2}&MG={3}\n", SEQ(), MyQQ, QQ, strContent);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;
        }
        public string SetStatus(string status)
        {

            string str = string.Format("VER=1.4&CON=1&CMD=Change_Stat&SEQ={0}&UIN={1}&SID=&XP=C4CA4238A0B92382&ST={2}\n", SEQ(), MyQQ, GetStatus(status));
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;

        }
        public string Quit()
        {
            if (isLoop)
            {
                isLoop = false;
                string str = string.Format("VER=1.4&CON=1&CMD=Logout&SEQ={0}&UIN={1}&SID=&XP=C4CA4238A0B92382\n", SEQ(), MyQQ);
                isRequest = true;
                isRev = false;
                Send(str);
                while (!isRev && isReturn) Application.DoEvents();
                isRequest = false;
            }
            return ret;

        }
        public string GetOnlineList()
        {

            string str = string.Format("VER=1.1&CMD=Query_Stat&SEQ={0}&UIN={1}&TN=50&UN=0\n", SEQ(), MyQQ);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;

        }
        
        public string AddFriend(string UN)
        {

            string str = string.Format("VER=1.4&CON=0&CMD=AddToList&SEQ={0}&UIN={1}&SID=&XP=28F8088A712FE94B&UN={2}\n", SEQ(), MyQQ, UN);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;
        }
        public string Ack_AddFriend(string UN,int OK_NO_OKAndAdd=2,string Res="你好")
        {

            string str = string.Format("VER=1.4&CON=0&CMD=Ack_AddToList&SEQ={0}&UIN={1}&SID=&XP=28F8088A712FE94B&UN={2}&CD={3}&RS={4}\n", SEQ(), MyQQ, UN,OK_NO_OKAndAdd,Res);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;
        }
        public string GetInfo(string QQ)
        {
            string str = string.Format("VER=1.1&CMD=GetInfo&SEQ={0}&UIN={1}&LV=2&UN={2}\n", SEQ(), MyQQ, QQ);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;
        }
        public string RequestFriend(string QQ, int Add_OKAdd_OK_No,string msg="")
        {
            if (Add_OKAdd_OK_No == 0) Add_OKAdd_OK_No = 3;
            if (Add_OKAdd_OK_No == 1) Add_OKAdd_OK_No = 0;
            if (Add_OKAdd_OK_No == 3) Add_OKAdd_OK_No = 2;
            string str = string.Format("VER=1.1&CMD=Ack_AddToList&SEQ={0}&UIN={1}&UN={2}&CD={3}&RS={4}\n", SEQ(), MyQQ, QQ, Add_OKAdd_OK_No,msg);
            isRequest = false;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRev = false;
            isRequest = false;
            return ret;
        }
        public string DelFriend(string QQ)
        {
            string str = string.Format("VER=1.4&CON=0&CMD=DelFromList&SEQ={0}&UIN={1}&SID=&XP=28F8088A712FE94B&UN={2}\n", SEQ(), MyQQ, QQ);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            
            isRequest = false;
            return ret;
            
        }
        public string SearchFriend(int age,bool isWoman,int location=2)
        { int sex=0;
        if (isWoman)
            sex = 1;
            string str = string.Format("VER=1.4&CON=0&CMD=Finger&SEQ={0}&UIN={1}&SID=&XP=28F8088A712FE94B&AG={2}&SX={3}&PV={4}\n", SEQ(),MyQQ,age,sex,location);
            isRequest = true;
            isRev = false;
            Send(str);
            while (!isRev && isReturn) Application.DoEvents();
            isRequest = false;
            return ret;
            
        }
        
        private string GetStatus(string status)
        {
            string m_webqq_type;
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
            return m_webqq_type;
        }
    }
}