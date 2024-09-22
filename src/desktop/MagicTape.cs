using Rhino;
using Rhino.PlugIns;
using MagicTape.Data;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using MagicTape;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MagicTape
{

    public class MagicTape : Rhino.PlugIns.PlugIn
    {
        public static string ServerSocketUrl { get; set; }
        private static ClientWebSocket _webSocket;

        public WebSocketState SocketState
        {
            get
            {
                if (_webSocket is null)
                {
                    return WebSocketState.None;
                }
                return _webSocket.State;
            }

        }

        public MagicTape()
        {
            Instance = this;
            if (Pipe is not null)
                Pipe.Enabled = false;

            ActiveDoc = new MagicTapeDoc();
            Pipe = new Pipeline(ActiveDoc);
        }

        private void RhinoDoc_BeginOpenDocument(object sender, DocumentOpenEventArgs e)
        {
            
        }

        public void LoadConfig()
        {
            ClientConfiguration config = JsonSerializer.Deserialize<ClientConfiguration>(System.IO.File.ReadAllText("config.json"));
            ServerSocketUrl = config.ServerSocketUrl;
        }

        public void StopListenerThread()
        {
            RhinoApp.InvokeOnUiThread(new Action(async () =>
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed connection", CancellationToken.None);
                this.ActiveDoc.Clear();

            }));
        }

        public void StartListenerThread()
        {
            RhinoApp.InvokeOnUiThread(new Action(async () =>
            {
                var receiveBuffer = new byte[2048];

                while (_webSocket.State == WebSocketState.Connecting)
                {
                    Task.Delay(1000).Wait();
                }

                // Receive messages loop
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer),
                        CancellationToken.None
                    );

                    // Check if the server closed the connection
                    if (result.MessageType == WebSocketMessageType.Close && 
                        _webSocket.State != WebSocketState.Closed)
                    {
                        try
                        {
                            await _webSocket.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Server closed connection",
                                CancellationToken.None
                            );
                        }
                        catch (Exception e)
                        {
                            RhinoApp.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        // Handle partial messages
                        var messageBytes = new List<byte>();
                        messageBytes.AddRange(receiveBuffer.Take(result.Count));

                        while (!result.EndOfMessage)
                        {
                            result = await _webSocket.ReceiveAsync(
                                new ArraySegment<byte>(receiveBuffer),
                                CancellationToken.None
                            );
                            messageBytes.AddRange(receiveBuffer.Take(result.Count));
                        }

                        // Decode the received message
                        string receivedMessage = Encoding.UTF8.GetString(messageBytes.ToArray());
                        RhinoApp.WriteLine(receivedMessage);

                        if (ActiveDoc is null) continue;

                        if (MagicDecoder.TryDecodeMessage(receivedMessage, out var record))
                            ActiveDoc.AddMeasurement(record);

                        RhinoDoc.ActiveDoc?.Views.Redraw();
                    }
                }
            }));
        }

        public async void ConnectToServer()
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri(ServerSocketUrl), System.Threading.CancellationToken.None);

        }

        public async void SendSocketMessage(string message)
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }

        public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            this.LoadConfig();
            return base.OnLoad(ref errorMessage);
        }

        ///<summary>Gets the only instance of the MagicTape plug-in.</summary>
        public static MagicTape Instance { get; private set; }

        public MagicTapeDoc ActiveDoc { get; private set; }
        public Pipeline Pipe { get; private set; }
    }
}