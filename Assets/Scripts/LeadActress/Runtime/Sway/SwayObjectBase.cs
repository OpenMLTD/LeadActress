using JetBrains.Annotations;

namespace LeadActress.Runtime.Sway {
    public abstract class SwayObjectBase {

        internal SwayObjectBase() {
            Path = string.Empty;
        }

        [NotNull]
        public string Path { get; internal set; }

    }
}
