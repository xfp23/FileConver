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

namespace FileConver
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppDevice.flag.isInitHistoryList = Flag.ON;
        }
        private static APPDevice_Class AppDevice = new APPDevice_Class();
        public enum PageSelect_t
        {
            FRONT_PAGE, // 首页
            HISTORY_PAGE,// 历史记录页面
            SETUP_PAGE, // 设置页面
            UART_PAGE, // 串口页面

        };

        private void PageManage(PageSelect_t page)
        {
            switch (page)
            {
                case PageSelect_t.FRONT_PAGE:
                    FrontePage_Grid.Visibility = Visibility.Visible; // 首页显示
                    HistoryFile_Grid.Visibility = Visibility.Collapsed;
                    /* 在此处关闭其它页面 */
                    break;
                case PageSelect_t.HISTORY_PAGE:
                    HistoryFile_Grid.Visibility = Visibility.Visible;
                    FrontePage_Grid.Visibility = Visibility.Collapsed;
                    break;
                case PageSelect_t.SETUP_PAGE:
                    HistoryFile_Grid.Visibility = Visibility.Collapsed;
                    FrontePage_Grid.Visibility = Visibility.Collapsed;
                    break;
                case PageSelect_t.UART_PAGE:
                    HistoryFile_Grid.Visibility = Visibility.Collapsed;
                    FrontePage_Grid.Visibility = Visibility.Collapsed;

                    break;   
                default:
                    HistoryFile_Grid.Visibility = Visibility.Collapsed;
                    FrontePage_Grid.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private static PageSelect_t PageSel = PageSelect_t.FRONT_PAGE;

        /**
         * @brief 首页按钮
         * 
         */
        private void FrontePage_butt(object sender, RoutedEventArgs e)
        {
            PageSel = PageSelect_t.FRONT_PAGE;
            PageManage(PageSel);
        }

        private void HistoryRecord_butt(object sender, RoutedEventArgs e)
        {
            PageSel = PageSelect_t.HISTORY_PAGE;
            PageManage(PageSel);
            if(AppDevice.flag.isInitHistoryList == Flag.ON)
            {
                AppDevice.flag.isInitHistoryList = Flag.OFF;

            }
        }

        private void Setup_button(object sender, RoutedEventArgs e)
        {
            PageSel = PageSelect_t.SETUP_PAGE;
            PageManage(PageSel);
        }

        /**
         * 此处开始转换
         * 
         * 
         */
        private void StartConvert_butt(object sender, RoutedEventArgs e)
        {
            //if (AppDevice.FileAlreadyUpload == false) return;
            if (GenerateCFile_area.IsChecked == false)
            {
                MainOutput.Text = AppDevice.GenerCArray(false);
                return;
            }
            MainOutput.Text = AppDevice.GenerCArray(true);


        }
        /**
         * 此处上传文件
         * 
         */
        private void Upload_butt(object sender, RoutedEventArgs e)
        {
            if(PageSel == PageSelect_t.FRONT_PAGE)
            {
                AppDevice.Upload_File();
            }
        }

        private void UartSend_butt(object sender, RoutedEventArgs e)
        {
            PageSel = PageSelect_t.UART_PAGE;
            PageManage(PageSel);
        }

        private void ClearHistory_butt(object sender, RoutedEventArgs e)
        {

        }

        private void HistoryFile_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void ShowConversionHistory(object sender, RoutedEventArgs e)
        {
            // TODO: 这里可以添加显示转换历史的逻辑
            MessageBox.Show("显示转换历史");
        }

    }
}