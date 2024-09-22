using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace MagicTape.Data
{
    public class MagicDecoder
    {

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        private static readonly JsonDocumentOptions jDocOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
        };

        private const double UnitScale = 1000.0;

        internal static bool TryDecodeMessage(string receivedMessage, out ActiveMeasurement record)
        {
            record = default;
            try
            {
                var instructions = new List<IRecordInstruction>();
                var jDoc = JsonDocument.Parse(receivedMessage.ToLowerInvariant(), jDocOptions);

                if (!jDoc.RootElement.TryGetProperty("id", out var id)) return false;
                if (!jDoc.RootElement.TryGetProperty("timestamp", out var timestamp)) return false;

                var longString = timestamp.ToString();

                if (!long.TryParse(longString, out var timestampLong)) return false;

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var stamp = origin.AddMilliseconds(timestampLong);

                if (jDoc.RootElement.TryGetProperty("x", out var x) &&
                    jDoc.RootElement.TryGetProperty("y", out var y) &&
                    jDoc.RootElement.TryGetProperty("z", out var z))
                    instructions.Add(new MoveToInstruction(new Point3d(x.GetDouble(), y.GetDouble(), z.GetDouble()), stamp));

                if (jDoc.RootElement.TryGetProperty("distance", out var length))
                    instructions.Add(new DistanceInstruction(length.GetDouble() / UnitScale, stamp));

                if (jDoc.RootElement.TryGetProperty("locked", out var locked) && locked.GetBoolean())
                    instructions.Add(new LockInstruction(stamp));

                if (jDoc.RootElement.TryGetProperty("alpha", out var rotationX) &&
                    jDoc.RootElement.TryGetProperty("beta", out var rotationY) &&
                    jDoc.RootElement.TryGetProperty("gamma", out var rotationZ))
                    instructions.Add(new RotateInstruction(new Vector3d(rotationX.GetDouble(), rotationY.GetDouble(), rotationZ.GetDouble()), stamp));

                if (jDoc.RootElement.TryGetProperty("ax", out var accelerationX) &&
                    jDoc.RootElement.TryGetProperty("ay", out var accelerationY) &&
                    jDoc.RootElement.TryGetProperty("az", out var accelerationZ))
                    instructions.Add(new AccelerateInstruction(new Vector3d(accelerationX.GetDouble(), accelerationY.GetDouble(), accelerationZ.GetDouble()), stamp));

                if (instructions is null || instructions.Count <= 0) return false;

                record = new ActiveMeasurement(id.GetString(), instructions);

                return true;
            }
            catch (Exception ex)
            {
                record = default;
                return false;
            }
        }
    }
}