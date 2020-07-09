using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Imas {
    [Serializable]
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public sealed class Curve {

        [NotNull]
        public string path;

        [NotNull, ItemNotNull]
        public string[] attribs;

        [NotNull]
        public float[] values;

    }
}
