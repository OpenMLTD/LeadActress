using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress {
    public sealed class LabelOverrideAttribute : PropertyAttribute {

        public LabelOverrideAttribute([NotNull] string label) {
            Label = label;
        }

        [NotNull]
        public string Label { get; }

    }
}
