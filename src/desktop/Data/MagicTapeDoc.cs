using System;
using System.Collections.Generic;
using System.Linq;
using GH_IO.Serialization;
using Rhino.Render;

namespace MagicTape.Data
{
    public class MagicTapeDoc
    {

        private Dictionary<string, List<LockedMeasurement>> _measurements { get; }

        private Dictionary<string, ActiveMeasurement> _temporary { get; }

        public MagicTapeDoc()
        {
            _measurements = new();
            _temporary = new();
        }

        public void Clear()
        {
            _measurements.Clear();
            _temporary.Clear();
        }

        public void AddMeasurement(ActiveMeasurement record)
        {
            ActiveMeasurement existingRecord;
            if (!_temporary.TryGetValue(record.Id, out existingRecord))
            {
                existingRecord = new ActiveMeasurement(record.Id, new List<IRecordInstruction>());
            }

            var combinedInstructions = existingRecord.Instructions.Union(record.Instructions).ToList();
            var updatedRecord = new ActiveMeasurement(record.Id, combinedInstructions);

            if (!record.Instructions.Any(i => i is LockInstruction))
            {
                _temporary[record.Id] = updatedRecord;
                return;
            }

            _temporary.Remove(record.Id);

            List<LockedMeasurement> lockedMeasurements = new List<LockedMeasurement>();
            if (_measurements.TryGetValue(record.Id, out var measure))
            {
                lockedMeasurements = measure;
            }

            var withOnedistance = ActiveMeasurement.WithOneDistanceInstruction(updatedRecord.Instructions) ?? new List<IRecordInstruction>();

            var pos = MagicTapePosition.FromInstructions(withOnedistance);

            lockedMeasurements.Add(new LockedMeasurement(record.Id, pos.Start, pos.End));

            _measurements[record.Id] = lockedMeasurements;
        }

        internal IReadOnlyList<LockedMeasurement> GetLockedMeasurements() => _measurements.Values.SelectMany(s => s).ToList();

        internal IReadOnlyList<ActiveMeasurement> GetTemporaryMeasurements() => _temporary.Values.ToList();

        internal IReadOnlyList<IMeasurementRecord> GetAllMeasurements() => _measurements.Values.SelectMany(s => s).Cast<IMeasurementRecord>().Union(_temporary.Values.Cast<IMeasurementRecord>()).ToList();

    }
}