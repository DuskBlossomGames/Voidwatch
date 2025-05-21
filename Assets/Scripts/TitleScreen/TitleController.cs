using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using Button = UnityEngine.UI.Button;

public class TitleController : MonoBehaviour
{
    public ParticleSystem ps;
    public AnimationCurve textFadeCurve, particleSpeed;
    public float fadeTime, speedupTime, waitTime;

    private TextMeshProUGUI[] _texts;
    private Image[] _images;

    private void Start()
    {
        _texts = GetComponentsInChildren<TextMeshProUGUI>();
        _images = GetComponentsInChildren<Image>();
    }

    public void Play() { StartCoroutine(PlayRoutine()); }
    private IEnumerator PlayRoutine()
    {
        foreach (var button in gameObject.GetComponentsInChildren<Button>()) button.interactable = false;
        
        for (float t = 0; t < fadeTime; t += Time.fixedDeltaTime)
        {
            yield return new WaitForFixedUpdate();
            foreach (var obj in _texts) obj.SetAlpha(textFadeCurve.Evaluate(1-t/fadeTime));
            foreach (var obj in _images) obj.SetAlpha(1-t/fadeTime);
        }

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
        throw new Exception("Unimplemented");
    }

    public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
    }

}
