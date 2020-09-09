// Copyright (c) Cloud Native Foundation. 
// Licensed under the Apache 2.0 license.
// See LICENSE file in the project root for full license information.

namespace HttpSend
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Net.Http;
    using System.Net.Mime;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudNative.CloudEvents;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;

    // This application uses the McMaster.Extensions.CommandLineUtils library for parsing the command
    // line and calling the application code. The [Option] attributes designate the parameters.
    class Program
    {
        [Option(Description = "CloudEvents 'source' (default: urn:example-com:mysource:abc)", LongName = "source",
            ShortName = "s")]
        string Source { get; } = "urn:example-com:mysource:abc";

        [Option(Description = "CloudEvents 'type' (default: com.example.myevent)", LongName = "type", ShortName = "t")]
        string Type { get; } = "com.example.myevent";

        [Option(Description = "HTTP(S) address to send the event to", LongName = "url", ShortName = "u"),]
        Uri Url { get; } = new Uri("http://echoapi.cloudapp.net/api");

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        async Task OnExecuteAsync()
        {
            var cloudEvent = new CloudEvent(this.Type, new Uri(this.Source))
            {
                DataContentType = new ContentType(MediaTypeNames.Application.Json),
                Data = JsonConvert.SerializeObject("hey there!")
            };

            var content = new CloudEventContent(cloudEvent, ContentMode.Structured, new JsonEventFormatter());

            /* test with websocket - couldn't connect to Azure Application GW
            var buffer = new byte[1024 * 4];
            buffer = Encoding.UTF8.GetBytes(cloudEvent.Data.ToString());
            var webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Ocp-Apim-Subscription-Key", "ca1fcd5b134341eb9179a4dffbe4d492");
            Uri wsUrl = new Uri("ws://51.116.139.102");
            await webSocket.ConnectAsync(wsUrl, CancellationToken.None);
            WebSocketReceiveResult wsresult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!wsresult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                wsresult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(wsresult.CloseStatus.Value, wsresult.CloseStatusDescription, CancellationToken.None);
            */

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "ca1fcd5b134341eb9179a4dffbe4d492");
            // your application remains in charge of adding any further headers or 
            // other information required to authenticate/authorize or otherwise
            // dispatch the call at the server.
            var result = (await httpClient.PostAsync(this.Url, content));

            Console.WriteLine(result.StatusCode);
        }
    }
}