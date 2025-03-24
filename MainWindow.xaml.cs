using System.Diagnostics;
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
using static APPDevice_Class;
using System.Threading;
using UserTim;

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
                Console.WriteLine("[线程] 处理 1ms 任务...");
                userTimer.UserTimFlag.system1ms_Flag = false;

            }

            if (userTimer.UserTimFlag.system10ms_Flag)
            {
                Console.WriteLine("[线程] 处理 10ms 任务...");
                userTimer.UserTimFlag.system10ms_Flag = false;
                // **在 UI 线程上调用 PageManage**
                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PageManage(PageSel);
                    });
                }

            }

            if (userTimer.UserTimFlag.system100ms_Flag)
            {
                Console.WriteLine("[线程] 处理 100ms 任务...");
                userTimer.UserTimFlag.system100ms_Flag = false;
                AppDevice.DealWith_Front(); // 处理首页
            }

            if (userTimer.UserTimFlag.system500ms_Flag)
            {
                Console.WriteLine("[线程] 处理 500ms 任务...");
                userTimer.UserTimFlag.system500ms_Flag = false;
                AppDevice.DealWith_History();
            }

            if (userTimer.UserTimFlag.system1000ms_Flag)
            {
                Console.WriteLine("[线程] 处理 1000ms 任务...");
                userTimer.UserTimFlag.system1000ms_Flag = false;
            }

            // 避免 CPU 过载
            Thread.Sleep(1);
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
        PageSel = PageSelect_t.FRONT_PAGE;
    }

    private void HistoryRecord_butt(object sender, RoutedEventArgs e)
    {
        PageSel = PageSelect_t.HISTORY_PAGE;
        if (AppDevice.flag.isInitHistoryList == Flag.ON)
        {
            AppDevice.flag.isInitHistoryList = Flag.OFF;
        }
    }

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
        if (PageSel == PageSelect_t.FRONT_PAGE)
        {
            AppDevice.Upload_File();
        }
    }

    // 串口发送
    private void UartSend_butt(object sender, RoutedEventArgs e)
    {
        PageSel = PageSelect_t.UART_PAGE;
        // PageManage(PageSel);
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

    private void SaveSettings_Click(object sender, RoutedEventArgs e) { }
    private void SelectSavePath_Click(object sender, RoutedEventArgs e) { }
    private void SelectLogPath_Click(object sender, RoutedEventArgs e) { }

    private void HistoryFile_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

}
}
