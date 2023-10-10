using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDGA
{
    internal class ComplexMovementComponent : MonoBehaviour
    {
        [SerializeField] private AnimationUtils.EAnimationCurve animationCurve;
        
        private void UniformScale(float x) => transform.localScale = new Vector3(x, x, x);
        public async UniTask AnimateScale(float targetScale, float duration, CancellationToken ct)
        {
            var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken); // Link to this destroy            
            await AnimationUtils.InterpAction(transform, UniformScale, transform.localScale.x, targetScale, duration, animationCurve, linked.Token);
        }
    }
}