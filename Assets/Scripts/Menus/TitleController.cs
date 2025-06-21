using System.Collections;
using Extensions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace Menus
{
    public class TitleController : OptionsHolder
    {
        public Image fadeIn;
        public float fadeInTime;

        public ParticleSystem ps;
        public AnimationCurve textFadeCurve, particleSpeed;
        public float fadeTime, speedupTime, waitTime;

        public GameObject options;
        
        public RectTransform credits;
        public float creditsTime, creditsHoldTime, anchorDist;
        public AnimationCurve creditsMultCurve;

        private TextMeshProUGUI[] _texts;
        private Image[] _images;

        public GameObject tutorialHint;
        
        private bool _initCredits;
        private void Start()
        {
            _texts = GetComponentsInChildren<TextMeshProUGUI>();
            _images = GetComponentsInChildren<Image>();

            _initCredits = GameObject.Find("RollCredits") != null;
            if (_initCredits) Destroy(GameObject.Find("RollCredits"));

            GetComponent<Canvas>().enabled = false;
            ps.gameObject.SetActive(false);

            if (SettingsInterface.isFirstTime)
            {
                tutorialHint.SetActive(true);
                SettingsInterface.isFirstTime = false; // since this is the only place it's used, easy solution for now, can change if needed elsewhere
            }
            
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            fadeIn.gameObject.SetActive(true);
            if (_initCredits) yield return Fade(false);

            GetComponent<Canvas>().enabled = true;
            ps.gameObject.SetActive(true);

            for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();

                fadeIn.SetAlpha(1 - t / fadeInTime);
            }

            fadeIn.gameObject.SetActive(false);
            if (_initCredits) yield return CreditsRoutine();
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
            for (var i = 0; i < transform.childCount; i++)
            {
                if (options.transform.GetSiblingIndex() == i) break;
                transform.GetChild(i).gameObject.SetActive(false);
            }
            
            options.SetActive(true);
        }

        public override void ExitOptions()
        {
            options.SetActive(false);
            
            for (var i = 0; i < transform.childCount; i++)
            {
                if (options.transform.GetSiblingIndex() == i) break;
                transform.GetChild(i).gameObject.SetActive(true);
            }
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

        public void OpenInstagram()
        {
            Application.OpenURL("https://instagram.com/duskblossomgames");
        }

        public void OpenSteam()
        {
            //Application.OpenURL("https://steam.com");
        }

        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

    }
}
