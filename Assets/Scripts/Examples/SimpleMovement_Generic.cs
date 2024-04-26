using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDGA
{
    /// <summary>
    /// Like <see cref="SimpleMovement_UniTask"/> This is a safe way to move a rect transform but using the UniTask library.
    /// To see this work, enable an object with this component.
    ///
    /// Diferences:
    /// 1. We're now using destroyCancellationToken to automatically cancel the animation CTS
    /// 2. Now were using <see cref="AnimationUtils.InterpAction"/> to animate in a reusable way
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    internal class SimpleMovement_Generic : MonoBehaviour
    {
        [SerializeField] private float startX;
        [SerializeField] private float endX;
        [SerializeField] private float duration = 1f;
        [SerializeField] private AnimationUtils.EAnimationCurve interpolation;
        private CancellationTokenSource animationCts;
        
        private RectTransform _rect;
        private float FEaseInOutCubic(float x)  => x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) * 0.5f;

        private void Awake()
        {
            // Note Awake is called before enable
            _rect = GetComponent<RectTransform>();
        }
        
        private void OnEnable()
        {
            // Stop all animations & create a new cancellation token source
            // NEW: Link to destroyCancellationToken
            Utils.CancelAndDisposeNew(ref animationCts, this); 
            DoAnimationLoop(animationCts.Token).Forget();
        }
        
        private void OnDisable()
        {
            Utils.CancelAndDispose(ref animationCts); // Stop all animations
        }

        private async UniTask DoAnimationLoop(CancellationToken ct)
        {
            try
            {
                while (isActiveAndEnabled & !ct.IsCancellationRequested) // These are redundant checks b/c of the try catch but good practice
                {
                    await Animate_Serialized(endX, destroyCancellationToken); // Animate forward
                    await Animate_Serialized(startX, destroyCancellationToken); // Animate back
                }
            }
            catch (System.OperationCanceledException)
            {
                // This is expected when ct is cancelled.
                Debug.Log($"Cancelled animation loop for {GetType().Name}");
            }
            finally
            {
                if(_rect) SetRectX(startX); // On cancellation, reset to start. Unless this is being destroyed
                else Debug.Log($"Cancellation was because of destroy for {GetType().Name}");
            }
        }

        /// <summary>
        /// Set the x component of this rect anchored position.
        /// </summary>
        private void SetRectX(float x) => _rect.anchoredPosition = new Vector2(x, _rect.anchoredPosition.y);

        /// <summary>
        /// This is what we were doing before in <see cref="SimpleMovement_UniTask"/> using FEaseInOutCubic
        /// Same thing but reusable! (By that I mean across multiple classes and methods)
        /// </summary>
        public async UniTask Animate_Func(float targetX, CancellationToken ct)
        {
            await AnimationUtils.InterpAction(transform, SetRectX, _rect.anchoredPosition.x, targetX, duration, FEaseInOutCubic, ct);
        }
        
        // Now it's serializable!
        public async UniTask Animate_Serialized(float targetX, CancellationToken ct)
        {
            await AnimationUtils.InterpAction(transform, SetRectX, _rect.anchoredPosition.x, targetX, duration, interpolation, ct);
        }
    }
}