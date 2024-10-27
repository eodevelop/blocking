using PacketDotNet;
using SharpPcap;
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
    public partial class MainWindow : Window
    {
        private List<string> urlList = new List<string>();
     
        public MainWindow()
        {
            InitializeComponent();
            StartPacketCapture();
        }

        private void StartPacketCapture()
        {
            var devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                MessageBox.Show("No capture devices found.");
                return;
            }

            foreach (var dev in devices)
            {
                dev.OnPacketArrival += OnPacketArrival;
                dev.Open(); // 기본 설정으로 Open (Promiscuous 모드 활성화)
                dev.StartCapture();
            }
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var tcpPacket = packet.Extract<TcpPacket>();

            if (tcpPacket != null && tcpPacket.PayloadData != null)
            {
                string payload = Encoding.UTF8.GetString(tcpPacket.PayloadData);

                // HTTP GET 요청의 호스트 정보 추출
                if (payload.Contains("GET"))
                {
                    int hostIndex = payload.IndexOf("Host: ");
                    if (hostIndex >= 0)
                    {
                        int endIndex = payload.IndexOf("\r\n", hostIndex);
                        string host = payload.Substring(hostIndex + 6, endIndex - hostIndex - 6);
                        string url = "http://" + host;

                        // URL을 중복 없이 리스트에 추가 및 화면에 출력
                        if (!urlList.Contains(url))
                        {
                            urlList.Add(url);
                            Dispatcher.Invoke(() =>
                            {
                                urlListBox.Items.Add(url);
                            });
                        }
                    }
                }
            }
        }
    }
}