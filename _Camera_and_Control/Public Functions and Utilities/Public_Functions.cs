using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//	0.3.0
public class Public_Functions : MonoBehaviour
{

    // Smooth Angle transform function
    public static void SMO_ANG(float curAng, float tarAng, float smoTim, out float result)
    {
        float smoothV = 0f; //calculation temp cache for Mathf.SmoothDamp

        result = Mathf.SmoothDampAngle(curAng, tarAng, ref smoothV, smoTim);
    }

    // Smooth Value transform function
    public static void SMO_VAL(float curVal, float tarVal, float smoTim, out float result)
    {
        float smoothV = 0f; //calculation temp cache for Mathf.SmoothDamp

        result = Mathf.SmoothDamp(curVal, tarVal, ref smoothV, smoTim);
    }

    //	Smooth Value and give Offset
    //	cache: curVal (current Value), target: target value, smoothDamp: smoothDamp
    //	out cacheOut: cache value output, out offset: offset result after calculation
    public static void SMO_VAL_OFFSET(float curVal, float target, float smoothDamp, out float cacheOut, out float offset)
    {
        float oldDisOff = curVal;   //	save old movement calculation cache value

        SMO_VAL(curVal, target, smoothDamp, out cacheOut);  //	smooth movemeant
        offset = (cacheOut - oldDisOff);    //	calculate offset value
    }

    public static float Clear_Angle(float angleIn)
    {
        //	take away unnecessary number, limit angle in 360
        float newAng = angleIn - (360f * Mathf.Floor(angleIn / 360f));

        if (newAng < -180f)
        {
            return (360f + newAng);
        }
        else if (newAng > 180f)
        {
            return (-360f + newAng);
        }
        else
        {
            return newAng;
        }
    }

    public static bool Mous_Click_Get_Pos_Dir(Camera cam, Transform curTrans, int maskIn, out Vector3 hitPos, out Quaternion tarRote)
    {

        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;

        if (Physics.Raycast(camRay, out floorHit, 100f, maskIn))
        {
            hitPos = floorHit.point;
            Vector3 playerToMouse = floorHit.point - curTrans.position;
            playerToMouse.y = 0f;
            tarRote = Quaternion.LookRotation(playerToMouse);

            return true;
        }
        else
        {
            hitPos = Vector3.zero;
            tarRote = Quaternion.Euler(Vector3.zero);   //	has to return 0
            return false;
        }
    }

    // this function come from
    // https://stackoverflow.com/questions/5597729/execution-of-code-just-once-in-c-sharp
    public static Action callOnlyOnce(Action action)
    {
        var context = new ContextCallOnlyOnce();
        Action ret = () =>
        {
            if (false == context.AlreadyCalled)
            {
                action();
                context.AlreadyCalled = true;
            }
        };
        return ret;
    }
}
class ContextCallOnlyOnce
{
    public bool AlreadyCalled;
}

namespace GeorgeScript
{
    public struct LookAtManuallyResult
    {
        public Quaternion QuaternionOut;
        public Vector3 EulerAngOut;
        public Quaternion LookAtRotationSlerpOut;
    }
    public interface IPublic_Functions
    {
        void LookAtManually(Vector3 _lookAtTarget, Transform _observer, out LookAtManuallyResult _result, float? _rotateDamp = null);
        float CircleAngleClamp(float _maxAng, float _minAng, float _targetAng, float _zeroPosition);
        bool NearestPosOnCircle(float _firstAng, float _secondAng, float _targetAng);
        float ArcLength_1R(float _angle);
        float Clear_Angle(float angleIn);
        void DrawDebugProjectilTail(bool enable, Vector3 CurrentPosition, float gapPerSeconds);
    }

    public class Funs : IPublic_Functions
    {
        // this function come from https://forum.unity.com/threads/writing-own-lookat.104888/
        public void LookAtManually(Vector3 _lookAtTarget, Transform _observer, out LookAtManuallyResult _result, float? _rotateDamp = null)
        {
            // if no input damp value then use distance of two objs
            if (_rotateDamp == null) _rotateDamp = Vector3.Distance(_lookAtTarget, _observer.position);
            // calculate rotate angle
            _result.QuaternionOut = Quaternion.LookRotation(_lookAtTarget - _observer.position);
            // take out euler angle
            _result.EulerAngOut = _result.QuaternionOut.eulerAngles;
            // calculate rotate behavior
            _result.LookAtRotationSlerpOut = Quaternion.Slerp(_result.QuaternionOut, _observer.rotation, _rotateDamp.Value * Time.deltaTime);
        }
        public float CircleAngleClamp(float _maxAng, float _minAng, float _targetAng, float _zeroPosition = 360)
        {
            /*
            // this function not work in negative situation 2019.05.15
            // _zeroInClamp = true means if 0(360) is in between max angle and min angle.
            if (_targetAng < 0) return 0;
            float _result = _targetAng;
            switch (_zeroInClamp)
            {
                case true:  //  zero in clamp, target move range 360 -> max, 0 -> min
                    //if (_targetAng >= 180) if (360 - _targetAng > _maxAng) _result = 360 - _maxAng;
                    //if (_targetAng < 180) if (_targetAng > _minAng) _result = _minAng;
                    if (NearestPosOnCircle(360 - _maxAng, _minAng, _targetAng))
                    {
                        Debug.Log("_targetAng  " + _targetAng);
                        if (_targetAng > 360 - _maxAng) _result = 360 - _maxAng;
                        Debug.Log("_result  " + _result);
                    }
                    else
                    {
                        if (_targetAng > _minAng) _result = _minAng;
                    }
                    break;
                case false: // zero not in clamp, target move range 180 -> max, 180 -> min
                    //if (_targetAng >= 180) if (_targetAng > _maxAng) _result = 360 - _maxAng;
                    //if (_targetAng < 180) if (_targetAng < _minAng) _result = _minAng;
                    if (NearestPosOnCircle(180 + _maxAng, 180 - _minAng, _targetAng))
                    {
                        if (_targetAng > 180 + _maxAng) _result = 180 + _maxAng;
                    }
                    else
                    {
                        if (_targetAng < 180 - _minAng) _result = 180 - _minAng;
                    }
                    break;
            }
            */
            float _result = _targetAng;
            float _max = Clear_Angle(_zeroPosition - _maxAng);
            float _min = Clear_Angle(_zeroPosition + _minAng);
            if (NearestPosOnCircle(_max, _min, _targetAng))
            {
                if (_targetAng < _max) _result = _max;
            }
            else if (_targetAng > _min) _result = _min;
            

            return _result;
        }
        public bool NearestPosOnCircle(float _firstAng, float _secondAng, float _targetAng)
        {
            // return true if target angle is near first angle, otherwise return false
            float _c = ArcLength_1R(Mathf.Abs(_firstAng - _targetAng)) - ArcLength_1R(Mathf.Abs(_secondAng - _targetAng));
            if (_c > 0) return false;
            else return true;
        }
        public float ArcLength_1R(float _angle)
        {
            // arc length = 2 * pi * r * (theta / 360)
            // this function r equal to 1
            return 2 * Mathf.PI * (_angle / 360);
        }

        public float Clear_Angle(float angleIn)
        {
            //	take away unnecessary number, limit angle between 0 ~ 360
            return angleIn - (360f * Mathf.Floor(angleIn / 360f));
        }

        // this function come from https://www.youtube.com/watch?v=LzdFuE8n4Uk
        protected bool DrawTailStart = false;
        protected float DrawTailTimer = 0f;
        protected List<Vector3> DrawTailCheckPoint = new List<Vector3>();

        // draw a object movement to trajectory in scene view for debug
        public void DrawDebugProjectilTail(bool _en, Vector3 _pos, float timeIn)
        {
            if (!_en)   // if not enable this function then reset all value and return
            {
                DrawTailStart = false;
                return;
            }
            if (!DrawTailStart)  // sign initial value at start
            {
                DrawTailCheckPoint.Clear(); // remove all postion previous stored
                DrawTailCheckPoint.Add(_pos);   // add current pos as firt pos
                DrawTailTimer = timeIn;
                DrawTailStart = true;
            }
            DrawTailTimer -= Time.deltaTime;    // every time reach the gap
            if (DrawTailTimer <= 0)
            {
                DrawTailCheckPoint.Add(_pos);   // add new check point
                DrawTailTimer = timeIn; // reset timer
            }

            for (int _i = 0; _i < DrawTailCheckPoint.Count - 1; _i++)
            {
                Debug.DrawLine(DrawTailCheckPoint[_i], DrawTailCheckPoint[_i + 1], Color.green); // draw only works for one frame, so need to be constantly call this function
            }
        }

    }


}