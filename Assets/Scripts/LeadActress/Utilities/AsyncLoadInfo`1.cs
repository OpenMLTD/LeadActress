using System;
using JetBrains.Annotations;

namespace LeadActress.Utilities {
    public sealed class AsyncLoadInfo<T> {

        public AsyncLoadInfo() {
            Result = default;
            State = AsyncLoadState.Loading;
        }

        [CanBeNull]
        public T Result;

        [CanBeNull]
        public Exception Exception;

        public AsyncLoadState State;

    }
}
