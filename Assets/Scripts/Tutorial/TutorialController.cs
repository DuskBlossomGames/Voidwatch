using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using LevelSelect;
using Player;
using Singletons.Static_Info;
using Spawnables.Controllers;
using Spawnables.Damage;
using TMPro;
using UnityEngine;
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
            Enemies,
            Finish
        }

        private static readonly Dictionary<Stage, string[]> Text = new()
        {
            {
                Stage.Movement, new[]
                {
                    "Welcome to the Voidwatch Academy. Here you will learn how to be a part of the galaxy's greatest fighting force. And our last hope.",
                    "Your ship accelerates towards the mouse when <b>{Accelerate}</b> is pressed. The camera also expands to follow the mouse; <b>it is recommended to keep the mouse farther out</b>.",
                    "Finally, <b>{Brake}</b> can be used to brake and slow movement. Continue, to practice the movement.",
                    "Practice Accelerating and Braking"
                }
            },
            {
                Stage.Dashing, new []
                {
                    "With <b>{Dash}</b>, your ship uses some Void Energy to briefly enter Voidspace, allowing you to pass through enemies and obstacles alike.",
                    "The purple bar on your HUD displays Void Energy. Your ship holds enough for around three dashes, and regenerates over time. Continue for your first training exercise.",
                    "Complete the Course"
                }
            },
            {
                Stage.Attacking, new []
                {
                    "Use <b>{PrimaryWeapon}</b> to attack, with your ammo displayed in a partial circle around your ship. Beware getting too close to enemies, as contact will damage both them and you.",
                    "All enemies also have an arrow pointing to them while offscreen. Continue, to practice on targets.",
                    "Destroy the Targets"
                }
            },
            {   
                Stage.Enemy, new[]
                {
                    "Your HUD displays a red health bar with blue shield on top, which automatically regenerates over time. It also displays a minimap with enemies highlighted in red.",
                    "The safety border around the play area will now be removed, allowing the orbital cannons to attack should you exit. Continue, to practice on a real enemy.",
                    "Defeat the Missiler"
                }
            },
            {
                Stage.UI, new[]
                {
                    "Every world you clear can be <b>Pilfered</b> for an upgrade, or <b>Scavenged</b> for a random quantity of scrap. Your current upgrades are visible in the pause menu.",
                    "Scrap is also recieved from enemies, and can be spent on <b>Pilfering Further</b> to see new upgrade options, or <b>Boosting</b> stats in the Shop. You have been given 500 scrap.",
                    "Select an Upgrade then Visit the Shop"
                }
            },
            {
                Stage.Enemies, new []
                {
                    "You will now practice on a group of real enemies. Good luck.",
                    "Hunt the Enemies"
                }
            },
            {
                Stage.Finish, new[]
                {
                    "Congratulations! You have completed the Voidwatch Academy. You are ready to join this elite force. Godspeed, soldier. You'll need it.",
                    ""
                }
            }
        };
    
        public DialogueController dialogueController;
        public GameObject instruction;
        public TextMeshProUGUI warning;
        public float instructionFadeTime, warningWaitTime, warningFadeTime;
        public Movement playerMovement;

        public CanvasGroup continueButton;
        public float continueFadeTime;
        public Pair<Vector2, Vector2>[] continuePositions;
        
        public GameObject scrapPrefab;
        
        public Image fadeOut;
        public float fadeOutTime;

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

        public NewUpgradeManager upgradeManager;
    
        private readonly Timer _warningTimer = new();
    
        private Camera _camera;
        private FollowPlayer _fp;
        private CustomRigidbody2D _playerRb;

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
                playerMovement.GetComponent<PlayerDamageable>().fadeOut = _instance.fadeOut.gameObject;
                
                StartCoroutine(FadeIn(() =>
                {
                    Destroy(gameObject);
                    Destroy(canvas);
                }));
                return;
            }
            _instance = this;
            
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
                _textIdx = -1;
                Continue();
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
                        ShowWarning("WARNING: keep your mouse further from the ship");
                    }
                    else if (InputManager.GetKey(KeyCode.A) || InputManager.GetKey(KeyCode.D))
                    {
                        ShowWarning("WARNING: A/D cannot strafe, move mouse to turn");
                    }
                    else if (warning.text.Contains("further")) // if it's the mouse one, make it fade immediately upon fixing
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
                        ShowWarning("WARNING: not all rings completed");
                    }
                    
                    var playerRot = Mathf.Rad2Deg * UtilFuncs.Angle(playerMovement.transform.position);
                    var found = false;
                    for (var i = 1; i < raceCourse.transform.childCount-1; i++)
                    {
                        if (playerMovement.Dodging || Mathf.Abs(Mathf.DeltaAngle(playerRot, 
                                raceCourse.transform.GetChild(i).rotation.eulerAngles.z+90)) > 5) continue;
                        
                        if (_genFloat == 0) _genFloat = Time.time;
                        else if (Time.time - _genFloat > 1.5f) ShowWarning("TIP: dash through the walls to progress");
                            
                        found = true;
                        break;
                    }
                    if (!found) _genFloat = 0;
                    
                    break;
                }
                case Stage.Attacking:
                {
                    if (_genFloat == 0) _genFloat = Time.time;
                    if (Time.time - _genFloat > 20)
                    {
                        if (!targets.GetComponentsInChildren<EnemyDamageable>().Any(d => d.Health < d.MaxHealth))
                        {
                            ShowWarning("TIP: follow the arrows at the edge of the screen");
                        }
                        else
                        {
                            _genFloat = 0;
                        }
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
                    _genFlag |= SceneManager.GetActiveScene().name == "Shop";
                    _metCondition |= _genFlag && SceneManager.GetActiveScene().name == "Tutorial";

                    if (SceneManager.GetActiveScene().name == "LevelSelect")
                    {
                        if (_genFlag && FindAnyObjectByType<MiniPlayerController>().IsTraveling) StartCoroutine(FadeInstruction());

                        if (_genFloat == 0) _genFloat = Time.time;
                        if (Time.time - _genFloat > 20) ShowWarning("TIP: click a selected planet to visit it");
                    }
                    else if (SceneManager.GetActiveScene().name == "Shop")
                    {
                        if (_genFloat == 0) _genFloat = Time.time;
                        if (Time.time - _genFloat > 20) ShowWarning("TIP: after buying upgrades, exit the shop");
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
                    if (_textIdx == 1)
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
                            Destroy(StaticInfoHolder.Instance.gameObject);
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
            
            playerMovement.SetInputBlocked(false);
            playerMovement.autoPilot = false;
            setup?.Invoke();

            var tmp = instruction.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = string.Format(_instrFormat, text);

            
            warning.gameObject.SetActive(true);
            warning.SetAlpha(0);
            instruction.SetActive(true);
            
            var img = instruction.GetComponentInChildren<Image>();
            tmp.ForceMeshUpdate();
            tmp.mesh.RecalculateBounds();
            img.rectTransform.sizeDelta = new Vector2(tmp.mesh.bounds.size.x + 40, img.rectTransform.sizeDelta.y);

            if (_stage == Stage.Dashing)
            {
                ShowWarning("Fly through the rings and cross the finish line");
                warning.SetAlpha(0); // it'll fix itself, but for now don't show as instruction fades in
            }

            var cg = instruction.GetComponent<CanvasGroup>();
            for (float t = 0; t < instructionFadeTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cg.alpha = Mathf.Lerp(0, 1, t / instructionFadeTime);
                if (_stage == Stage.Dashing) warning.SetAlpha(Mathf.Lerp(0, 1, t / instructionFadeTime));
            }

            instruction.GetComponent<FlashUI>().enabled = true;
            _inTask = true;
        }

        private IEnumerator FadeInstruction()
        {
            var flashUI = instruction.GetComponent<FlashUI>();
            if (!flashUI.enabled) yield break; // we've already faded
            
            flashUI.enabled = false;
            var cg = instruction.GetComponent<CanvasGroup>();
            var start = cg.alpha;
            var warnStart = warning.color.a;
            for (float t = 0; t < instructionFadeTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                cg.alpha = Mathf.Lerp(start, 0, t / instructionFadeTime);
                warning.SetAlpha(Mathf.Lerp(warnStart, 0, t / instructionFadeTime));
            }
            instruction.SetActive(false);
            warning.gameObject.SetActive(false);
        }
        private IEnumerator ShowDialogue(string text, Action setup=null)
        {
            playerMovement.SetInputBlocked(true);
            playerMovement.autoPilot = true;

            yield return FadeInstruction();
            
            dialogueController.ShowText(text, true);
            yield return dialogueController.Open(1.5f);
            setup?.Invoke();
            
            dialogueController.Continue += Continue;
        }

        private void ShowWarning(string text)
        {
            _warningTimer.Value = warningWaitTime + warningFadeTime;
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
