using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("George's Script")]
    [TaskDescription("Just read target from Selectable_Unit_Controller")]
    public class SelectableUnitValueManager : Action
    {
        [Tooltip("set walk speed to this unit")]
        public SharedFloat inputUnitWalkSpeed;  // set walk speed to this unit

        [Tooltip("set run speed to this unit")]
        public SharedFloat inputUnitRunSpeed;  // set run speed to this unit

        [Tooltip("give out managed unit speed to other task")]
        public SharedFloat outputUnitSpeed;  // give out managed unit speed to other task

        [Tooltip("give out unit next movement target to other task")]
        public SharedVector3 outputMoveTarget;  // give out unit next movement target to other task

        [Tooltip("indicate whether unit running or not")]
        public SharedBool isRunning;    // indicate whether unit running or not

        protected GeorgeScript.Selectable_Unit_Controller selectUnit;
        public override void OnStart()
        {
            selectUnit = GetComponent<GeorgeScript.Selectable_Unit_Controller>();
            selectUnit.newOrder += IsNewOrder;
            outputMoveTarget.Value = transform.position;    // give current pos as initial position
        }
        public override TaskStatus OnUpdate()
        {
            if (selectUnit != null)
            {
                return TaskStatus.Running;
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
            isRunning.Value = selectUnit.isRunning;
            if (isRunning.Value)
            {
                outputUnitSpeed.Value = inputUnitRunSpeed.Value;
            }
            else outputUnitSpeed.Value = inputUnitWalkSpeed.Value;
            outputMoveTarget.Value = selectUnit.newTar;
        }
        protected void unsubscribe_event()
        {
            if (selectUnit != null) selectUnit.newOrder -= IsNewOrder;
        }
    }
}