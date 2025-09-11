using System.Collections;
using System.Linq;
using Extensions;
using Menus.Pause;
using Menus.Util;
using Singletons;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using Button = UnityEngine.UI.Button;
using static Singletons.Static_Info.Statistics;
using static Singletons.Static_Info.LevelSelectData;

namespace Menus
{
    public class TitleController : OptionsHolder
    {
        public Image fadeIn;
        public float fadeInTime;

        public ParticleSystem ps;
        public AnimationCurve textFadeCurve, buttonsFadeCurve, particleSpeed;
        public float fadeTime, speedupTime, waitTime;

        public GameObject options;
        
        public RectTransform credits;
        public float creditsTime, creditsHoldTime, anchorDist;
        public AnimationCurve creditsMultCurve;

        private TextMeshProUGUI[] _texts;
        private FlashUI[] _flashes;
        private ScaleUI _scale;
        private Button[] _buttons;
        private Image[] _images;

        public GameObject tutorialHint;
        public CheckboxController hardMode;
        public GameObject rank;
        private bool _firstTime;
        
        private bool _initCredits;
        private void Start()
        {
            _texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            _flashes = GetComponentsInChildren<FlashUI>(true);
            _scale = GetComponentInChildren<ScaleUI>();
            _buttons = GetComponentsInChildren<Button>(true);
            _images = GetComponentsInChildren<Image>(true)
                .Where(i => i != fadeIn && !i.transform.IsChildOf(options.transform)).ToArray(); // options should always be closed, so just don't mess with it (breaks slider hitboxes)
            
            _initCredits = GameObject.Find("RollCredits") != null;
            if (_initCredits) Destroy(GameObject.Find("RollCredits"));

            GetComponent<Canvas>().enabled = false;
            ps.gameObject.SetActive(false);

            // ReSharper disable once AssignmentInConditionalExpression
            if (_firstTime = SettingsInterface.isFirstTime)
            {
                tutorialHint.SetActive(true);
                SettingsInterface.isFirstTime = false; // since this is the only place it's used, easy solution for now, can change if needed elsewhere
            }

            hardMode.transform.parent.gameObject.SetActive(SettingsInterface.rank >= SettingsInterface.Rank.Captain);
            for (var i = 0; i < rank.transform.childCount; i++)
            {
                rank.transform.GetChild(i).gameObject.SetActive((int) SettingsInterface.rank == i);
            }

            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            fadeIn.gameObject.SetActive(true);
            if (_initCredits) yield return Fade(false, false);

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

        private IEnumerator Fade(bool includeCredits, bool wait=true)
        {
            foreach (var flash in _flashes) flash.enabled = false;
            _scale.enabled = false;
            foreach (var button in _buttons) button.interactable = false;

            for (float t = 0; t < fadeTime; t += Time.fixedDeltaTime)
            {
                if (wait) yield return new WaitForFixedUpdate();
                foreach (var obj in _texts)
                {
                    if (!includeCredits && obj.transform.IsChildOf(credits)) continue;
                    obj.SetAlpha(Mathf.Min(obj.color.a, textFadeCurve.Evaluate(1-t/fadeTime)));
                }

                foreach (var obj in _images)
                {
                    if (!includeCredits && obj.transform.IsChildOf(credits)) continue;

                    var alpha = 1 - t / fadeTime;
                    if (obj.GetComponent<Button>() != null) alpha = buttonsFadeCurve.Evaluate(alpha); 
                    obj.SetAlpha(Mathf.Min(obj.color.a, alpha));
                }
            }
        }

        private IEnumerator UnFade()
        {
            for (float t = 0; t < fadeTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                foreach (var obj in _texts)
                {
                    obj.SetAlpha(Mathf.Max(obj.color.a, 1-textFadeCurve.Evaluate(1-t/fadeTime)));
                }

                foreach (var obj in _images)
                {
                    obj.SetAlpha(Mathf.Max(obj.color.a, t/fadeTime));
                }
            }

            foreach (var flash in _flashes) flash.enabled = true;
            foreach (var button in _buttons) button.interactable = true;
            _scale.enabled = true;
        }

        public void Play() { StartCoroutine(PlayRoutine()); }
        private IEnumerator PlayRoutine()
        {
            LevelSelectDataInstance.hardMode = SettingsInterface.rank >= SettingsInterface.Rank.Captain && hardMode.Value;
            yield return Fade(true);

            for (float t = 0; t < speedupTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                var main = ps.main;
                main.simulationSpeed = particleSpeed.Evaluate(t/speedupTime);
            }

            ps.gameObject.SetActive(false);

            yield return new WaitForSeconds(waitTime);
            StatisticsInstance.startTime = Time.time;
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
            
            for (var i = 0; i < options.transform.GetSiblingIndex(); i++) transform.GetChild(i).gameObject.SetActive(true);
            tutorialHint.SetActive(_firstTime);
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

                var fast = InputManager.GetKey(KeyCode.Space) || InputManager.GetKey(KeyCode.Mouse0);
                if (fast) t += Time.fixedDeltaTime;
                anchorMod += (fast ? 2 : 1) * anchorDist / creditsTime * Time.fixedDeltaTime;
                credits.anchorMin = new Vector2(0, creditsMultCurve.Evaluate(anchorMod/anchorDist)*anchorDist);
                credits.anchorMax = new Vector2(1, 1+creditsMultCurve.Evaluate(anchorMod/anchorDist)*anchorDist);
            }

            yield return new WaitForSeconds(creditsHoldTime);
            yield return Fade(true);
            yield return new WaitForSeconds(0.5f);

            credits.anchorMin = Vector2.zero;
            credits.anchorMax = Vector2.one;

            yield return UnFade();
        }

        public void OpenInstagram()
        {
            Application.OpenURL("https://instagram.com/duskblossomgames");
        }

        public void OpenSteam()
        {
            Application.OpenURL("https://store.steampowered.com/app/3764010/Voidwatch");
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
