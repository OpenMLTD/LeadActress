using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress {
    internal static class NullObjectExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull([CanBeNull] this Object obj) {
            return !IsNotNull(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNull([CanBeNull] this Object obj) {
            return !ReferenceEquals(obj, null) && obj;
        }

    }
}
