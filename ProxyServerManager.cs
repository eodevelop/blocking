using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy;

namespace Blocking
{
    public class ProxyServerManager
    {
        private readonly ProxyServer proxyServer;
        private HashSet<string> blockedWords;

        public ProxyServerManager()
        {
            proxyServer = new ProxyServer();

            // HTTPS 지원을 위해 루트 인증서 설치
            proxyServer.CertificateManager.EnsureRootCertificate();

            // 요청 처리 이벤트 핸들러 설정
            proxyServer.BeforeRequest += OnRequest;

            // 프록시 엔드포인트 설정
            var explicitEndPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, 8080, true);
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
