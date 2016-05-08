using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace ShowNumberIn4BitLED
{
    class Program
    {
        private static SerialPort m_sp = null;        
        public const string CLEAR_COMMAND = "CLEAR";// 清空命令
        public const string LOOP_COMMAND = "LOOP";// 清空命令
        public const string SHOW_NUM_COMMAND = "NUM";// 指定数字，必须为一个0-9999之间的整数
        private const string CLOSE_COMMAND = "CLOSE"; //关闭串口命令
        private const string OPEN_COMMAND = "OPEN"; //打开串口命令
        private const string LIST_COMMAND = "LIST";//列出当前可用窗口命令
        private const string HELP_COMMAND = "HELP";//显示可用命令
        private const string EXIT_COMMAND = "EXIT";

        private static void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            UpdateSerialPortData(indata);
        }

        private delegate void UpdateSerialPortDataHandler(string cnt);
        private static void UpdateSerialPortData(string cnt)
        {
            Console.WriteLine(cnt);
        }

        private static void ShowHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LIST命令显示本地计算机上所有可用串口");
            sb.AppendLine("OPEN命令打开指定的串口，后面必须指定一个串口名称为参数，例如OPEN com3");
            sb.AppendLine("CLOSE命令关闭当前打开的串口");
            sb.AppendLine("CLEAR命令清除4位数码管中显示的数据");
            sb.AppendLine("NUM命令指定4位数码管中显示的数据，后面必须指定一个0-9999之间的整数，例如NUM 5555");
            sb.AppendLine("LOOP命令指定每个数码管循环显示数字");
            sb.AppendLine("EXIT命令退出程序");
            sb.AppendLine("HELP命令显示帮助说明");

            Console.WriteLine(sb.ToString());
        }

        private static void ListPorts()
        {
            StringBuilder sb = new StringBuilder();

            string[] result = SerialPort.GetPortNames();

            if (result != null && result.Length > 0)
            {
                foreach (string s in result)
                {
                    sb.AppendLine(s);
                }
            }

            Console.WriteLine(sb.Length > 0 ? sb.ToString() : "没有可用的串口");
        }

        private static void ClosePort()
        {
            try
            {
                if (m_sp != null && m_sp.IsOpen)
                {
                    m_sp.Close();
                    m_sp = null;                    
                }

                Console.WriteLine("串口已经关闭");
            }
            catch (Exception ex)
            {
                Console.WriteLine("关闭串口失败，错误消息为：" + ex.Message);
            }
        }

        private static void OpenPort(string portName)
        {
            if (m_sp != null)
            {
                ClosePort();
            }

            try
            {
                m_sp = new SerialPort(portName);
                m_sp.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
                m_sp.Open();
                Console.WriteLine(string.Format("串口{0}打开成功", portName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("打开串口{0}失败，错误消息为：{1}", portName, ex.Message));
            }
        }

        private static void SendClearCmd()
        {
            if (m_sp == null || !m_sp.IsOpen)
            {
                Console.WriteLine("请先打开串口再发送清除命令");
                return;
            }

            m_sp.Write(string.Format("{0};", CLEAR_COMMAND));
            Console.WriteLine("发送清除命令成功");
        }

        private static void LoopNumCmd()
        {
            if (m_sp == null || !m_sp.IsOpen)
            {
                Console.WriteLine("请先打开串口再发送循环命令");
                return;
            }

            m_sp.Write(string.Format("{0};", LOOP_COMMAND));
            Console.WriteLine("发送循环命令成功");
        }

        private static void SendNumCmd(string num)
        {
            if (m_sp == null || !m_sp.IsOpen)
            {
                Console.WriteLine("请先打开串口再发送显示数字命令");
                return;
            }

            int result = 0;
            if (!Int32.TryParse(num, out result) || result < 0 || result > 9999)
            {
                Console.WriteLine("输入的整数必须在0-9999之间，请重新输入");
                    return;
            }

            m_sp.Write(string.Format("{0}:{1};", SHOW_NUM_COMMAND,result));
            Console.WriteLine(string.Format("发送显示数字{0}命令成功",result));
        }

        static void Main(string[] args)
        {
            bool isQuit = false;
            ShowHelp();

            while (!isQuit)
            {
                Console.WriteLine("请输入命令");

                try
                {
                    string userInput = Console.ReadLine();

                    string[] cmds = userInput.Split(' ');

                    switch (cmds[0].ToUpper())
                    {
                        case EXIT_COMMAND:
                            isQuit = true;
                            break;

                        case HELP_COMMAND:
                            ShowHelp();
                            break;

                        case LIST_COMMAND:
                            ListPorts();
                            break;

                        case CLOSE_COMMAND:
                            ClosePort();
                            break;

                        case OPEN_COMMAND:
                            if (cmds.Length < 2)
                            {
                                Console.WriteLine("OPEN命令后面必须有串口名称,请重新输入命令");
                                break;
                            }

                            OpenPort(cmds[1]);
                            break;

                        case CLEAR_COMMAND:
                            SendClearCmd();
                            break;

                        case LOOP_COMMAND:
                            LoopNumCmd();
                            break;

                        case SHOW_NUM_COMMAND:
                            if (cmds.Length < 2)
                            {
                                Console.WriteLine("NUM命令后面必须有一个0-9999之间的整数,请重新输入命令");
                                break;
                            }

                            SendNumCmd(cmds[1]);
                            break;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("处理用户命令失败，错误消息为：" + ex.Message);
                }                
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
