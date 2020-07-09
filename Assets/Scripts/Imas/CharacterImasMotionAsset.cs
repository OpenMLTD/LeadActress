using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Imas {
    public sealed class CharacterImasMotionAsset : ScriptableObject {

        [NotNull]
        public string kind = string.Empty;

        // public object[] attribs;

        public float time_length;

        [NotNull]
        public string date = string.Empty;

        [NotNull, ItemNotNull]
        public Curve[] curves = Array.Empty<Curve>();

    }
}
