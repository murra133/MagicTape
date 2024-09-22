using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MagicTape.Data
{
    public class MagicTapePosition
    {
        /// <summary>
        /// X, Y, Z location
        /// </summary>
        public Point3d Location { get; set; }
        /// <summary>
        /// Current velocity in X, Y, Z directions
        /// </summary>
        public Vector3d Velocity { get; set; }
        /// <summary>
        /// X rotation in degrees
        /// </summary>
        public double Pitch { get; set; } // X
        /// <summary>
        /// Y totation in degrees
        /// </summary>
        public double Roll { get; set; }  // Y
        /// <summary>
        /// Z rotation in degrees
        /// </summary>
        public double Yaw { get; set; } // Z

        public DateTime LastLocationUpdate { get; set; }
        public DateTime LastRotationUpdate { get; set; }

        public Rhino.Geometry.Quaternion Orientation
        {
            get
            {
                var orientation = Rhino.Geometry.Quaternion.CreateFromRotationZYX(this.Yaw, -this.Pitch, this.Roll);
                return orientation;
            }
        }

        /// <summary>
        /// Distance in millimeters
        /// </summary>
        private double _distance;
        public double Distance
        {
            get
            {
                double mm = _distance;
                double dist = mm;
                Rhino.UnitSystem modelUnits = Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem;
                if (modelUnits != Rhino.UnitSystem.Millimeters)
                {
                    dist = Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Millimeters, modelUnits) * mm;
                }
                return dist;
            }
            set
            {
                double dist = value;
                Rhino.UnitSystem modelUnits = Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem;
                if (modelUnits != Rhino.UnitSystem.Millimeters)
                {
                    dist = Rhino.RhinoMath.UnitScale(modelUnits, Rhino.UnitSystem.Millimeters) * value;
                }
                _distance = dist;
            }
        }

        public Point3d Start => this.Location;
        public Point3d End
        {
            get
            {
                Point3d end = new Point3d(this.Start) + Vector3d.XAxis * this.Distance;
                Rhino.Geometry.Plane plane = Rhino.Geometry.Plane.Unset;
                this.Orientation.GetRotation(out plane);

                Transform t = Transform.Rotation(Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, plane.XAxis, plane.YAxis, plane.ZAxis);
                end.Transform(t);

                return end;
            }
        }


        public MagicTapePosition(Point3d location, double pitch, double roll, double yaw)
        {
            this.Location = location;
            this.Pitch = pitch;
            this.Roll = roll;
            this.Yaw = yaw;
            this.LastLocationUpdate = DateTime.UtcNow;
            this.LastRotationUpdate = DateTime.UtcNow;
        }

        public void UpdateDistance(DistanceInstruction distance)
        {
            this.Distance = distance.Distance;
        }

        public void SetPosition(MoveToInstruction moveTo)
        {
            this.Location = moveTo.Point;
        }

        public void UpdateLocation(AccelerateInstruction acceleration)
        {
            DateTime accelerationStart = this.LastLocationUpdate;
            DateTime accelerationEnd = acceleration.TimeStamp;
            double timeDelta = Math.Abs((accelerationEnd - accelerationStart).TotalMilliseconds);

            ComputeLocationAndVelocity(
                this.Location,
                this.Velocity,
                acceleration.Acceleration,
                timeDelta,
                out Point3d newLocation,
                out Vector3d newVelocity);

            this.Location = newLocation;
            this.Velocity = newVelocity;
            this.LastLocationUpdate = accelerationEnd;
        }

        public void UpdateOrientation(RotateInstruction rotation)
        {
            this.Pitch = rotation.Rotation.Y;
            this.Roll = rotation.Rotation.Z;
            this.Yaw = rotation.Rotation.X;
        }

        public static void ComputeLocationAndVelocity(Point3d initialLocation,          // Initial positions (meters)
                                                       Vector3d initialVelocity,       // Initial velocities (m/s)
                                                       Vector3d gForce,       // G-forces in X, Y, Z directions
                                                       double timedelta,                      // Timedelta in milliseconds
                                                       out Point3d newLocation,  // New positions (meters)
                                                       out Vector3d newVelocity // New velocities (m/s)
                                                       )
        {
            // Time in seconds
            double T = timedelta / 1000.0;

            // Standard acceleration due to gravity in m/s²
            const double g = 9.80665;

            // Convert g-forces to accelerations in m/s²
            double aX = gForce.X * g;
            double aY = gForce.Y * g;
            double aZ = gForce.Z * g;

            double threshold = 0.2;

            if (aX < threshold) aX = 0;
            if (aY < threshold) aY = 0;
            if (aZ < threshold) aZ = 0;


            // Calculate new velocities using: v = v0 + a * t
            double Vx_new = initialVelocity.X + aX * T;
            double Vy_new = initialVelocity.Y + aY * T;
            double Vz_new = initialVelocity.Z + aZ * T;
            newVelocity = new Vector3d(Vx_new, Vy_new, Vz_new);

            // Calculate new positions using: s = s0 + v0 * t + 0.5 * a * t²
            double X_new = initialLocation.X + initialVelocity.X * T + 0.5 * aX * T * T / 100;
            double Y_new = initialLocation.Y + initialVelocity.Y * T + 0.5 * aY * T * T / 100;
            double Z_new = initialLocation.Z + initialVelocity.Z * T + 0.5 * aZ * T * T / 100;



            newLocation = new Point3d(X_new, Y_new, Z_new);
        }

        
        internal static MagicTapePosition FromInstructions(List<IRecordInstruction> instructions)
        {
            // TODO: Provide correct starting values for location and orientation
            MagicTapePosition position = new MagicTapePosition(Point3d.Origin, 0, 0, 0);
            position.Distance = 100;
            foreach (IRecordInstruction instruction in instructions)
            {
                if (instruction is AccelerateInstruction accelerateInstruction)
                {
                    //position.UpdateLocation(accelerateInstruction);
                }
                else if (instruction is RotateInstruction rotateInstruction)
                {
                    position.UpdateOrientation(rotateInstruction);
                }
                else if (instruction is MoveToInstruction moveToInstruction)
                {
                    position.SetPosition(moveToInstruction);
                }
                else if (instruction is DistanceInstruction distanceInstruction)
                {
                    position.UpdateDistance(distanceInstruction);
                }
            }

            return position;
        }
    }
}
