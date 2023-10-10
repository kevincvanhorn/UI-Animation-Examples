using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace SDGA
{
    internal class ComplexMovement_LINQ : MonoBehaviour
    {
        private List<ComplexMovementComponent> children = new();
        private CancellationTokenSource animationCts;
        [SerializeField] private float animationDelay = 0.1f; // Delay between each child's animation
        [SerializeField] private float radius = 0.1f; // Animation radius. How far out from center to send children.
        [SerializeField] private float duration = 0.1f;
        [SerializeField] private AnimationUtils.EAnimationCurve interpolationCurveOut;
        [SerializeField] private AnimationUtils.EAnimationCurve interpolationCurveIn;
        
        [ContextMenu("Animate")]
        public void Animate()
        {
            children = GetComponentsInChildren<ComplexMovementComponent>(true).ToList();
            
            // This is much better practice than the other examples. Link to the destroy token.
            Utils.CancelAndDisposeNew(ref animationCts); // Restart any running animations
            animationCts = CancellationTokenSource.CreateLinkedTokenSource(animationCts.Token, destroyCancellationToken); // Link to destroy token
            Animate(animationCts.Token).Forget();
        }

        private float DelayFromIndex(int i) => i * animationDelay;

        private Vector3 GetNormalFromPercentage(float t)
        {
            return Quaternion.Euler(0, 0, 360 * t)*Vector3.up;
        }
        
        private async UniTask Animate(CancellationToken ct)
        {
            children.ForEach(x=>x.transform.localScale= Vector3.zero); // set children scale to zero
            
            await UniTask.Yield(ct); // Wait for editor to prevent lagging.
            int i = 0; // Delay index for offset
            
            // Animate radially out
            IEnumerable<UniTask> animationsOut = from c in children
                select AnimateChildWithDelay(
                    isOut:true,
                    child: c,
                    targetPos: transform.position + radius * GetNormalFromPercentage((float)i / children.Count),
                    targetScale: 1f,
                    delayInSec: DelayFromIndex(++i), 
                    ct);
            
            await UniTask.WhenAll(
                UniTask.WhenAll(animationsOut), Example()
            );

            i = 0;
            // Animate radially back in
            IEnumerable<UniTask> animationsIn = from c in children 
                select AnimateChildWithDelay(false, c, transform.position, 0f, DelayFromIndex(++i), ct);
            await UniTask.WhenAll(animationsIn);
        }

        private UniTask Example()
        {
            return UniTask.CompletedTask;
        }

        private async UniTask AnimateChildWithDelay(bool isOut, ComplexMovementComponent child, Vector3 targetPos, float targetScale, float delayInSec, CancellationToken ct)
        {
            await UniTask.Delay((int)(1000f*delayInSec),cancellationToken:ct);
            if (!child) return; // Does child still exist?

            void setter(Vector3 pos) => child.transform.position = pos; 
            await UniTask.WhenAll(
                // DANGEROUS (child can be destroyed without cancelling & throw exception)
                AnimationUtils.InterpAction(child.transform, setter, child.transform.position, targetPos, duration, isOut ? interpolationCurveOut :interpolationCurveIn, ct),
                
                // SAFE (chlid cancels its own animation and links to this token)
                child.AnimateScale(targetScale, duration, ct)
            );
        }
    }
}