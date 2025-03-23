using FileConver;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Timers;
using UserTim;

public class APPDevice_Class // 设备类
{

public enum Flag
{
    OFF = 0x00,
    ON = 0x01,
};

    private MainWindow mainWindow; // 用于访问 MainWindow 的引用
    public APPDevice_Class(MainWindow window)
{
        this.mainWindow = window;

    }


public struct APPDevice_Flag
{
    public Flag isHistoryFile; // 历史文件功能发否可以启用
    public Flag isInitHistoryList; // 是否初始化历史数据列表
    public Flag ischkGenCFile; // 是否勾选了首页的生成.c文件

    public Flag isStartConvertButt; // 是否按下了开始转换按钮
    public Flag isFileAlUpload; // 文件是否已经上传

    // public Flag isFrontPageButt; // 是否按下了首页按钮
    // public Flag isHistoryButt; // 是否按下了历史记录按钮
    // public Flag isSetupButt; // 是否按下了设置按钮
    // public Flag isUartSendButt; // 是否按下了串口发送按钮
    public Flag isUploadFile; // 是否需要上传文件

};
public APPDevice_Flag flag = default(APPDevice_Flag);
private static string Filepath = " ";
private static string FileName = " ";
public bool FileAlreadyUpload = false;

public HistoryFile[] historyFile;


public UInt16 historySize = 0;
public UInt16 historyIndex = 0;

public struct HistoryFile
{
    public string CArrary_conten; // c数组内容
    public string FileName;        // 文件名
    public string FilePath;       // 文件路径
    public bool GenerateCFile;    // 是否生成了C文件
};
private static OpenFileDialog openFileDialog = new OpenFileDialog
{
    Title = "选择一个文件",
    Filter = "所有文件 (*.*)|*.*", // 允许所有文件,
    Multiselect = false
};
/**
    *  此方法上传文件
    */
public void Upload_File()
{

    if (openFileDialog.ShowDialog() == true)
    {
        Filepath = openFileDialog.FileName; // 文件路径
        FileAlreadyUpload = true;
        this.flag.isFileAlUpload = Flag.ON;
        // MessageBox.Show($"你选择的文件: {Filepath}", "文件上传");
    }

}
/**
    * 将文件转换成C数组并生成C数组
    * 参数: CFile 传入true 生成c文件 反之亦然
    */
public string GenerCArray(bool CFile)
{
    if (string.IsNullOrWhiteSpace(Filepath)) return "null";  // 确保已经选择了文件

    FileAlreadyUpload = false;
    FileName = System.IO.Path.GetFileNameWithoutExtension(Filepath);
    byte[] fileData = File.ReadAllBytes(Filepath);
    string cArrayContent = FileToCArray(fileData, FileName);
    if (flag.isHistoryFile == Flag.ON)
    {
        if (historyIndex >= historySize) historyIndex = 0;
        historyFile[historyIndex].FileName = FileName;
        historyFile[historyIndex].CArrary_conten = cArrayContent;
        historyFile[historyIndex].FilePath = Filepath;
        historyIndex++;
    }


    if (CFile == true)
    {
        string directory = System.IO.Path.GetDirectoryName(Filepath);  // 获取文件所在目录
        string outputFilePath = System.IO.Path.Combine(directory, FileName + ".c");  // 在同级目录生成 C 文件  
        File.WriteAllText(outputFilePath, cArrayContent);  // 写入 C 文件  
    }

    return cArrayContent;
}

private static string FileToCArray(byte[] data, string variableName)
{
    StringBuilder sb = new StringBuilder();

    sb.AppendLine($"#include <stdint.h>");
    sb.AppendLine();
    sb.AppendLine($"const uint8_t {variableName}_data[] = {{"); // 格式化字符串

    // 每行 16 个字节
    for (int i = 0; i < data.Length; i++)
    {
        if (i % 16 == 0) sb.Append("    ");
        sb.Append($"0x{data[i]:X2}, ");

        if ((i + 1) % 16 == 0 || i == data.Length - 1)
            sb.AppendLine();
    }

    sb.AppendLine("};");
    sb.AppendLine($"const size_t {variableName}_size = {data.Length};");

    return sb.ToString();
}


public void setHistory_Size(UInt16 size)
{
    flag.isHistoryFile = Flag.ON;
    historySize = size;
    historyIndex = 0;
    historyFile = new HistoryFile[size];
}


public void DealWith_Front()
{
    
    if(this.flag.isUploadFile == Flag.ON)
    {
        this.flag.isUploadFile = Flag.OFF; // 关闭标志位
        this.Upload_File();
    }


        if(this.flag.isStartConvertButt == Flag.ON)
        {

            this.flag.isStartConvertButt = Flag.OFF;
             this.flag.isFileAlUpload = Flag.OFF;
                mainWindow.Dispatcher.Invoke(() =>
                {
                    if (this.flag.ischkGenCFile == Flag.ON)
                    {
                        // 生成并更新 C 数组内容到 MainOutput
                        mainWindow.MainOutput.Text = this.GenerCArray(true);
                    }
                    else
                    {
                        // 生成并更新 C 数组内容到 MainOutput
                        mainWindow.MainOutput.Text = this.GenerCArray(false);
                    }
                });
            }
    
}

}
