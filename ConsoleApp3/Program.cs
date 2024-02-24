using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


            //主程序
            service.Event.OnGroupMessage += async (sender, EventArgs) =>
            {
                /*******************************************
                 * 读取消息，提取文本
                 *******************************************/
                string message = EventArgs.Message.GetText();
                string message_no_point = message.Remove(0, 1);
                //判断是否为指令
                char first = message[0];
                if (first != char.Parse("."))
                {
                    return;
                }

                /******************************************
                 * 消息处理和回复
                 ******************************************/

                /*0.聊天*/
                //早上好
                if (message_no_point == "早上好")
                {
                    MessageBody reply = new MessageBody(new List<SoraSegment>()
                    {
                        SoraSegment.Text("早上好……啊！已经早上了吗"),
                    });
                    await EventArgs.SourceGroup.SendGroupMessage(reply);
                };
                //晚上好
                if (message_no_point == "晚上好")
                {
                    MessageBody reply = new MessageBody(new List<SoraSegment>()
                    {
                        SoraSegment.Text("晚上了吗……我刚睡醒欸"),
                    });
                    await EventArgs.SourceGroup.SendGroupMessage(reply);
                }

                /*1.计算价格【.y】*/
                if (message_no_point[0] == char.Parse("y"))
                {
                    string text = "";
                    //读取汇率
                    if (rate == 0)
                    {
                        text = "未设置汇率，请先使用.c+汇率设置当前汇率";
                        await EventArgs.SourceGroup.SendGroupMessage(text);
                        return;
                    }
                    //计算价格
                    double jp = 0;
                    int point = 0;
                    //double jp = double.Parse(message_no_point.Substring(1, message_no_point.IndexOf('p') - 1));
                    //string pointstring = message_no_point.Substring(message_no_point.IndexOf('p') + 1);
                    //int point = string.IsNullOrEmpty(pointstring) ? 0 : int.Parse(pointstring);
                    Match match = Regex.Match(message_no_point, @"(?<=y)\d+(?=p|$)");
                    if (match.Success)
                    {
                        jp = double.Parse(match.Value);
                        point = message_no_point.Contains("p") ? int.Parse(message_no_point.Substring(message_no_point.IndexOf("p") + 1)) : 0;
                    }
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
                    //消息模块
                    text += type1 + type2 + type3 + ptext;
                    text = text.TrimEnd('\n');
                    MessageBody reply = ReplyTextMessage(text, EventArgs.Message.MessageId);
                    await EventArgs.Reply(reply);
                }

                /*2.修改汇率【.c】*/
                if (message_no_point[0] == char.Parse("c"))
                {
                    string text = "";
                    string newrate = message_no_point.Remove(0, 1);
                    //检查汇率合法性
                    if (Regex.IsMatch(newrate, @"^\d+(\.\d{2,4})?$") == true)
                    {
                        rate = double.Parse(newrate);
                        text = "已修改汇率为" + rate;
                    }
                    else
                    {
                        text = "汇率格式错误，请检查";
                    }
                    MessageBody reply = ReplyTextMessage(text, EventArgs.Message.MessageId);
                    await EventArgs.Reply(reply);
                }

                /*3.常用链接【.l】*/
                if (message_no_point[0] == char.Parse("l"))
                {
                    string text = "";
                    switch (message_no_point.Remove(0, 1))
                    {
                        case "收集表" or "1":
                            {
                                text = "切煤收集表:\nhttps://flowus.cn/form/4c511090-12d8-4558-8ec8-b622dc7ea182";
                                break;
                            };
                        case "记录表" or "2":
                            {
                                text = "切煤记录表:\nhttps://flowus.cn/share/c4d1ab4e-08fe-43b0-8dc8-459595702111";
                                break;
                            };
                        case "maetown":
                            {
                                text = "Maetown下载链接:\nhttps://statics.maetown.cn/UploadFile/download/maetown_android/3d4e5041253f488db9dd8706aaaaeb62.apk";
                                break;
                            }
                        default:
                            {
                                text = "没有找到你要的链接哦";
                                break;
                            };
                    };
                    MessageBody reply = ReplyTextMessage(text, EventArgs.Message.MessageId);
                    await EventArgs.Reply(reply);
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

        static MessageBody ReplyTextMessage(string text, int messageId)  //构建回复纯文本文字
        {
            MessageBody reply = new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Reply(messageId),
                SoraSegment.Text(text),
            });
            return reply;
        }

    }
}
