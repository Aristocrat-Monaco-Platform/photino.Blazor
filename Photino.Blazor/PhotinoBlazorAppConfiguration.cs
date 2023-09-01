using System;
using System.Runtime.InteropServices;

namespace Photino.Blazor
{
    public class PhotinoBlazorAppConfiguration
    {
        // On Windows, we can't use a custom scheme to host the initial HTML,
        // because webview2 won't let you do top-level navigation to such a URL.
        // On Linux/Mac, we must use a custom scheme, because their webviews
        // don't have a way to intercept http:// scheme requests.
        public string AppScheme { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "http"
            : "app";

        public Uri AppBaseUri => new Uri($"{AppScheme}://localhost/");

        public string HostPage { get; set; }
    }
}
