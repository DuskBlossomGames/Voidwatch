using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Util;
using Button = UnityEngine.UI.Button;

public class TitleController : MonoBehaviour
{
    public Image fadeIn;
    public float fadeInTime;
    
    public ParticleSystem ps;
    public AnimationCurve textFadeCurve, particleSpeed;
    public float fadeTime, speedupTime, waitTime;
    
    public RectTransform credits;
    public float creditsTime, creditsHoldTime, anchorDist;
    public AnimationCurve creditsMultCurve;
    
    private TextMeshProUGUI[] _texts;
    private Image[] _images;

    private void Start()
    {
        _texts = GetComponentsInChildren<TextMeshProUGUI>();
        _images = GetComponentsInChildren<Image>();

        GetComponent<Canvas>().enabled = false;
        ps.gameObject.SetActive(false);
        GetComponent<VideoPlayer>().loopPointReached += _ => StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        fadeIn.gameObject.SetActive(true);
        
        GetComponent<Canvas>().enabled = true;
        ps.gameObject.SetActive(true);
        GetComponent<VideoPlayer>().enabled = false;

        for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();

            fadeIn.SetAlpha(1 - t / fadeInTime);
        }

        fadeIn.gameObject.SetActive(false);
    }

    private IEnumerator Fade(bool includeCredits)
    {
        foreach (var button in gameObject.GetComponentsInChildren<Button>()) button.interactable = false;
        
        for (float t = 0; t < fadeTime; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            foreach (var obj in _texts)
            {
                if (!includeCredits && obj.transform.IsChildOf(credits)) continue;
                obj.SetAlpha(Mathf.Min(obj.color.a, textFadeCurve.Evaluate(1-t/fadeTime)));
            }

            foreach (var obj in _images)
            {
                if (!includeCredits && obj.transform.IsChildOf(credits)) continue;
                obj.SetAlpha(Mathf.Min(obj.color.a, 1-t/fadeTime));
            }
        }
    }

    private void UnFade()
    {
        foreach (var button in gameObject.GetComponentsInChildren<Button>()) button.interactable = true;
        foreach (var obj in _texts) obj.SetAlpha(1);
        foreach (var obj in _images) obj.SetAlpha(1);
    }
    
    public void Play() { StartCoroutine(PlayRoutine()); }
    private IEnumerator PlayRoutine()
    {
        yield return Fade(true);
        
        for (float t = 0; t < speedupTime; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            var main = ps.main;
            main.simulationSpeed = particleSpeed.Evaluate(t/speedupTime);
        }

        ps.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("LevelSelect");
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void Options()
    {
        throw new Exception("Unimplemented");
    }

    public void Credits()
    {
        StartCoroutine(CreditsRoutine());
    }

    private IEnumerator CreditsRoutine()
    {
        yield return Fade(false);

        float anchorMod = 0;
        for (float t = 0; t < creditsTime; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();

            anchorMod += anchorDist / creditsTime * Time.fixedDeltaTime;
            credits.anchorMin = new Vector2(0, creditsMultCurve.Evaluate(anchorMod/anchorDist)*anchorDist);
            credits.anchorMax = new Vector2(1, 1+creditsMultCurve.Evaluate(anchorMod/anchorDist)*anchorDist);
        }
        
        yield return new WaitForSeconds(creditsHoldTime);
        yield return Fade(true);
        yield return new WaitForSeconds(0.5f);

        credits.anchorMin = Vector2.zero;
        credits.anchorMax = Vector2.one;

        UnFade();
    }

    public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

}
