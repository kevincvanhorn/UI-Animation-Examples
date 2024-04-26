using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDGA
{
    internal static partial class AnimationUtils
    {

        // Animation Curves
        private const double C4 = (2.0f * Math.PI) / 3.0f;
        public static readonly Func<float, float> FLinear = (x) => x;
        public static readonly Func<float, float> FEaseOutExpo = (x) => x == 1.0f ? 1 : 1 - Mathf.Pow(2.0f, -10.0f * x);
        public static readonly Func<float, float> FEaseOutQuart = (x) => 1.0f - Mathf.Pow(1.0f - x, 4.0f);
        public static readonly Func<float, float> FEaseInOutCubic = (x) => x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) * 0.5f;
        public static readonly Func<float, float> FEaseOutElastic = (x) =>
        {
            return x == 0 ? 0 : x == 1.0f ? 1 : Mathf.Pow(2.0f, -10.0f * x) * Mathf.Sin((float)((x * 10.0f - 0.75f) * C4)) + 1.0f;
        };

        /// <summary>
        /// Interpolate a setter function using an interpolation function from t=[0,1].
        /// </summary>
        /// <param name="transform"> Transform being modified. </param>
        /// <param name="setter"> Setter interpolated from startValue to endValue</param>
        /// <param name="startValue"> Value passed to setter at t = 0 </param>
        /// <param name="endValue"> Value passed to setter at t = 1 </param>
        /// <param name="duration"> How long in seconds the interpolation should take. </param>
        /// <param name="interpolation"> The interpolation curve. </param>
        /// <param name="ct"> Cancellation token for early exit. Throws System.OperationCancelledException. </param>
        public static async UniTask InterpAction(Transform transform, Action<float> setter, float startValue, float endValue, float duration, Func<float, float> interpolation, CancellationToken ct)
        {
            if (ct.IsCancellationRequested || setter == null) return;
            float timeElapsed = 0;
            float durInv = 1.0f / duration;
            while (timeElapsed < duration)
            {
                setter(startValue + interpolation(timeElapsed * durInv) * (endValue - startValue));
                await UniTask.Yield(cancellationToken:ct); // / Note that with a cancellation here, we will throw an exception immediately from this point. This is the equivalent of `if(ct.IsCancellationRequested) return;`
                timeElapsed += Time.deltaTime;
            }

            // Note if we got here there wasn't a cancellation
            Debug.Assert(!ct.IsCancellationRequested, "This shouldn't be possible");
            if (transform) setter(endValue); // Can pass null as transform to prevent setting on completion
        }
    }
}
