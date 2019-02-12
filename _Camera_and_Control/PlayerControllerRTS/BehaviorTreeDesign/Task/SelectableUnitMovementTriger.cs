using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("George's Script")]
    [TaskDescription("Selectable Unit mouse right click tirger for next movement task")]
    public class SelectableUnitMovementTriger : Action
    {

        protected GeorgeScript.Selectable_Unit_Controller selectUnit;
        protected bool isNewOrder = false;
        public override void OnStart()
        {
            selectUnit = GetComponent<GeorgeScript.Selectable_Unit_Controller>();
            selectUnit.newOrder += IsNewOrder;
        }
        public override TaskStatus OnUpdate()
        {
            if (selectUnit != null)
            {
                if (isNewOrder)
                {
                    isNewOrder = false;
                    return TaskStatus.Success;
                }
                else return TaskStatus.Running;
            }
            else
            {
                Debug.LogWarning("No Selectable_Unit_Controller script attached but try to access in behavior manager");
                return TaskStatus.Failure;
            }
        }
        public override void OnBehaviorComplete()
        {
            // unsubscribe after this behavior is finish or destroy
            // use null detect is because no matter this task is start or not, it execute this code when behavior tree comlete or destory
            unsubscribe_event();
        }
        public override void OnEnd()
        {
            // unsubscribe because OnStart will call every time after this task successed
            unsubscribe_event();
        }

        protected void IsNewOrder()
        {
            isNewOrder = true;
        }

        protected void unsubscribe_event()
        {
            if (selectUnit != null) selectUnit.newOrder -= IsNewOrder;
        }
    }
}