using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeorgeScript
{

    public class Camera_Follow_Target : MonoBehaviour
    {
        private static Camera_Follow_Target _instance;
        public static Camera_Follow_Target Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("No Camera Follow Target assign");

                return _instance;
            }
        }

        virtual public void Awake()
        {
            _instance = this;
        }
    }
}
