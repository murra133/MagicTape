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
    public class StopMagicTapeCommand : Command
    {
        public StopMagicTapeCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static StopMagicTapeCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "MagicTapeStop";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Shutting Down Magic Tape...");
            MagicTape.Instance.StopListenerThread();

            double timePassed = 0;

            // Wait for the socket to connect
            while (MagicTape.Instance.SocketState != WebSocketState.Closed && timePassed <= 5000)
            {
                Task.Delay(1000).Wait();
            }

            if (MagicTape.Instance.SocketState != WebSocketState.Closed)
            {
                RhinoApp.WriteLine("Failed to close socket connection");
                return Result.Failure;
            }

            RhinoApp.WriteLine($"Socket connection closed");
            return Result.Success;
        }
    }
}
