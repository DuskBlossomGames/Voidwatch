using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tutorial;
using UnityEditor.Experimental;
using UnityEngine.SceneManagement;
using Util;

public class TutorialController : MonoBehaviour
{
    private enum Stage
    {
        Camera,
        Movement,
        Dashing,
        Race,
        Minimap,
        Targets,
        Enemy,
        Wrapup
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
                "All movement is produced using the powerful rear thrusters. The ship will always accelerate in the direction it is facing, when the <b>W key</b> is pressed.",
                "However, lack of air resistance enables pilots to cease acceleration while maintaining velocity. As such, ship orientation and velocity can be decoupled.",
                "Deceleration is achieved through the braking apparatus, generating thrust backwards. Pilots can use the <b>S key</b> to slow down.",
                "Finally, as a convenience to the pilot, the ship's camera system will always orient 'down' as towards the nearest planet. <b>Practice the accelerating and braking.</b>",
                "When you feel comfortable, continue to see what makes this a Voidhawk-class starship."
                
            }
        },
        {
            Stage.Dashing, new []
            {
                "You'll notice a new readout on your HUD. Voidhawk ships contain an onboard Void Energy eXtractor (V.E.X.). This energy can be used to bridge the boundary between this world and <i>theirs</i>.",
                "While it recharges over time, your ship only holds enough energy for about three dashes. During these jaunts into Voidspace, you are incorporeal to enemies and obstacles alike.",
                "Press the <b>SPACE key</b> to dash. <b>Try dashing now.</b>",
                "When you are ready, the simulation will load the first training exercise."
            }
        },
        {   
            Stage.Race, new[]
            {
                "A short track has been placed in orbit around this planet. <b>Fly through the rings and cross the finish line.</b>",
                "Excellent work. You may replay the track to attempt a faster time, if you wish. When you are comfortable, proceed to learn the minimap feature."
            }
        },
        {
            Stage.Minimap, new[]
            {
                "Your minimap has now been activated, in the top right of your HUD. It gives an overview of the entire planet. Enemies will appear on the map, so you can hunt them down.",
                "The safety border on the play area has also been disabled. When infiltrating a planet, you will have gotten past the "
            }
        },
        {
            Stage.Targets, new[]
            {
                ""
            }
        },
        {
            Stage.Enemy, new[]
            {
                ""
            }
        },
        {
            Stage.Wrapup, new[]
            {
                ""
            }
        }
    };
    
    public DialogueController dialogueController;
    public Movement playerMovement;
    public float minStageWaitTime;

    public GameObject dashBar;
    public GameObject minimap;
    public GameObject healthBar;

    public GameObject securityBorder;
    
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
            case Stage.Minimap:
                break;
            case Stage.Targets:
                break;
            case Stage.Enemy:
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
                case Stage.Race:
                    raceCourse.SetActive(false);
                    minimap.SetActive(true);
                    securityBorder.SetActive(false);
                    break;
                case Stage.Minimap:
                    playerMovement.SetInputBlocked(false); // enable player shoot
                    break;
            }
            
            _stage += 1;
            _textIdx = 0;
            _genFlag = false; // reset for the next stage to use
        }

        _waitingForAction = _textIdx == Text[_stage].Length - 2;
        if (_waitingForAction)
        {
            _wait.Value = minStageWaitTime;

            if (_stage == Stage.Race) raceCourse.SetActive(true);
        }
        
        dialogueController.ShowText(Text[_stage][_textIdx], !_waitingForAction);
    }
}
