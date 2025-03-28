using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static APPLogic.APPDevice_Class;
using UserTim;
using APPLogic;
using System.CodeDom;

namespace FileConver
{
    public enum PageSelect_t
    {
        FRONT_PAGE, // 首页
        HISTORY_PAGE, // 历史记录页面
        SETUP_PAGE, // 设置页面
        UART_PAGE, // 串口页面
    }
    public partial class MainWindow : Window
    {
        private APPDevice_Class AppDevice;
        private static PageSelect_t PageSel = PageSelect_t.FRONT_PAGE;
        private UserTim_Class userTimer;

        private Thread workerThread; // 工作线程
        private bool isRunning = true;



        public MainWindow()
        {
            InitializeComponent();
            AppDevice = new APPDevice_Class(this);
            AppDevice.flag.isInitHistoryList = Flag.ON;
            userTimer = new UserTim_Class(); // 创建一个用户定时器
            workerThread = new Thread(Taskloop);
            workerThread.IsBackground = true;
            workerThread.Start();
        }


        // while(1)
        private void Taskloop()
        {
            Debug.WriteLine($"Taskloop 被调用, UserTimer={true}");

            while (isRunning)
            {
                if (userTimer.UserTimFlag.system1ms_Flag)
                {
                    Debug.WriteLine("[线程] 处理 1ms 任务...");
                    userTimer.UserTimFlag.system1ms_Flag = false;
                    AppDevice.MonitorSystemTheme();
                }

                if (userTimer.UserTimFlag.system10ms_Flag)
                {
                    Debug.WriteLine("[线程] 处理 10ms 任务...");
                    userTimer.UserTimFlag.system10ms_Flag = false;
                    // **在 UI 线程上调用 PageManage**
                    if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.HasShutdownStarted)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            PageManage(PageSel);
                        });
                    }

                    //PageManage(PageSel);
                    AppDevice.DealWith_Front(); // 处理首页
                    AppDevice.DealWith_SysFlagUpdate();

                }

                if (userTimer.UserTimFlag.system100ms_Flag)
                {
                    Debug.WriteLine("[线程] 处理 100ms 任务...");
                    userTimer.UserTimFlag.system100ms_Flag = false;
                    AppDevice.DealWith_History();
                }

                if (userTimer.UserTimFlag.system500ms_Flag)
                {
                    Debug.WriteLine("[线程] 处理 500ms 任务...");
                    userTimer.UserTimFlag.system500ms_Flag = false;
                   
                    AppDevice.DealWith_SetPage(); // 处理设置页面
                }

                if (userTimer.UserTimFlag.system1000ms_Flag)
                {
                    Debug.WriteLine("[线程] 处理 1000ms 任务...");
                    userTimer.UserTimFlag.system1000ms_Flag = false;
                    AppDevice.DealWith_Log();
                }

                // 避免 CPU 过载
                Thread.Sleep(1);
            }
        }

        private void ExitSetup_Page()
        {
            if (PageSel == PageSelect_t.SETUP_PAGE) // 如果是设置页面
            {
                AppDevice.flag.isExitSetupPage = Flag.ON;
            }
        }

        /**
        * 管理页面
*/
        private void PageManage(PageSelect_t page)
        {
            FrontePage_Grid.Visibility = (page == PageSelect_t.FRONT_PAGE) ? Visibility.Visible : Visibility.Collapsed;
            HistoryFile_Grid.Visibility = (page == PageSelect_t.HISTORY_PAGE) ? Visibility.Visible : Visibility.Collapsed;
            SettingsPanel_Grid.Visibility = (page == PageSelect_t.SETUP_PAGE) ? Visibility.Visible : Visibility.Collapsed;
        }
        // 处理导航栏按钮
        private void FrontePage_butt(object sender, RoutedEventArgs e)
        {
            ExitSetup_Page();
            PageSel = PageSelect_t.FRONT_PAGE;
        }

        // 历史记录按钮导航栏
        private void HistoryRecord_butt(object sender, RoutedEventArgs e)
        {
            ExitSetup_Page();
            PageSel = PageSelect_t.HISTORY_PAGE;
            if (AppDevice.flag.isInitHistoryList == Flag.ON)
            {
                AppDevice.flag.isInitHistoryList = Flag.OFF;
            }

        }

        // 设置按钮导航栏
        private void Setup_button(object sender, RoutedEventArgs e)
        {
            PageSel = PageSelect_t.SETUP_PAGE;
        }

        // 转换功能
        private void StartConvert_butt(object sender, RoutedEventArgs e)
        {

            AppDevice.flag.isStartConvertButt = Flag.ON;
            if (GenerateCFile_area.IsChecked == false)
            {
                AppDevice.flag.ischkGenCFile = Flag.OFF;
                // MainOutput.Text = AppDevice.GenerCArray(false);
                return;
            }
            // MainOutput.Text = AppDevice.GenerCArray(true);
            AppDevice.flag.ischkGenCFile = Flag.ON;


        }

        // 上传文件
        private void Upload_butt(object sender, RoutedEventArgs e)
        {
            //if (PageSel == PageSelect_t.FRONT_PAGE)
            //{
            //    AppDevice.Upload_File();
            //}
            AppDevice.flag.isUploadFile = Flag.ON;
        }

        // 串口发送
        private void UartSend_butt(object sender, RoutedEventArgs e)
        {
            ExitSetup_Page();
            PageSel = PageSelect_t.UART_PAGE;


        }

        // 清除历史
        private void ClearHistory_butt(object sender, RoutedEventArgs e)
        {
            AppDevice.flag.isClearHistoryButt = Flag.ON;

        }

        private void ShowConversionHistory(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("显示转换历史");
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            AppDevice.flag.isGlobalSetUpdate = Flag.ON; // 全局设置更新标志位打开
        }

        // 选择文件保存按钮
        private void SelectSavePath_Click(object sender, RoutedEventArgs e)
        {
            AppDevice.flag.isLocalSetUpdate = Flag.ON; // 局部设置更新标志位打开
            AppDevice.flag.isSelSavePathButt = Flag.ON; // 文件保存路径
            AppDevice.flag.isClickFileSavePath = Flag.ON;
            //AppDevice.Select_SaveFilePath();
        }

        // 选择日志文件保存按钮
        private void SelectLogPath_Click(object sender, RoutedEventArgs e)
        {

            AppDevice.flag.isLocalSetUpdate = Flag.ON; // 局部设置更新标志位打开
            AppDevice.flag.isFollowLogPath = Flag.OFF; // 日志跟随文件保存路径关闭
            AppDevice.flag.isClickLogPath = Flag.ON;
            //AppDevice.Select_SaveLogPath();
        }

        // 取消设置就是恢复默认设置
        private void CancelSettings_Click(object sender, RoutedEventArgs e)
        {
            AppDevice.flag.isCancelSet = Flag.ON; // 取消当前设置
        }
        private void HistoryFile_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void HistorySizeValue_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AppDevice == null) return;
            AppDevice.setHistorySize((UInt16)e.NewValue); // 取值

        }

        // 选择文件格式的回调
        private void DataFormat_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || AppDevice == null) return;
            AppDevice.flag.isLocalSetUpdate = Flag.ON;
            AppDevice.flag.isUpdateDataFormate = Flag.ON; // 开两个标志位是为了避免频繁刷新
            if (DataFormate_ComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                AppDevice.setLogic_buff.DataFormate_StringTemp = selectedItem.Tag.ToString();
            }
        }

        // 选择软件主题的回调
        private void ThemeSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || AppDevice == null) return;
            AppDevice.flag.isLocalSetUpdate = Flag.ON; // 局部刷新
            AppDevice.flag.isUpdateThemes = Flag.ON;
            if (ThemeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                AppDevice.setLogic_buff.DisplayMode_StringTemp = selectedItem.Tag.ToString();
            }
        }

        private void SaveFileHeader_ComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || AppDevice == null) return; // 非空检查
            AppDevice.flag.isLocalSetUpdate = Flag.ON;
            AppDevice.flag.isUpdateFileHeader = Flag.ON;
            if (SaveFileHeader_ComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                AppDevice.setLogic_buff.SaveFileHeader_StringTemp = selectedItem.Tag.ToString();
            }
        }

    }
}
