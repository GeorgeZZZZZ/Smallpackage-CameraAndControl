using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeorgeScript
{
    public class Selectable_Unit_Controller : MonoBehaviour
    {
        public event System.Action newOrder;
        public Vector3 newTar;
        public bool isRunning;
        public string orderLayerName = "Floor";
        public GameObject selectionCircle;  //	use for add circle above obj after been selected	

        protected bool mousRBTiggerOnceFlag;
        protected bool mousRBTiggerTwiceFlag;
        protected bool mousRBTiggerOnce;

        protected float timer = 0f;
        protected bool timerTriger;

        protected bool isRTSMode = false;
        protected Camera_Controller camC;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            camC = Camera_Controller.Instance;
            camC.Camera_Mode_Change_Event += Cam_Mode_Change;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            //	only start pathfind when player get in to RTS mode
            if (isRTSMode)
            {
                // if only been selected
                if (selectionCircle != null)
                {
                    Detect_New_pos();
                }
            }
            else
            {
                if (selectionCircle != null)
                {
                    Destroy(selectionCircle.gameObject);
                    selectionCircle = null;
                }
            }

        }

        protected virtual void OnDestroy()
        {
            if (camC != null) camC.Camera_Mode_Change_Event -= Cam_Mode_Change;
        }

        /********************************
         * --- Functions
         ********************************/
        protected virtual void Cam_Mode_Change(Camera_Controller_Mode _ccm)
        {
            if (_ccm == Camera_Controller_Mode.RTS) isRTSMode = true;
            else isRTSMode = false;
        }
        /* 
        protected virtual void Detect_New_pos()
        {

            bool mousRightButton = Input.GetMouseButton(1);

            //	set a bool vaule and judge left mouse button
            //	only triger one cycle for each time left mouse button has been pusshed down
            //	do this is because sometimes Input.GetMouseButtonUp (0) miss value from mouse button
            if (mousRightButton & !mousRBTiggerOnceFlag)
            {
                mousRBTiggerOnce = true;
                mousRBTiggerOnceFlag = true;
            }
            else if (mousRightButton & mousRBTiggerOnceFlag)
                mousRBTiggerOnce = false;
            else if (!mousRightButton)
                mousRBTiggerOnceFlag = false;

            //	if right mouse button has been click then give mouse clcik postion to calculate path
            if (mousRBTiggerOnce)
            {
                //	double click judgement
                if (timer <= 0f)
                {
                    isRunning = false;
                    timer = 0.3f;
                }
                else if (timer > 0)
                {
                    isRunning = true;
                }

                Vector3 mousHitPos;
                Quaternion roteTo;
                if (Public_Functions.Mous_Click_Get_Pos_Dir(Camera.main, transform, LayerMask.GetMask(orderLayerName), out mousHitPos, out roteTo))
                {
                    newTar = mousHitPos;
                    if (newOrder != null) newOrder();   // sent a event to tell subscriber is a new order
                }
            }
            
        }
        */
        protected virtual void Detect_New_pos()
        {
            //	double click judgement
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (timer <= 0 && timerTriger) // insure this function only execute once
            {
                if (mousRBTiggerTwiceFlag) isRunning = true;
                else isRunning = false;
                if (newOrder != null) newOrder();   // sent a event to tell subscriber is a new order
                timerTriger = mousRBTiggerOnceFlag = mousRBTiggerTwiceFlag = false;   // reset flags
            }
            bool _mRb = Input.GetMouseButton(1);
            //	set a bool vaule and judge left mouse button
            //	only triger one cycle for each time left mouse button has been pusshed down
            //	do this is because sometimes Input.GetMouseButtonUp (0) miss value from mouse button
            if (_mRb & !mousRBTiggerOnceFlag)   // if mouse button been hit
            {
                mousRBTiggerOnce = true;
                mousRBTiggerOnceFlag = true;
                if (!timerTriger)   // if timer is not ticking
                {
                    timerTriger = true;
                    timer = 0.3f;   // start timer
                }
                else mousRBTiggerTwiceFlag = true;  // double click before timer finish
            }
            else if (!_mRb) // if mouse button has been release
                mousRBTiggerOnceFlag = false;

            //	if right mouse button has been click then give mouse clcik postion to calculate path
            if (mousRBTiggerOnce)
            {
                mousRBTiggerOnce = false;
                Vector3 mousHitPos;
                Quaternion roteTo;
                if (Public_Functions.Mous_Click_Get_Pos_Dir(Camera.main, transform, LayerMask.GetMask(orderLayerName), out mousHitPos, out roteTo))
                {
                    newTar = mousHitPos;
                }
            }
        }
    }
}
