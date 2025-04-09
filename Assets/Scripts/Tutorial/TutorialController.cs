using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public enum Stage
    {
        mouse,
        move,
        shoot,
        minimap,
        enemy,
        finish,
    }

    public Stage stage = Stage.move;

    bool isNewStage;
    Stage _oldStage = Stage.finish;

    public GameObject wasd_w;
    public GameObject wasd_asd;

    public GameObject hbar;
    public GameObject dbar;
    public GameObject minimap;
    public GameObject enemy_no_atk, enemy;

    public float _timer = 0;
    public float _fadeIn = 0;

    TMP_Text w_img, asd_img;
    public TMP_Text generalInfo;
    int x;

    bool doProtectPlayer = true;
    GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<Spawnables.Player.PlayerDamageable>().godmode = true;
        RecuSetImage(hbar, false);
        RecuSetImage(dbar, false);
    }

    void Update()
    {
        player.GetComponent<Player.Movement>().autoPilot = doProtectPlayer && player.transform.position.sqrMagnitude > 80 * 80;

        _timer += Time.deltaTime;
        isNewStage = false;
        if (stage != _oldStage)
        {
            _oldStage = stage;
            isNewStage = true;
        }

        switch(stage)
        {
            case Stage.mouse:
                if (isNewStage)
                {
                    generalInfo.text = "Hello, welcome to VoidWatch\n";
                    x = 0;
                    _timer = -3f;
                }
                if (_timer > 0) switch (x)
                {
                        case 0:
                            generalInfo.text = "Try moving your mouse around to move the camera";
                            _timer = -3f; x += 1;
                            break;

                        case 1:
                            generalInfo.text = "If you move the mouse futher the camera will pan further and zoom out!";
                            _timer = -4f; x += 1;
                            break;

                        case 2:
                            generalInfo.text = "Otherwise the camera always keeps the nearest planet down";
                            _timer = -5f; x += 1;
                            break;

                        case 3:
                            generalInfo.text = "";
                            IncrementStage();
                            break;
                    }
                break;
            case Stage.move:
                if (isNewStage)
                {
                    wasd_w.SetActive(true);
                    wasd_asd.SetActive(true);
                    w_img = wasd_w.GetComponent<TMP_Text>();
                    asd_img = wasd_asd.GetComponent<TMP_Text>();
                    //w_img.text = "Set";
                    _fadeIn = 1;
                    x = 0;
                }
                if (_fadeIn > 0)
                {
                    _fadeIn -= Time.deltaTime;
                    w_img.alpha = 1 - _fadeIn;
                    asd_img.alpha = 1 - _fadeIn;
                }
                if (x == 0 && _timer > 0f)
                {
                    wasd_w.SetActive(!wasd_w.activeSelf);
                    _timer -= .8f;
                }
                if (x == 0 && Input.GetKeyDown("w"))
                {
                    x = 1;
                    wasd_w.SetActive(false);
                    wasd_asd.SetActive(false);
                    generalInfo.text = "Good job! You will drift in a straight line unless you \npush [w], in which case you'll move towards the mouse";
                    _timer = -3f;
                }
                if (x == 1 && _timer >= 0)
                {
                    _timer = 0;
                    RecuSetImage(dbar, true);
                    generalInfo.text = "Press [s] to brake.\nWhen you're ready, try dashing with space bar";
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        generalInfo.text = "The purple bar is your Void Energy which recharges and is used for dashes";
                        _timer = -3f;
                        x = 2;
                    }
                }
                if (x == 2 && _timer >= 0)
                {
                    IncrementStage();
                }
                break;
            case Stage.shoot:
                generalInfo.text = "Now push Left Mouse Button (LMB) to shoot towards the mouse!";
                if (Input.GetMouseButtonDown(0))
                {
                    IncrementStage();
                }

                break;
            case Stage.minimap:
                if (isNewStage)
                {
                    generalInfo.text = "Your minimap is in the top left and shows you as a triangle as well as the planet and other enemies.";
                    enemy_no_atk.SetActive(true);
                    minimap.SetActive(true);
                    _timer = -2f; x = 0;
                }
                if (x == 0 && _timer >= 0)
                {
                    generalInfo.text = "Fly over and shoot the enemy";
                    if (enemy_no_atk == null)
                    {
                        generalInfo.text = "Well done.";
                        _timer = -2f; x = 1;
                    }
                }
                if (x == 1 && _timer >= 0)
                {
                    IncrementStage();
                }
                break;
            case Stage.enemy:
                if (isNewStage)
                {
                    player.GetComponent<Spawnables.Player.PlayerDamageable>().godmode = false;
                    doProtectPlayer = false;
                    RecuSetImage(hbar, true);
                    generalInfo.text = "Out of bounds saftey and immortality disabled.\nKill the enemy to progress";
                    enemy.SetActive(true);
                    _timer = -2f; x = 0;
                }
                if (x == 0 && _timer >= 0)
                {
                    generalInfo.text = "";
                    if (enemy == null)
                    {
                        IncrementStage();
                    }
                }
                break;
            case Stage.finish:
                if (isNewStage)
                {
                    generalInfo.text = "Thank you for playing the tutorial, now tackle the full game :)\n";
                    x = 0;
                    _timer = -5f;
                }
                if (_timer > 0)
                {
                    SceneManager.LoadScene("TitleScreen");
                    //Escape Tutorial Here
                }
                break;

        }
    }

    void IncrementStage() {stage = (Stage)((int)stage + 1);}

    void RecuSetImage(GameObject obj, bool enabled)
    {
        if (obj.transform.childCount == 0)
        {
            var img = obj.GetComponent<Image>();
            if (img != null)
            {
                img.enabled = enabled;
            }
            return;
        }
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            RecuSetImage(obj.transform.GetChild(i).gameObject, enabled);
        }
    }
}
