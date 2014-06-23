using System.Windows;

namespace WinAirvid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            libairvidproto.LibIniter.Init(new ByteOrderConvAdp());
            InitializeComponent();
        }
    }
}
