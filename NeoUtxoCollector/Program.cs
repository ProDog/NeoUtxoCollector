using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeoUtxoCollector
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string Config = "config.json";

        static void Main(string[] args)
        {
            //var a = ThinNeo.Helper.Bytes2HexString(new ThinNeo.Hash160("0x7e2b538aa6015e06b0a036f2bfdc07077c5368b4"));
            //Console.WriteLine(a);

            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(@"log4net.config"));
            GlobalContext.Properties["pname"] = Assembly.GetEntryAssembly().GetName().Name;
            GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "--test":
                        Config = "config.testnet.json";
                        break;
                    case "--main":
                        Config = "config.mainnet.json";
                        break;
                    default:
                        Config = "config.json";
                        break;
                }
            }
            //C#为了使用并发
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;

            ProjectInfo.head();

            MysqlConn.conf = Settings.Default.MysqlConfig;
            MysqlConn.dbname = Settings.Default.DataBaseName;

            StartChainSpider();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }

        }

        public static void StartChainSpider()
        {
            ChainSpider spider = new ChainSpider();
            spider.Start();
        }


        public enum LogLevel : byte
        {
            Fatal,
            Error,
            Warning,
            Info,
            Debug
        }

        private static LogLevel logLevel = LogLevel.Warning;
        private static object logLock = new object();

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (FileStream fs = new FileStream("error.log", FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter w = new StreamWriter(fs))
                if (e.ExceptionObject is Exception ex)
                {
                    PrintErrorLogs(w, ex);
                }
                else
                {
                    w.WriteLine(e.ExceptionObject.GetType());
                    w.WriteLine(e.ExceptionObject);
                }
        }

        private static void PrintErrorLogs(StreamWriter writer, Exception ex)
        {
            writer.WriteLine(ex.GetType());
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.StackTrace);
            if (ex is AggregateException ex2)
            {
                foreach (Exception inner in ex2.InnerExceptions)
                {
                    writer.WriteLine();
                    PrintErrorLogs(writer, inner);
                }
            }
            else if (ex.InnerException != null)
            {
                writer.WriteLine();
                PrintErrorLogs(writer, ex.InnerException);
            }
        }

        public static void Log(string message, LogLevel lv, string dir = null)
        {
            if (lv <= logLevel)
            {
                DateTime now = DateTime.Now;
                string line = $"[{now.TimeOfDay:hh\\:mm\\:ss\\.fff}] {message}";
                Console.WriteLine(line);
                string log_dictionary = dir != null ? $"Logs/{dir}" : $"Logs/default";
                string path = Path.Combine(log_dictionary, $"{now:yyyy-MM-dd}.log");
                lock (logLock)
                {
                    Directory.CreateDirectory(log_dictionary);
                    File.AppendAllLines(path, new[] { line });
                }
            }
        }

    }

    class ProjectInfo
    {
        static private string appName = "Zoro-Spider";
        public static void head()
        {
            string[] info = new string[] {
                "*** Start to run "+appName,
                "*** Auth:Grip",
                "*** Version:0.0.0.1",
                "*** CreateDate:2018-10-25",
                "*** LastModify:2019-05-09"
            };
            foreach (string ss in info)
            {
                log(ss);
            }
            //LogHelper.printHeader(info);
        }
        public static void tail()
        {
            log("Program." + appName + " exit");
        }

        static void log(string ss)
        {
            Console.WriteLine(DateTime.Now + " " + ss);
        }
    }
}
