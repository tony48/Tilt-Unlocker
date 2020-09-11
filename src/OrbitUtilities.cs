using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiltUnlocker
{
    public static class OrbitUtilities
    {
        private static Quaternion Rotation(this Orbit orbit)
        {
            return Quaternion.Euler((float)orbit.inclination, (float)orbit.LAN, 0.0F);
        }

        public static void Rotate(this Orbit orbit, Quaternion rotation)
        {
            Quaternion baseRot = orbit.Rotation();

            Quaternion rotated = baseRot * rotation;

            Vector3 angles = rotated.eulerAngles;

            orbit.inclination = angles.z;
            orbit.LAN = -angles.x;

            orbit.Init();
            orbit.UpdateFromUT(Planetarium.GetUniversalTime());
        }
    }
}
