﻿using FileConver;
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
using System.ComponentModel.Design;
using Device_Log; // 设备日志

namespace APPLogic
{

    public class SettingLogic_Class  // 设置对象
    {
        // 构造函数
        public SettingLogic_Class()
        {
            SetParam = new setParm_t();

            // 默认参数设置
            this.SetParam.LogSavePath = "/default";
            this.SetParam.FileSavePath = "/default";
            this.SetParam.HistorySize = 20;
            this.SetParam.AutoGenerateC = false;
            this.SetParam.GenerateLog = false;
            this.SetParam.DisplayMode = DisplayMode_t.AutoMode; // 跟随系统
            this.SetParam.Dataformate = DataFormate_t.ALL;
            this.SetParam.SaveFileHeader = SaveFileHeader_t.YES;
            DisplayMode_StringTemp = "AutoMode";
            SaveFileHeader_StringTemp = "YES";
            DataFormate_StringTemp = "ALL";
        }
        public enum DisplayMode_t
        {

            AutoMode = 0x00, // 自动模式
            LightMode = 0x01, // 浅色模式

            DarkMode = 0x02, // 深色模式
        }

        // 数据格式类型
        public enum DataFormate_t
        {
            ALL = 0x00,
            WAV,
            MP3,
            BIN,
            HEX,
            JPG,
            PNG,
        };

        // 保存文件头类型枚举
        public enum SaveFileHeader_t
        {
            YES, // 保存 对应xaml里的Tag
            NO // 不保存
        };
        // 系统运行的参数
        public struct setParm_t
        {
            public UInt16 HistorySize; // 保留历史文件记录数
            public string FileSavePath; // 文件保存路径
            public string LogSavePath; // 日志保存路径
            public DisplayMode_t DisplayMode; // 显示模式
            public bool AutoGenerateC; // 自动生成C文件
            public bool GenerateLog; // 生成日志
            public DataFormate_t Dataformate; // 数据格式
            public SaveFileHeader_t SaveFileHeader; // 保存文件头?  这个作用是区分只要数据还是整个文件
        };

        public setParm_t SetParam;
        public string DisplayMode_StringTemp; // 显示模式的字符串缓存
        public string DataFormate_StringTemp; // 数据格式字符串缓存
        public string SaveFileHeader_StringTemp; // 是否保存文件头字符串缓存
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
            public Flag isClickFileSavePath; // 是否触发文件路径选择
            public Flag isClickLogPath; // 是否触发日志路径选择按钮
            public Flag isExitSetupPage;      // 退出setup的时候恢复页面
            public Flag isUpdateThemes;       // 更新了主题
            public Flag isUpdateDataFormate; // 更新了数据格式
            public Flag isMonitorSysThemeChanged; // 是否监听系统主题变化
            public Flag isInitMonitorTheme;       // 初始化监听主题
            public Flag isUpdateFileHeader;       // 是否更新保存文件头
            public Flag isNeedInitLog;           // 是否需要初始化日志
            public Flag isGenSetLog;            // 生成设置日志
            public Flag isGenFrontLog;          // 生成首页日志
            public Flag isGenHistoryLog;         // 生成历史日志
            public Flag isGenUartLog;            // 生成串口日志

        };

        private MainWindow mainWindow; // 用于访问 MainWindow 的引用
        private static string Filepath = null;
        private SettingLogic_Class setLogic;
        private static string FileName = null;
        public bool FileAlreadyUpload = false;
        public APPDevice_Flag flag = default(APPDevice_Flag);
        public SettingLogic_Class setLogic_buff;

        private UInt16 historySize = 0;
        private UInt16 historyIndex = 0;
        private AppLog_Class Log;
        private static OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Title = "选择一个文件",
            Filter = "所有文件 (*.*)|*.*", // 允许所有文件,
            Multiselect = false
        }; // 选择文件
        private ObservableCollection<HistoryFile> historyFile { get; set; }
        ResourceDictionary GlobalTheme;

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
            setLogic_buff = new SettingLogic_Class();
            GlobalTheme = new ResourceDictionary();
            this.flag.isFollowFilePath = Flag.ON;
            this.flag.isFollowLogPath = Flag.ON;
            this.flag.isInitMonitorTheme = Flag.ON;
            this.flag.isNeedInitLog = Flag.ON;
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

        private void Select_SaveFilePath()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dlg = new CommonOpenFileDialog
                {
                    IsFolderPicker = true, // 选择文件夹
                    Title = "请选择保存文件的文件夹"
                };

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.setLogic_buff.SetParam.FileSavePath = dlg.FileName; // 选中的文件夹路径
                    this.flag.isFollowFilePath = Flag.OFF; // 不跟随
                }
            }, DispatcherPriority.Normal); // 使用 UI 线程
        }

        private void Select_SaveLogPath()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dlg = new CommonOpenFileDialog
                {
                    IsFolderPicker = true, // 选择文件夹
                    Title = "请选择保存日志的文件夹"
                };

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.flag.isFollowLogPath = Flag.OFF; // 不跟随
                    this.setLogic_buff.SetParam.LogSavePath = dlg.FileName; // 选中的文件夹路径
                }
            });
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
            this.flag.isGenFrontLog = Flag.ON; // 生成首页日志
            FileName = System.IO.Path.GetFileNameWithoutExtension(Filepath);
            byte[] fileData = File.ReadAllBytes(Filepath);
            string cArrayContent = FileToCArray(fileData, FileName);
            if ((FileName != FileName_buff || FilePath_buff != Filepath || GenerateC_buff != CFile) || this.flag.isInitHistoryC == Flag.ON)
            {
                if (this.flag.isInitHistoryC == Flag.ON) this.flag.isInitHistoryC = Flag.OFF;
                if (historyIndex >= this.setLogic.SetParam.HistorySize)
                {
                    this.historyIndex = 0;
                    this.flag.isHistoryFileFull = Flag.ON; // 文件已满
                }
                this.flag.isHistoryFileNew = Flag.ON; // 有新的历史文件记录被更新
                this.flag.isGenHistoryLog = Flag.ON;  // 记录日志
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

                if (this.flag.isFollowFilePath == Flag.ON )
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


        public void DealWith_History()
        {
            if (this.flag.isHistoryFileNew == Flag.ON) // 有新的文件被上传
            {
                this.flag.isHistoryFileNew = Flag.OFF;
                mainWindow.Dispatcher.Invoke(() =>
                {
                    // 访问ui

                    mainWindow.HistoryFile_DataGrid.Items.Refresh();
                });

            }

            if (this.flag.isClearHistoryButt == Flag.ON)
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
            string content = null;
            bool generC_temp = false;

            if (this.flag.isUploadFile == Flag.ON)
            {
                this.flag.isUploadFile = Flag.OFF; // 关闭标志位
                this.Upload_File();
            }

            if (this.flag.isStartConvertButt == Flag.ON)
            {
                this.flag.isStartConvertButt = Flag.OFF;
                this.flag.isFileAlUpload = Flag.OFF;

                if (this.setLogic.SetParam.AutoGenerateC == true) // 自动生成c数组
                {
                    content = this.GenerCArray(true);
                }
                else
                {
                    if (this.flag.ischkGenCFile == Flag.ON)
                    {
                        content = this.GenerCArray(true);
                    }
                    else
                    {
                        content = this.GenerCArray(false);
                    }
                }

                if (mainWindow?.Dispatcher != null && !mainWindow.Dispatcher.HasShutdownStarted)
                {
                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        if (mainWindow.MainOutput != null)
                        {
                            mainWindow.MainOutput.Text = content;
                        }
                        generC_temp = this.mainWindow.GenerateCFile_area?.IsChecked ?? false;
                    });
                }
            }

            if (this.setLogic.SetParam.AutoGenerateC == true && generC_temp == false)
            {
                if (mainWindow?.Dispatcher != null && !mainWindow.Dispatcher.HasShutdownStarted)
                {
                    mainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        if (this.mainWindow.GenerateCFile_area != null)
                        {
                            this.mainWindow.GenerateCFile_area.IsChecked = true;
                        }
                    });
                }
            }
        } 


        // 更新设置界面
        private void UpdateSetPage_Page(SettingLogic_Class.setParm_t param)
        {
            this.mainWindow.Dispatcher.Invoke(() =>
            {
                this.mainWindow.HistorySizeValue.Text = param.HistorySize.ToString(); // 历史记录大小
                this.mainWindow.SaveFilePath.Text = param.FileSavePath; // 文件保存路径
                this.mainWindow.LogFilePath.Text = param.LogSavePath;   // 日志保存路径
                this.mainWindow.AutoGenerateCFile.IsChecked = param.AutoGenerateC; // 设置自动生成C文件
                this.mainWindow.EnableLogging.IsChecked = param.GenerateLog; // 设置生成日志

                foreach (var item in this.mainWindow.ThemeSelector.Items)
                {
                    if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag.ToString() == param.DisplayMode.ToString())
                    {
                        this.mainWindow.ThemeSelector.SelectedItem = comboBoxItem;
                        break;
                    }
                }
                // if(this.flag.isUpdateDataFormate == Flag.ON)
                {
                   // this.flag.isUpdateDataFormate = Flag.OFF;
                    foreach (var item in this.mainWindow.DataFormate_ComboBox.Items)
                    {
                        if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag.ToString() == param.Dataformate.ToString())
                        {
                            this.mainWindow.DataFormate_ComboBox.SelectedItem = comboBoxItem;
                            break;
                        }
                    }
                }

              //  if(this.flag.isUpdateFileHeader == Flag.ON)
                {
                    // this.flag.isUpdateFileHeader = Flag.OFF;
                    foreach (var item in this.mainWindow.SaveFileHeader_ComboBox.Items)
                    {
                      if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag.ToString() == param.SaveFileHeader.ToString())
                        {
                            this.mainWindow.SaveFileHeader_ComboBox.SelectedItem = comboBoxItem;
                            break;
                        }
                    }
                }


               if (this.flag.isUpdateThemes == Flag.ON)
                {
                    this.flag.isUpdateThemes = Flag.OFF;

 
                    if (param.DisplayMode == SettingLogic_Class.DisplayMode_t.LightMode)
                    {
                        GlobalTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);

                    }
                    else if (param.DisplayMode == SettingLogic_Class.DisplayMode_t.DarkMode)
                    {
                        GlobalTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                    }
                    else if(param.DisplayMode == SettingLogic_Class.DisplayMode_t.AutoMode)
                    {
                        switch(GetSystem_Themes())
                        {
                            case SettingLogic_Class.DisplayMode_t.DarkMode:
                            GlobalTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
                            break;

                            case SettingLogic_Class.DisplayMode_t.LightMode:
                            GlobalTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
                            break;
                            default:

                            break;
                        }
                        
                    }

                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(GlobalTheme);
                }
                // 更新滑动条的数值
                this.mainWindow.HistorySizeSlider.Value = param.HistorySize;

                // 更新是否保留文件头的下拉框
                //this.mainWindow.SaveFileHeader_ComboBox.SetValue();

            });
        }

        // 显示模式
        private void Check_ItemBox(ref SettingLogic_Class.DisplayMode_t mode)
        {
            switch(this.setLogic_buff.DisplayMode_StringTemp)
            {
                case "DarkMode":
                    mode = SettingLogic_Class.DisplayMode_t.DarkMode; // 黑暗模式
                    break;
                case "LightMode":
                    mode = SettingLogic_Class.DisplayMode_t.LightMode;
                    break;
                case "AutoMode":
                    mode = SettingLogic_Class.DisplayMode_t.AutoMode; // 跟随系统
                    break;
                default:
                    break;

            }
        }
        // 数据格式
        private void Check_ItemBox(ref SettingLogic_Class.DataFormate_t formate)
        {
            switch (this.setLogic_buff.DataFormate_StringTemp) // 设置数据格式
            {
                case "ALL":
                    formate = SettingLogic_Class.DataFormate_t.ALL;
                    break;
                case "PNG":
                    formate = SettingLogic_Class.DataFormate_t.PNG;
                    break;
                case "HEX":
                    formate = SettingLogic_Class.DataFormate_t.HEX;
                    break;
                case "JPG":
                    formate = SettingLogic_Class.DataFormate_t.JPG;
                    break;
                case "BIN":
                    formate = SettingLogic_Class.DataFormate_t.BIN;
                    break;
                case "WAV":
                    formate = SettingLogic_Class.DataFormate_t.WAV;
                    break;
                case "MP3":
                    formate = SettingLogic_Class.DataFormate_t.MP3;
                    break;
                default:
                    formate = SettingLogic_Class.DataFormate_t.ALL;
                    break;
            }
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
                this.mainWindow.LogFilePath.Text = "/default";   // 日志保存路径
                this.mainWindow.AutoGenerateCFile.IsChecked = false; // 取消自动生成c文件勾选
                this.mainWindow.EnableLogging.IsChecked = false; // 取消日志功能勾选
                this.mainWindow.HistorySizeSlider.Value = 20;
                foreach (var item in this.mainWindow.ThemeSelector.Items)
                {
                    if (item is ComboBoxItem comboBoxItem && (string)comboBoxItem.Tag == "AutoMode")
                    {
                        this.mainWindow.ThemeSelector.SelectedItem = comboBoxItem;
                        break;
                    }
                }

                foreach(var item in this.mainWindow.DataFormate_ComboBox.Items)
                {
                    // 遍历寻找ALL
                    if(item is ComboBoxItem comboBoxItem && (string)comboBoxItem.Tag == "ALL")
                    {
                        this.mainWindow.DataFormate_ComboBox.SelectedItem = comboBoxItem;
                        break;
                    }
                }
                this.mainWindow.HistorySizeSlider.Value = 20;
            });
        }


        // 处理设置页面
        public void DealWith_SetPage()
        {
            if (this.flag.isClickLogPath == Flag.ON)
            {
                Select_SaveLogPath();
                this.flag.isClickLogPath = Flag.OFF;
            }

            if (this.flag.isClickFileSavePath == Flag.ON)
            {
                Select_SaveFilePath();
                this.flag.isClickFileSavePath = Flag.OFF;
            }

            if (this.flag.isLocalSetUpdate == Flag.ON && this.flag.isCancelSet == Flag.OFF) // 局部打开
            {
                this.flag.isLocalSetUpdate = Flag.OFF;
                if(this.flag.isUpdateThemes == Flag.ON)
                {
                    //this.flag.isUpdateThemes = Flag.OFF; // 这里先不要关掉标志位，，这里主题还没刷
                    Check_ItemBox(ref setLogic_buff.SetParam.DisplayMode);
                }
                    
                if (this.flag.isUpdateDataFormate == Flag.ON)
                {
                    this.flag.isUpdateDataFormate = Flag.OFF;
                    Check_ItemBox(ref setLogic_buff.SetParam.Dataformate);
                }
                
                if(this.flag.isUpdateFileHeader == Flag.ON)
                {
                    this.flag.isUpdateFileHeader = Flag.OFF;

                    UpdateSave_FileHeader(ref setLogic_buff);
                }

                UpdateSetPage_Page(setLogic_buff.SetParam); // 刷ui

            }


            if (this.flag.isGlobalSetUpdate == Flag.ON && this.flag.isCancelSet == Flag.OFF) // 如果是全局设置
            {
                this.flag.isGlobalSetUpdate = Flag.OFF; // 关闭全局设置
                this.flag.isGenSetLog = Flag.ON; // 生成设置日志
                setLogic.SetParam = setLogic_buff.SetParam;
                UpdateSetPage_Page(setLogic.SetParam); // 刷ui 因为缓存不一定是设置，设置也不一定是缓存，所以两个参数刷两次

            }

            if (this.flag.isCancelSet == Flag.ON)
            {
                this.flag.isCancelSet = Flag.OFF;
                // 此处恢复默认值
                setLogic_buff.SetParam.HistorySize = 20;
                setLogic_buff.SetParam.AutoGenerateC = false;
                setLogic_buff.SetParam.GenerateLog = false;
                setLogic_buff.SetParam.LogSavePath = "/default";
                setLogic_buff.SetParam.FileSavePath = "/default";
                this.setLogic.SetParam = setLogic_buff.SetParam;
                this.flag.isFollowFilePath = Flag.OFF; // 不跟随
                this.flag.isFollowLogPath = Flag.OFF; // 不跟随
                // 刷默认ui
                UpdateSetPage_Page();
            }

            if(this.flag.isExitSetupPage == Flag.ON)
            {
                this.flag.isExitSetupPage = Flag.OFF;
                // 在用户切换页面的时候可能不想保存当前设置
                // 那么此处可以恢复之前的设置
                setLogic_buff.SetParam = this.setLogic.SetParam;
                UpdateSetPage_Page(this.setLogic.SetParam);
            }
        }

        public void setHistorySize(UInt16 size)
        {
            this.setLogic_buff.SetParam.HistorySize = size;
            this.flag.isLocalSetUpdate = Flag.ON; 
        }
        // 系统标志位自更新
        public void DealWith_SysFlagUpdate()
        {
            if (Application.Current?.Dispatcher == null) return; // 非空检查，保护软件安全
            
                Application.Current.Dispatcher.Invoke(() =>
                {
                    setLogic_buff.SetParam.AutoGenerateC = this.mainWindow.AutoGenerateCFile.IsChecked ?? false;
                    setLogic_buff.SetParam.GenerateLog = this.mainWindow.EnableLogging.IsChecked ?? false;
                });
            



            if (setLogic_buff.SetParam.AutoGenerateC != setLogic.SetParam.AutoGenerateC)
            {
                this.flag.isLocalSetUpdate = Flag.ON; // 开始局部刷新
            }
            if(setLogic_buff.SetParam.GenerateLog != setLogic.SetParam.GenerateLog)
            {
                this.flag.isLocalSetUpdate = Flag.ON;
            }
        }

        /**
         * 
         * 获取系统主题
         */
        private const string RegistryKeyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";
        private SettingLogic_Class.DisplayMode_t GetSystem_Themes()
        {
            object themeValue = Registry.GetValue(RegistryKeyPath, RegistryValueName, 1);
            return (themeValue is int value && value == 0) ? SettingLogic_Class.DisplayMode_t.DarkMode : SettingLogic_Class.DisplayMode_t.LightMode;
        }

        /**
         * 监听主题变化
         */
        public void MonitorSystemTheme()
        {
            if(this.setLogic_buff.SetParam.DisplayMode == SettingLogic_Class.DisplayMode_t.AutoMode
                || this.setLogic.SetParam.DisplayMode == SettingLogic_Class.DisplayMode_t.AutoMode)
            {
                if(this.flag.isInitMonitorTheme == Flag.ON)
                {
                    this.flag.isInitMonitorTheme = Flag.OFF;
                    this.flag.isLocalSetUpdate = Flag.ON;
                    this.flag.isUpdateThemes = Flag.ON;
                }
                SystemEvents.UserPreferenceChanged += (sender, e) =>
                {
                    if (e.Category == UserPreferenceCategory.General )
                    {
                        this.flag.isLocalSetUpdate = Flag.ON;
                        this.flag.isUpdateThemes = Flag.ON;
                    }
                };
            }

        }

        /**
         * 更新是否保留头文件的系统参数
         */
        private void UpdateSave_FileHeader(ref SettingLogic_Class param)
        {
            switch(param.SaveFileHeader_StringTemp)
            {
                case "YES":
                    param.SetParam.SaveFileHeader = SettingLogic_Class.SaveFileHeader_t.YES;
                    break;
                case "NO":
                    param.SetParam.SaveFileHeader = SettingLogic_Class.SaveFileHeader_t.NO;
                    break;
                default:
                    param.SetParam.SaveFileHeader = SettingLogic_Class.SaveFileHeader_t.YES;
                    break;
            }
        }

        // 处理日志逻辑
        public void DealWith_Log()
        {
            // 日志总开关
            if (this.setLogic.SetParam.GenerateLog == false) return;
            // 初始化日志逻辑
            if(this.flag.isNeedInitLog == Flag.ON)
            {
                this.flag.isNeedInitLog = Flag.OFF;
                if(this.flag.isFollowLogPath == Flag.ON && Filepath != null) // 如果日志跟随路径
                {
                    Log = new AppLog_Class(Filepath);
                } else if(this.flag.isFollowLogPath == Flag.OFF && (this.setLogic.SetParam.LogSavePath != null || this.setLogic.SetParam.LogSavePath != "/default"))
                {
                    Log = new AppLog_Class(this.setLogic.SetParam.LogSavePath);
                }
            }

            if (Log == null) return;
            // 生成日志逻辑

            // 生成设置日志
            if(this.flag.isGenSetLog == Flag.ON)
            {
                this.flag.isGenSetLog = Flag.OFF;
                this.Log.GeneralLog_Set(this.setLogic);
            }

            // 生成首页日志
            if(this.flag.isGenFrontLog == Flag.ON)
            {
                this.flag.isGenFrontLog = Flag.OFF;
                this.Log.GeneralLog_Front();
            }

            // 生成历史记录日志
            if(this.flag.isGenHistoryLog == Flag.ON)
            {
                this.flag.isGenHistoryLog = Flag.OFF;
                this.Log.GeneralLog_History();
            }

            if(this.flag.isGenUartLog == Flag.ON)
            {
                this.flag.isGenUartLog = Flag.OFF;
                this.Log.GeneralLog_Uart();
            }
        }

    }

}