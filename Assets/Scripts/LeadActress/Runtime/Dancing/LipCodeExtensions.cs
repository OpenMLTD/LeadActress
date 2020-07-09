using System;

namespace LeadActress.Runtime.Dancing {
    internal static class LipCodeExtensions {

        public static bool IsVoice(this LipCode code) {
            switch (code) {
                case LipCode.A:
                case LipCode.I:
                case LipCode.U:
                case LipCode.E:
                case LipCode.O:
                case LipCode.N:
                case LipCode.Closed:
                    return true;
                case LipCode.Control1:
                case LipCode.Control2:
                case LipCode.Control3:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }

        public static bool IsControl(this LipCode code) {
            return !IsVoice(code);
        }

    }
}
