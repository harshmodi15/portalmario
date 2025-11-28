using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyAnimation : MonoBehaviour
{
    [Tooltip("Anaimation duration")]
    public float duration = 0.5f;

    public bool destroyOnComplete = true;

    public void StartShrink()
    {
        StopAllCoroutines();
        StartCoroutine(ShrinkCoroutine());
    }

    private IEnumerator ShrinkCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        transform.localScale = Vector3.zero;

        if (destroyOnComplete)
            Destroy(gameObject);
    }

    public void StartGrow()
    {
        StopAllCoroutines();
        StartCoroutine(GrowCoroutine());
    }
    private IEnumerator GrowCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
