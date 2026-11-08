using System.Net;

namespace KgWebApi.Net.Services;

public sealed class WebApiCookieContainerHandler(CookieContainer cookieContainer) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestUri = request.RequestUri;
        if (requestUri is not null)
        {
            var cookieHeader = cookieContainer.GetCookieHeader(requestUri);
            if (!string.IsNullOrWhiteSpace(cookieHeader))
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (requestUri is not null && response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            foreach (var setCookieHeader in setCookieHeaders)
            {
                try
                {
                    cookieContainer.SetCookies(requestUri, setCookieHeader);
                }
                catch (CookieException)
                {
                    // Ignore malformed upstream cookies while preserving the response.
                }
            }
        }

        return response;
    }
}
