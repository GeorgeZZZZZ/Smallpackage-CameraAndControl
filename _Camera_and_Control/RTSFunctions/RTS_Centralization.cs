using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  centralize all rts related function to one script
namespace GeorgeScript
{
    public class RTS_Centralization : MonoBehaviour
    {
        public GameObject Select_Circle_Prefab;
        public GameObject Target_Circle_Prefab; // 2019.02.14  add this for mouse click to pick attack target
        public GameObject Nertual_Circle_Prefab;    // 2019.02.15 mouse click to pick a nertual target like resource
        protected bool someOneSeleted_single;  // 2019.02.19 if ever some selected unity have been selecte
        protected bool someOneSeleted_area;  // 2019.02.19 if ever some selected unity have been selecte
        protected bool mouseAreaSelec = false;    //	mouse area selecting flag
        protected Vector3 curMousPos;
        protected Camera_Controller cc;
        protected Camera playerCam;
        protected bool aiOn = false;
        protected Pathfinding.RichAI pathRichAI;
        protected Pathfinding.AIPath pathAIPath;
        protected Pathfinding.AILerp pathAILerp;
        protected Player_Controller_RPG playerController;
        protected GameObject playerOBJ;
        protected GameObject targetOBJ;
        public enum RC_InfoType
        {
            None
            , pos
            , enemyTar
            , resource
        }
        public struct RightClickInfoToUnits
        {
            public bool isRun;
            public RC_InfoType type;
            public Vector3 newPos;
            public GameObject newTar;
        }
        public event System.Action<RightClickInfoToUnits> newOrder;
        protected Vector3 newFloorPos; // give pos when click on the floor
        protected bool mousRBTiggerOnceFlag;
        protected bool mousRBTiggerTwiceFlag;
        protected bool mousRBTiggerOnce;
        public string RightClickOrderLayerName = "Floor";
        protected float timer = 0f;
        protected bool timerTriger;
        public bool isRunning;
        protected RC_InfoType rightClickInfoType;
        protected bool cleanOnce = false;

        // 2019.02.15 add for faster call from other script
        private static RTS_Centralization _instance;
        public static RTS_Centralization Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("No RTS_Centralization script assign!!");

                return _instance;
            }
        }

        protected virtual void Awake()
        {   // point static value to this script, works if there are only one script running in scene
            if (_instance == null)
                _instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            cc = Camera_Controller.Instance;    // camera instance
            playerController = Player_Controller_RPG.Instance;  // player instance

            if (cc == null) Debug.LogError("There is no camera controller in the scene!!");
            else playerCam = cc.Cam_Obj.GetComponent<Camera>();
            if (playerCam == null) Debug.LogError("There is no camera controller in camera controller!!");

            if (playerController == null)
            {
                Debug.LogError("There is no player controller in scene");
                return;
            }
            // read player controller script attached object
            playerOBJ = playerController.gameObject;
            // looking for ai script
            pathRichAI = playerOBJ.GetComponent<Pathfinding.RichAI>();
            pathAIPath = playerOBJ.GetComponent<Pathfinding.AIPath>();
            pathAILerp = playerOBJ.GetComponent<Pathfinding.AILerp>();
            // disable ai because in rpg mode it will conflict with the control
            aiOn = false;
            if (pathRichAI != null) pathRichAI.enabled = false;
            else if (pathAIPath != null) pathAIPath.enabled = false;
            else if (pathAILerp != null) pathAILerp.enabled = false;

            rightClickInfoType = RC_InfoType.None;
        }

        // Update is called once per frame
        void Update()
        {
            //	only start pathfind when player get in to RTS mode
            if (!cc.followPlayerFlag)
            {
                if (!cleanOnce) cleanOnce = true;
                if (!someOneSeleted_area && !someOneSeleted_single) return; // if no unit have been selected then return
                Detect_New_pos();
            }
            else
            {
                if (!cleanOnce) return; // only execute below code once after get out of RTS cam mode
                cleanOnce = false;
                //	look for all gameobjs with script
                foreach (var selectableObj in FindObjectsOfType<Selectable_Unit_Controller>())
                {
                    if (selectableObj.selectionCircle != null)
                    {
                        Destroy(selectableObj.selectionCircle.gameObject);
                        selectableObj.selectionCircle = null;
                    }
                }

                //	if targetObj still has value
                if (targetOBJ != null)
                {
                    DestoryHitableCircle();
                }
            }
        }

        public virtual void FixedUpdate()
        {
            bool mousLefButt = Input.GetMouseButton(0);

            if (!cc.followPlayerFlag)
            {
                //	if not follow then move camera center point directilly, keyboard now control camera
                // 2019.02.12 is in rts mode then turn on Astart pathfinding AI
                if (!aiOn)
                {
                    aiOn = true;
                    if (pathRichAI != null) pathRichAI.enabled = true;
                    else if (pathAIPath != null) pathAIPath.enabled = true;
                    else if (pathAILerp != null) pathAILerp.enabled = true;
                }

                // mouse select function
                RTS_Point_Selec(mousLefButt);

                RTS_Area_Selec(mousLefButt);
            }
            else if (cc.followPlayerFlag && aiOn)
            {// if return to rpg mode then run below code once
                aiOn = false;
                if (pathRichAI != null) pathRichAI.enabled = false;
                else if (pathAIPath != null) pathAIPath.enabled = false;
                else if (pathAILerp != null) pathAILerp.enabled = false;
            }

        }

        void OnGUI()
        {

            //	RTS selection function, mouse click then start draw selection area
            if (mouseAreaSelec)
            {
                // Create a rect from both mouse positions
                var rect = Utils_RTS_Draw.GetScreenRect(curMousPos, Input.mousePosition);
                Utils_RTS_Draw.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils_RTS_Draw.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        //	RTS style Point Selection
        private void RTS_Point_Selec(bool _mlb)
        {

            //	search obj which contain Selectable_Unit_Controller.cs
            if (_mlb & !mouseAreaSelec)
            {
                //	cast a ray from camera and go through mouse position
                Ray camMousRay = playerCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit selectionHit;
                bool _selected = false;

                //	cast a ray from camera and go through mouse position, read all objs with script
                foreach (var selectableObj in FindObjectsOfType<Selectable_Unit_Controller>())
                {   // if ray hit some thing
                    if (Physics.Raycast(camMousRay, out selectionHit, 50f))
                    {   // if objet contain this ray hit
                        if (selectableObj.GetComponent<Collider>().bounds.Contains(selectionHit.point))
                        {   // if projector container is empty
                            if (selectableObj.selectionCircle == null)
                            {
                                selectableObj.selectionCircle = Instantiate(Select_Circle_Prefab);
                                selectableObj.selectionCircle.transform.SetParent(selectableObj.transform, false);
                                selectableObj.selectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
                            }
                            _selected = true;
                        }
                        else
                        {
                            if (selectableObj.selectionCircle != null)
                            {
                                Destroy(selectableObj.selectionCircle.gameObject);
                                selectableObj.selectionCircle = null;
                            }
                        }
                    }
                }
                someOneSeleted_single = _selected;
            }
        }

        //	RTS style Area Selection
        private void RTS_Area_Selec(bool mousLB)
        {

            //	if press left mouse button start draw square
            if (mousLB & !mouseAreaSelec)
            {
                mouseAreaSelec = true;
                curMousPos = Input.mousePosition;
            }

            //	if release left mouse button stop draw square
            //	original detect mousLefButtUp, but some times can't get release signal
            if (!mousLB)
            {
                mouseAreaSelec = false;
            }

            //	use projector give a circle under selected unity
            if (mouseAreaSelec)
            {
                bool _selected = false;
                //	search obj which contain component Selectable_Unit_Controller.cs
                foreach (var selectableObj in FindObjectsOfType<Selectable_Unit_Controller>())
                {
                    //	call judgement function and see if obj is in selection area
                    if (IsWithinSelectionBounds(selectableObj.gameObject))
                    {
                        if (selectableObj.selectionCircle == null)
                        {
                            selectableObj.selectionCircle = Instantiate(Select_Circle_Prefab);
                            selectableObj.selectionCircle.transform.SetParent(selectableObj.transform, false);
                            selectableObj.selectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
                        }
                        _selected = true;
                    }
                }
                someOneSeleted_area = _selected;
            }
        }
        //  2019.02.15 add right mouse target judgement
        protected bool RTS_Right_Mouse_Point_Select(bool _mrb)
        {
            // if click mouse right button
            if (_mrb)
            {
                //	cast a ray from camera and go through mouse position
                Ray camMousRay = playerCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit targetHit;
                DestoryHitableCircle();
                // if ray hit some thing
                if (Physics.Raycast(camMousRay, out targetHit, 50f))
                {   // if objet contain this ray hit
                    var _obj = targetHit.collider.gameObject;
                    if (_obj == null) return false;
                    var _tar = _obj.GetComponent<Unselectable_Unit_but_Hitable>();
                    if (_tar == null) return false; // if not contain valid script then return
                    if (_obj.GetComponent<Collider>().bounds.Contains(targetHit.point))
                    {   // if projector container is empty
                        if (_tar.targetCircle == null) // if obj is a hitable target
                        {
                            _tar.targetCircle = Instantiate(Target_Circle_Prefab);
                            _tar.targetCircle.transform.SetParent(_obj.transform, false);
                            _tar.targetCircle.transform.eulerAngles = new Vector3(90, 0, 0);
                        }
                        targetOBJ = _tar.gameObject;
                        return true;
                    }
                    else
                    {// useless code
                        if (_tar.targetCircle != null)
                        {
                            Destroy(_tar.targetCircle.gameObject);
                            _tar.targetCircle = null;
                        }
                    }
                }
            }
            return false;
        }

        //	RTS selection function, judgement for selectable obj in or not in selction area from camera view angle 
        public bool IsWithinSelectionBounds(GameObject gameObject)
        {

            if (!mouseAreaSelec)
                return false;

            var cam = playerCam;
            var viewportBounds =
                Utils_RTS_Draw.GetViewportBounds(cam, curMousPos, Input.mousePosition);

            return viewportBounds.Contains(
                cam.WorldToViewportPoint(gameObject.transform.position));   //	use bounds() search if obj is in selection area
        }

        //	get current mouse position on screen
        private void MousPos(out float mousXPos, out float mousYPos)
        {
            //	current mouse position on screen in pixels
            //	0 point is at left, down of game window
            mousXPos = Input.mousePosition.x;
            mousYPos = Input.mousePosition.y;
        }
        protected virtual void Detect_New_pos()
        {
            //	double click judgement
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (timer <= 0 && timerTriger) // insure this function only execute once
            {
                if (rightClickInfoType != RC_InfoType.None)
                {// if there are some ords
                    if (mousRBTiggerTwiceFlag) isRunning = true;
                    else isRunning = false;
                    if (newOrder != null)
                    {// if there are subscribers
                        var _info = new RightClickInfoToUnits
                        {
                            isRun = isRunning
                            ,
                            type = rightClickInfoType
                            ,
                            newPos = newFloorPos
                            ,
                            newTar = targetOBJ
                        };
                        newOrder(_info);   // sent a event to tell subscriber is a new order
                    }
                }
                timerTriger = mousRBTiggerOnceFlag = mousRBTiggerTwiceFlag = false;   // reset flags
                rightClickInfoType = RC_InfoType.None;
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
                //Debug.Log("mouseHIt");
                mousRBTiggerOnce = false;
                Vector3 mousHitPos;
                Quaternion roteTo;

                if (RTS_Right_Mouse_Point_Select(_mRb))
                {// if not hit floor
                    rightClickInfoType = RC_InfoType.enemyTar;
                    //Debug.Log("---objHit");
                }
                else if (Public_Functions.Mous_Click_Get_Pos_Dir(Camera.main, transform, LayerMask.GetMask(RightClickOrderLayerName), out mousHitPos, out roteTo))
                {// if hit floor
                    newFloorPos = mousHitPos;
                    rightClickInfoType = RC_InfoType.pos;
                    //Debug.Log("--------------------floorHit");
                }
                else
                {
                    //Debug.Log("------------------------------------hitnothing");
                }
            }
        }
        protected virtual void DestoryHitableCircle()
        {
            if (targetOBJ == null) return;  // if no record then return
            var _tar = targetOBJ.GetComponent<Unselectable_Unit_but_Hitable>();

            if (_tar.targetCircle != null)
            {
                Destroy(_tar.targetCircle.gameObject);
                _tar.targetCircle = null;
            }
            targetOBJ = null;
        }

    }
}

