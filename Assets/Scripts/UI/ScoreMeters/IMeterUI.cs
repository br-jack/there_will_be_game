using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Score
{
    public abstract class IMeterUI : MonoBehaviour
    {
        [SerializeField] protected RectTransform barFillTransform;
        [SerializeField] protected Image barFillImage;
        [SerializeField] protected TextMeshProUGUI scoreText;
        [SerializeField] protected TextMeshProUGUI titleText;
        [SerializeField] protected Image statusIcon;
        [SerializeField] protected RectTransform panelTransform;

        //should probably replace these next five with abstract classes but then we cant edit in unity editor
        protected int max = 2000;
        protected float fullBarWidth = 280.0f;

        protected Color lowScoreColor = new Color(1.0f, 0.72f, 0.15f, 1.0f);
        protected Color midScoreColor = new Color(1.0f, 0.45f, 0.10f, 1.0f);
        protected Color highScoreColor = new Color(0.90f, 0.15f, 0.15f, 1.0f);

        [SerializeField] protected float pulseScale = 1.06f;
        [SerializeField] protected float pulseUpTime = 0.08f;
        [SerializeField] protected float pulseDownTime = 0.12f;

        [SerializeField] protected Coroutine pulseCoroutine;

        protected abstract void Start();

        protected abstract void OnDestroy();

        protected void HandleScoreChanged(int newScore)
        {
            UpdateScoreUI(newScore);
            PlayPulse();
        }

        protected void UpdateScoreUI(int currentScore)
        {
            float normalizedScore = 0.0f;

            if (max > 0)
            {
                normalizedScore = Mathf.Clamp01((float)currentScore / (float)max);
            }

            float newWidth = fullBarWidth * normalizedScore;
            Color currentColor = GetScoreColor(normalizedScore);

            if (barFillTransform != null)
            {
                barFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            }

            if (barFillImage != null)
            {
                barFillImage.color = currentColor;
            }

            if (scoreText != null)
            {
                scoreText.text = currentScore.ToString();
            }

            if (titleText != null)
            {
                titleText.color = currentColor;
            }

            if (statusIcon != null)
            {
                statusIcon.color = currentColor;
            }
        }

        protected Color GetScoreColor(float normalizedScore)
        {
            if (normalizedScore < 0.4f)
            {
                return lowScoreColor;
            }
            else if (normalizedScore < 0.75f)
            {
                return midScoreColor;
            }
            else
            {
                return highScoreColor;
            }
        }

        protected void PlayPulse()
        {
            if (panelTransform == null)
            {
                return;
            }

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        protected IEnumerator PulseRoutine()
        {
            Vector3 normalScale = Vector3.one;
            Vector3 enlargedScale = new Vector3(pulseScale, pulseScale, 1.0f);

            float elapsedTime = 0.0f;
            while (elapsedTime < pulseUpTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / pulseUpTime;
                panelTransform.localScale = Vector3.Lerp(normalScale, enlargedScale, t);
                yield return null;
            }

            elapsedTime = 0.0f;
            while (elapsedTime < pulseDownTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / pulseDownTime;
                panelTransform.localScale = Vector3.Lerp(enlargedScale, normalScale, t);
                yield return null;
            }

            panelTransform.localScale = normalScale;
            pulseCoroutine = null;
        }
    }
}
