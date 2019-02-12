using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  vr:
 *  - 0.1.0
 *  sent mouse left and right click as event
 *  sent mouse left click position as event
 */

public class MouseFunctions : MonoBehaviour {
    public static MouseFunctions instance;

    public event System.Action<Vector3> Left_Click_Once_Event;
    public event System.Action<Vector3> Right_Click_Once_Event;

    public event System.Action<Vector3> Left_Double_Click_Event;
    public event System.Action<Vector3> Right_Double_Click_Event;

    private bool rB_Tigger_Once_Flag;
    private bool lB_Tigger_Once_Flag;

    private byte rB_Tigger_Counter;
    private byte lB_Tigger_Counter;
    
    private float timer_r = 0f;

    private float timer_l = 0f;

    private void Awake ()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {

	}

    private void FixedUpdate()
    {
        LeftClickOnceJudge(Input.GetMouseButton(0));

        RightClickOnceJudge(Input.GetMouseButton(1));

        if (timer_r > 0) timer_r -= Time.deltaTime;
        if (timer_l > 0) timer_l -= Time.deltaTime;

    }

    private void RightClickOnceJudge (bool _r)
    {
        //	set a bool vaule and judge right mouse button
        //	only triger one cycle for each time right mouse button has been pusshed down
        //	do this is because sometimes Input.GetMouseButtonUp (1) miss value from mouse button
        if (_r && !rB_Tigger_Once_Flag)
        {
            rB_Tigger_Counter++;
            rB_Tigger_Once_Flag = true;
            Right_Click_Agent();
        }
        else if (!_r)   rB_Tigger_Once_Flag = false;
        
    }
    
    private void LeftClickOnceJudge(bool _l)
    {
        if (_l & !lB_Tigger_Once_Flag)
        {
            lB_Tigger_Counter++;
            lB_Tigger_Once_Flag = true;
            Left_Click_Agent();
        }
        else if (!_l)   lB_Tigger_Once_Flag = false;
    }
    
    private void Right_Click_Agent()
    {
        Vector3 mousHitPos;
        Quaternion roteTo;
        Public_Functions.Mous_Click_Get_Pos_Dir(Camera.main, transform, LayerMask.GetMask("Floor"), out mousHitPos, out roteTo);

        //	double click judgement
        if (timer_r <= 0f)  // if timer is stop counting then make this time first
        {
            // is a single click
            if (Right_Click_Once_Event != null)
                Right_Click_Once_Event(mousHitPos);
            rB_Tigger_Counter = 1;
            timer_r = 0.3f;
        }
        else if (timer_r > 0 && rB_Tigger_Counter >= 2) // if button has been click twice in less than 0.3s
        {
            // is a double click
            if (Right_Double_Click_Event != null)
                Right_Double_Click_Event(mousHitPos);
            rB_Tigger_Counter = 0;
        }
    }
    
    private void Left_Click_Agent()
    {
        Vector3 mousHitPos;
        Quaternion roteTo;
        Public_Functions.Mous_Click_Get_Pos_Dir(Camera.main, transform, LayerMask.GetMask("Floor"), out mousHitPos, out roteTo);

        //	double click judgement
        if (timer_l <= 0f)  // if timer is stop counting then make this time first
        {
            // is a single click
            if (Left_Click_Once_Event != null)
                Left_Click_Once_Event(mousHitPos);
            lB_Tigger_Counter = 1;
            timer_l = 0.3f;
        }
        else if (timer_l > 0 && lB_Tigger_Counter >= 2) // if button has been click twice in less than 0.3s
        {
            // is a double click
            if (Left_Double_Click_Event != null)
                Left_Double_Click_Event(mousHitPos);
            lB_Tigger_Counter = 0;
        }
    }
}
