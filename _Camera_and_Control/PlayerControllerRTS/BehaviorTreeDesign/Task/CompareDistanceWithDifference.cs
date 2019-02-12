using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("George's Script")]
    [TaskDescription(@"Calculate distance between two obj and compare with value." +
                      "If distance is smaller than value then return success.")]
    public class CompareDistanceWithDifference : Conditional
    {
        [Tooltip("The first Vector to compare, if comparing this obj it self then remain empty")]
        public SharedGameObject ObjFirst;
        [Tooltip("False use obj to compare, True use Vector3 to compare")]
        public SharedBool SelectVector3First;
        public SharedVector3 Vector3First;
        [Tooltip("The second Vector to compare to, if comparing this obj it self then remain empty")]
        public SharedGameObject ObjSecond;
        [Tooltip("False use obj to compare, True use Vector3 to compare")]
        public SharedBool SelectVector3Second;
        public SharedVector3 Vector3Second;
        [Tooltip("If using AstartPathfinding and movement task like seek is using arrive distance then put that value in")]
        public SharedFloat AstarArriveDistance = 0.2f;
        [Tooltip("If the defference between two objs is greater than this value then keep executing, other wise stop and return success.")]
        public SharedFloat DistanceDifference = 0.1f;
        [Tooltip("Enable this will ignore y difference")]
        public SharedBool DonotCompareY;

        private Vector3 firObj, secObj;
        public override void OnStart()
        {
        }

        public override TaskStatus OnUpdate()
        {
            if (SelectVector3First.Value) firObj = Vector3First.Value;
            else firObj = GetDefaultGameObject(ObjFirst.Value).transform.position;

            if (SelectVector3Second.Value) secObj = Vector3Second.Value;
            else secObj = GetDefaultGameObject(ObjSecond.Value).transform.position;

            if (DonotCompareY.Value) firObj.y = secObj.y;

            float _dis = Vector3.Distance(firObj, secObj) - AstarArriveDistance.Value;
            if (_dis <= DistanceDifference.Value)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }

    }
}