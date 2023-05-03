using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Action that calls the input processing logic to identify and cache user input.
    /// Fires the swapInputHappenedTrigger when a swap input occurs.
    /// </summary>
    public class ProcessUserInput : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Action that calls the input processing logic to identify and cache user input.\n" +
            "Fires the swapInputHappenedTrigger when a swap input occurs."
        )]
#endif
        #endregion
        [SerializeField]
        private string swapInputHappenedTrigger;

        private Animator fsm;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            this.fsm = fsm;

            InputManager.Instance.OnSwapInput += OnSwapInput;
            InputManager.Instance.StartProcessingInput();
        }

        private void OnSwapInput(InputManager.SwapInputInfo swapInputInfo)
        {
            InputManager.Instance.OnSwapInput -= OnSwapInput;
            InputManager.Instance.StopProcessingInput();
            fsm.SetTrigger(swapInputHappenedTrigger);
        }
    }
}