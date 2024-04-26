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
    /// <summary>
    /// Example using LINQ to run multiple animations in sync.
    /// To execute, use the context menu.
    /// </summary>
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
            
            // This is much better practice. Link to the destroy token (showing here manually)
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
            // Ensure the scale of everything is reset when Animate is entered
            children.ForEach(x=>x.transform.localScale= Vector3.zero); // set children scale to zero
            
            await UniTask.Yield(ct); // Wait for next frame from editor to prevent lagging.
            int i = 0; // Delay index for offset
            
            // Animate radially out
            // NOTE this is using deferred execution: meaning that these aren't executed until ITERATED over
            IEnumerable<UniTask> animationsOut = from c in children
                select AnimateChildWithDelay(
                    isOut:true,
                    child: c,
                    targetPos: transform.position + radius * GetNormalFromPercentage((float)i / children.Count),
                    targetScale: 1f,
                    delayInSec: DelayFromIndex(++i), 
                    ct);

            UniTask dummyHandle = Example1(ct);
            UniTask emptyHandle = true ? default : Example3(ct); // Note Example3 is never executed  
            
            // Now let's wait for three different sets of animations (running them all at the same time)
            await UniTask.WhenAll(
                UniTask.WhenAll(animationsOut), // 1. the set of all children animations. NOTE now this is iterated over and executed
                Example2(ct),                   // 2. Another animation just to prove a point that this can be done as many times as needed 
                dummyHandle,                    // 3. We can wait on Handles explicitly
                emptyHandle                     // 4. Can just wait on nothing
            );

            i = 0; 
            // Animate radially back in
            IEnumerable<UniTask> animationsIn = from c in children 
                select AnimateChildWithDelay(false, c, transform.position, 0f, DelayFromIndex(++i), ct);
            
            await UniTask.WhenAll(animationsIn);
        }
        
        private async UniTask Example1(CancellationToken ct)
        {
            Debug.Log("Started Example 1");
            await UniTask.Yield(cancellationToken:ct);
        }

        private UniTask Example2(CancellationToken ct)
        {
            Debug.Log("Started Example 2");
            return UniTask.CompletedTask;
        }
        
        private UniTask Example3(CancellationToken ct)
        {
            Debug.Log("Started Example 3");
            return UniTask.CompletedTask;
        }

        private async UniTask AnimateChildWithDelay(bool isOut, ComplexMovementComponent child, Vector3 targetPos, float targetScale, float delayInSec, CancellationToken ct)
        {
            await UniTask.Delay((int)(1000f*delayInSec),cancellationToken:ct);
            if (!child) return; // Does child still exist?

            void setter(Vector3 pos) => child.transform.position = pos; 
            await UniTask.WhenAll(
                // DANGEROUS (child can be destroyed without cancelling & throw exception)
                // Important to note that ct here is linked to THIS lifetime (the parent component)
                AnimationUtils.InterpAction(child.transform, setter, child.transform.position, targetPos, duration, isOut ? interpolationCurveOut :interpolationCurveIn, ct),
                
                // SAFE (chlid cancels its own animation and links to this token)
                child.AnimateScale(targetScale, duration, ct)
            );
        }
    }
}