//using UnityEngine;

//public class AllLegs : MonoBehaviour
//{
//    [SerializeField] 
//    private LegData[] legs;

//    [SerializeField]
//    private float stepLength = 0.75f;


//    private void Update()
//    {
//        foreach(var legData in legs)
//        {
//            if (!legData.Leg.IsMoving &&
//                !(Vector3.Distance(legData.Leg.Position, legData.Raycast.Position) > stepLength)) continue;
//            legData.leg.MoveTo(legData.Raycast.Position);
//        }


//    }

//    public void MoveTo(Vector3 targetPosition)
//    {
//        if (movement == null)
//        {
//            movement = new Movement
//            {
//                Progress = 0,
//                FromPosition = position,
//                ToPosition = targetPosition

//            };
//        }
//        else
//        {
//            movement = new Movement
//            {
//                Progress = movement.Value.Progress,
//                FromPosition = movement.Value.FromPosition,
//                ToPosition = targetPosition

//            };
//        }
//        [SerializeField]
//        private struct LegData
//    {
//        public LegTarget leg;
//        public LegRaycast Raycast;

//    }
//}
