using JetBrains.Annotations;

namespace LeadActress.Runtime.Dancing {
    public sealed class ModelFixerOptions {

        public bool EyesHighlights { get; set; } = true;

        public bool HairHighlights { get; set; } = true;

        [NotNull]
        public static ModelFixerOptions CreateDefault() {
            return new ModelFixerOptions();
        }

    }
}
