using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Linq;
using System.Text;

//	0.7.8
// 2019.01.08 change animation control from bool to triger
// 2019.02.07 extract from Player_Controller_RTS_RPG_AstarPathfing_Project.cs to a only rpg player Controller
// 2019.02.15
//  add instance for fast use for other script
//  change how camera obj been asign
// 2019.05.10 add recheck instance in start()
namespace GeorgeScript
{
    public enum Player_Move_Behivior
    {
        Move_Or_Turn_Player_According_To_Camera,
        Move_Player_towards_Character_Facing,
        Move_Player_Along_World_Axis
    }

    public enum Player_Turn_Behivior
    {
        Turn_Player_by_Keyboard,
        Turn_Player_by_Mouse_Point
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Player_Controller_RPG : MonoBehaviour
    {

        //  drop-manu
        public Player_Move_Behivior PlayerMoveBehivior; //  declar serializable enum for customer inspector script to look for
        public Player_Turn_Behivior PlayerTurnBehivior;

        public GameObject Cam_Center_Point;
        protected Camera_Controller cc;

        public int Edge_Boundary = 1;   //	valuable use for detect limit movement which mouse move near screen edge, unit in pixel 
        public float Player_Normal_Speed = 1f;
        public float Player_Run_Speed = 2.5f;
        public float Player_Turnning_Speed = 180f; //180 degree per second
        public float Jump_Speed = 200f;

        public bool Move_Player_towards_Character_Facing = false;   //	WASD control forward/ backward/ left shift/ right shift
        public bool Move_Player_Along_World_Axis = false;   //	WASD control forward/ backward/ left shift/ right shift
        public bool Turn_Player_by_Keyboard = false;    //	QE control turn left/ turn right
        public bool Turn_Player_by_Mouse_Point = false; //	turn to mouse position

        public bool Move_Or_Turn_Player_According_To_Camera = false;    //	WASD control forward/ backward/ left shift or turn left by Camera behavior/ right shift or turn right by Camera behavior
        private bool characterMovingFlag = false;  //	flag is true if get input for character move
        public event Action<bool> CharacterMoveEvent;
        //protected Camera playerCam;
        private Vector3 moveCalculation;
        private Rigidbody playerRigidbody;
        private Animator anim;

        private float speed;
        private float turnSpeed;
        private float retreatDivisor = 2f; // when character go backward or sideward, he's run and walk speed will divide by this number
        protected bool camFollowFlag;

        private float newYAng;  //	y angle container, for MoTbKaCB () to calculate next character y angle depending on camera center

        private bool isRunning;
        private bool isJumpping;
        private int layerMaskFloor;
        private int layerMaskObstacles;
        private int layerMaskHeightAdjust;
        private float jumpTimer;
        private float timer = 1;

        public bool turnOnAnimating = true; // some times may want to turn off animating to avoid error message 

        private bool isThisScriptAsigned = false;
        // 2019.02.15 add for faster call from other script
        private static Player_Controller_RPG _instance;
        public static Player_Controller_RPG Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("No player controller script assign!!");

                return _instance;
            }
        }

        public virtual void Awake()
        {   // point static value to this script, works if there are only one script running in scene
            if (_instance == null)
                _instance = this;
        }
        // Use this for initialization
        public virtual void Start()
        {
            if (!isThisScriptAsigned)
            {
                isThisScriptAsigned = true;
                _instance = this;
            }
            playerRigidbody = GetComponent<Rigidbody>();
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;      //freeze rigidbody's rotation to prevent fall down to floor
                                                                                                                            //playerRigidbody.drag = Mathf.Infinity;
            playerRigidbody.angularDrag = Mathf.Infinity;   //prevant character keep turn not stop after finish rotation
            anim = GetComponent<Animator>();
            cc = Camera_Controller.Instance;
            if (Cam_Center_Point == null) Cam_Center_Point = cc.gameObject;
            //playerCam = Cam_Center_Point.GetComponent<Camera_Controller>().Cam_Obj.GetComponent<Camera>();

            newYAng = transform.eulerAngles.y;  //	initialize value in first run
            layerMaskFloor = LayerMask.GetMask("Floor");
            layerMaskObstacles = LayerMask.GetMask("Obstacles");
            layerMaskHeightAdjust = LayerMask.GetMask("HeightAdjust");
        }

        // Update is called once per physics update
        public virtual void FixedUpdate()
        {

            DEBUG();    //	Call debug functions

            float moveFBForAnimeRPG = 0f;
            float moveLRForAnimeRPG = 0f;

            //	get physics input
            float moveFB = Input.GetAxis("Vertical");
            float moveLR = Input.GetAxis("Horizontal");
            float turnLR = Input.GetAxisRaw("Rotate");
            bool mousLefButt = Input.GetMouseButton(0);
            float spaceBar = Input.GetAxis("Jump");
            bool _jump = false;

            if (spaceBar != 0f && jumpTimer <= 0)
            {
                jumpTimer = 1.067f;
                isJumpping = _jump = true;   //	jump function is not yet ready
            }
            else if (jumpTimer <= 0)
            {
                isJumpping = false;
            }

            if (jumpTimer > 0)
                jumpTimer -= Time.deltaTime;

            if (moveFB != 0f | moveLR != 0f | spaceBar != 0f)
            {
                if (!characterMovingFlag) if (CharacterMoveEvent != null) CharacterMoveEvent(characterMovingFlag = true);
            }
            else
            {
                if (characterMovingFlag) if (CharacterMoveEvent != null) CharacterMoveEvent(characterMovingFlag = false);
            }

            //when push shift button, not only increase walking speed also increase turning speed
            if (Input.GetButton("Run"))
            {
                speed = Player_Run_Speed;
                turnSpeed = Player_Turnning_Speed * 2.5f;
                isRunning = true;
            }
            else
            {
                speed = Player_Normal_Speed;
                turnSpeed = Player_Turnning_Speed;
                isRunning = false;
            }

            camFollowFlag = Cam_Center_Point.GetComponent<Camera_Controller>().followPlayerFlag;    //check if camera disconnect form player

            if (camFollowFlag)
            {   //	if camera sitll follow player, keyboard control character
                //	only call move block when player received movement command
                if (moveFB != 0f || moveLR != 0f)
                {
                    if (Move_Player_Along_World_Axis)
                    {
                        //	Move Player along World axis	
                        MPaW(moveFB, moveLR, speed, retreatDivisor);
                    }
                    else if (Move_Player_towards_Character_Facing)
                    {
                        //	Move Player towards Character Facing
                        MPtCF(moveFB, moveLR, speed, retreatDivisor);
                    }
                    moveFBForAnimeRPG = moveFB;
                    moveLRForAnimeRPG = moveLR;
                }

                if (Turn_Player_by_Mouse_Point)
                {
                    //	turning by mouse point
                    TPbMP();
                }
                else if (Turn_Player_by_Keyboard)
                {
                    //	turning by keyboard
                    if (turnLR != 0f)
                    {
                        TPbKC(turnLR, turnSpeed);
                    }
                }

                if (Move_Or_Turn_Player_According_To_Camera)
                {
                    //	move or turn by keyboard according camera behavior
                    MoTbKaCB(moveFB, moveLR, speed, turnSpeed, retreatDivisor);
                }

                if (_jump)
                {
                    JumpCharacter();
                }
            }

            if (camFollowFlag && turnOnAnimating)
            {
                Animating(moveFBForAnimeRPG, moveLRForAnimeRPG, _jump);  //	Animation management
            }
        }

        /********************************
         * --- Functions
         ********************************/
        //	add acceleration to rigidbody
        public virtual void JumpCharacter()
        {
            playerRigidbody.AddForce(transform.up * Jump_Speed, ForceMode.Acceleration);
        }

        //	move or turn by keyboard according camera behavior, typical RPG control system
        //	FB: forward/backward, LR: Left/Right, SP: Speed, RD: retreatDivisor
        public virtual void MoTbKaCB(float FB, float LR, float movSP, float turnSP, float RD)
        {
            float tarYAng = Cam_Center_Point.transform.eulerAngles.y;
            float curYAng = transform.eulerAngles.y;

            if (LR > 0f)
            {   //	if character is shiftting right
                newYAng = tarYAng + 90f;
                if (FB > 0f)
                    newYAng -= 45f;
                else if (FB < 0f)
                    newYAng += 45f;
            }
            else if (LR < 0f)
            {   //	if character is shiftting left
                newYAng = tarYAng - 90f;
                if (FB > 0f)
                    newYAng += 45;
                else if (FB < 0f)
                    newYAng -= 45f;
            }
            else if (FB > 0f)
            {   //	if character is moving forward
                newYAng = tarYAng;
            }
            else if (FB < 0f)
            {   //	if character is moving backward

                //	prenvent character and camera stuck at certain point and making camera shake badly
                float curBackAng = curYAng + 180f;  //	safety value for judgement after character move pass 0 point
                float tarBackAng = tarYAng + 180f;  //	safety value for judgement after camera center move pass 0 point
                if (curBackAng > 360f)  //	make sure value is in 360
                    curBackAng -= 360f;
                if (tarBackAng > 360f)
                    tarBackAng -= 360f;
                if (tarYAng <= curYAng)
                {
                    if (tarBackAng > curBackAng & tarYAng < curBackAng)
                        newYAng = tarYAng - 179f;   //	if character direction is at right of the camera center direction
                    else
                        newYAng = tarYAng + 179f;
                }
                else if (tarYAng > curYAng)
                {
                    if (tarBackAng <= curBackAng & tarYAng > curBackAng)
                        newYAng = tarYAng + 179f;   //	if character direction is at left of the camera center direction
                    else
                        newYAng = tarYAng - 179f;
                }
            }

            Quaternion tempQuat = Quaternion.Euler(new Vector3(0f, newYAng, 0f));
            Quaternion newAng = Quaternion.RotateTowards(transform.rotation, tempQuat, turnSP * Time.deltaTime);

            if (transform.eulerAngles.y != newAng.eulerAngles.y)
                playerRigidbody.MoveRotation(newAng);

            if (FB != 0 | LR != 0)
            {
                //move forward on charactor faceing basis on the direction of Camera
                moveCalculation = (Cam_Center_Point.transform.forward * FB) + (Cam_Center_Point.transform.right * LR);
                playerRigidbody.MovePosition(transform.position + moveCalculation.normalized * movSP * Time.deltaTime);
            }
        }

        //	Move Player towards Character Facing
        //	FB: forward/backward, LR: Left/Right, SP: Speed, RD: retreatDivisor
        public virtual void MPtCF(float FB, float LR, float SP, float RD)
        {

            //move basis on the character face on (character local axis x, y, z)
            moveCalculation = (transform.forward * FB) + (transform.right * LR);
            if (FB > 0f && LR == 0f)
            {
                playerRigidbody.MovePosition(transform.position + moveCalculation.normalized * SP * Time.deltaTime);
            }
            else
            {
                playerRigidbody.MovePosition(transform.position + moveCalculation.normalized * SP / RD * Time.deltaTime);
            }
        }

        //	Move Player along world axis x, y, z
        //	FB: forward/backward, LR: Left/Right, SP: Speed, RD: retreatDivisor
        public virtual void MPaW(float FB, float LR, float SP, float RD)
        {

            moveCalculation.Set(LR, 0f, FB);        //package keyboard value into vector3 type for later calcuation

            //judgment if character is walk or run backward or sideward, speed will dive by divier
            if (FB > 0f && LR == 0f)
            {
                moveCalculation = moveCalculation.normalized * SP * Time.deltaTime;
            }
            else
            {
                moveCalculation = moveCalculation.normalized * SP / RD * Time.deltaTime;
            }
            playerRigidbody.MovePosition(transform.position + moveCalculation);
        }

        //	Turning Player by Keyboard Control
        //	LR: Left/Right, TS: Turn Speed
        public virtual void TPbKC(float LR, float TS)
        {
            Vector3 playerToKeyboard = new Vector3(0f, LR * TS * Time.deltaTime, 0);    //Same if write: float playerToKeyboard = lr*TS*Time.deltaTime; 
            Quaternion tbkcRotation = Quaternion.Euler(playerToKeyboard);   //if use float then the code will be: Quaternion.Euler (0f, playerToKeyboard, 0f);

            playerRigidbody.MoveRotation(playerRigidbody.rotation * tbkcRotation);
        }

        //	Turning Player by Mouse Pointing
        public virtual void TPbMP()
        {/* 
            Quaternion roteTo;
            Vector3 mousHitPos;

            if (Public_Functions.Mous_Click_Get_Pos_Dir(playerCam, transform, layerMaskFloor, out mousHitPos, out roteTo))
            {
                playerRigidbody.MoveRotation(roteTo);
            }
*/
        }

        //	Animation management
        public virtual void Animating(float FB, float LR, bool JP)
        {
            // only keep execute if there are animation in animator
            if (!anim.hasBoundPlayables) return;

            bool walking = false;
            //walking = FB != 0f || LR != 0f;
            // 19.01.09 change key judgement as getkey not as getaxis is because getaxis has a very slow value reduce rate
            // and animation always collide with others like pickup
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                walking = true;


            AnimatorClipInfo[] _anime_info = anim.GetCurrentAnimatorClipInfo(0);

            if (isJumpping)
            {
                walking = false;
                isRunning = false;
            }

            // 19.01.08 set timer reduce animtion check frequency to avoid multiple triger animtion in short time
            timer = timer - Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0.35f;
                // 2019.01.08 change to bool to triger
                // current animation
                if (walking && isRunning && _anime_info[0].clip.name != "standing_run_forward_inPlace") anim.SetTrigger("IsRunning");
                else if (walking && !isRunning && _anime_info[0].clip.name != "standing_walk_forward_inPlace") anim.SetTrigger("IsWalking");

                if (!walking && !isRunning && _anime_info[0].clip.name == "standing_run_forward_inPlace") anim.SetTrigger("IsNotRunning");
                if (!walking && !isRunning && _anime_info[0].clip.name == "standing_walk_forward_inPlace") anim.SetTrigger("IsNotWalking");
            }
            //if (walking) anim.Play("walking");
            /*
            if (_walkStatus != walking)
            {
                _walkStatus = walking;
                if (walking)anim.SetTrigger ("IsWalking");
                else anim.SetTrigger ("IsNotWalking");
            }

            if (_runStatus != isRunning)
            {
                _runStatus = isRunning;
                if (isRunning)anim.SetTrigger ("IsRunning");
                else anim.SetTrigger ("IsNotRunning");
            }
            */
            anim.SetBool("IsJumpping", JP);
        }

        //	force camera center stop follow player
        public virtual void RELEASE()
        {
            Cam_Center_Point.GetComponent<Camera_Controller>().followPlayerFlag = false;
        }

        public virtual void DEBUG()
        {

            //debug try disconnect camera follow
            if (Input.GetKey(KeyCode.T))
            {
                RELEASE();
            }
        }
    }

    //		Debug.Log ("GetButtonDown:  " + Input.GetButtonDown ("Run"));
}