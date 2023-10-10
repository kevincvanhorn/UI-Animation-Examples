using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static SDGA.AnimationUtils;

namespace SDGA{
    internal class UIHoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        
        [SerializeField] private float hoverScaleMult = 1.04f;
        [SerializeField] private float hoverAnimDur = 0.2f;
        [SerializeField] private EAnimationCurve animationCurve = EAnimationCurve.EaseOutQuart;
        private float _startScale = 1.0f;
        
        protected CancellationTokenSource animationCts;
        
        /// <summary>  Scales a transform by a uniform factor x. </summary>
        private void UniformScale(float x) => transform.localScale = new Vector3(x, x, 1);
        
        private void Awake()
        {
            _startScale = transform.localScale.x;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Utils.CancelAndDisposeNew(ref animationCts);
            AnimationUtils.InterpAction(transform, UniformScale, transform.localScale.x, hoverScaleMult*_startScale, hoverAnimDur, animationCurve, animationCts.Token).Forget();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Utils.CancelAndDisposeNew(ref animationCts);
            AnimationUtils.InterpAction(transform, UniformScale, transform.localScale.x, _startScale, hoverAnimDur, animationCurve, animationCts.Token).Forget();
        }

        protected void OnDestroy()
        {
            Utils.CancelAndDispose(ref animationCts);
        }
    }
}