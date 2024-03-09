using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sora;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Interfaces;
using Sora.Net.Config;
using Sora.Util;
using YukariToolBox.LightLog;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Sora.Net;
using System.Drawing.Imaging;
using Sora.Entities.Info;

namespace DinnerBot
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            //设置log等级
            Log.LogConfiguration
                .EnableConsoleOutput()
                .SetLogLevel(LogLevel.Info);

            //全局变量
            double rate = 0; //汇率
            string[,] memberState = new string[10, 2];  //成员变量初始化
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    memberState[i, j] = null;
                }
            }
            string cn_mNum = "";

            /****************
             * 实例化sora服务
             ****************/

            ISoraService service = SoraServiceFactory.CreateService(new ServerConfig());


            /*************
             * 监听事件
             *************/


            service.Event.OnPrivateMessage += async (sender, EventArgs) =>  //私聊消息处理
            {
                /*
                //判断临时会话，是则退出
                if (EventArgs.IsTemporaryMessage == true)
                {
                    return;
                }
                */


                //////////////////////////////////////////////
                ///图片处理
                //////////////////////////////////////////////
                /*
                //图片处理入口
                if (EventArgs.Message.IsMultiImageMessage() == true || EventArgs.Message.IsSingleImageMessage() == true)
                {

                    //获取图片(本地路径)
                    IEnumerable<Sora.Entities.Segment.DataModel.ImageSegment> T = new List<Sora.Entities.Segment.DataModel.ImageSegment>();
                    T = EventArgs.Message.GetAllImage();
                    List<Sora.Entities.Segment.DataModel.ImageSegment> imageSegments = new List<Sora.Entities.Segment.DataModel.ImageSegment>();
                    imageSegments = T.ToList();
                    string imagePath = imageSegments[0].ImgFile;
                    imagePath = imagePath.Remove(0, 7);  //移除imgFile路径中的"file://"

                    //图片处理函数
                    Bitmap bitmap = new Bitmap(imagePath);
                    Bitmap newBitmap = new Bitmap(bitmap.Width / 2, bitmap.Height / 2);
                    for (int i = 0; i < newBitmap.Width; i++)
                    {
                        for (int j = 0; j < newBitmap.Height; j++)
                        {
                            newBitmap.SetPixel(i, j, bitmap.GetPixel(i, j));
                        }
                    }
                    MemoryStream ms = new MemoryStream();
                    newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                    //当state为1.2时，获取图片内容


                    //发送消息
                    MessageBody mb = SayPhoto(ms);
                    await EventArgs.Sender.SendPrivateMessage(mb);
                };
                */


                ///////////////////////////////////////////////
                ///文本处理
                ///////////////////////////////////////////////

                string rawMessage = EventArgs.Message.GetText();
#if false
                //业务二级目录判断
                for (int i = 0; i < 10; i++)
                {
                    //判断是否有业务状态，是则获取状态码
                    if (memberState[i, 0] == EventArgs.SenderInfo.UserId.ToString())
                    {
                        //标cn流程
                        switch (memberState[i, 1])
                        {
                            case "1.1":
                                {
                                    if (EventArgs.Message.GetText() == "结束")
                                    {
                                        memberState[i, 0] = null;
                                        memberState[i, 1] = null;
                                        MessageBody mb1 = SayTextMessage("已退出标记cn功能");
                                        await EventArgs.Sender.SendPrivateMessage(mb1);
                                        break;
                                    }
                                    cn_mNum = EventArgs.Message.GetText();
                                    string[] cn__mNum = cn_mNum.Split("，");
                                    if (cn__mNum.Length != 2)
                                    {
                                        MessageBody mb3 = SayTextMessage("格式有误，请重新输入");
                                        await EventArgs.Sender.SendPrivateMessage(mb3);
                                        break;
                                    }
                                    memberState[i, 1] = "1.2";
                                    MessageBody mb2 = SayTextMessage("已收到cn和m码，请发送对应图片。\n发送“重置”清除当前记录");
                                    await EventArgs.Sender.SendPrivateMessage(mb2);
                                    break;
                                }
                            case "1.2":
                                {
                                    if (EventArgs.Message.GetText() == "重置")
                                    {
                                        memberState[i, 1] = "1.1";
                                        MessageBody mb1 = SayTextMessage("已清除当前记录，请重新发送\n发送“结束”退出当前状态");
                                        await EventArgs.Sender.SendPrivateMessage(mb1);
                                        break;
                                    }
                                    else if (EventArgs.Message.GetText() == "结束")
                                    {
                                        memberState[i, 0] = null;
                                        memberState[i, 1] = null;
                                        MessageBody mb1 = SayTextMessage("已退出标记cn功能");
                                        await EventArgs.Sender.SendPrivateMessage(mb1);
                                        break;
                                    }
                                    else if (EventArgs.Message.IsMultiImageMessage() == false && EventArgs.Message.IsSingleImageMessage() == false)
                                    {
                                        MessageBody mb2 = SayTextMessage("图片格式错误，请重新发送");
                                        await EventArgs.Sender.SendPrivateMessage(mb2);
                                        break;
                                    }

                                    MessageBody mb5 = SayTextMessage("已收到图片，正在处理，请稍候");
                                    await EventArgs.Sender.SendPrivateMessage(mb5);
                                    //获取图片
                                    IEnumerable<Sora.Entities.Segment.DataModel.ImageSegment> T = new List<Sora.Entities.Segment.DataModel.ImageSegment>();
                                    T = EventArgs.Message.GetAllImage();
                                    List<Sora.Entities.Segment.DataModel.ImageSegment> imageSegments = new List<Sora.Entities.Segment.DataModel.ImageSegment>();
                                    imageSegments = T.ToList();
                                    string imagePath = imageSegments[0].ImgFile;

                                    imagePath = Directory.GetCurrentDirectory() + "\\" + imagePath;
                                    //imagePath = imagePath.Remove(0, 7);  //移除imgFile路径中的"file://"
                                    //处理图片
                                    Bitmap image = new Bitmap(imagePath);
                                    Bitmap newimage = new Bitmap(image.Width, image.Height);
                                    string[] cn__mNum = cn_mNum.Split("，");
                                    string cn = cn__mNum[0];
                                    string mNum = cn__mNum[1];
                                    /*
                                    for(int x = 0; x < image.Width; x++)
                                    {
                                        for(int y = 0;y < image.Height; y++)
                                        {
                                            newimage.SetPixel(x, y, image.GetPixel(x, y));
                                        }
                                    }
                                    */
                                    Bitmap reBitmap;
                                    if (image.Width > 1000 && image.Height > 1000)
                                    {
                                        double zoom;
                                        if (image.Width > image.Height)
                                        {
                                            zoom = 1000.0 / image.Width;
                                        }
                                        else
                                        {
                                            zoom = 1000.0 / image.Height;
                                        }
                                        reBitmap = new Bitmap((int)(image.Width * zoom), (int)(image.Height * zoom));
                                        for (int y1 = 0; y1 < reBitmap.Height; y1++)
                                        {
                                            for (int x1 = 0; x1 < reBitmap.Width; x1++)
                                            {
                                                int x = (int)Math.Round(x1 * (1 / zoom));
                                                int y = (int)Math.Round(y1 * (1 / zoom));
                                                if (x > (image.Width - 1))
                                                {
                                                    x = image.Width - 1;
                                                }
                                                if (y > (image.Height - 1))
                                                {
                                                    y = image.Height - 1;
                                                }
                                                reBitmap.SetPixel(x1, y1, image.GetPixel(x, y));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        reBitmap = new Bitmap(image.Width, image.Height);
                                        for (int y1 = 0; y1 < image.Height; y1++)
                                        {
                                            for (int x1 = 0; x1 < image.Width; x1++)
                                            {
                                                reBitmap.SetPixel(x1, y1, image.GetPixel(x1, y1));
                                            }
                                        }
                                    }


                                    Graphics graphics = Graphics.FromImage(reBitmap);
                                    Brush brush = new SolidBrush(Color.White);
                                    Brush outlineBrush = new SolidBrush(Color.Black);
                                    float fontNum = reBitmap.Width / 9;
                                    float outlineWidth = reBitmap.Width / 45;
                                    System.Drawing.Font font = new System.Drawing.Font("黑体", fontNum);
                                    Pen outlintPen = new Pen(Color.Black, outlineWidth);
                                    int x_cn = (reBitmap.Width - (int)graphics.MeasureString(cn, font).Width) / 2;
                                    int x_mNum = (reBitmap.Width - (int)graphics.MeasureString(mNum, font).Width) / 2;
                                    int y_cn = (reBitmap.Height - (int)graphics.MeasureString(cn, font).Height) / 2;
                                    int y_mNum = y_cn + (int)graphics.MeasureString(cn, font).Height;
                                    for (float p = -outlineWidth; p <= outlineWidth; p += 0.5f)
                                    {
                                        graphics.DrawString(cn, font, outlineBrush, x_cn + p, y_cn + p);
                                        graphics.DrawString(mNum, font, outlineBrush, x_mNum + p, y_mNum + p);
                                    }
                                    graphics.DrawString(cn, font, brush, x_cn, y_cn);
                                    graphics.DrawString(mNum, font, brush, x_mNum, y_mNum);
                                    graphics.Dispose();
                                    string outputPath = Directory.GetCurrentDirectory() + "\\" + GetRanStr(16) + ".jpg";
                                    reBitmap.Save(outputPath, ImageFormat.Jpeg);

                                    /*
                                    MemoryStream ms = new MemoryStream();
                                    reBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                                    MessageBody mb3 = SayPhoto(ms);
                                    await EventArgs.Sender.SendPrivateMessage(mb3);
                                    */

                                    MessageBody mb3 = SayPhoto(outputPath);
                                    MessageBody mb4 = SayTextMessage("已完成处理。可以发送下一组cn和m码或发送“结束”退出当前状态");
                                    await EventArgs.Sender.SendPrivateMessage(mb3);
                                    await EventArgs.Sender.SendPrivateMessage(mb4);
                                    memberState[i, 1] = "1.1";
                                    break;
                                }
                        }
                    }
                }
#endif
                //判断文本类型
                char first = rawMessage[0];


                //无提示词聊天
                if (Chat(ref rawMessage) == true)
                {
                    await EventArgs.Sender.SendPrivateMessage(rawMessage);
                }

                //【.】常规命令入口
                if (first == char.Parse("."))
                {
                    //提取内容
                    string message = rawMessage.Remove(0, 1);

                    /*0.聊天*/
                    if (Chat(ref message) == true)
                    {
                        await EventArgs.Sender.SendPrivateMessage(message);
                    }

                    /*1.计算价格【.y】*/
                    if (message[0] == char.Parse("y"))
                    {
                        if (PriceCalculator(message, rate, out string text) == true)
                        {
                            MessageBody mb = SayTextMessage(text);
                            await EventArgs.Sender.SendPrivateMessage(mb);
                        }
                    }

                    /*2.修改汇率【.c】*/
                    if (message[0] == char.Parse("c"))
                    {
                        if (Changerate(message, ref rate, out string text) == true)
                        {
                            MessageBody mb = SayTextMessage(text);
                            await EventArgs.Sender.SendPrivateMessage(mb);
                        }
                    }

                    /*3.常用链接【.l】*/
                    if (message[0] == char.Parse("l"))
                    {
                        if (Link(message, out string text) == true)
                        {
                            MessageBody mb = SayTextMessage(text);
                            await EventArgs.Sender.SendPrivateMessage(mb);
                        }
                    }
                }
#if false

                //【/】业务命令入口
                if (first == char.Parse("/"))
                {
                    //标cn
                    if (rawMessage.Remove(0, 1) == "标cn")
                    {
                        string state = "0";
                        int path = 0;
                        for (; path < 10; path++)
                        {
                            if (memberState[path, 0] == null)
                            {
                                break;
                            }
                            else
                            {
                                //需要添加缓冲区溢出语句
                                break;
                            }
                        }
                        memberState[path, 0] = EventArgs.SenderInfo.UserId.ToString();
                        memberState[path, 1] = "1.1";
                        MessageBody mb = SayTextMessage("已进入标记cn功能，请发送cn和m码，用“，”分隔。\n发送“结束”退出当前状态");
                        await EventArgs.Sender.SendPrivateMessage(mb);
                    }
                }
#endif
            };

            service.Event.OnGroupMessage += async (sender, EventArgs) =>  //群消息处理
                {
                    ///////////////////////////////////////////////
                    ///文本处理
                    ///////////////////////////////////////////////

                    string rawMessage = EventArgs.Message.GetText();

                    //判断文本类型
                    char first = rawMessage[0];
                    //【.】命令入口
                    if (first == char.Parse("."))
                    {
                        //提取内容
                        string message = rawMessage.Remove(0, 1);

                        /*0.聊天*/
                        if (Chat(ref message) == true)
                        {
                            MessageBody mb = SayTextMessage(message);
                            await EventArgs.SourceGroup.SendGroupMessage(mb);
                        }

                        /*1.计算价格【.y】*/
                        if (message[0] == char.Parse("y"))
                        {
                            if (PriceCalculator(message, rate, out string text) == true)
                            {
                                MessageBody mb = ReplyTextMessage(text, EventArgs.Message.MessageId);
                                await EventArgs.Reply(mb);
                            }
                        }

                        /*2.修改汇率【.c】*/
                        if (message[0] == char.Parse("c"))
                        {
                            if (Changerate(message, ref rate, out string text) == true)
                            {
                                MessageBody mb = ReplyTextMessage(text, EventArgs.Message.MessageId);
                                await EventArgs.Reply(mb);
                            }
                        }

                        /*3.常用链接【.l】*/
                        if (message[0] == char.Parse("l"))
                        {
                            if (Link(message, out string text) == true)
                            {
                                MessageBody mb = ReplyTextMessage(text, EventArgs.Message.MessageId);
                                await EventArgs.Reply(mb);
                            }
                        }
                    }
                    //【/】命令入口
                    else if (first == char.Parse("/"))
                    {
                        //提取内容
                        string message = rawMessage.Remove(0, 1);
                    }


                    /*****************************************
                     * api测试区域
                     *****************************************/

                    /*读取消息发送者qq号
                    string senderid = EventArgs.Sender.Id.ToString();
                     */

                    /*回复消息设置
                    int messageId = 0;
                    messageId = (int)EventArgs.Message.MessageId;
                    string testText = "这是一段测试文本";
                    MessageBody testReply = ReplyTextMessage(testText, messageId);
                    await EventArgs.Reply(testReply);
                    */
                };


            //启动服务并捕捉错误

            await service.StartService()
            .RunCatch(e => Log.Error("Sora Service", Log.ErrorLogBuilder(e)));
            await Task.Delay(-1);

        }

        #region 方法：消息处理，返回消息内容

        static bool Chat(ref string message)   //聊天
        {
            switch (message)
            {
                //////早上好
                case "早上好":
                    {
                        if (GetRanNum(1, 3, out int num) == true)
                        {
                            switch (num)
                            {
                                case -1 or 1:
                                    {
                                        message = "已经是早上了吗，曲子还差一点，再努力会吧。";
                                        break;
                                    }
                                case 2:
                                    {
                                        message = "早上好，我刚写好一首demo，你能听听吗？想知道你的感想。";
                                        break;
                                    }
                                case 3:
                                    {
                                        message = "早。啊不用担心，曲子快写完了，我结尾完就会去小睡一会，真的。";
                                        break;
                                    }
                            }
                        }
                        return true;
                    }
                //////晚上好
                case "晚上好":
                    {
                        if (GetRanNum(1, 3, out int num) == true)
                        {
                            switch (num)
                            {
                                case -1 or 1:
                                    {
                                        message = "欸？晚上了吗，嗯我刚醒。";
                                        break;
                                    }
                                case 2:
                                    {
                                        message = "晚上好，我打算一会去看看冰箱里，望月小姐给我留了什么，重新加热一下。";
                                        break;
                                    }
                                case 3:
                                    {
                                        message = "到了晚上变得有点凉呢，泡点果茶或者花茶喝，暖暖身子，顺便提提神吧。";
                                        break;
                                    }
                            }
                        }
                        return true;
                    }
                //////吃了吗
                case "吃了吗":
                    {
                        if (GetRanNum(1, 3, out int num) == true)
                        {
                            switch (num)
                            {
                                case -1 or 1:
                                    {
                                        message = "欸，已经这个点了吗？一会去弄点泡面吃吧。";
                                        break;
                                    }
                                case 2:
                                    {
                                        message = "(怕打扰到别人的奏宝关闭了麦克风嗦面中)";
                                        break;
                                    }
                                case 3:
                                    {
                                        message = "嗯，望月小姐做的饭菜很好吃，你也不要忘了好好吃饭。";
                                        break;
                                    }
                            }
                        }
                        return true;
                    }
                //////喜欢曲子
                case "喜欢你的曲子":
                    {
                        if (GetRanNum(1, 3, out int num) == true)
                        {
                            switch (num)
                            {
                                case -1 or 1:
                                    {
                                        message = "谢谢，下次我也会努力的，希望你还会喜欢。";
                                        break;
                                    }
                                case 2:
                                    {
                                        message = "谢谢，如果这首歌能触及你的心底就好了。";
                                        break;
                                    }
                                case 3:
                                    {
                                        message = "嗯，我好开心，但这还不够，下一次的曲子一定能够……";
                                        break;
                                    }
                            }
                        }
                        return true;
                    }
                case "h":
                    {
                        message = "帮助文档:\nhttps://flowus.cn/freemanf/share/7ddb325a-f1fd-48db-bf62-964fa6cb8cf4";
                        return true;
                    };
            }
            return false;
        }

        static bool PriceCalculator(string message, double rate, out string text)  //计算汇率
        {
            text = "";
            //判断汇率值合法性
            if (rate == 0)
            {
                text = "未设置汇率，请先使用.c+汇率设置当前汇率";
                return true;
            }
            //定义变量,源文本处理，赋值
            double jp = 0;
            int point = 0;
            Match match = Regex.Match(message, @"(?<=y)\d+(?=p|$)");
            if (match.Success)
            {
                jp = double.Parse(match.Value);
                point = message.Contains("p") ? int.Parse(message.Substring(message.IndexOf("p") + 1)) : 0;
            }
            //判断参数合法性
            if (jp <= 0)
            {
                text = "价格输入错误";
                return true;
            }
            else if (point < 0)
            {
                text = "点数输入错误";
                return true;
            }
            //计算
            double cny1 = 0, cny2 = 0, cny3 = 0;
            double pprice = 0;  //均价
            text = "原价：" + jp + "y\n" + "当前参考汇率为:" + rate + "\n";
            //①
            string type1 = "";
            if (jp > 1000)
            {
                cny1 = jp * 0.052;
                type1 = "52汇";
            }
            else
            {
                cny1 = jp * 0.053;
                type1 = "53汇";
            }
            cny1 = Math.Round(cny1, 2, MidpointRounding.AwayFromZero);
            type1 = "人工切(" + type1 + "):" + cny1 + "r\n";
            //②
            string type2 = "";
            cny2 = (jp + 50) * (rate + 0.003);
            cny2 = Math.Round(cny2, 2, MidpointRounding.AwayFromZero);
            type2 = "机切浮动汇:" + cny2 + "r\n";
            //③
            string type3 = "";
            if (jp > 1000)
            {
                cny3 = jp * 0.052;
            }
            else
            {
                cny3 = (jp + 100) * 0.052;
            }
            cny3 = Math.Round(cny3, 2, MidpointRounding.AwayFromZero);
            type3 = "机切52汇:" + cny3 + "r\n";
            //计算最小值
            double min = Math.Min(cny1, Math.Min(cny2, cny3));
            if (min == cny1)
            {
                type1 = "⭐" + type1;
            }
            else if (min == cny2)
            {
                type2 = "⭐" + type2;
            }
            else if (min == cny3)
            {
                type3 = "⭐" + type3;
            }
            //计算均价
            string ptext = "";
            if (point != 0)
            {
                pprice = min / point;
                pprice = Math.Round(pprice, 2, MidpointRounding.AwayFromZero);
                ptext = "共" + point + "点，每点" + pprice + "r\n";
            }
            //构建消息
            text += type1 + type2 + type3 + ptext;
            text = text.TrimEnd('\n');
            return true;
        }

        static bool Changerate(string message, ref double rate, out string text)  //修改汇率
        {
            text = "";
            string newRate = message.Remove(0, 1);
            //检查汇率合法性
            if (Regex.IsMatch(newRate, @"^\d+(\.\d{2,4})?$") == true)
            {
                rate = double.Parse(newRate);
                text = "已修改汇率为" + rate;
                return true;
            }
            else
            {
                text = "汇率格式错误，请检查";
                return true;
            }
        }

        static bool Link(string message, out string text)  //链接内容
        {
            text = "";
            switch (message.Remove(0, 1))
            {
                case "收集表" or "1":
                    {
                        text = "切煤收集表:\nhttps://flowus.cn/form/4c511090-12d8-4558-8ec8-b622dc7ea182";
                        return true;
                    };
                case "记录表" or "2":
                    {
                        text = "切煤记录表:\nhttps://flowus.cn/share/c4d1ab4e-08fe-43b0-8dc8-459595702111";
                        return true;
                    };
                case "maetown" or "3":
                    {
                        text = "Maetown下载链接:\nhttps://statics.maetown.cn/UploadFile/download/maetown_android/3d4e5041253f488db9dd8706aaaaeb62.apk";
                        return true;
                    }
                case "帮助" or "4":
                    {
                        text = "帮助文档:\nhttps://flowus.cn/freemanf/share/7ddb325a-f1fd-48db-bf62-964fa6cb8cf4";
                        return true;
                    }
                default:
                    {
                        text = "没有找到你要的链接哦";
                        return true;
                    };
            };
        }

        #endregion

        #region  方法：构建消息体
        static MessageBody SayTextMessage(string text)    //纯文本文字，不引用原文
        {
            MessageBody reply = new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Text(text),
            });
            return reply;
        }

        static MessageBody ReplyTextMessage(string text, int messageId)  //纯文本文字，引用原文
        {
            MessageBody reply = new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Reply(messageId),
                SoraSegment.Text(text),
            });
            return reply;
        }

        static MessageBody SayPhoto(MemoryStream ms)   //构建回复：单一图片(通过图片流)
        {
            MessageBody reply = new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Image(ms),
            });
            return reply;
        }

        static MessageBody SayPhoto(string path)    //构建回复：单一图片(通过绝对路径)
        {
            MessageBody reply = new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Image(path),
            });
            return reply;

        }

        #endregion

        #region  方法：函数体

        static bool GetRanNum(int min, int max, out int num)   //快速随机数生成器
        {
            num = -1;
            Random random = new Random();
            num = random.Next(min, max);
            if (num == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        static string GetRanStr(int length)   //随机字符串生成
        {
            string str;
            string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random(); // 创建Random对象
            char[] result = new char[length]; // 存放结果的char数组
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(characters.Length); // 获取随机索引值
                result[i] = characters[index]; // 将随机字符添加到结果数组中
            }
            str = new string(result); // 转换为字符串形式输出
            return str;
        }

        #endregion


        #region  测试方法

        /*
        static Bitmap CreateNullPhoto(Bitmap photo)
        {
            Bitmap newPhoto = new Bitmap(photo.Width, photo.Height);
            return newPhoto;
        }
        */

        #endregion
    }
}
