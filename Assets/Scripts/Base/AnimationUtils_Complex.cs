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
        public enum EAnimationCurve
        {
            Linear,
            EaseOutExpo,
            EaseInOutCubic,
            EaseOutQuart,
            EaseOutElastic
        }

        
        public static Dictionary<EAnimationCurve, Func<float,float>> AnimationFuncs = new()
        {
            {EAnimationCurve.Linear, FLinear},
            {EAnimationCurve.EaseOutQuart, FEaseOutQuart},
            {EAnimationCurve.EaseOutExpo, FEaseOutExpo},
            {EAnimationCurve.EaseInOutCubic, FEaseInOutCubic},
            {EAnimationCurve.EaseOutElastic, FEaseOutElastic},
        };
        private static float Value(this EAnimationCurve lhs, float x) => AnimationFuncs[lhs](x);

        
        public static async UniTask InterpAction(Transform transform, Action<float> setter, float startValue, float endValue, float duration, EAnimationCurve interpolation, CancellationToken ct)
        {
            if (ct.IsCancellationRequested || setter == null) return;
            float timeElapsed = 0;
            float durInv = 1.0f / duration;
            while (timeElapsed < duration)
            {
                setter(startValue + interpolation.Value(timeElapsed * durInv) * (endValue - startValue));
                await UniTask.Yield(cancellationToken:ct); // Note that with a cancellation here, we will throw an exception immediately from this point. This is the equivalent of `if(ct.IsCancellationRequested) return;`
                timeElapsed += Time.deltaTime;
            }

            // Note if we got here there wasn't a cancellation
            Debug.Assert(!ct.IsCancellationRequested, "This shouldn't be possible");
            if (transform) setter(endValue); // Can pass null as transform to prevent setting on completion
        }
        
        public static async UniTask InterpAction(Transform transform, Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, EAnimationCurve interpolation, CancellationToken ct)
        {
            if (ct.IsCancellationRequested || setter == null) return;
            float timeElapsed = 0;
            float durInv = 1.0f / duration;
            while (timeElapsed < duration)
            {
                setter(startValue + interpolation.Value(timeElapsed * durInv) * (endValue - startValue));
                await UniTask.Yield(cancellationToken:ct); // Note that with a cancellation here, we will throw an exception immediately from this point. This is the equivalent of `if(ct.IsCancellationRequested) return;`
                timeElapsed += Time.deltaTime;
            }

            // Note if we got here there wasn't a cancellation
            Debug.Assert(!ct.IsCancellationRequested, "This shouldn't be possible");
            if (transform) setter(endValue); // Can pass null as transform to prevent setting on completion
        }
    }
    
}
