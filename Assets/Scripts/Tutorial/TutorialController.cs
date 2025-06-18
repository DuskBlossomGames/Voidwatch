using System.Collections;
using System.Collections.Generic;
using Extensions;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

namespace Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        private enum Stage
        {
            Camera,
            Movement,
            Dashing,
            Race,
            Shooting,
            Targets,
            Enemy,
        }

        private static readonly Dictionary<Stage, string[]> Text = new()
        {
            {
                Stage.Camera, new[]
                {
                    "Welcome to the Voidwatch Academy. Here you will learn how to be a part of the galaxy's greatest fighting force. And our last hope.",
                    "The latest Voidhawk Infiltrator starship is equipped with state-of-the-art scouting technology. <b>Move your mouse around to see further.</b>",
                    "Great work. When you're ready, the simulation will enable the propulsion system."
                }
            },
            {
                Stage.Movement, new[]
                {
                    "Infiltrators have a unique thruster design to maximize maneuverability. The lack of horizontal propulsion can take some practice, however.",
                    "The ship will always accelerate in the direction it is facing, when the <b>W key</b> is pressed. It is recommended to keep your mouse some distance from the ship.",
                    "Lack of air resistance enables pilots to cease acceleration while maintaining velocity. As such, ship orientation and velocity can be decoupled.",
                    "Deceleration is achieved through the braking apparatus, generating thrust backwards. Pilots can use the <b>S key</b> to slow down.",
                    "Finally, as a convenience to the pilot, the ship's camera system will always orient 'down' as towards the nearest planet. <b>Practice accelerating and braking.</b>",
                    "When you feel comfortable, continue to see what makes this a Voidhawk-class starship."
                
                }
            },
            {
                Stage.Dashing, new []
                {
                    "Voidhawk ships contain an onboard Void Energy eXtractor (V.E.X.) device. This energy can be used to bridge the boundary between this world and <i>theirs</i>.",
                    "While it recharges over time, your ship only holds enough energy for about three dashes. During these jaunts into Voidspace, you are incorporeal to enemies and obstacles alike.",
                    "When exiting a dash, the Infiltrator will immediately resume velocity in the direction it is facing, allowing for instant pivoting. Press the <b>SPACE key</b> to dash. <b>Try dashing now.</b>",
                    "When you are ready, the simulation will load the first training exercise."
                }
            },
            {   
                Stage.Race, new[]
                {
                    "A short track has been placed in orbit around this planet. <b>Fly through the rings and cross the finish line.</b>",
                    "Excellent work. You may replay the track to attempt a faster time, if you wish. When you are comfortable, proceed to learn your weapons system."
                }
            },
            {
                Stage.Shooting, new[]
                {
                    "Your starship converts electrical energy into concentrated packets of destruction. Your HUD displays your energy stores in a partial circle around your ship when you shoot.",
                    "The energy bullets cannot be replenished while you are shooting, and if you fully deplete your stores, you cannot shoot until the ship recharges fully.",
                    "Also be aware that the planet's gravity will not affect your bullets. This will cause apparent curvature if you are flying around the planet, as skilled pilots must take into account.",
                    "Practice shooting now, and reloading without fully draining it. Then, <b>empty your clip and allow it to refill.</b>",
                    "Next, it is time to learn your minimap and practice on some targets."
                }
            },
            {
                Stage.Targets, new[]
                {
                    "Your minimap has now been activated in the top right of your HUD. It gives an overview of the entire planet. Enemies will appear on the map so you can hunt them down.",
                    "The safety border on the play area has also been disabled. When infiltrating a planet, the orbital defense cannons will prevent you from leaving until the planet has been retaken.",
                    "Also of note is collisions. Ramming enemy ships will naturally damage both vessels. However, should you destroy the enemy, your shielding capabilities will protect you from damage.",
                    "With two new sources of damage, your health bar has also been enabled. The blue bar represents your shield, which will regenerate automatically if you don't take damage.",
                    "Note you cannot repair your ship without docking at a space station. In this simulation, you will be respawned should you perish. Out there... be careful.",
                    "The ship's camera system places indicators at the edge of your screen pointing towards enemies. Targets have been spawned around the system. <b>Hunt them.</b>",
                    "When you're ready, continue to practice against real enemies."
                }
            },
            {
                Stage.Enemy, new[]
                {
                    "A few enemies have been spawned. These are exact replicas of Cult of the Void vessels. Beware that you will also encounter other, more... fleshy opponents. <b>Eliminate the enemies.</b>",
                    "You have completed the Voidwatch Academy. You are ready to join this elite force. Godspeed, soldier. You'll need it."
                }
            }
        };
    
        public DialogueController dialogueController;
        public Movement playerMovement;
        public PlayerGunHandler playerGun;
        public float minStageWaitTime;

        public Image fadeOut;
        public float fadeOutTime;
    
        public GameObject dashBar;
        public GameObject minimap;
        public GameObject healthBar;

        public GameObject securityBorder;

        public GameObject targets;

        public GameObject enemies;
    
        public GameObject raceCourse;
        private RaceController _raceController;
    
        private readonly Timer _wait = new();
    
        private Camera _camera;
        private FollowPlayer _fp;
        private CustomRigidbody2D _playerRb;

        private Stage _stage = Stage.Camera;
        private int _textIdx = -1;
        private bool _waitingForAction;

        private bool _genFlag;

        private void Start()
        {
            _camera = Camera.main;
            _fp = _camera!.GetComponent<FollowPlayer>();
        
            _playerRb = playerMovement.GetComponent<CustomRigidbody2D>();
        
            _raceController = raceCourse.GetComponentInChildren<RaceController>();
        
            dialogueController.Continue += Continue;
            playerMovement.SetInputBlocked(true);
        
            Continue();
        }

        private void Update()
        {
            _wait.Update();
            if (!_wait.IsFinished || !_waitingForAction) return;

            switch (_stage)
            {
                case Stage.Camera:
                {
                    if (_camera.orthographicSize > _fp.baseSize + _fp.mouseZoomScale * 2 / 3f) Continue();
                    break;
                }
                case Stage.Movement:
                    _genFlag |= _playerRb.velocity.sqrMagnitude > 400; // greater than 20 u/s
                    if (_genFlag && _playerRb.velocity.sqrMagnitude < 25) Continue(); // less than 5 u/s
                    break;
                case Stage.Dashing:
                    _genFlag |= playerMovement.Dodging;
                    if (_genFlag && !playerMovement.Dodging) Continue();
                    break;
                case Stage.Race:
                    if (_raceController.Completed) Continue();
                    break;
                case Stage.Shooting:
                    _genFlag |= playerGun.EmptyRefilling;
                    if (_genFlag && !playerGun.EmptyRefilling) Continue();
                    break;
                case Stage.Targets:
                    if (targets.transform.childCount == 0) Continue();
                    break;
                case Stage.Enemy:
                    if (enemies.transform.childCount == 0) Continue();
                    break;
            }
        }

        private void Continue()
        {
            _textIdx += 1;
            if (_textIdx >= Text[_stage].Length)
            {
                switch (_stage)
                {
                    case Stage.Camera:
                        playerMovement.inputBlocked = false; // only blocked for camera stage (but shoot stays blocked)
                        break;
                    case Stage.Movement:
                        dashBar.SetActive(true);
                        break;
                    case Stage.Dashing:
                        raceCourse.SetActive(true);
                        break;
                    case Stage.Race:
                        raceCourse.SetActive(false);
                        playerMovement.SetInputBlocked(false); // enable player shoot
                        break;
                    case Stage.Shooting:
                        minimap.SetActive(true);
                        securityBorder.SetActive(false);
                        healthBar.SetActive(true);
                        break;
                    case Stage.Targets:
                        enemies.SetActive(true);
                        break;
                    case Stage.Enemy:
                        StartCoroutine(FadeOut());
                        return;
                }
            
                _stage += 1;
                _textIdx = 0;
                _genFlag = false; // reset for the next stage to use
            }

            _waitingForAction = _textIdx == Text[_stage].Length - 2;
            if (_waitingForAction)
            {
                _wait.Value = minStageWaitTime;
                if (_stage == Stage.Targets) targets.SetActive(true);
            }
        
            dialogueController.ShowText(Text[_stage][_textIdx], !_waitingForAction);
        }

        private IEnumerator FadeOut()
        {
            fadeOut.gameObject.SetActive(true);
            fadeOut.SetAlpha(0);
            for (float t = 0; t < fadeOutTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                fadeOut.SetAlpha(t / fadeOutTime);
            }

            SceneManager.LoadScene("TitleScreen");
        }
    }
}
