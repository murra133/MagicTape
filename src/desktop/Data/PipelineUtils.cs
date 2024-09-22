using Rhino.Display;
using Rhino.Geometry;

namespace MagicTape.Data
{
    public class PipelineUtils
    {

        public static void RenderMeasurement(DrawEventArgs e, IMeasurementRecord measurement)
        {
            bool IsLocked = measurement is LockedMeasurement;
            if (!measurement.TryGetPoints(out var start, out var end)) return;
            if (start == end) return;

            var color = IsLocked ? System.Drawing.Color.Blue : System.Drawing.Color.Red;
            var thickness = IsLocked ? 2 : 5;
            e.Display.DrawLine(start, end, color, thickness);
        }

    }
}