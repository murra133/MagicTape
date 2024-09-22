using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace MagicTape.Data
{

    public interface IMeasurementRecord
    {
        public string Id { get; }
        public bool TryGetPoints(out Point3d start, out Point3d end);
    }

    public record struct ActiveMeasurement(string Id, List<IRecordInstruction> Instructions) : IMeasurementRecord
    {
        public bool TryGetPoints(out Point3d start, out Point3d end)
        {
            var correctedInstructions = WithOneDistanceInstruction(Instructions);
            var pos = MagicTapePosition.FromInstructions(correctedInstructions);
            start = pos.Start;
            end = pos.End;

            return true;
        }

        internal static List<IRecordInstruction> WithOneDistanceInstruction(IEnumerable<IRecordInstruction> inInstructions)
        {
            return inInstructions.ToList();

            var outInstructions = inInstructions.Where(i => i is not DistanceInstruction).ToList();
            var distInstruction = inInstructions.FirstOrDefault(i => i is DistanceInstruction);

            if (distInstruction is not null)
                outInstructions.Add(distInstruction);

            // / return outInstructions.Where(i => i is not null).OrderBy(i => i.TimeStamp).ToList();
            return inInstructions.ToList();
        }

    }

    public record struct LockedMeasurement(string Id, Point3d Start, Point3d End) : IMeasurementRecord
    {
        public readonly bool TryGetPoints(out Point3d start, out Point3d end)
        {
            start = Start;
            end = End;
            return true;
        }
    }

    public interface IRecordInstruction
    {
        DateTime TimeStamp { get; }
    }

    public record struct LockInstruction(DateTime TimeStamp) : IRecordInstruction { }

    public readonly record struct MoveToInstruction(Point3d Point, DateTime TimeStamp) : IRecordInstruction { }

    public readonly record struct DistanceInstruction(double Distance, DateTime TimeStamp) : IRecordInstruction { }

    public readonly record struct RotateInstruction(Vector3d Rotation, DateTime TimeStamp) : IRecordInstruction { };

    public record struct AccelerateInstruction(Vector3d Acceleration, DateTime TimeStamp) : IRecordInstruction { }

}