using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDGA
{
    /// <summary>
    /// Like <see cref="SimpleMovement_Coroutine"/> This is a safe way to move a transform but using the UniTask library.
    /// To see this work, enable an object with this component.
    ///
    /// Diferences:
    /// 1. We're now using a rect transform offset to make this more consistent
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    internal class SimpleMovement_UniTask : MonoBehaviour
    {
        [SerializeField] private float startX;
        [SerializeField] private float endX;
        [SerializeField] private float duration = 1f;
        
        private RectTransform _rect;
        private CancellationTokenSource _animationCts;
        
        private static float FEaseInOutCubic(float x)  => x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) * 0.5f;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }
        
        private void OnEnable()
        {
            Utils.CancelAndDisposeNew(ref _animationCts); // Stop all animations & create a new cancellation token source
            DoAnimationLoop(_animationCts.Token).Forget(); // Use that CTS to fire off a UniTask & not keep track of the handle.
        }

        private void OnDisable()
        {
            Utils.CancelAndDispose(ref _animationCts); // Stop all animations
        }

        private async UniTask DoAnimationLoop(CancellationToken ct)
        {
            try
            {
                while (isActiveAndEnabled & !ct.IsCancellationRequested) // These are redundant checks b/c of the try catch but good practice
                {
                    await Animate(endX, _animationCts.Token); // Animate forward
                    await Animate(startX, _animationCts.Token); // Animate back
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
        /// Animate the x component of the anchored rect position.
        /// </summary>
        private async UniTask Animate(float targetX, CancellationToken ct)
        {
            float x0 = _rect.anchoredPosition.x;
            float timeElapsed = 0;
            while (timeElapsed < duration && !ct.IsCancellationRequested)
            {
                var t =  FEaseInOutCubic(timeElapsed / duration); // Interpolate [0,1]
                SetRectX(x0 + t* (targetX - x0)); // Action
                await UniTask.Yield(cancellationToken: ct); // Throws System.OperationCancelledException
                timeElapsed += Time.deltaTime; // Progress t
            }
        }
        
        private void OnDestroy()
        {
            Utils.CancelAndDispose(ref _animationCts);
        }
    }
}