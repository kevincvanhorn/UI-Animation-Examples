using System;
using System.Collections;
using UnityEngine;

namespace SDGA
{
    /// <summary>
    /// Safe way to move a transform using co-routines.
    /// To see this work, enable an object with this component.
    /// </summary>
    internal class SimpleMovement_Coroutine : MonoBehaviour
    {
        [SerializeField] private float startX;
        [SerializeField] private float endX;
        
        [SerializeField] private float duration = 1f;

        private static float FEaseInOutCubic(float x)  => x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) * 0.5f;
        
        private void OnEnable()
        {
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
            float x0 = transform.position.x;
            float timeElapsed = 0;
            while (timeElapsed < duration)
            {
                var t =  FEaseInOutCubic(timeElapsed / duration); // Interpolate [0,1]
                transform.position = new Vector3(x0 + t* (targetX - x0), transform.position.y, transform.position.z); // Action
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