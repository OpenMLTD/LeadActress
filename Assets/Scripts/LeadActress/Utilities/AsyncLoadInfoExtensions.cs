using System;
using JetBrains.Annotations;

namespace LeadActress.Utilities {
    public static class AsyncLoadInfoExtensions {

        public static void Success<T>([NotNull] this AsyncLoadInfo<T> info, in T result) {
            info.Result = result;
            info.State = AsyncLoadState.Successful;
        }

        public static void Fail<T>([NotNull] this AsyncLoadInfo<T> info, [NotNull] Exception exception) {
            info.State = AsyncLoadState.Failed;
            info.Exception = exception;
        }

        public static void Fail<T>([NotNull] this AsyncLoadInfo<T> info) {
            info.State = AsyncLoadState.Failed;
        }

        public static bool IsLoading<T>([NotNull] this AsyncLoadInfo<T> info) {
            return info.State == AsyncLoadState.Loading;
        }

        public static bool IsSuccessful<T>([NotNull] this AsyncLoadInfo<T> info) {
            return info.State == AsyncLoadState.Successful;
        }

        public static bool IsFailed<T>([NotNull] this AsyncLoadInfo<T> info) {
            return info.State == AsyncLoadState.Failed;
        }

    }
}
