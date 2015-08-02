using System;
using System.IO;

namespace MRGLogsSystem
{
    /// <summary>
    /// 多线程日志记录
    /// 添加对多线程的支持
    /// 日志记录格式
    ///     [时间 日志等级] : 日志内容
    /// 日志等级
    ///     INFO = 0, WARN = 1, ERROR = 2, DEBUG = -1
    /// </summary>
    public class LogsSystem
    {
        #region 多线程单例模式
        private volatile static LogsSystem _instance = null;
        private static readonly object lockHelper = new object();
        private LogsSystem()
        {
            SetLogFileInfo();
            writeFileLock = new object();
        }
        public static LogsSystem GetInstance()
        {
            if (_instance == null)
            {
                lock (lockHelper)
                {
                    if (_instance == null)
                        _instance = new LogsSystem();
                }
            }
            return _instance;
        }
        #endregion

        #region 静态简易访问
        public static bool Log(string mainLog, LogLevel level = LogLevel.INFO)
        {
            try
            {
                LogsSystem.GetInstance().Print(mainLog, level);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("日志出现异常" + ex.ToString());
                return false;
            }
        }
        #endregion

        private string logDate;
        private string logPath;
        private string logFileName;
        private Object writeFileLock;//多线程文件读写锁

        /// <summary>
        /// 设置文件IO的信息
        /// 并创建文件夹
        /// logDate:日期
        /// logPath:文件夹地址
        /// logFileName:日志文件完整地址
        /// </summary>
        private void SetLogFileInfo()
        {
            try
            {
                logDate = DateTime.Now.ToString("yyyy-MM-dd");
                logPath = Environment.CurrentDirectory + "/Logs/";
                logFileName = logPath + logDate + ".log";
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常:" + ex.ToString());
            }
        }

        /// <summary>
        /// 用于跨天数的日志记录更改
        /// 每次调用文件时先调用该函数检查一遍日期是否更换
        /// </summary>
        private void CheckLogFileInfo()
        {
            if (logDate != DateTime.Now.ToString("yyyy-MM-dd"))
            {
                SetLogFileInfo();//重新设置文件信息
            }
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="mainLog">日志主体内容</param>
        /// <param name="level">日志等级</param>
        public void Print(string mainLog, LogLevel level = LogLevel.INFO)
        {
            CheckLogFileInfo();//检查是否已经更换日期了
            try
            {
                string log = string.Format("[{0} {1}] : {2}", DateTime.Now.ToString("HH:mm:ss"), level.ToString(), mainLog);

                lock (writeFileLock)
                {
                    //写入数据
                    FileStream fs = new FileStream(logFileName, FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(log);
                    sw.Close();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常" + ex.ToString());
            }
        }

        #region 对外接口
        /// <summary>
        /// 获取日志文件夹
        /// </summary>
        /// <returns>日志文件夹地址</returns>
        public string GetLogFileFolderDir()
        {
            CheckLogFileInfo();
            return logPath.Replace(@"\\", @"\").Replace(@"/", @"\");
        }

        /// <summary>
        /// 获取日志文件
        /// </summary>
        /// <returns>日志文件地址</returns>
        public string GetLogFileDir()
        {
            CheckLogFileInfo();
            return logFileName.Replace(@"\\", @"\").Replace(@"/", @"\");
        }
        #endregion

    }

    public enum LogLevel
    {
        INFO = 0, WARN = 1, ERROR = 2, DEBUG = -1
    }
}
