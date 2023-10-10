using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SDGA
{
    [RequireComponent(typeof(RectTransform))]
    internal class SimpleMovement_UniTask : MonoBehaviour
    {
        [SerializeField] private float startX;
        [SerializeField] private float endX;
        
        [SerializeField] private float duration = 1f;
        private RectTransform _rect;
        private CancellationTokenSource animationCts;
        private float FEaseInOutCubic(float x)  => x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) * 0.5f;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }
        
        private void OnEnable()
        {
            if (!Application.isPlaying) return; // Don't call in editor
            Utils.CancelAndDisposeNew(ref animationCts); // Stop all animations
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
                    await Animate(endX, destroyCancellationToken); // Animate forward
                    await Animate(startX, destroyCancellationToken); // Animate back
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
        
        /// <summary>
        /// Animate the x component of the anchored rect position.
        /// </summary>
        private async UniTask Animate(float targetX, CancellationToken ct)
        {
            float startX = _rect.anchoredPosition.x;
            float timeElapsed = 0;
            while (timeElapsed < duration && !ct.IsCancellationRequested)
            {
                var t =  FEaseInOutCubic(timeElapsed / duration); // Interpolate [0,1]
                SetRectX(startX + t* (targetX - startX)); // Action
                await UniTask.Yield(cancellationToken: ct); // Throws System.OperationCancelledException
                timeElapsed += Time.deltaTime; // Progress t
            }
        }
        
        private void OnDestroy()
        {
            Utils.CancelAndDispose(ref animationCts);
        }
    }
}