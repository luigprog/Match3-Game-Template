using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Fires the trigger instantly.
    /// </summary>
    public class SetTrigger : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Fires the trigger instantly."
        )]
#endif
        #endregion
        [SerializeField]
        private string triggerName;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            fsm.SetTrigger(triggerName);
        }
    }
}