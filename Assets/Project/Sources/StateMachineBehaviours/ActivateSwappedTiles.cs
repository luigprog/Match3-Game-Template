using UnityEngine;

namespace StateMachineBehaviours
{
    public class ActivateSwappedTiles : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            InputManager.Instance.LastSwapInputInfo.tileA.Activate(InputManager.Instance.LastSwapInputInfo.tileB);
            InputManager.Instance.LastSwapInputInfo.tileB.Activate(InputManager.Instance.LastSwapInputInfo.tileA);
        }
    }
}