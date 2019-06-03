using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2019.05.13 create this function for all script to use
public interface IPlayerControllerRPG
{
    Rigidbody TargetRigidbody { get; set; }
    float TurnTargetTowardsCameraFacing(float turnSpeed, float targetEulerAngle);
    float TurnTargetTowardsCameraFacingByTransform(float turnSpeed, float targetEulerAngle);
}
public class Methods : IPlayerControllerRPG
{
    public Rigidbody TargetRigidbody { get; set; }

    //	turn according camera facing, typical RPG control system
    public virtual float TurnTargetTowardsCameraFacing(float turnSP, float tar)
    {
        //float curYAng = TargetRigidbody.transform.eulerAngles.y;
        
        Quaternion tempQuat = Quaternion.Euler(new Vector3(0f, tar, 0f));
        Quaternion newAng = Quaternion.RotateTowards(TargetRigidbody.transform.rotation, tempQuat, turnSP * Time.deltaTime);

        // moverotation is not working in some case, just use rotation instead
        if (TargetRigidbody.transform.eulerAngles.y != newAng.eulerAngles.y)
            TargetRigidbody.rotation = newAng; //TargetRigidbody.MoveRotation(newAng);
        
        return Mathf.Abs(TargetRigidbody.transform.eulerAngles.y - newAng.eulerAngles.y);
    }

    //	same as TurnTargetTowardsCameraFacing but controll by transform turn not by rigidbody
    public virtual float TurnTargetTowardsCameraFacingByTransform(float turnSP, float tar)
    {
        Quaternion tempQuat = Quaternion.Euler(new Vector3(0f, tar, 0f));
        Quaternion newAng = Quaternion.RotateTowards(TargetRigidbody.transform.rotation, tempQuat, turnSP * Time.deltaTime);

        if (TargetRigidbody.transform.eulerAngles.y != newAng.eulerAngles.y)
            TargetRigidbody.transform.rotation = newAng;
        // return angle gap between current angle and target angle
        return Mathf.Abs(TargetRigidbody.transform.eulerAngles.y - tar);
    }
}