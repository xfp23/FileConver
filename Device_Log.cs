using System;
using System.IO;
using APPLogic; // 软件逻辑


namespace Device_Log
{


    // 设备日志类
    class AppLog_Class
    {
        private readonly string LogPath; // 日志文件完整路径
        private const string LogFileName_Suffix = "FileConver-log.log"; // 日志文件名后缀
        private readonly string ConfFileName = "FileConver-conf.json";
        private readonly string LogFileName_Prefix; // 日志文件名前缀
        private UInt16 SetOperaCount;

        // 构造函数
        public AppLog_Class(string logDirectory)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory); // 确保日志文件夹存在
            }
            SetOperaCount = 0; // 操作次数
            LogFileName_Prefix = GetSystem_Date();
            LogPath = Path.Combine(logDirectory, LogFileName_Prefix + "-" + LogFileName_Suffix);
        }

        // 获取系统日期，年月日
        private static string GetSystem_Date()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        // 记录日志（通用方法）
        private void WriteLog(string logMessage)
        {
            string logEntry = $"{logMessage}\n";
            File.AppendAllText(LogPath, logEntry);
        }

        public void GeneralLog_Front() // 生成首页日志
        {
            WriteLog("前端日志已记录");
        }

        public void GeneralLog_History() // 生成历史页面的日志
        {
            WriteLog("历史页面日志已记录");
        }

        public void GeneralLog_Set(SettingLogic_Class set) // 生成设置页面的日志
        {
            SetOperaCount++;
            string SetLog = "{\n\"SettingParam\":\n" +
                "   {\n"+
                $"      \"SetOperaCount\": {SetOperaCount},\n" +  // 操作次数
                $"      \"DataFormate\": \"{set.SetParam.Dataformate.ToString()}\",\n" +
                $"      \"FileSavePath\": \"{set.SetParam.FileSavePath}\",\n" +
                $"      \"LogSavePath\": \"{set.SetParam.LogSavePath}\",\n" +
                $"      \"AutoGenerateC\": {set.SetParam.AutoGenerateC.ToString().ToLower()},\n" + 
                $"      \"DisplayMode\": \"{set.SetParam.DisplayMode.ToString()}\",\n" +
                $"      \"HistoryMaxNumber\": \"{set.SetParam.HistorySize}\",\n" +  
                $"      \"SaveFileHeader\": \"{set.SetParam.SaveFileHeader.ToString().ToLower()}\"\n" +
                "   }\n},"; 


            WriteLog(SetLog);
        }

        public void GeneralLog_Uart() // 串口发送的日志
        {
            WriteLog("串口数据日志已记录");
        }

        // 生成配置文件
        public void GeneraConfigFile(SettingLogic_Class set)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory; // 获取当前程序所在目录

            string confDirectory = Path.Combine(currentDirectory, "conf");  // 构建 "conf" 文件夹路径

            // 创建文件夹
            if (!Directory.Exists(confDirectory))
            {
                Directory.CreateDirectory(confDirectory);
            }

            // 将完整路径与文件名组合
            string configFilePath = Path.Combine(confDirectory, ConfFileName);

            string content = "{\n\"SettingParam\":\n" +
    "   {\n" +
    $"      \"DataFormate\": \"{set.SetParam.Dataformate.ToString()}\",\n" + 
    $"      \"FileSavePath\": \"{set.SetParam.FileSavePath}\",\n" +
    $"      \"LogSavePath\": \"{set.SetParam.LogSavePath}\",\n" +
    $"      \"AutoGenerateC\": {set.SetParam.AutoGenerateC.ToString().ToLower()},\n" +
    $"      \"DisplayMode\": \"{set.SetParam.DisplayMode.ToString()}\",\n" +
    $"      \"HistoryMaxNumber\": \"{set.SetParam.HistorySize}\",\n" +
    $"      \"SaveFileHeader\": \"{set.SetParam.SaveFileHeader.ToString().ToLower()}\"\n" +
    "   }\n},";
            // 写入配置文件
            File.WriteAllText(configFilePath, content);
        }

        // 读取配置文件
        public void ReadConfigFile()
        {

        }
    }
}
