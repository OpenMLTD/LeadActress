using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace LeadActress.Utilities {
    public static class AsyncLoadInfo {

        public static async UniTask<T> ReturnExistingAsync<T>([NotNull] AsyncLoadInfo<T> info, [NotNull] string errorMessage) {
            Debug.Assert(info != null);

            while (info.IsLoading()) {
                await UniTask.Yield();
            }

            if (info.IsSuccessful()) {
                return info.Result;
            } else {
                throw new ApplicationException(errorMessage);
            }
        }

    }
}
