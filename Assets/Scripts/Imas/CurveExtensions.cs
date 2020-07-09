using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Imas {
    public static class CurveExtensions {

        public static PropertyType GetPropertyType([NotNull] this Curve curve) {
            var s = curve.attribs[0].Substring(14); // - "property_type "

            switch (s) {
                case "General":
                    return PropertyType.General;
                case "AngleX":
                    return PropertyType.AngleX;
                case "AngleY":
                    return PropertyType.AngleY;
                case "AngleZ":
                    return PropertyType.AngleZ;
                case "PositionX":
                    return PropertyType.PositionX;
                case "PositionY":
                    return PropertyType.PositionY;
                case "PositionZ":
                    return PropertyType.PositionZ;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
        }

        public static KeyType GetKeyType([NotNull] this Curve curve) {
            var attribIndex = -1;

            for (var i = 0; i < curve.attribs.Length; i += 1) {
                if (curve.attribs[i].StartsWith("key_type")) {
                    attribIndex = i;
                    break;
                }
            }

            if (attribIndex < 0) {
                throw new ArgumentException("Missing attribute: key_type");
            }

            var s = curve.attribs[attribIndex].Substring(9); // - "key_type "

            switch (s) {
                case "Const":
                    return KeyType.Const;
                case "Discreate": // This typo exists in MLTD, not my fault.
                    return KeyType.Discrete;
                case "FullFrame":
                    return KeyType.FullFrame;
                case "FCurve":
                    return KeyType.FCurve;
                default:
                    throw new ArgumentOutOfRangeException(nameof(s), s, null);
            }
        }

        [NotNull]
        public static string GetPropertyName([NotNull] this Curve curve) {
            var index = -1;

            for (var i = 0; i < curve.attribs.Length; ++i) {
                if (curve.attribs[i].StartsWith("property_name ")) {
                    index = i;
                    break;
                }
            }

            if (index < 0) {
                throw new KeyNotFoundException();
            }

            return curve.attribs[index].Substring(14); // - "property_name "
        }

    }
}
