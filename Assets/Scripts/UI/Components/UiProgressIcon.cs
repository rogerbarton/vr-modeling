using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Handles showing the progress icon and animating it based on the PreExecute and PostExecute.
    /// This indicates the state of the libigl thread.
    /// </summary>
    public class UiProgressIcon : MonoBehaviour
    {
        public Gradient progressGradient;

        [Tooltip("Only show the progress icon after a certain time.")]
        public float progressDelayTime = 1f;

        [Tooltip("Once the icon is showing, how long to wait until a timeout occurs.")]
        public float timeoutTime = 9f;

        private Image _backgroundImage;
        private Image _iconImage;
        private Sprite _defaultSprite;
        public Sprite progressErrorSprite;

        private Coroutine _progressCoroutine;

        private void Start()
        {
            _backgroundImage = GetComponent<Image>();
            _iconImage = _backgroundImage.transform.GetChild(0).GetComponent<Image>();
            _defaultSprite = _iconImage.sprite;
        }

        public void PreExecute()
        {
            _progressCoroutine = StartCoroutine(ShowProgressAfterTime());
        }

        public void PostExecute()
        {
            StopCoroutine(_progressCoroutine);
            _backgroundImage.enabled = false;
            _iconImage.enabled = false;
            _iconImage.sprite = _defaultSprite;
        }

        /// <summary>
        /// Coroutine that animates progress icon over time. Can be stopped
        /// </summary>
        private IEnumerator ShowProgressAfterTime()
        {
            yield return new WaitForSeconds(progressDelayTime);
            _backgroundImage.enabled = true;
            _iconImage.enabled = true;

            var startTime = Time.time;
            while (Time.time - startTime <= timeoutTime)
            {
                _backgroundImage.color = progressGradient.Evaluate((Time.time - startTime) / timeoutTime);
                _iconImage.transform.Rotate(new Vector3(0, 0, -180 * Time.deltaTime));
                yield return null;
            }

            _iconImage.transform.localRotation = Quaternion.identity;
            _iconImage.sprite = progressErrorSprite;
        }
    }
}