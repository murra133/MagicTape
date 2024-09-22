using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Text.Json;

namespace MagicTape
{
    public class StartMagicTapeCommand : Command
    {
        public StartMagicTapeCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static StartMagicTapeCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "MagicTape";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Starting Magic Tape...");
            MagicTape.Instance.ConnectToServer();
            MagicTape.Instance.StartListenerThread();

            // Wait for the socket to connect
            while (MagicTape.Instance.SocketState == WebSocketState.Connecting)
            {
                Task.Delay(1000).Wait();
            }

            RhinoApp.WriteLine($"Connected to {MagicTape.ServerSocketUrl}");

            string message = JsonSerializer.Serialize(new { action = "test" });
            MagicTape.Instance.SendSocketMessage(message);
            return Result.Success;
        }
    }
}
