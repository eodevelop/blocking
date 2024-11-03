using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy;
using System.Windows;
using Titanium.Web.Proxy.Network;

namespace Blocking
{
    public class ProxyServerManager
    {
        private readonly ProxyServer proxyServer;
        private HashSet<string> blockedWords;

        public ProxyServerManager()
        {
            proxyServer = new ProxyServer();

            // 인증서 엔진 설정 (필요 시)
            proxyServer.CertificateManager.CertificateEngine = CertificateEngine.BouncyCastle; // 또는 CertificateEngine.DefaultWindows

            // 인증서 생성 및 설치
            try
            {
                // 루트 인증서 생성
                bool success = proxyServer.CertificateManager.CreateRootCertificate();
                if (!success)
                {
                    MessageBox.Show("루트 인증서 생성에 실패했습니다.", "인증서 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    // 루트 인증서를 신뢰할 수 있는 루트 인증 기관에 설치
                    proxyServer.CertificateManager.TrustRootCertificate(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"인증서 설치 중 오류 발생: {ex.Message}", "인증서 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 요청 처리 이벤트 핸들러 설정
            proxyServer.BeforeRequest += OnRequest;

            // 프록시 엔드포인트 설정
            var explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, 8000, true);
            proxyServer.AddEndPoint(explicitEndPoint);

            // 프록시 서버 시작
            proxyServer.Start();

            // 시스템 프록시 설정
            proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

            // 차단 단어 리스트 초기화
            blockedWords = new HashSet<string>();
        }

        // 요청을 가로채서 검사하는 메서드
        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            string url = e.HttpClient.Request.Url.ToLower();

            if (blockedWords.Any(word => url.Contains(word.ToLower())))
            {
                // 요청 차단 및 사용자에게 메시지 표시
                e.Ok("<html><body><h1>접근이 차단되었습니다.</h1></body></html>");
            }
        }

        // 차단할 단어 리스트 업데이트 메서드
        public void UpdateBlockedWords(IEnumerable<string> words)
        {
            blockedWords = new HashSet<string>(words);
        }

        // 프록시 서버 정지 및 인증서 제거
        public void Stop()
        {
            proxyServer.Stop();
            proxyServer.CertificateManager.RemoveTrustedRootCertificate();
        }
    }
}
