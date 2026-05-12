using Microsoft.Identity.Web;
using Microsoft.Kiota.Abstractions.Authentication;

namespace CAPolicyLab.Services;

// ITokenAcquisition(Microsoft.Identity.Web) 을 Kiota 의 IAccessTokenProvider 로 어댑터.
// Microsoft.Identity.Web.MicrosoftGraph 를 제거한 뒤 GraphServiceClient 를 수동 등록할 때 사용한다.
// IAuthenticationProviderOption 은 Graph.Core 2.x 에만 존재하므로
// Graph 5.x(Graph.Core 3.x) 환경에서는 이 방식으로 대체해야 TypeLoadException 이 발생하지 않는다.
public sealed class TokenAcquisitionTokenProvider(ITokenAcquisition tokenAcquisition)
    : IAccessTokenProvider
{
    private static readonly string[] Scopes = ["Policy.Read.All", "Directory.Read.All"];

    public AllowedHostsValidator AllowedHostsValidator { get; } = new(["graph.microsoft.com"]);

    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
        => await tokenAcquisition.GetAccessTokenForUserAsync(Scopes);
}
