using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Blocking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProxyServerManager proxyManager;
        private ObservableCollection<string> blockedWords;

        public MainWindow()
        {
            InitializeComponent();
            // ProxyServerManager 인스턴스 생성
            proxyManager = new ProxyServerManager();

            // 차단 단어 리스트 초기화
            blockedWords = new ObservableCollection<string>();
            BlockedWordsListBox.ItemsSource = blockedWords;
        }

        private void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            string word = BlockedWordTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(word) && !blockedWords.Contains(word))
            {
                blockedWords.Add(word);
                proxyManager.UpdateBlockedWords(blockedWords);

                BlockedWordTextBox.Clear();
            }
            else
            {
                MessageBox.Show("이미 존재하는 단어이거나 유효하지 않은 입력입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 프록시 서버를 정지하고 리소스를 해제합니다.
            proxyManager.Stop();
            base.OnClosing(e);
        }
    }
}