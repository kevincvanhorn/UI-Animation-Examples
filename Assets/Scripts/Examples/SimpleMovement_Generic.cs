using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDGA
{
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
            _rect = GetComponent<RectTransform>();
        }
        
        private void OnEnable()
        {
            if (!Application.isPlaying) return; // Don't call in editor
            Utils.CancelAndDisposeNew(ref animationCts);
            DoAnimationLoop(animationCts.Token).Forget();
        }
        
        private void OnDisable()
        {
            if (!Application.isPlaying) return; // Don't call in editor
            Utils.CancelAndDispose(ref animationCts); // Stop all animations
        }

        private async UniTask DoAnimationLoop(CancellationToken ct)
        {
            try
            {
                while (isActiveAndEnabled & !ct.IsCancellationRequested)
                {
                    await Animate_Serialized(endX, destroyCancellationToken);   // Animate forward
                    await Animate_Serialized(startX, destroyCancellationToken); // Animate back
                }
            }
            finally
            {
                SetRectX(startX); // On cancellation, reset to start
            }
        }

        /// <summary>
        /// Set the x component of this rect anchored position.
        /// </summary>
        private void SetRectX(float x) => _rect.anchoredPosition = new Vector2(x, _rect.anchoredPosition.y);

        // Same thing but reusable!
        public async UniTask Animate_Func(float targetX, CancellationToken ct)
        {
            await AnimationUtils.InterpAction(transform, SetRectX, _rect.anchoredPosition.x, targetX, duration, FEaseInOutCubic, ct);
        }
        
        // Now it's serializable!
        public async UniTask Animate_Serialized(float targetX, CancellationToken ct)
        {
            await AnimationUtils.InterpAction(transform, SetRectX, _rect.anchoredPosition.x, targetX, duration, interpolation, ct);
        }

        private void OnDestroy()
        {
            Utils.CancelAndDispose(ref animationCts);
        }
    }
}