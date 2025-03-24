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



public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
    
    public void Execute(object parameter) => _execute();

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}


public class HistoryFile : INotifyPropertyChanged
{
    private string _fileName;
    private string _filePath;
    private bool _generateCFile;
    private string _cArrary_conten;

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    }

    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged(nameof(FilePath));
        }
    }

    public bool IsGenerated
    {
        get => _generateCFile;
        set
        {
            _generateCFile = value;
            OnPropertyChanged(nameof(IsGenerated));
        }
    }

    public string CArrary_conten
    {
        get => _cArrary_conten;
        set
        {
            _cArrary_conten = value;
            OnPropertyChanged(nameof(CArrary_conten));
        }
    }

    public ICommand ShowDetailsCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public HistoryFile()
    {
        ShowDetailsCommand = new RelayCommand(ShowDetails);
    }

    private void ShowDetails()
    {
        MessageBox.Show(CArrary_conten, "转换结果");
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    };
    // public struct HistoryFile
    // {
    //     public string CArrary_conten; // c数组内容
    //     public string FileName;        // 文件名
    //     public string FilePath;       // 文件路径
    //     public bool GenerateCFile;    // 是否生成了C文件
    // };

    private MainWindow mainWindow; // 用于访问 MainWindow 的引用
    private static string Filepath = null;
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
       

    }
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
    private string FileName_buff = null, FilePath_buff = null;
    public string GenerCArray(bool CFile)
    {
        if (string.IsNullOrWhiteSpace(Filepath)) return "null";  // 确保已经选择了文件

        FileAlreadyUpload = false;
        FileName = System.IO.Path.GetFileNameWithoutExtension(Filepath);
        byte[] fileData = File.ReadAllBytes(Filepath);
        string cArrayContent = FileToCArray(fileData, FileName);
        if (FileName != FileName_buff || FilePath_buff != Filepath)
        {
                if (historyIndex >= historySize)
                {
                    this.historyIndex = 0;
                    this.flag.isHistoryFileFull = Flag.ON; // 文件已满
                }
                this.flag.isHistoryFileNew = Flag.ON; // 有新的历史文件记录被更新
                historyFile.Add(new HistoryFile
                {
                    FileName = FileName,
                    CArrary_conten = cArrayContent,
                    FilePath = Filepath,
                    IsGenerated  = CFile
                }); // 添加
                this.historyIndex++;
            
            FileName_buff = FileName;
            FilePath_buff = Filepath;
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
            });

        }
    }
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

}
