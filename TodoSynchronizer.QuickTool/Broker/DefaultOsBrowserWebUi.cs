// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client.Platforms.Shared.DefaultOSBrowser;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Microsoft.Identity.Client.Platforms.Shared.Desktop.OsBrowser
{
    internal class DefaultOsBrowserWebUi
    {
        internal const string DefaultSuccessHtml = @"<html>
  <head><title>授权完成</title><meta http-equiv=""content-type"" content=""text/html"" charset=""utf-8"" /> </head>
  <body>
    <h2>
    授权已完成。您现在可以关闭此标签页并回到之前的应用程序。
    </h2>
  </body>
</html>";

        internal const string DefaultFailureHtml = @"<html>
  <head><title>Authentication Failed</title></head>
  <body>
    Authentication failed. You can return to the application. Feel free to close this browser tab.
</br></br></br></br>
    Error details: error {0} error_description: {1}
  </body>
</html>";

        private readonly IUriInterceptor _uriInterceptor;

        public DefaultOsBrowserWebUi(IUriInterceptor uriInterceptor = null)
        {
            _uriInterceptor = uriInterceptor ?? new HttpListenerInterceptor();
        }

        public async Task<string> AcquireAuthorizationAsync(
            Uri authorizationUri,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            try
            {
                var authCodeUri = await InterceptAuthorizationUriAsync(
                    authorizationUri,
                    redirectUri,
                    true,
                    cancellationToken)
                    .ConfigureAwait(true);

                if (!authCodeUri.Authority.Equals(redirectUri.Authority, StringComparison.OrdinalIgnoreCase) ||
                   !authCodeUri.AbsolutePath.Equals(redirectUri.AbsolutePath))
                {
                    throw new Exception("");
                }

                return authCodeUri.OriginalString;
            }
            catch (System.Net.HttpListenerException) // sometimes this exception sneaks out (see issue 1773)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
        }

        public Uri UpdateRedirectUri(Uri redirectUri)
        {
            if (!redirectUri.IsLoopback)
            {
                throw new Exception(
                    string.Format(CultureInfo.InvariantCulture,
                        "Only loopback redirect uri is supported, but {0} was found. " +
                        "Configure http://localhost or http://localhost:port both during app registration and when you create the PublicClientApplication object. " +
                        "See https://aka.ms/msal-net-os-browser for details", redirectUri.AbsoluteUri));
            }

            // AAD does not allow https:\\localhost redirects from any port
            if (redirectUri.Scheme != "http")
            {
                throw new Exception(
                    string.Format(CultureInfo.InvariantCulture,
                        "Only http uri scheme is supported, but {0} was found. " +
                        "Configure http://localhost or http://localhost:port both during app registration and when you create the PublicClientApplication object. " +
                        "See https://aka.ms/msal-net-os-browser for details", redirectUri.Scheme));
            }

            return FindFreeLocalhostRedirectUri(redirectUri);
        }

        private static Uri FindFreeLocalhostRedirectUri(Uri redirectUri)
        {
            if (redirectUri.Port > 0 && redirectUri.Port != 80)
            {
                return redirectUri;
            }

            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                return new Uri("http://localhost:" + port);
            }
            finally
            {
                listener?.Stop();
            }
        }

        private async Task<Uri> InterceptAuthorizationUriAsync(
            Uri authorizationUri,
            Uri redirectUri,
            bool isBrokerConfigured,
            CancellationToken cancellationToken)
        {
            Func<Uri, Task> defaultBrowserAction = (Uri u) => StartDefaultOsBrowserAsync(u.AbsoluteUri, isBrokerConfigured);
            Func<Uri, Task> openBrowserAction = defaultBrowserAction;

            cancellationToken.ThrowIfCancellationRequested();
            await openBrowserAction(authorizationUri).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();
            return await _uriInterceptor.ListenToSingleRequestAndRespondAsync(
                redirectUri.Port,
                redirectUri.AbsolutePath,
                GetResponseMessage,
                cancellationToken)
            .ConfigureAwait(false);
        }

        internal /* internal for testing only */ MessageAndHttpCode GetResponseMessage(Uri authCodeUri)
        {
            // Parse the uri to understand if an error was returned. This is done just to show the user a nice error message in the browser.
            return new MessageAndHttpCode(HttpStatusCode.OK, DefaultSuccessHtml);
        }

        public Task StartDefaultOsBrowserAsync(string url, bool isBrokerConfigured)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }

            return Task.FromResult(0);
        }
    }
}
