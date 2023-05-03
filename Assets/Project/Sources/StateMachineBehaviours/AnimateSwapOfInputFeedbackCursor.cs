using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Animate the input feedback cursor, indicating the swap that just happened.
    /// Fires the finishTrigger.
    /// </summary>
    public class AnimateSwapOfInputFeedbackCursor : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Animate the input feedback cursor, indicating the swap that just happened.\n" +
            "Fires the finishTrigger.\n" +
            "\n" +
            "waitAnimationComplete: If true, the finishTrigger will be fired only after the animation completes, otherwise, the trigger gets fired right away."
        )]
#endif
        #endregion
        [SerializeField]
        private float animationTime;

        /// <summary>
        /// If true, the finishTrigger will be fired only after the animation completes, otherwise, the trigger gets fired right away.
        /// </summary>
        [SerializeField]
        private bool waitAnimationComplete;

        [SerializeField]
        private string finishTrigger;

        private Animator fsm;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            this.fsm = fsm;

            InputManager.Instance.AnimateCursorIndicateSwap(animationTime, AnimationOnDone);
            if (!waitAnimationComplete)
            {
                fsm.SetTrigger(finishTrigger);
            }
        }

        private void AnimationOnDone()
        {
            if (waitAnimationComplete)
            {
                fsm.SetTrigger(finishTrigger);
            }
        }
    }
}