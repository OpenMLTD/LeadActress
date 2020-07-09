using JetBrains.Annotations;
using LeadActress.Runtime.Sway;
using UnityEngine;

namespace LeadActress.Runtime.Dancing {
    public sealed class ModelLoadResult {

        internal ModelLoadResult([NotNull] GameObject head, [NotNull] GameObject body, [NotNull] SwayController headSway, [NotNull] SwayController bodySway) {
            Head = head;
            Body = body;
            HeadSway = headSway;
            BodySway = bodySway;
        }

        [NotNull]
        public GameObject Head { get; }

        [NotNull]
        public GameObject Body { get; }

        [NotNull]
        public SwayController HeadSway { get; }

        [NotNull]
        public SwayController BodySway { get; }

    }
}
