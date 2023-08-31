using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PhotinoNET;

namespace Photino.Blazor
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorDesktop(this IServiceCollection services, string appScheme = null)
        {
            services
                .AddOptions<PhotinoBlazorAppConfiguration>()
                .Configure(opts =>
                {
                    opts.AppScheme = appScheme ?? opts.AppScheme;
                    opts.HostPage = "index.html";
                });

            return services
                .AddScoped(sp =>
                {
                    var handler = sp.GetService<PhotinoHttpHandler>();
                    var opts = sp.GetService<IOptions<PhotinoBlazorAppConfiguration>>();
                    return new HttpClient(handler) { BaseAddress = opts.Value.AppBaseUri };
                })
                .AddSingleton(sp =>
                {
                    var manager = sp.GetService<PhotinoWebViewManager>();
                    var store = sp.GetService<JSComponentConfigurationStore>();

                    return new BlazorWindowRootComponents(manager, store);
                })
                .AddSingleton<Dispatcher, PhotinoDispatcher>()
                .AddSingleton<IFileProvider>(_ =>
                {
                    var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
                    return new PhysicalFileProvider(root);
                })
                .AddSingleton<JSComponentConfigurationStore>()
                .AddSingleton<PhotinoBlazorApp>()
                .AddSingleton<PhotinoHttpHandler>()
                .AddSingleton<PhotinoSynchronizationContext>()
                .AddSingleton<PhotinoWebViewManager>()
                .AddSingleton(new PhotinoWindow())
                .AddBlazorWebView();
        }
    }
}
