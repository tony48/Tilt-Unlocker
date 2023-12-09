using System;
using Kopernicus;
using Kopernicus.ConfigParser.Attributes;
using Kopernicus.ConfigParser.BuiltinTypeParsers;
using Kopernicus.ConfigParser.Interfaces;
using Kopernicus.Configuration.Parsing;
using Kopernicus.UI;


namespace TiltUnlocker
{
    [ParserTargetExternal("Body", "Inclination", "Kopernicus")]
    public class TiltBodyLoader : BaseLoader, ITypeParser<TiltedBody>
    {
        public TiltedBody Value { get; set; }

        [ParserTarget("obliquity")]
        public NumericParser<Double> obliquity
        {
            get
            {
                return Value.Obliquity;
            }

            set
            {
                Value.Obliquity = value;
            }
        }

        [ParserTarget("rightAscension")]
        public NumericParser<Double> rightAscension
        {
            get
            {
                return Value.RightAscension;
            }

            set
            {
                Value.RightAscension = value;
            }
        }

        [ParserTarget("rotateOrbits")]
        public NumericParser<Boolean> rotateOrbits
        {
            get
            {
                return Value.RotateOrbits;
            }

            set
            {
                Value.RotateOrbits = value;
            }
        }

        public TiltBodyLoader()
        {
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            Value = generatedBody.celestialBody.gameObject.AddComponent<TiltedBody>();
        }


        [KittopiaConstructor(KittopiaConstructor.ParameterType.CelestialBody)]
        public TiltBodyLoader(CelestialBody body)
        {
            if (body?.scaledBody == null || Injector.IsInPrefab)
            {
                throw new InvalidOperationException("The body must be already spawned by the PSystemManager.");
            }

            Value = body.GetComponent<TiltedBody>();
            if (Value == null)
            {
                Value = body.gameObject.AddComponent<TiltedBody>();
            }
        }
    }
}
