using System.Runtime.CompilerServices;
using Imas.Live;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    public sealed class CommonResourceProperties : ScriptableObject {

        public AppealType appealType = AppealType.None;

        [NotNull]
        public string baseResourceName = "shtstr";

        [NotNull]
        public string audioResourceName {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => baseResourceName;
        }

        [NotNull]
        public string danceResourceName = "shtstr";

        [NotNull]
        public string appealResourceName = "shtstr";

        [NotNull]
        public string cameraResourceName = "shtstr";

        [NotNull]
        public string scenarioResourceName = "shtstr";

    }
}
