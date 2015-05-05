
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
namespace yiwoSDK
{
    public class CData
    {
        public class json
        {
            public json(string JsonData = "", bool IsFormatJson = true)
            {
                initJson(JsonData, IsFormatJson);
            }
            private static bool _isEnd = false;
            
            private int json_p = 0;
            private bool isLoop = false;
            private string Data;
            private string CurPlaceData;//父节点包括的数据
            private string CurPlaceName;//父节点名称
            private string strFirstName = "_null_";//最靠前的字段，记录为每个数据块开始的标记
            private int NameMinP = -1;//读取的几个字段中，最靠前的字段名称的位置，作为每次读取数据块的首部
            private bool IsFindFirstNameOver=false ;//是否完成第一个数据块获取位置最小的名字
            private void initJson(string d="",bool IsFormatJson=true)
            {
                if (d.Length > 0)
                {
                    if (IsFormatJson)
                        Data = FormatJson(d);
                    else
                        Data = d;
                }
                CurPlaceData = Data;
                strFirstName = "_null_";
                isLoop = false;
                _isEnd = false;
                IsFindFirstNameOver = false;
                json_p = 0;
            }
            /// <summary>
            /// 格式化json数据，删除没用的空格等字符
            /// </summary>
            /// <param name="JsonData">Json数据</param>
            /// <returns>格式化后的json数据</returns>
            public static string FormatJson(string JsonData)
            {
                char[] cha = JsonData.ToCharArray();
                bool YinIsOver = true;
                string newdata = "";
                for (int i = 0; i <= cha.GetUpperBound(0); i++)
                {
                  
                    if (cha[i] == '"')
                    {
                        if (!YinIsOver)
                            YinIsOver = true;
                        else

                            YinIsOver = false;
                    }
                    if (YinIsOver)
                    {
                        if (cha[i] != ' ' && cha[i] != 13 && cha[i] != 10)
                            newdata += cha[i];
                    }
                    else
                    {
                        newdata += cha[i];
                    }
                }
                return newdata;
            }
            /// <summary>
            /// 设置json数据
            /// </summary>
            /// <param name="JsonData">json数据</param>
            /// <param name="IsFormatJson">是否对数据格式化</param>
            public void SetData(string JsonData, bool IsFormatJson = true) { initJson(JsonData,IsFormatJson); }
            /// <summary>
            /// 是否已经读取到记录末尾
            /// </summary>
            /// <returns>记录读取完毕返回True</returns>
            public bool isEnd()
            {//此函数一旦执行，说明是循环依次读取数据块
                isLoop = true;
                if (_isEnd == true)//当读取数据函数设置为_isEnd = true，那么此函数立刻返回初始化，返回true
                {
                    initJson();
                    return true;
                }
                else return false;
            }
            /// <summary>
            /// 获取json数据中的字段的值
            /// </summary>
            /// <param name="strName">字段名称，可以用.表示子节点和父节点的关系，例如Person.Student.Age</param>
            /// <param name="strJsonData">欲读取的json数据</param>
            /// <param name="isDelNotNeedChar">去掉Json中的不需要的字符</param>
            /// <returns></returns>
            public string GetValue(string strName, string strJsonData = "", bool isDelNotNeedChar = true)
            {//第一次查找最小字段结束，并且进入>=2次循环读取，反过来就是第一次循环读取的时候，第一次读取前赋值
                //第二次以上读取只需要使用CurPlaceData就可以了

              
                    if (strJsonData.Length != 0)
                    {
                        CurPlaceData = strJsonData;
                    }
            
                //如果不是循环读取 每次开头置零
                if (!isLoop)
                {
                    json_p = 0;//位置从0开始
                    strFirstName = "_null_";
                }

                //try{
                string result = "";//读取字段的结果，即字段的值
                string[] arrName = strName.Split(new char[] { '.' });

                if (arrName.GetLength(0) == 1)//如果是没有父节点，直接读取
                {
                    result = _getJsonValue("\"" + arrName[0] + "\":");
                }
                else
                {//逐层获取值的区域，例如result.message.value,最后找到区域名result.message，区域名就是result.message：{区域值}
                    string tmpPlaceName = strName.Substring(0, strName.LastIndexOf("."));
                    if (tmpPlaceName != CurPlaceName) //当前的读取字段的区域名是否和上次读取的区域名相同，不相同需要逐层读取获取区域值，相同用上次的区域值
                    {
                        CurPlaceName = tmpPlaceName;
                        //CurPlaceData = strJsonData;
                        string tmp1,tmp2;
                        for (int i = 0; i < arrName.GetUpperBound(0); i++)
                        {
                            tmp1 = "\"" + arrName[i] + "\":[";
                            tmp2 = "\"" + arrName[i] + "\":{";
                            if (CurPlaceData.IndexOf(tmp1) >= 0)
                                CurPlaceData = CurPlaceData.Substring(CurPlaceData.IndexOf(tmp1) + tmp1.Length + 1);
                            else
                                CurPlaceData = CurPlaceData.Substring(CurPlaceData.IndexOf(tmp2) + tmp2.Length );

                        }
                      
                    }
            
                    //在找到的区域中获取值
                    string strLastName = "\"" + arrName[arrName.GetUpperBound(0)] + "\":";
                    result = _getJsonValue(strLastName);
             
                    
                }

                if (isDelNotNeedChar == true)
                {
                    result = DelNotNeedChar(result);
                }
                return result;
                //}
                // catch { return ""; }
            }
           /// <summary>
           /// 移动到下一条记录，再循环读取记录的时候需要在每次循环中执行
           /// </summary>
            public void MoveNext()
            {if(!IsFindFirstNameOver)//这个可以判断是否为第一次执行，是则获取第一个字段的位置，并且赋值给json_p，以便在这个位置基础增加
                json_p = CurPlaceData.IndexOf(strFirstName);
              json_p =CurPlaceData.IndexOf(strFirstName, ++json_p);
              if (json_p < 0) _isEnd = true;
              IsFindFirstNameOver = true;
            }
            private string _getJsonValue(string strLastName )
            {

                try
                {
                    int curr_p;

                    if (isLoop == false)
                        curr_p = CurPlaceData.IndexOf(strLastName);
                    else
                    {
                        
                        curr_p  = CurPlaceData.IndexOf(strLastName, json_p);
                        if ((NameMinP == -1 || curr_p < NameMinP)&&!IsFindFirstNameOver)
                        {
                            NameMinP = curr_p;//记录比较最小的位置，以便下次在比较
                            strFirstName = strLastName;//记录这个比较小的位置的字段名
                            
                        }
                        

                    }

                    if (curr_p < 0)
                    {
                        return "";
                       
                    }

                    char[] arrCurPlaceData = CurPlaceData.ToCharArray();
                    bool ishave = false;
                    int tmp = 0;
                    int p1 = 0, p2 = 0;
                    bool isSpecial = false;//[face,1]
                    for (int i = curr_p; i < arrCurPlaceData.GetLength(0); i++)
                    {

                        if (arrCurPlaceData[i] == ':' || arrCurPlaceData[i] == ',')//记录下开始的字符位置
                        {

                            if (CurPlaceData.IndexOf(strLastName + ",") > 0 && p1 == 0)//face
                            {
                                isSpecial = true;
                            }
                            if (p1 == 0) p1 = i;
                        };
                        if (arrCurPlaceData[i] == '{' || arrCurPlaceData[i] == '[')
                        { tmp++; ishave = true; }
                        else if (arrCurPlaceData[i] == '}' || arrCurPlaceData[i] == ']')
                        {
                            tmp--;
                            p2 = i;
                            if (isSpecial)
                            {
                                break;
                            }
                        }
                        if (tmp <= 0 && arrCurPlaceData[i] == ',' && !isSpecial)
                        {
                            p2 = i;
                            break;

                        }
                        if (tmp <= 0 && ishave == true)
                        {
                            p2 = i + 1;
                            break;

                        }
                    }
                    if (p2 == 0) p2 = arrCurPlaceData.GetLength(0);
       
                    int len = p2 - p1 - 1;
                    if (len <= 0) return "";
                    return CurPlaceData.Substring(p1 + 1, p2 - p1 - 1).Trim();
                }
                catch { _isEnd = true; return ""; }

            }
            /// <summary>
            /// 删除不需要的字符，也就是返回值的内容，例如{}
            /// </summary>
            /// <param name="result">数据</param>
            /// <returns>删除后的数据</returns>
            public static string DelNotNeedChar(string result)
            {
                result = result.Replace("\":", "");
                //删除没截取掉的括号等字符
                char[] chDel = { '{', '}', '[', ']', '"', ',' };
                for (int i = 0; i < chDel.GetLength(0); i++)
                {
                    result = result.Replace(chDel[i].ToString(), "");
                }
                return result;
            }
        }
        public class xml
        {
            private static string strData = "";
            private static bool isLoop = false;
            private static bool isFirst = true;
            private static int p = 0;
            public static void SetData(string xmlData)
            {
                strData = xmlData;

            }
            public static string GetParent(string tmpData, string Name)
            {
                try
                {
                    int p1 = tmpData.IndexOf("<" + Name + ">");
                    int p2 = tmpData.IndexOf("</" + Name + ">");
                    return tmpData.Substring(p1 + Name.Length + 2, p2 - p1 - Name.Length - 4);
                }
                catch { return "字段不存在"; }
            }
            public static string GetValue(string Name)
            {
                try
                {
                    string[] aName;
                    string tmpData = strData;
                    string curName = Name;
                    if (Name.IndexOf(".") > 0)
                    {
                        aName = Name.Split('.');
                        for (int i = 0; i <= aName.GetUpperBound(0) - 1; i++)
                            tmpData = GetParent(tmpData, aName[i]);
                        curName = aName[aName.GetUpperBound(0)];

                    }

                    p = tmpData.IndexOf("<" + curName + ">", p);
                    int p2 = tmpData.IndexOf("</" + curName + ">", p);

                    string ret = tmpData.Substring(p + curName.Length + 2, p2 - p - curName.Length - 2);
                    if (isLoop == true)
                    {
                        p++;
                        int tmp = tmpData.IndexOf("<" + curName + ">", p);
                        if (tmp < 0)
                            isLoop = false;
                    }
                    if (ret.IndexOf("<![CDATA[") >= 0)
                        ret = ret.Substring(9, ret.Length - 13);
                    return ret;
                }
                catch { return "出错！字段不存在"; }
            }
            public static bool isEnd()
            {
                if (isFirst == true)//第一次执行此函数，设置isLoop为True
                {
                    isFirst = false;
                    isLoop = true;
                }
                if (isFirst == false)//如果不是第一次执行
                {
                    if (isLoop == false)//并且如果读取到末尾的命令
                    {
                        isFirst = true;//初始值
                        p = 0;
                    }
                }
                return isLoop;
            }
        }
    }
    public class CFace
    {   
        public string face = "";
        public string file_path = "";
        public string file_id = "";
        public string name = "";
        public string server = "";
        static int[,] aa = new int[,] { { 14, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 0 }, { 50, 51, 96, 53, 54, 73, 74, 75, 76, 77, 78, 55, 56, 57, 58 }, { 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 97, 98, 99, 100, 101 }, { 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 32, 113, 114, 115 }, { 63, 64, 59, 33, 34, 116, 36, 37, 38, 91, 92, 93, 29, 117, 72 }, { 45, 42, 39, 62, 46, 47, 71, 95, 118, 119, 120, 121, 122, 123, 124 }, { 27, 21, 23, 25, 26, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134 } };
        public static int GetSendFaceID(int id)
        {
            int y = id % 15;
            int x = (id - y) / 15;
            return aa[x, y];
        }
        /// <summary>
        /// 转换收到的表情ID
        /// </summary>
        /// <param name="id">收到的表情ID</param>
        /// <returns>返回正确的表情ID</returns>
        public static int GetRevFaceID(int id)
        {
            int rid = 0;
            for (int x = 0; x <= aa.GetUpperBound(0); x++)
                for (int y = 0; y <= aa.GetUpperBound(1); y++)
                {
                    if (aa[x, y] == id)
                    {
                        rid = x * 15 + y;
                        break;
                    }
                }
            return rid;
        }

    }
    public class CFont
    {
        public string size = "10";
        public string color = "000000";
        public string style = "0,0,0";
        public string name = "宋体";

    }
    public class SystemMsgData
    {
        public SystemMsgData(string JsonData = "")
        {
            if (JsonData.Length > 4)
            {
                CData.json json = new CData.json();
                json.SetData(JsonData);
                CEncode ce = new CEncode();
                poll_type = json.GetValue("poll_type");
                type = json.GetValue("type");
                from_uin = json.GetValue("from_uin");
                account = json.GetValue("account");
                msg = ce.DeUnicode(json.GetValue("msg"));
                allow = json.GetValue("allow");
                gcode = json.GetValue("gcode");
                admin_uin = json.GetValue("admin_uin");
                admin_nick = json.GetValue("admin_nick");
            }
        }
        public string poll_type = "";
        public string type = "";
        public string from_uin = "";
        public string account = "";
        public string msg = "";
        public string allow = "";
        public string gcode = "";
        public string admin_uin = "";
        public string admin_nick = "";
    }
    public class FriendMsgData
    {
        public FriendMsgData(string strFriendMsg = "")
        {
            CEncode ce = new CEncode();
            if (strFriendMsg.Length > 4)
            {
                CData.json json = new CData.json(strFriendMsg);

                this.poll_type = json.GetValue("poll_type");
                this.time = json.GetValue("time");
                this.from_uin = json.GetValue("from_uin");
                this.to_uin = json.GetValue("to_uin");
                this.ruin = json.GetValue("ruin");
                this.id = json.GetValue("id");
                this.cfont = new CFont();
                this.cfont.size = json.GetValue("size");
                this.cfont.color = json.GetValue("color");
                string style = json.GetValue("style");
                this.cfont.style = style[0] + "," + style[1] + "," + style[2];
                this.cfont.name = ce.DeUnicode(json.GetValue("name"));

                FullContent = json.GetValue("content", "", false);
                FullContent = FullContent.Replace("],\"", "],[{\"").Replace("\",[", "\"}],[").Replace("],[\"", "],[{\"").Replace("[\"offpic", "[{\"offpic");

                string[] strcon = FullContent.Split(new string[] { "[{" }, StringSplitOptions.RemoveEmptyEntries);
                content = new string[strcon.GetLength(0) - 2];
                for (int i = 2; i <= strcon.GetUpperBound(0); i++)
                {
                    string curValue = CData.json.DelNotNeedChar(strcon[i]);
                    int pface = curValue.IndexOf("face");
                    if (pface >= 0)
                    {
                        this.IsExistImage = true;
                        curValue = "face=" + curValue.Substring(pface + 4);

                    }

                    int pfile = curValue.IndexOf("file_path");
                    if (pfile > 0)
                    {
                        this.IsExistImage = true;
                        curValue = "file_path=" + curValue.Substring(pfile + 9);
                    }
                    else
                    {
                        OnlyText += ce.DeUnicode(curValue);
                    }


                    curValue = ce.DeUnicode(curValue);

                    content[i - 2] = curValue;

                }

                // CData.json json2 = new CData.json(strFriendMsg);



                //  this.cface.file_path = json.GetValue("file_path"); 

            }
        }
        public string OnlyText = "";
        public string poll_type = "";
        public string time = "";
        public string FullContent = "";
        public string from_uin = "";
        public string to_uin = "";
        public string ruin = "";//真实QQ
        public string id = "";
        public CFont cfont = new CFont();
        public string[] content;
        public bool IsExistImage = false;

    }

    public class GroupMsgData
    {
        public GroupMsgData(string strGroupMsg = "")
        {
            CEncode ce = new CEncode();
            if (strGroupMsg.Length > 4)
            {
                CData.json json = new CData.json(strGroupMsg);
                this.poll_type = json.GetValue("poll_type");
                this.time = json.GetValue("time");
                this.from_uin = json.GetValue("from_uin");
                this.to_uin = json.GetValue("to_uin");
                this.group_code = json.GetValue("group_code");
                this.send_uin = json.GetValue("send_uin");
                this.cfont = new CFont();
                this.cfont.size = json.GetValue("size");
                this.cfont.color = json.GetValue("color");
                string style = json.GetValue("style");
                this.cfont.style = style[0] + "," + style[1] + "," + style[2];
                this.cfont.name = ce.DeUnicode(json.GetValue("name"));

                FullContent = json.GetValue("content", "", false);
                FullContent = FullContent.Replace("],\"", "],[{\"").Replace("\",[", "\"}],[").Replace("],[\"", "],[{\"").Replace("[\"cface", "[{\"cface");
                string[] strcon = FullContent.Split(new string[] { "[{" }, StringSplitOptions.RemoveEmptyEntries);
                content = new string[strcon.GetLength(0) - 2];
                for (int i = 2; i <= strcon.GetUpperBound(0); i++)
                {
                    string curValue = CData.json.DelNotNeedChar(strcon[i]);
                    int pcface = curValue.IndexOf("cface");
                    if (pcface >= 0)
                    {
                        this.IsExistImage = true;
                        curValue = "cface=" + curValue.Substring(pcface + 5).Replace("name", "").Replace("file_id", ",").Replace("server", ",");

                    }
                    else
                    {
                        int pface = curValue.IndexOf("face");
                        if (pface >= 0)
                        {
                            this.IsExistImage = true;
                            curValue = "face=" + curValue.Substring(pface + 4);

                        }
                        else
                        {
                            OnlyText += ce.DeUnicode(curValue);
                        }
                    }


                    curValue = ce.DeUnicode(curValue);

                    content[i - 2] = curValue;


                }

            }
        }

        public string OnlyText;
        public string FullContent;
        public string poll_type;
        public string time;
        public string from_uin;
        public string to_uin;
        public string group_code;
        public string send_uin;
        public CFont cfont = new CFont();
        public string[] content;
        public bool IsExistImage = false;
    }
    public class JSONObject
    {
        private string json;
        public JSONObject(string json)
        {
            this.json = FormatJson(json);
        }
        public override string ToString()
        {
            return this.json;
        }
        public JSONObject getJSONObject(string name)
        {
            try
            {

                // return new  JSONObject()
                int p1 = getObjectPosition(name) + name.Length + 2;
                char[] ac = json.Substring(p1).ToCharArray();
                int ln = 0;
                int lp = 0;
                bool IsFirst = true;

                for (int i = 0; i <= ac.GetUpperBound(0); i++)
                {
                    if (ac[i] == '{')
                    {
                        ln++;
                        if (IsFirst)
                            lp = i;
                        IsFirst = false;
                    }
                    else if (ac[i] == '}')
                    {
                        ln--;
                    }

                    if (!IsFirst && ln == 0)
                    {
                        return new JSONObject(json.Substring(p1 + lp, i));
                    }
                }
            }
            catch { }
            return null;
        }
        public string getString(string name)
        {
            try
            {
                int p1 = json.IndexOf("\"" + name + "\"") + name.Length + 1;
                p1 = json.IndexOf(":", p1) + 1;
                int p2 = getCharP(json.Substring(p1), ',');
                int p3 = getCharP(json.Substring(p1), '}');

                if (p2 == -1 && p3 == -1 || p1 == -1)
                    return "";
                else
                {
                    if (p2 != -1 && p2 < p3)
                    {
                        string s1 = json.Substring(p1, p2).Trim();
                        return s1.Substring(1, s1.Length - 2);
                    }
                    else
                    {
                        string s1 = json.Substring(p1, p3).Trim();
                        return s1.Substring(1, s1.Length - 2);
                    }
                }
            }
            catch
            {
          
            }
            return "";
        }
        public long getLong(string name)
        {
            try
            {

                int p1 = json.IndexOf("\"" + name + "\"") + name.Length + 1;
                p1 = json.IndexOf(":", p1) + 1;
                int p2 = getCharP(json.Substring(p1), ',');
                int p3 = getCharP(json.Substring(p1), '}');

                if (p2 == -1 && p3 == -1 || p1 == -1)
                    return 0;
                else
                {

                    if (p2 != -1 && p2 < p3)
                        return Convert.ToInt64(json.Substring(p1, p2).Trim());
                    else
                        return Convert.ToInt64(json.Substring(p1, p3).Trim());
                }
            }
            catch { }
            return -9999;
        }
        public bool getBoolean(string name)
        {
            try
            {
                int p1 = json.IndexOf("\"" + name + "\"") + name.Length + 3;
                int p2 = json.Substring(p1).IndexOf(",");
                int p3 = json.Substring(p1).IndexOf("}");

                if (p2 == -1 && p3 == -1 || p1 == -1)
                    return false;
                else
                {
                    if (p2 != -1 && p2 < p3)
                        return Convert.ToBoolean(json.Substring(p1, p2).Trim());
                    else
                        return Convert.ToBoolean(json.Substring(p1, p3).Trim());
                }
            }
            catch
            {
               
            }
            return false;
        }
        public JSONArray getJSONArray(string name)
        {
            try
            {
                int p1 = getArrayPosition(name) + name.Length + 2;
                if (p1 == -1)
                    throw new Exception(name + "字段不存在");
                char[] ac = json.Substring(p1).ToCharArray();
                int ln = 0;
                int lp = 0;
                bool IsFirst = true;
                for (int i = 0; i <= ac.GetUpperBound(0); i++)
                {
                    if (ac[i] == '[' && ac[i == 0 ? 0 : i - 1] != '\\')
                    {
                        ln++;
                        if (IsFirst)
                            lp = i;
                        IsFirst = false;
                    }
                    else if (ac[i] == ']' && ac[i == 0 ? 0 : i - 1] != '\\')
                    {
                        ln--;
                    }
                    if (!IsFirst && ln == 0)
                    {
                        return new JSONArray(json.Substring(p1 + lp, i));
                    }
                }
                
            }
            catch
            {
               
            }
            return null;
        }
        private int getCharP(string json, char ch)
        {

            char[] ac = json.ToCharArray();
            if (ac[0] == ch) return 0;
            for (int i = 1; i <= ac.GetUpperBound(0); i++)
            {
                if (ac[i] == ch && ac[i - 1] != '\\')
                    return i;
            }
            return -1;

        }
        /// <summary>
        /// 获取真正的数组，因为有可能其他字段名称和数组名称相同
        /// </summary>
        /// <param name="name">数组名称</param>
        /// <returns></returns>
        private int getArrayPosition(string name)
        {
            try
            {
                int p = json.IndexOf("\"" + name + "\"");

                while (p != -1)
                {
                    if (json.Substring(p + name.Length + 3, 1) == "[")
                        return p;
                    p = json.IndexOf("\"" + name + "\"", p + 1);
                }
            }
            catch  {  }
            return -1;
        }
        private int getObjectPosition(string name)
        {
            try
            {
                int p = json.IndexOf("\"" + name + "\"");

                while (p != -1)
                {
                    if (json.Substring(p + name.Length + 3, 1) == "{")
                    {

                        return p;
                    }
                    p = json.IndexOf("\"" + name + "\"", p + 1);



                }
            }
            catch { }
            return -1;
        }
        /// <summary>
        /// 格式化json数据，删除没用的空格等字符
        /// </summary>
        /// <param name="JsonData">Json数据</param>
        /// <returns>格式化后的json数据</returns>
        public static string FormatJson(string JsonData)
        {
            char[] cha = JsonData.ToCharArray();
            bool YinIsOver = true;
            string newdata = "";
            for (int i = 1; i <= cha.GetUpperBound(0); i++)
            {

                if (cha[i] == '"' && cha[i-1] != '\\')
                {
                    if (!YinIsOver)
                        YinIsOver = true;
                    else

                        YinIsOver = false;
                }
                if (YinIsOver)
                {
                    if (cha[i] != ' ' && cha[i] != 13 && cha[i] != 10)
                        newdata += cha[i];
                }
                else
                {
                    newdata += cha[i];
                }
            }
            return newdata;
        }
            
    
    }

   public  class JSONArray
   {
       private string json;
        public JSONArray()
        {
            ja = new List<string>();

        }
        public override string ToString()
        {
            return this.json;
        }
        public JSONArray(string json)
        {
            this.json = JSONObject.FormatJson(json);
         
                ja = new List<string>();

                char[] ac = json.ToCharArray();
                int ln = 0;
                int lp = 0;


                for (int i = 1; i <= ac.GetUpperBound(0); i++)
                {

                    if ((ac[i] == '{' || ac[i] == '[') && ac[i - 1] != '\\')
                    {
                        ln++;

                    }
                    else if ((ac[i] == '}' || ac[i] == ']') && ac[i - 1] != '\\')
                    {
                        ln--;
                    }
                    if (ln == 0 && ac[i] == ',' && ac[i - 1] != '\\' || ac.GetUpperBound(0) == i)
                    {
                        ja.Add(json.Substring(lp + 1, i - lp - 1));
                        lp = i;
                    }
                }
       
        }
        private List<string> ja;
        private string ErrOutOfIndex = "参数index索引越界";
        private string ErrConvert = "数据类型行转换错误";
        public object get(int index)
        {
            if (index >= ja.Count)
                throw new Exception(ErrOutOfIndex);
            return ja[index];
        }
        public string getString(int index)
        {
            if (index >= ja.Count)
                throw new Exception("参数index索引越界");
            try
            {
                return ja[index].Substring(1, ja[index].Length - 2);
            }
            catch
            {
                throw new Exception("此字段读取时发生错误");
            }
        }
        public long getLong(int index)
        {
            if (index >= ja.Count)
                throw new Exception(ErrOutOfIndex);
            return Convert.ToInt64(ja[index]);
        }
        public bool getBoolean(int index)
        {
            if (index >= ja.Count)
                throw new Exception(ErrOutOfIndex);
            try
            {
                return Convert.ToBoolean(ja[index]);
            }
            catch
            {
                throw new Exception(ErrConvert);
            }
        }
        public JSONObject getJSONObject(int index)
        {
            if (index >= ja.Count)
                throw new Exception(ErrOutOfIndex);
            // int p1 = ja[index].IndexOf(":");
            // return new JSONObject(ja[index].Substring(p1+1, ja[index].Length -p1-2));
            try
            {
                return new JSONObject(ja[index]);
            }
            catch
            {
                throw new Exception(ErrConvert);
            }
        }
        public JSONArray getJSONArray(int index)
        {
            if (index >= ja.Count)
                throw new Exception(ErrOutOfIndex);
            try
            {
                return new JSONArray(ja[index]);
            }
            catch
            {
                throw new Exception(ErrConvert);
            }
        }
        public int length()
        {
            return ja.Count;
        }

    }

}
