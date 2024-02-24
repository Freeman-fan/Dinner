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
                //读取消息内容
                string message = EventArgs.Message.GetText();
                string message_no_point = message.Remove(0, 1);
                //判断是否为指令
                char first = message[0];
                if (first != char.Parse("."))
                {
                    return;
                }
                //判断内容，返回数据
                /*0.聊天*/
                if (message_no_point == "早上好")
                {
                    MessageBody reply = new MessageBody(new List<SoraSegment>()
                    {
                        SoraSegment.Text("早上好……啊！已经早上了吗"),
                    });
                    await EventArgs.SourceGroup.SendGroupMessage(reply);
                }
                else if (message_no_point == "晚上好")
                {
                    MessageBody reply = new MessageBody(new List<SoraSegment>()
                    {
                        SoraSegment.Text("晚上好"),
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
                    /*使用ini配置文件读取汇率，已废弃
                    string filePath = "rate.ini";
                    try
                    {
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                rate = double.Parse(line);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string debug = "【错误】" + e.Message + "\n" + "出错了！请将此消息与您发送的消息截图，并发送给管理员“泛进中举”";
                        await EventArgs.SourceGroup.SendGroupMessage(debug);
                        return;
                    }
                    */

                    //计算价格
                    string num = message_no_point.Remove(0, 1);
                    double jp = float.Parse(num);
                    double cny1 = 0, cny2 = 0, cny3 = 0;
                    string type0 = "原价：" + jp + "y\n" + "当前参考汇率为:" + rate + "\n";
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
                    cny1 = Math.Round(cny1, MidpointRounding.AwayFromZero);
                    type1 = "人工切(" + type1 + "):" + cny1 + "r\n";
                    //②
                    string type2 = "";
                    cny2 = jp * rate;
                    cny2 = Math.Round(cny2, MidpointRounding.AwayFromZero);
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
                    cny3 = Math.Round(cny3, MidpointRounding.AwayFromZero);
                    type3 = "机切52汇:" + cny3 + "r";
                    //消息模块
                    text = type0 + type1 + type2 + type3;
                    MessageBody reply = new MessageBody(new List<SoraSegment>()
                    {
                        SoraSegment.Text(text),
                    });
                    await EventArgs.SourceGroup.SendGroupMessage(reply);
                }

                /*2.修改汇率【.c】*/
                if (message_no_point[0] == char.Parse("c"))
                {
                    string text = "";
                    string newrate = message_no_point.Remove(0, 1);
                    //检查汇率合法性
                    if(Regex.IsMatch(newrate, @"^\d+(\.\d{2,4})?$") == true)
                    {
                        rate=double.Parse(newrate);
                        text = "已修改汇率为" + rate;
                    }
                    else
                    {
                        text = "汇率格式错误，请检查";
                    }
                    await EventArgs.SourceGroup.SendGroupMessage(text);
                }

                /*api测试区域*/

                //读取消息发送者qq号
                //string senderid = EventArgs.Sender.Id.ToString();

                };



                //启动服务并捕捉错误

                await service.StartService()
                .RunCatch(e => Log.Error("Sora Service", Log.ErrorLogBuilder(e)));
            await Task.Delay(-1);

        }
    }
}
