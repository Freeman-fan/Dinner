using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
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

namespace ConsoleApp3
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
            double rate = 0;//汇率

            /****************
             * 实例化sora服务
             ****************/

            ISoraService service = SoraServiceFactory.CreateService(new ServerConfig());


            service.Event.OnPrivateMessage += async (sender, EventArgs) =>  //私聊消息处理
            {
                //判断临时会话，是则退出
                if (EventArgs.IsTemporaryMessage == true)
                {
                    return;
                }


                /*
                //////////////////////////////////////////////
                ///图片处理
                //////////////////////////////////////////////

                //图片处理入口
                if (EventArgs.Message.IsMultiImageMessage() == true || EventArgs.Message.IsSingleImageMessage() == true)
                {
                    Bitmap photo = EventArgs.Message.GetAllImage();

                };
                */


                ///////////////////////////////////////////////
                ///文本处理
                ///////////////////////////////////////////////

                string rawMessage = EventArgs.Message.GetText();

                //判断文本类型
                char first = rawMessage[0];

                //无提示词聊天
                if (Chat(ref rawMessage) == true)
                {
                    await EventArgs.Sender.SendPrivateMessage(rawMessage);
                }

                //【.】命令入口
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

            service.Event.OnFriendRequest += async (sender, EventArgs) =>  //好友申请事件
            {
                string comment = EventArgs.Comment;
                if (comment == "0209")
                {
                    await EventArgs.Accept();
                }
                else
                {
                    await EventArgs.Reject();
                }
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
                                        message = "欸，已经这个点了吗?一会去弄点泡面吃吧。";
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
                                        message = "谢谢，下次我也会努力的，喜欢你还会喜欢。";
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

        static bool Link(string message, out string text)
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


        static MessageBody ReplyPhoto(System.Drawing.Image photo)   //构建回复：单一图片
        {
            //将image转为base64流
            string photoStr = "";
            MemoryStream memoryStream = new MemoryStream();
            photo.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[memoryStream.Length];
            memoryStream.Position = 0;
            memoryStream.Read(arr, 0, (int)memoryStream.Length);
            memoryStream.Close();
            photoStr = Convert.ToBase64String(arr);
            //构建回复体
            MessageBody reply = new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Image(photoStr),
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
