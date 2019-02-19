using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GeorgeScript.RTS_Centralization;

namespace GeorgeScript
{
    public class Selectable_Unit_Controller : MonoBehaviour
    {
        public event System.Action<RightClickInfoToUnits> newOrder;
        public GameObject selectionCircle;  //	use for add circle above obj after been selected	

        protected RTS_Centralization RTScenter;


        // Start is called before the first frame update
        protected virtual void Start()
        {
            RTScenter = RTS_Centralization.Instance;
            RTScenter.newOrder += ReceiveNewOrder;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            // if only been selected
            if (selectionCircle != null)
            {
                // put next target here
            }


        }

        protected virtual void OnDestroy()
        {
            if (RTScenter != null) RTScenter.newOrder -= ReceiveNewOrder;
        }

        /********************************
         * --- Functions
         ********************************/
        protected virtual void ReceiveNewOrder(RightClickInfoToUnits _info)
        {
            if (selectionCircle != null)    // if this object has been selected
                if (newOrder != null) newOrder(_info);
        }

    }
}
