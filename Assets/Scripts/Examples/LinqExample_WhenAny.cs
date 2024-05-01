using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SDGA
{
    internal class LinqExample_WhenAny : MonoBehaviour
    {
        // Text Animation
        [SerializeField] private float textAnimDuration = 0.1f;
        [SerializeField] private float textAnimSize = 0.1f;
        [SerializeField] private AnimationUtils.EAnimationCurve textInterpolation;

        // Square Movement
        [SerializeField] private float endX;
        
        private Text text;
        private CancellationTokenSource animationCts;
        private List<SimpleMovement_Generic> children = new();
        
        [ContextMenu("Animate Loose")]
        public void AnimateLoose()
        {
            if (!TryInit()) return;
            
            Utils.CancelAndDisposeNew(ref animationCts, this); // Restart any running animations
            AnimateLoose(animationCts.Token).Forget();
        }
        
        [ContextMenu("Animate Strict")]
        public void Animate()
        {
            if (!TryInit()) return;
            
            Utils.CancelAndDisposeNew(ref animationCts, this); // Restart any running animations
            AnimateStrict(animationCts.Token).Forget();
        }

        private bool TryInit()
        {
            text = GetComponentInChildren<Text>();
            children = GetComponentsInChildren<SimpleMovement_Generic>(true).ToList();
            
            if (!text || children.Count == 0)
            {
                Debug.LogError("Failed to init.");
                return false;
            }
            
            // Ensure the scale of everything is reset when Animate is entered
            SetTextScale(0f);
            text.text = "0";
            foreach (var x in children)
            {
                x.SetRectX(0); // Reset left
            }
            return true;
        }
        
        private void SetTextScale(float x) => text.transform.localScale = Vector3.one * x;
        
        /// <summary>
        /// Don't stop all tasks when the condition in the ANY is met.
        /// </summary>
        /// <param name="ct"> external cancellation (ex. this object is destroyed).</param>
        private async UniTask AnimateLoose(CancellationToken ct)
        {
            // Collect animations - nothing has actually started running
            IEnumerable<UniTask> animationsRight = from c in children select c.Animate_Func(endX, ct);

            // Trigger animations
            var rightHandles = animationsRight.ToList();

            int whichFinishedFirst = await UniTask.WhenAny(rightHandles); // Start running the animations of squares moving to the right (no longer deferred)
            var numberHandle = AnimateNumber(whichFinishedFirst, ct); // Start running number size animation

            await UniTask.WhenAll(rightHandles); // ERROR: cannot do this
            
            // Collect animations - nothing has actually started running
            var animationsLeft = from c in children select c.Animate_Func(0, ct);
            await UniTask.WhenAll(
                UniTask.WhenAll(animationsLeft), // Starts running all animations left
                numberHandle // Ensure number animation finishes.
            );
        }

        
        /// <summary>
        /// Stop all tasks when the condition in the ANY is met.
        /// </summary>
        /// <param name="ct"> external cancellation (ex. this object is destroyed).</param>
        private async UniTask AnimateStrict(CancellationToken ct)
        {
            // ct is our EXTERNAL token - meaning that it should be treated like anything outside of this function can cause ct to cancel.
            var localCTS = new CancellationTokenSource(); // Now we're making a new token SOURCE for cancelling the squares moving to the right
            using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(localCTS.Token, ct))
            {
                // We need this new token to be linked in case of external cancellation
                IEnumerable<UniTask> animationsRight = from c in children select c.Animate_Func(endX, linkedTokenSource.Token);
                int whichFinishedFirst = await UniTask.WhenAny(animationsRight); // Wait for ANY of the squares to reach the right
                
                // Now cancel all square animations & show the number animation:
                Utils.CancelAndDispose(ref localCTS);
                AnimateNumber(whichFinishedFirst, animationCts.Token).Forget(); // Prevent double forgets, competing for the animation. This is not best practice but I don't want to make this example more confusing.
            }
            
            // Collect animations - nothing has actually started running
            var animationsLeft = from c in children select c.Animate_Func(0, ct);
            await UniTask.WhenAll(animationsLeft); // Ensure everything gets to the left
        }

        private async UniTask AnimateNumber(int amnt, CancellationToken ct)
        {
            text.text = (amnt).ToString();
            await AnimationUtils.InterpAction(transform, SetTextScale, 0, textAnimSize, textAnimDuration, textInterpolation, ct);
        }
    }
}