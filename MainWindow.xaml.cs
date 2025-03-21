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
        }
        private static APPDevice_Class AppDevice = new APPDevice_Class();
        public enum PageSelect_t
        {
            FRONT_PAGE, // 首页
            HISTORY_PAGE,// 历史记录页面
            SETUP_PAGE, // 设置页面

        };

        private void PageManage(PageSelect_t page)
        {
            switch (page)
            {
                case PageSelect_t.FRONT_PAGE:
                    PageButton_area.Visibility = Visibility.Visible;
                    Text_box.Visibility = Visibility.Visible;
                    /* 在此处关闭其它页面 */
                    break;
                case PageSelect_t.HISTORY_PAGE:
                    PageButton_area.Visibility = Visibility.Collapsed;
                    Text_box.Visibility = Visibility.Collapsed;
                    break;
                case PageSelect_t.SETUP_PAGE:
                    PageButton_area.Visibility = Visibility.Collapsed;
                    Text_box.Visibility = Visibility.Collapsed;
                    break;
                default:
                    PageButton_area.Visibility = Visibility.Collapsed;
                    Text_box.Visibility = Visibility.Collapsed;
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
            if (AppDevice.FileAlreadyUpload == false) return;
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
    }
}