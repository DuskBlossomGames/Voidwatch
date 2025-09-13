using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using LevelSelect;
using Player;
using ProgressBars;
using Singletons;
using Singletons.Static_Info;
using Spawnables.Controllers;
using Spawnables.Damage;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using static Singletons.Static_Info.PlayerData;
using Random = UnityEngine.Random;

namespace Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        private enum Stage
        {
            Movement,
            Dashing,
            Attacking,
            Enemy,
            UI,
            Shop,
            Enemies,
            Finish
        }

        private static readonly Dictionary<Stage, string[]> Text = new()
        {
            {
                Stage.Movement, new[]
                {
                    "LOOK ALIVE, MAGGOT. My name is General Forrest Benedict. Welcome to Voidwatch Academy. You’re here for one reason and one reason only: kill these Voidborne monsters so we can all go home.",
                    "Your ship – the Voidhawk Infiltrator – accelerates towards the mouse when <b>{Accelerate}</b> is pressed. The camera also expands to follow the mouse; <b>I’d recommend you keep the mouse farther out.</b>",
                    "One last thing. <b>{Brake}</b> can be used to brake and slow movement. <b>NOW GET MOVIN’!!!</b>",
                    "Practice Accelerating and Braking"
                }
            },
            {
                Stage.Dashing, new []
                {
                    "Alright, that’s enough. <b>{Dash}</b> uses <b>Void Energy</b> to enter <b>Voidspace</b>. That’ll let ya pass through enemies and obstacles.",
                    "That purple bar up there shows your <b>Void Energy</b>. You’ve got enough for about three dashes, but it’ll regenerate over time. Go ahead and give it a spin.",
                    "Complete the Course"
                }
            },
            {
                Stage.Attacking, new []
                {
                    "Very nice. Now’s the fun part: use your <b>{PrimaryWeapon}</b> to attack. Your <b>ammo</b> is shown in that partial circle around your ship. Careful gettin’ too close to enemies, or it’ll damage the both you.",
                    "All enemies have an <b>arrow</b> pointing to them while offscreen. Alright kid, GO HAVE SOME FUN!",
                    "Destroy the Targets"
                }
            },
            {   
                Stage.Enemy, new[]
                {
                    "<b>HELL YEAH!</b> Alright, now take a look at that <b>red health bar</b>. That <b>blue shield</b> on top will automatically regenerate over time. Also check your <b>minimap</b>. The enemies are in <b>red</b>.",
                    "The safety border around the drill field will now be removed. <b>Orbital cannons</b> will lock on if you exit. They’ll turn you to debris if you’re not careful. Now you’re ready: shoot some real <b>Voidborne</b>.",
                    "Defeat the Missiler"
                }
            },
            {
                Stage.UI, new[]
                {
                    "You just got yourself <b>500 Scrap</b>. Not bad, kid! If I were you, I’d go see about some <b>upgrade options</b> and <b>Boosting stats</b> in the <b>space station</b>.",
                    "Continue Your Journey"
                }
            },
            {
                Stage.Shop, new []
                {
                    "AHHH! Wh-what was that?? Oh, an infiltrator. Y-you must be in the Academy.",
                    "I’m <b>Jacob Trembley</b>, the Voidwatch’s engineer and m-m-mechanic. My consciousness is downloaded into the space stations across th-the galaxy so I can h-help pilots like you.",
                    "Every world you clear out there can be either <b>Pilfered</b> for an upgrade or <b>Scavanged</b> for a random amount of <b>Scrap</b>. Y-your current upgrades are visible in your <b>Pause Menu</b>.",
                    "<b>Scrap</b> is also received from enemies, and it can be spent on either <b>Pilfering Further</b> to see new upgrade options, or <b>Boosting stats</b>, r-right here in the space station.",
                    "W-well, I think that’s everything. Good luck out there, c-c-cadet! You’re gonna need it...",
                    "Continue Your Journey"
                }
            },
            {
                Stage.Enemies, new []
                {
                    "Alright, now we’re talking. Go fight some more <b>Voidborne</b> with those fancy new weapons. KNOCK ‘EM DEAD!",
                    "Hunt the Enemies"
                }
            },
            {
                Stage.Finish, new[]
                {
                    "WOOOOO! Looks like we got a new member of the world’s finest pilots: the Voidwatch. Godspeed, kid. Wipe that scum out of our reality!",
                    ""
                }
            }
        };
    
        public DialogueController dialogueController;
        public GameObject instruction;
        public TextMeshProUGUI warning;
        public float instructionFadeTime, warningFadeTime;
        public Movement playerMovement;
        public ProgressBar progressBar;

        public CanvasGroup continueButton;
        public float continueFadeTime;
        public Pair<Vector2, Vector2>[] continuePositions;
        
        public GameObject scrapPrefab;

        public Image fadeOut;
        public float fadeOutTime;
        public GameObject blackScreen;

        public GameObject canvas;
        
        public GameObject dashBar;
        public GameObject minimap;
        public GameObject healthBar;
        public GameObject scrap;

        public GameObject securityBorder;

        public GameObject targets;

        public GameObject enemy;
        
        public GameObject enemies;
    
        public GameObject raceCourse;
        private RaceController _raceController;

        public UpgradeManager upgradeManager;
    
        private readonly Timer _warningTimer = new();
    
        private Camera _camera;
        private FollowPlayer _fp;
        private CustomRigidbody2D _playerRb;

        private string _pbFormat;

        private Stage _stage;
        private int _textIdx = -1;
        private string _instrFormat;

        private float _genFloat;
        private bool _genFlag, _metCondition;

        private static TutorialController _instance;

        private void Start()
        {
            if (_instance != null)
            {
                // setup for re-entry
                dashBar.SetActive(true);
                healthBar.SetActive(true);
                minimap.SetActive(true);
                scrap.SetActive(true);
                securityBorder.SetActive(false);
                _instance.playerMovement = playerMovement;
                _instance.enemies = enemies;
                _instance.blackScreen = blackScreen;
                playerMovement.GetComponent<PlayerDamageable>().fadeOut = _instance.fadeOut.gameObject;
                
                StartCoroutine(FadeIn(() =>
                {
                    Destroy(gameObject);
                    Destroy(canvas);
                }));
                return;
            }
            _instance = this;

            _pbFormat = progressBar.GetComponentInChildren<TextMeshProUGUI>().text;
            
            _camera = Camera.main;
            _fp = _camera!.GetComponent<FollowPlayer>();

            _playerRb = playerMovement.GetComponent<CustomRigidbody2D>();

            _raceController = raceCourse.GetComponentInChildren<RaceController>();

            _instrFormat = instruction.GetComponentInChildren<TextMeshProUGUI>().text;
            dialogueController.Continue += Continue;
            playerMovement.SetInputBlocked(true);
            playerMovement.GetComponent<PlayerDamageable>().godmode = true;

            StartCoroutine(FadeIn(Continue));

            PlayerDataInstance.IsTutorial = true;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(canvas);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (InputManager.GetKeyDown(KeyCode.LeftBracket))
            {
                _stage = Stage.UI;
                // _textIdx = -1;
                // Continue();
            }
#endif
            if (!_inTask) return;
            
            _warningTimer.Update();
            
            warning.SetAlpha(Mathf.Clamp01(_warningTimer.Value/warningFadeTime));
            
            switch (_stage)
            {
                case Stage.Movement:
                {
                    var tooClose = Vector2.SqrMagnitude(_camera.ScreenToWorldPoint(InputManager.mousePosition) -
                                                        playerMovement.transform.position) < 5 * 5;
                    
                    _genFlag |= !tooClose && _playerRb.linearVelocity.sqrMagnitude > 25*25;
                    _metCondition |= !tooClose && _genFlag && _playerRb.linearVelocity.sqrMagnitude < 5 * 5;

                    if (tooClose)
                    {
                        ShowWarning("WARNING: keep your mouse farther from the ship", 0.5f);
                    }
                    else if (InputManager.GetKey(KeyCode.A) || InputManager.GetKey(KeyCode.D))
                    {
                        ShowWarning("WARNING: A/D cannot strafe, move mouse to turn", 1);
                    }
                    else if (warning.text.Contains("farther")) // if it's the mouse one, make it fade immediately upon fixing
                    {
                        _warningTimer.Value = Mathf.Min(_warningTimer.Value, warningFadeTime);
                    }
                    break;
                }
                case Stage.Dashing:
                {
                    _metCondition |= _raceController.Completed;

                    if (_raceController.GetComponent<Collider2D>().OverlapPoint(playerMovement.transform.position) &&
                        _raceController.CompletedRings > 0 && // make sure you're not on the starting block
                        _raceController.CompletedRings < _raceController.rings.transform.childCount)
                    {
                        ShowWarning("WARNING: not all rings completed", 1);
                    }
                    
                    var playerRot = Mathf.Rad2Deg * UtilFuncs.Angle(playerMovement.transform.position);
                    var found = false;
                    for (var i = 1; i < raceCourse.transform.childCount-1; i++)
                    {
                        if (playerMovement.Dodging || Mathf.Abs(Mathf.DeltaAngle(playerRot, 
                                raceCourse.transform.GetChild(i).rotation.eulerAngles.z+90)) > 5) continue;
                        
                        if (_genFloat == 0) _genFloat = Time.time;
                        else if (Time.time - _genFloat > 1.5f) ShowWarning("TIP: dash through the walls to progress", 0);
                            
                        found = true;
                        break;
                    }
                    if (!found) _genFloat = 0;
                    
                    break;
                }
                case Stage.Attacking:
                {
                    var anyDamaged = targets.GetComponentsInChildren<EnemyDamageable>().Any(d => d.Health < d.MaxHealth);
                    
                    if (_genFloat == 0 || anyDamaged) _genFloat = Time.time;
                    if (Time.time - _genFloat > 10 && !anyDamaged)
                    {
                        ShowWarning("TIP: follow the arrows at the edge of the screen", 0);
                    }

                    _metCondition |= targets.transform.childCount == 0;
                    break;
                }
                case Stage.Enemy:
                {
                    _metCondition |= enemy.transform.childCount == 0;
                    break;
                }
                case Stage.UI:
                {
                    _metCondition |= SceneManager.GetActiveScene().name == "Shop";

                    if (SceneManager.GetActiveScene().name == "LevelSelect")
                    {
                        if (FindAnyObjectByType<MiniPlayerController>().TravelingTo == 2) StartCoroutine(FadeInstruction(true));

                        if (_genFloat == 0)
                        {
                            _genFloat = Time.time;
                            if (_genFlag) ShowWarning("Proceed to the next planet", 3);
                        }
                        if (Time.time - _genFloat > 20) ShowWarning("TIP: click a selected planet to visit it", 0);
                    }

                    break;
                }
                case Stage.Shop:
                {
                    _metCondition |= SceneManager.GetActiveScene().name == "Tutorial";

                    if (SceneManager.GetActiveScene().name == "LevelSelect")
                    {
                        warning.rectTransform.offsetMax = Vector2.zero;
                        instruction.SetActive(true);
                        progressBar.gameObject.SetActive(true);
                        if (FindAnyObjectByType<MiniPlayerController>().TravelingTo == 3) StartCoroutine(FadeInstruction(true));

                        if (_genFloat == 0) _genFloat = Time.time;
                        if (Time.time - _genFloat > 20) ShowWarning("TIP: click a selected planet to visit it", 0);
                    }
                    else if (SceneManager.GetActiveScene().name == "Shop")
                    {
                        if (_genFloat == 0) _genFloat = Time.time;
                        if (Time.time - _genFloat > 20) ShowWarning("TIP: after buying upgrades, exit the shop", 0);
                    }

                    break;
                }
                case Stage.Enemies:
                {
                    _metCondition |= enemies.transform.childCount == 0;
                    break;
                }
            }
            
            if (_metCondition)
            {
                if (_stage is Stage.Movement or Stage.Dashing)
                {
                    if (!_continueState) StartCoroutine(FadeContinue(true));
                }
                else
                {
                    FinishTask();
                }
            }
        }

        public void FinishTask()
        {
            _inTask = false;
            Continue();
            
            if (_continueState) StartCoroutine(FadeContinue(false));
        }

        private bool _continueState;
        private int _continueIdx;
        private IEnumerator FadeContinue(bool shown)
        {
            if (shown)
            {
                var pos = continuePositions[_continueIdx++];
                continueButton.GetComponent<RectTransform>().anchorMin = pos.a;
                continueButton.GetComponent<RectTransform>().anchorMax = pos.b;
            }
            continueButton.interactable = shown;
            FindAnyObjectByType<EventSystem>().SetSelectedGameObject(null); // so space, etc doesn't trigger it
            
            _continueState = shown;
            for (float t = 0; t < continueFadeTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                continueButton.alpha = Mathf.Lerp(shown ? 0 : 1, shown ? 1 : 0, t / continueFadeTime);
            }
        }

        private void ResetGenFloat(Scene oldScene, Scene newScene) => _genFloat = 0;

        private static IEnumerator FadeObjIn(GameObject obj)
        {
            obj.SetActive(true);
            const float time = 1f;
            var cg = obj.GetComponent<CanvasGroup>();
            cg.alpha = 0;
            for (float t = 0; t < time; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cg.alpha = Mathf.Lerp(0, 1, t / time);
            }
        }
        
        private bool _inTask;
        private void Continue()
        {
            _textIdx += 1;
            if (_textIdx >= Text[_stage].Length)
            {
                _stage += 1;
                _textIdx = 0;
                _genFloat = 0;
                _genFlag = _metCondition = false; // reset for the next stage to use
            }

            var task = _textIdx == Text[_stage].Length - 1;
            Action setup = null;
            switch (_stage)
            {
                case Stage.Dashing:
                {
                    if (_textIdx == 1) StartCoroutine(FadeObjIn(dashBar));
                    break;
                }
                case Stage.Attacking:
                {
                    if (_textIdx == 0) raceCourse.SetActive(false);
                    else if (task) setup = () => targets.SetActive(true);
                    break;
                }
                case Stage.Enemy:
                {
                    if (_textIdx == 0) setup = () =>
                    {
                        playerMovement.GetComponent<PlayerDamageable>().godmode = false;
                        StartCoroutine(FadeObjIn(healthBar));
                        StartCoroutine(FadeObjIn(minimap));
                    };
                    else if (task) setup = () =>
                    {
                        enemy.SetActive(true);
                        securityBorder.SetActive(false);
                    };
                    break;
                }
                case Stage.UI:
                {
                    if (_textIdx == 0)
                    {
                        StartCoroutine(FadeObjIn(scrap));
                        PlayerDataInstance.Scrap = 500;
                    }
                    else if (task)
                    {
                        setup = () => upgradeManager.Show();
                        SceneManager.activeSceneChanged += ResetGenFloat;
                    }
                    break;
                }
                case Stage.Shop:
                {
                    if (_textIdx == 0)
                    {
                        warning.rectTransform.offsetMax = new Vector2(0, 175);
                        instruction.SetActive(false);
                        progressBar.gameObject.SetActive(false);
                        FindAnyObjectByType<GraphicRaycaster>().enabled = false;
                    }
                    else if (task) FindAnyObjectByType<GraphicRaycaster>().enabled = true;

                    break;
                }
                case Stage.Enemies:
                {
                    if (_textIdx == 0) SceneManager.activeSceneChanged -= ResetGenFloat;
                    if (task) setup = () =>
                    {
                        enemies.SetActive(true);
                        foreach (var ev in enemies.GetComponentsInChildren<EnemyVariant>())
                        {
                            ev.ScrapPrefab = scrapPrefab;
                            ev.ScrapCount = Random.Range(20, 60);
                        }
                    };
                    break;
                }
                case Stage.Finish:
                {
                    if (task)
                    {
                        dialogueController.Continue -= Continue;
                        StartCoroutine(FadeOut(() =>
                        {
                            blackScreen.SetActive(true);
                            Destroy(StaticInfoHolder.Instance.gameObject);
                            SettingsInterface.SetMinRank(SettingsInterface.Rank.Lieutenant);
                            SceneManager.LoadScene("TitleScreen");
                            ExitTutorial();
                        }));
                        return;
                    }

                    break;
                }
            }

            if (task)
            {
                StartCoroutine(ShowInstruction(Text[_stage][_textIdx], setup));
            }
            else if (_textIdx == 0 && _stage != 0)
            {
                StartCoroutine(ShowDialogue(Text[_stage][_textIdx], setup));
            }
            else
            {
                dialogueController.ShowText(Text[_stage][_textIdx], true);
            }
        }

        private IEnumerator ShowInstruction(string text, Action setup=null)
        {
            dialogueController.Continue -= Continue;

            if (_stage == Stage.Dashing)
            {
                yield return FadeOut();
                dialogueController.SetClosed();
                raceCourse.SetActive(true);
                yield return FadeIn();
            }
            else yield return dialogueController.Close(1.5f);

            if (playerMovement != null)
            {
                playerMovement.SetInputBlocked(false);
                playerMovement.autoPilot = false;
            }
            setup?.Invoke();

            var tmp = instruction.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = string.Format(_instrFormat, text);

            
            warning.gameObject.SetActive(true);
            warning.SetAlpha(0);
            if (_stage != Stage.Shop) instruction.SetActive(true);
            
            var img = instruction.GetComponentInChildren<Image>();
            tmp.ForceMeshUpdate();
            tmp.mesh.RecalculateBounds();
            img.rectTransform.sizeDelta = new Vector2(tmp.mesh.bounds.size.x + 40, img.rectTransform.sizeDelta.y);

            if (_stage == Stage.Dashing)
            {
                ShowWarning("Fly through the rings and cross the finish line", 3);
                warning.SetAlpha(0); // it'll fix itself, but for now don't show as instruction fades in
            } else if (_stage == Stage.UI)
            {
                ShowWarning("Select an upgrade", 0.2f);
                warning.SetAlpha(0);
            }

            var cur = (int)_stage + 1;
            var max = Enum.GetValues(typeof(Stage)).Length;
            progressBar.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(_pbFormat, cur, max);
            progressBar.UpdatePercentage(cur, max);

            var cg = instruction.GetComponent<CanvasGroup>();
            var pbCg = progressBar.GetComponentInParent<CanvasGroup>();
            for (float t = 0; t < instructionFadeTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cg.alpha = pbCg.alpha = Mathf.Lerp(0, 1, t / instructionFadeTime);
                if (!_warningTimer.IsFinished) warning.SetAlpha(Mathf.Lerp(0, 1, t / instructionFadeTime));
            }

            instruction.GetComponent<FlashUI>().enabled = true;
            _inTask = true;
        }

        private IEnumerator FadeInstruction(bool early=false)
        {
            var flashUI = instruction.GetComponent<FlashUI>();
            if (!flashUI.enabled) yield break; // we've already faded
            
            flashUI.enabled = false;
            var cg = instruction.GetComponent<CanvasGroup>();
            var start = cg.alpha;
            var warnStart = warning.color.a;
            var pbCg = progressBar.GetComponentInParent<CanvasGroup>();
            
            var cur = (int)_stage + (early ? 1 : 0); // already been incremented
            var max = Enum.GetValues(typeof(Stage)).Length;
            progressBar.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(_pbFormat, cur+1, max);
            for (float t = 0; t < instructionFadeTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cg.alpha = Mathf.Lerp(start, 0, t / instructionFadeTime);
                warning.SetAlpha(Mathf.Lerp(warnStart, 0, t / instructionFadeTime));

                if (t <= instructionFadeTime / 2)
                {
                    progressBar.UpdatePercentage(Mathf.Lerp(cur, cur+1, t/instructionFadeTime*2), max); // animation ✨
                }
                else
                {
                    pbCg.alpha = Mathf.Lerp(1, 0, (t-instructionFadeTime/2) / instructionFadeTime*2);
                }
            }
            instruction.SetActive(false);
            warning.gameObject.SetActive(false);
            _warningTimer.Value = 0;
        }
        private IEnumerator ShowDialogue(string text, Action setup=null)
        {
            if (playerMovement != null)
            {
                playerMovement.SetInputBlocked(true);
                playerMovement.autoPilot = true;
            }

            yield return FadeInstruction();
            
            dialogueController.ShowText(text, true);
            yield return dialogueController.Open(_stage == Stage.Shop
                ? DialogueController.People.Mechanic
                : DialogueController.People.General, 1.5f);
            setup?.Invoke();
            
            dialogueController.Continue += Continue;
        }

        private void ShowWarning(string text, float waitTime)
        {
            _warningTimer.Value = waitTime + warningFadeTime;
            warning.text = text;
        }

        private IEnumerator FadeIn(Action then=null)
        {
            fadeOut.gameObject.SetActive(true);
            fadeOut.SetAlpha(1);
            for (float t = 0; t < fadeOutTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                fadeOut.SetAlpha(1 - (t / fadeOutTime));
            }

            fadeOut.gameObject.SetActive(false);
            then?.Invoke();
        }
        
        private IEnumerator FadeOut(Action then=null)
        {
            fadeOut.gameObject.SetActive(true);
            fadeOut.SetAlpha(0);
            for (float t = 0; t < fadeOutTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                fadeOut.SetAlpha(t / fadeOutTime);
            }

            fadeOut.gameObject.SetActive(false);
            then?.Invoke();
        }

        public void ExitTutorial()
        {
            PlayerDataInstance.IsTutorial = false;
            Destroy(canvas);
            Destroy(gameObject);
        }
    }
}
