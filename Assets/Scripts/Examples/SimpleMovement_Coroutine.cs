using System;
using System.Collections;
using UnityEngine;

namespace SDGA
{
    internal class SimpleMovement_Coroutine : MonoBehaviour
    {
        [SerializeField] private float startX;
        [SerializeField] private float endX;
        
        [SerializeField] private float duration = 1f;

        private float FEaseInOutCubic(float x)  => x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) * 0.5f;
        
        private void OnEnable()
        {
            if (!Application.isPlaying) return; // Don't call in editor
            StopAllCoroutines(); // Stop other running tasks
            StartCoroutine(DoAnimationLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines(); // Stop other running tasks
        }

        private IEnumerator DoAnimationLoop()
        {
            while (isActiveAndEnabled)
            {
                yield return Animate(endX);   // Animate Forward
                yield return Animate(startX); // Animate Back
            }
        }
        
        /// <summary>
        /// Animate the x position of this transform.
        /// </summary>
        private IEnumerator Animate(float targetX)
        {
            float startX = transform.position.x;
            float timeElapsed = 0;
            while (timeElapsed < duration)
            {
                var t =  FEaseInOutCubic(timeElapsed / duration); // Interpolate [0,1]
                transform.position = new Vector3(startX + t* (targetX - startX), transform.position.y, transform.position.z); // Action
                yield return null; // Wait a frame
                timeElapsed += Time.deltaTime; // Progress t
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}