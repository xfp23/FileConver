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
using System.Collections.ObjectModel;
using System.Windows.Automation;
using System.ComponentModel;
using HistoryContent;
using System.Security.Cryptography;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace APPLogic{

public class SettingLogic_Class  // 设置界面
{
    // 构造函数
 public   SettingLogic_Class()
    {
        SetParam = new setParm_t();

        // 默认参数设置
        SetParam.HistorySize = 20;
        // SetParam.FileSavePath = null;
        // SetParam.LogSavePath = null;
        SetParam.DataColumns = 20;
        SetParam.DisplayMode = DisplayMode_t.AutoMode; // 跟随系统
    }
   public  enum DisplayMode_t {

        AutoMode = 0x00, // 自动模式
        LightMode = 0x01, // 浅色模式

        DarkMode = 0x02, // 深色模式
    }

// 系统运行的参数
    public struct setParm_t {
        public UInt16 HistorySize; // 保留历史文件记录数
        public UInt16 DataColumns; // 数据显示的列数
        public string FileSavePath; // 文件保存路径
        public string LogSavePath; // 日志保存路径
        public DisplayMode_t DisplayMode; // 显示模式
        public bool AutoGenerateC; // 自动生成C文件
        public bool GenerateLog; // 生成日志

    };

    public setParm_t SetParam;
}
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

    public void Execute(object parameter) => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}

public class HistoryFile
{
    public string CArrary_conten { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public bool IsGenerated { get; set; }
    public ICommand ShowDetailsCommand { get; set; }

    public HistoryFile()
    {
        ShowDetailsCommand = new RelayCommand(param => ShowDetails());
    }

    private void ShowDetails()
    {
        string details = $"{FileName}: {CArrary_conten}";

        // 创建并显示自定义窗口
        var detailsWindow = new FileDetailsWindow();
        detailsWindow.ContentTextBox.Text = details;  // 将内容传递给窗口
        detailsWindow.ShowDialog();
    }


}


public class APPDevice_Class // 设备类
{
    public enum Flag
    {
        OFF = 0x00,
        ON = 0x01,
    };
    public struct APPDevice_Flag
    {
        public Flag isHistoryFileNew; // 是否有新历史记录添加
        public Flag isInitHistoryList; // 是否初始化历史数据列表
        public Flag ischkGenCFile; // 是否勾选了首页的生成.c文件
        public Flag isStartConvertButt; // 是否按下了开始转换按钮
        public Flag isFileAlUpload; // 文件是否已经上传

        public Flag isUploadFile; // 是否需要上传文件
        public Flag isHistoryFileFull; // 历史文件记录是否已满
        public Flag isClearHistoryButt; // 清除历史记录按钮是否被按下
        public Flag isGlobalSetUpdate; // 全局设置更新
        public Flag isLocalSetUpdate; // 局部设置更新
        public Flag isCancelSet;       // 取消当前设置
        public Flag isInitHistoryC; // 初始化生成c
        public Flag isSelSavePathButt; // 选择文件保存路径按钮
        public Flag isFollowFilePath; // 跟随文件路径
        public Flag isFollowLogPath; // 日志自动跟随文件保存路径

    };

    private MainWindow mainWindow; // 用于访问 MainWindow 的引用
    private static string Filepath = null;
    public SettingLogic_Class setLogic;
    private static string FileName = null;
    public bool FileAlreadyUpload = false;
    public APPDevice_Flag flag = default(APPDevice_Flag);

    private UInt16 historySize = 0;
    private UInt16 historyIndex = 0;
    private static OpenFileDialog openFileDialog = new OpenFileDialog
    {
        Title = "选择一个文件",
        Filter = "所有文件 (*.*)|*.*", // 允许所有文件,
        Multiselect = false
    };
    private ObservableCollection<HistoryFile> historyFile { get; set; }

    // 构造函数
    public APPDevice_Class(MainWindow window)
    {
        this.mainWindow = window;
        this.historyFile = new ObservableCollection<HistoryFile>();
        mainWindow.Dispatcher.Invoke(() =>
        {
            // 访问ui
            mainWindow.HistoryFile_DataGrid.ItemsSource = this.historyFile;
        });
       setLogic = new SettingLogic_Class();
       this.flag.isFollowFilePath = Flag.ON;
       this.flag.isFollowLogPath = Flag.ON;
    }
    /**
        *  此方法上传文件
        */
    public void Upload_File()
    {

        if (openFileDialog.ShowDialog() == true)
        {
            Filepath = openFileDialog.FileName; // 文件路径
            //FileAlreadyUpload = true;
            this.flag.isFileAlUpload = Flag.ON;
            // MessageBox.Show($"你选择的文件: {Filepath}", "文件上传");
        }

    }

        public void Select_SaveFilePath()
        {
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true, // 选择文件夹
                Title = "请选择保存文件的文件夹"
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.setParam_buff.FileSavePath = dlg.FileName; // 选中的文件夹路径
            }
        }

        public void Select_SaveLogPath()
        {
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true, // 选择文件夹
                Title = "请选择保存日志的文件夹"
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.setParam_buff.LogSavePath = dlg.FileName; // 选中的文件夹路径
            }
        }
    /**
        * 将文件转换成C数组并生成C数组
        * 参数: CFile 传入true 生成c文件 反之亦然
        */
    private string FileName_buff = null, FilePath_buff = null;
     private bool GenerateC_buff = false;
    public string GenerCArray(bool CFile)
    {
        if (string.IsNullOrWhiteSpace(Filepath)) return "null";  // 确保已经选择了文件

        FileAlreadyUpload = false;
        FileName = System.IO.Path.GetFileNameWithoutExtension(Filepath);
        byte[] fileData = File.ReadAllBytes(Filepath);
        string cArrayContent = FileToCArray(fileData, FileName);
        if ((FileName != FileName_buff || FilePath_buff != Filepath || GenerateC_buff != CFile) || this.flag.isInitHistoryC ==Flag.ON)
        {
                if(this.flag.isInitHistoryC == Flag.ON) this.flag.isInitHistoryC = Flag.OFF;
                if (historyIndex >= this.setLogic.SetParam.HistorySize)
                {
                    this.historyIndex = 0;
                    this.flag.isHistoryFileFull = Flag.ON; // 文件已满
                }
                this.flag.isHistoryFileNew = Flag.ON; // 有新的历史文件记录被更新
            mainWindow.Dispatcher.Invoke(() =>
            {
                historyFile.Add(new HistoryFile
                {
                    FileName = FileName,
                    CArrary_conten = cArrayContent,
                    FilePath = Filepath,
                    IsGenerated = CFile
                });
            });

            this.historyIndex++;
            
            FileName_buff = FileName;
            FilePath_buff = Filepath;
        }

if (CFile == true)
{
    string directory = System.IO.Path.GetDirectoryName(Filepath);  // 获取文件所在目录
    string outputFilePath;

    if (this.flag.isFollowFilePath == Flag.ON)
    {
        outputFilePath = System.IO.Path.Combine(directory, FileName + ".c");  // 在同级目录生成 C 文件  
    }
    else
    {
        outputFilePath = System.IO.Path.Combine(setLogic.SetParam.FileSavePath, FileName + ".c"); // 在指定的路径生成 C 文件
    }

    File.WriteAllText(outputFilePath, cArrayContent); // 写入 C 文件
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
        // flag.isHistoryFileNew = Flag.ON;
        // historySize = size;
        // historyIndex = 0;
        // historyFile = new HistoryFile[size];
    }

    public void DealWith_History()
    {
        if(this.flag.isHistoryFileNew == Flag.ON) // 有新的文件被上传
        {
            this.flag.isHistoryFileNew = Flag.OFF;
            mainWindow.Dispatcher.Invoke(()=>
            {
                // 访问ui
               
                mainWindow.HistoryFile_DataGrid.Items.Refresh();
            });
            
        }

        if(this.flag.isClearHistoryButt == Flag.ON)
        {
            this.flag.isClearHistoryButt = Flag.OFF;
            mainWindow.Dispatcher.Invoke(() =>
            {
                // 访问ui
                mainWindow.HistoryFile_DataGrid.ItemsSource = this.historyFile;
                this.historyFile.Clear(); // 清空
                this.mainWindow.HistoryFile_DataGrid.Items.Refresh();
                historyIndex = 0; // 清空
            });

        }
    }
        // 处理首页
    public void DealWith_Front()
    {

        if (this.flag.isUploadFile == Flag.ON)
        {
            this.flag.isUploadFile = Flag.OFF; // 关闭标志位
            this.Upload_File();
        }


        if (this.flag.isStartConvertButt == Flag.ON)
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

        // 更新设置界面
        private void UpdateSetPage_Page( SettingLogic_Class.setParm_t param)
        {
            this.mainWindow.Dispatcher.Invoke(() =>
            {
                this.mainWindow.HistorySizeValue.Text = param.HistorySize.ToString(); // 历史记录大小
                this.mainWindow.SaveFilePath.Text = param.FileSavePath; // 文件保存路径
                this.mainWindow.LogFilePath.Text = param.LogSavePath;   // 日志保存路径
                //this.mainWindow.AutoGenerateCFile.IsChecked = param.AutoGenerateC; // 设置自动生成C文件
                //this.mainWindow.EnableLogging.IsChecked = param.GenerateLog; // 设置生成日志
            });
        }

        /**
         * 恢复默认界面
         */
        private void UpdateSetPage_Page()
        {
            this.mainWindow.Dispatcher.Invoke(() =>
            {
                this.mainWindow.HistorySizeValue.Text = "20";
                this.mainWindow.SaveFilePath.Text = "/default"; // 文件保存路径
                this.mainWindow.LogFilePath.Text ="/default";   // 日志保存路径
                this.mainWindow.AutoGenerateCFile.IsChecked = false; // 取消自动生成c文件勾选
                this.mainWindow.EnableLogging.IsChecked = false; // 取消日志功能勾选
            });
        }

    private SettingLogic_Class.setParm_t setParam_buff = default(SettingLogic_Class.setParm_t); // 设置参数缓存
    // 处理设置页面
    public void DealWith_SetPage()
    {

        if(this.flag.isLocalSetUpdate == Flag.ON)
        {
            this.flag.isLocalSetUpdate = Flag.OFF;

                UpdateSetPage_Page(setParam_buff);
        }
        if(this.flag.isGlobalSetUpdate == Flag.ON) // 如果是全局设置
        {
            this.flag.isGlobalSetUpdate = Flag.OFF; // 关闭全局设置
            setLogic.SetParam.HistorySize = setParam_buff.HistorySize; // 更新设置
            setLogic.SetParam.FileSavePath = setParam_buff.FileSavePath;
                setLogic.SetParam.LogSavePath = setParam_buff.LogSavePath;
                UpdateSetPage_Page(setLogic.SetParam);
                if (this.flag.isSelSavePathButt == Flag.ON && this.setLogic.SetParam.FileSavePath != null) 
            {
                // 按了按钮但是没有正确选择路径的清况
                this.flag.isFollowFilePath = Flag.ON; // 自动开启文件跟随
            }else {
                this.flag.isFollowFilePath = Flag.OFF;
                // if(this.flag.isFollowFilePath == Flag.ON)
                // {
                //     this.setLogic.SetParam.LogSavePath = this.setLogic.SetParam.FileSavePath;
                // }
            }
            
        }

        if(this.flag.isCancelSet == Flag.ON)
        {
            // 此处恢复默认值
            this.flag.isFollowFilePath = Flag.ON; // 生成的文件跟随转换文件路径
            this.flag.isFollowLogPath = Flag.ON; // 日志自动跟随
            this.setLogic.SetParam.FileSavePath = "/default";
            this.setLogic.SetParam.LogSavePath = "/default";
            this.setLogic.SetParam.HistorySize = 20; /// 默认成20个
            // 刷新ui
            this.mainWindow.Dispatcher.Invoke(() =>
            {
                this.mainWindow.HistorySizeValue.Text = this.setLogic.SetParam.HistorySize.ToString(); // 历史记录大小
                this.mainWindow.SaveFilePath.Text = this.setLogic.SetParam.FileSavePath; // 文件保存路径
                this.mainWindow.LogFilePath.Text = this.setLogic.SetParam.LogSavePath;   // 日志保存路径
                 this.mainWindow.AutoGenerateCFile.IsChecked = false; // 取消自动生成c文件勾选
                 this.mainWindow.EnableLogging.IsChecked = false; // 取消日志功能勾选
            });
        }
    }

}

}