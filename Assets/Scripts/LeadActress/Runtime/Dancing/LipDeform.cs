using System;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    internal struct LipDeform {

        public float a, i, u, e, o, n;

        public static LipDeform A() {
            return new LipDeform {
                a = 100
            };
        }

        public static LipDeform I() {
            return new LipDeform {
                i = 100
            };
        }

        public static LipDeform U() {
            return new LipDeform {
                u = 100
            };
        }

        public static LipDeform E() {
            return new LipDeform {
                e = 100
            };
        }

        public static LipDeform O() {
            return new LipDeform {
                o = 100
            };
        }

        public static LipDeform N() {
            return new LipDeform {
                n = 100
            };
        }

        public static LipDeform FromLipCode(LipCode code) {
            switch (code) {
                case LipCode.A:
                    return A();
                case LipCode.I:
                    return I();
                case LipCode.U:
                    return U();
                case LipCode.E:
                    return E();
                case LipCode.O:
                    return O();
                case LipCode.N:
                    return N();
                case LipCode.Closed:
                    return Silence();
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }

        public static LipDeform Silence() {
            return new LipDeform();
        }

        public static LipDeform Lerp(in LipDeform v1, in LipDeform v2, float t) {
            return new LipDeform {
                a = Mathf.Lerp(v1.a, v2.a, t),
                i = Mathf.Lerp(v1.i, v2.i, t),
                u = Mathf.Lerp(v1.u, v2.u, t),
                e = Mathf.Lerp(v1.e, v2.e, t),
                o = Mathf.Lerp(v1.o, v2.o, t),
                n = Mathf.Lerp(v1.n, v2.n, t),
            };
        }

    }
}
