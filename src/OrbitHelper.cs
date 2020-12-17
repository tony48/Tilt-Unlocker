using UnityEngine;

namespace TiltUnlocker
{
    public static class OrbitHelper
    {
        public static Orbit TiltOrbit(Orbit original, Quaternion rotation)
        {
            Orbit o = original;
            Quaternion r = rotation;
            
            QuaternionD orbitQuat = QuaternionD.Euler(o.inclination, 0, 0);
            QuaternionD rotD = new QuaternionD(r.x, r.y, r.z, r.w);

            QuaternionD mult = rotD * orbitQuat;
            Vector3d multEul = mult.eulerAngles;

            Orbit orbit = new Orbit(
                multEul.x, 
                o.eccentricity, 
                o.semiMajorAxis, 
                o.LAN, 
                o.argumentOfPeriapsis, 
                o.meanAnomalyAtEpoch,
                o.epoch,
                o.referenceBody);

            return orbit;
        }
    }
}