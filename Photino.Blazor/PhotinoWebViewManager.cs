﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using PhotinoNET;

namespace Photino.Blazor
{
    public class PhotinoWebViewManager : WebViewManager
    {
        private readonly PhotinoWindow _window;
        private readonly Channel<string> _channel;
        private readonly IOptions<PhotinoBlazorAppConfiguration> _config;

        public PhotinoWebViewManager(PhotinoWindow window, IServiceProvider provider, Dispatcher dispatcher,
            IFileProvider fileProvider, JSComponentConfigurationStore jsComponents, IOptions<PhotinoBlazorAppConfiguration> config)
            : base(provider, dispatcher, config.Value.AppBaseUri, fileProvider, jsComponents, config.Value.HostPage)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Create a scheduler that uses one threads.
            var sts = new Utils.SynchronousTaskScheduler();

            _window.WebMessageReceived += (sender, message) =>
            {
                // On some platforms, we need to move off the browser UI thread
                Task.Factory.StartNew(message =>
                {
                    // TODO: Fix this. Photino should ideally tell us the URL that the message comes from so we
                    // know whether to trust it. Currently it's hardcoded to trust messages from any source, including
                    // if the webview is somehow navigated to an external URL.
                    var messageOriginUrl = config.Value.AppBaseUri;

                    MessageReceived(messageOriginUrl, (string)message!);
                }, message, CancellationToken.None, TaskCreationOptions.DenyChildAttach, sts);
            };

            //Create channel and start reader
            _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = false });
            Task.Run(messagePump);
        }

        public Stream HandleWebRequest(object sender, string schema, string url, out string contentType)
        {
            // It would be better if we were told whether or not this is a navigation request, but
            // since we're not, guess.
            var localPath = (new Uri(url)).LocalPath;
            var hasFileExtension = localPath.LastIndexOf('.') > localPath.LastIndexOf('/');

            //Remove parameters before attempting to retrieve the file. For example: http://localhost/_content/Blazorise/button.js?v=1.0.7.0
            if (url.Contains('?')) url = url.Substring(0, url.IndexOf('?'));

            if (url.StartsWith(_config.Value.AppBaseUri.OriginalString, StringComparison.Ordinal)
                && TryGetResponseContent(url, !hasFileExtension, out var statusCode, out var statusMessage,
                    out var content, out var headers))
            {
                headers.TryGetValue("Content-Type", out contentType);
                return content;
            }
            else
            {
                contentType = default;
                return null;
            }
        }

        protected override void NavigateCore(Uri absoluteUri)
        {
            _window.Load(absoluteUri);
        }

        protected override void SendMessage(string message)
        {
            while (!_channel.Writer.TryWrite(message))
                Thread.Sleep(200);
        }

        async Task messagePump()
        {
            var reader = _channel.Reader;
            try
            {
                while (true)
                {
                    var message = await reader.ReadAsync();
                    _window.SendWebMessage(message);
                }
            }
            catch (ChannelClosedException) { }
        }

        protected override ValueTask DisposeAsyncCore()
        {
            //complete channel
            try { _channel.Writer.Complete(); } catch { }

            //continue disposing
            return base.DisposeAsyncCore();
        }
    }
}