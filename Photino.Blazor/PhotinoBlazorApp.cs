using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PhotinoNET;

namespace Photino.Blazor
{
    public class PhotinoBlazorApp
    {
        /// <summary>
        /// Gets configuration for the service provider.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Gets configuration for the root components in the window.
        /// </summary>
        public BlazorWindowRootComponents RootComponents { get; private set; }
        public PhotinoBlazorAppConfiguration Configuration { get; private set; }

        internal void Initialize(IServiceProvider services, RootComponentList rootComponents)
        {
            Services = services;
            RootComponents = Services.GetService<BlazorWindowRootComponents>();
            MainWindow = Services.GetService<PhotinoWindow>();
            WindowManager = Services.GetService<PhotinoWebViewManager>();
            Configuration = Services.GetService<IOptions<PhotinoBlazorAppConfiguration>>().Value;

            MainWindow
                .SetTitle("Photino.Blazor App")
                .SetUseOsDefaultLocation(false)
                .SetWidth(WindowConstants.DefaultWidth)
                .SetHeight(WindowConstants.DefaultHeight)
                .SetLeft(WindowConstants.DefaultLeft)
                .SetTop(WindowConstants.DefaultTop);

            MainWindow.RegisterCustomSchemeHandler(Configuration.AppScheme, HandleWebRequest);

            foreach (var component in rootComponents)
            {
                RootComponents.Add(component.Item1, component.Item2);
            }
        }

        public PhotinoWindow MainWindow { get; private set; }

        public PhotinoWebViewManager WindowManager { get; private set; }

        public void Run()
        {
            if (string.IsNullOrWhiteSpace(MainWindow.StartUrl))
                MainWindow.StartUrl = "/";

            WindowManager.Navigate(MainWindow.StartUrl);
            MainWindow.WaitForClose();
        }

        public Stream HandleWebRequest(object sender, string scheme, string url, out string contentType)
                => WindowManager.HandleWebRequest(sender, scheme, url, out contentType!)!;
    }
}
