using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*  vr:
 *  - 0.0.1
 *  two event from Camera_Controller
 */

public interface ILowLevelCameraController
{
    //event Action<bool> RPG_Mode;
    //event Action<bool> RTS_Mode;

    event Action<Camera_Controller_Mode> Camera_Mode_Change_Event;
}
