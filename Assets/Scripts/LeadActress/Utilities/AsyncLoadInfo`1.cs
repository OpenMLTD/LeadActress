using JetBrains.Annotations;

namespace LeadActress.Utilities {
    public sealed class AsyncLoadInfo<T> {

        public AsyncLoadInfo() {
            Result = default;
            State = AsyncLoadState.Loading;
        }

        [CanBeNull]
        public T Result;

        public AsyncLoadState State;

    }
}
