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
        private CancellationTokenSource _animationCts;
        
        /// <summary>  Scales a transform by a uniform factor x. </summary>
        private void UniformScale(float x) => transform.localScale = new Vector3(x, x, 1);
        
        private void Awake()
        {
            _startScale = transform.localScale.x;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Utils.CancelAndDisposeNew(ref _animationCts, this);
            AnimationUtils.InterpAction(transform, UniformScale, transform.localScale.x, hoverScaleMult*_startScale, hoverAnimDur, animationCurve, _animationCts.Token).Forget();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Utils.CancelAndDisposeNew(ref _animationCts, this);
            AnimationUtils.InterpAction(transform, UniformScale, transform.localScale.x, _startScale, hoverAnimDur, animationCurve, _animationCts.Token).Forget();
        }
    }
}