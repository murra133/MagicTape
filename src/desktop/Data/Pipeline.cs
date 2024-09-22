using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.UI;

namespace MagicTape.Data
{
    public class Pipeline
    {


        public Pipeline(MagicTapeDoc doc)
        {
            Doc = doc;
            Enabled = true;
        }

        private MagicTapeDoc Doc { get; }

        private bool enabled { get; set; }

        /// <summary>
        ///     Pipeline enabled, disabling hides it
        /// </summary>
        internal bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                {
                    return;
                }

                enabled = value;

                if (enabled)
                {
                    DisplayPipeline.CalculateBoundingBox += CalculateBoundingBox;
                    DisplayPipeline.PostDrawObjects += PostDrawObjects;
                }
                else
                {
                    DisplayPipeline.CalculateBoundingBox -= CalculateBoundingBox;
                    DisplayPipeline.PostDrawObjects -= PostDrawObjects;
                }
            }
        }

        private void PostDrawObjects(object sender, DrawEventArgs e)
        {
            foreach (var measurement in Doc.GetAllMeasurements())
            {
                PipelineUtils.RenderMeasurement(e, measurement);
            }
        }

        private void CalculateBoundingBox(object sender, CalculateBoundingBoxEventArgs e)
        {
            Box box = new Box(Plane.WorldXY, new Interval(-100_000, 100_000), new Interval(-100_000, 100_000), new Interval(-100_000, 100_000));
            e.IncludeBoundingBox(box.BoundingBox);

            foreach (var measurement in Doc.GetAllMeasurements())
            {
                if (!measurement.TryGetPoints(out var start, out var end)) continue;
                var bounds = new Rhino.Geometry.BoundingBox(start, end);
                e.IncludeBoundingBox(bounds);
            }
        }
    }
}