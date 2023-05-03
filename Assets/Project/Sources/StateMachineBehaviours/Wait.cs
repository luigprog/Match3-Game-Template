using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Action that waits a given time and then fires the finishTrigger.
    /// </summary>
    public class Wait : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Action that waits a given time and then fires the finishTrigger."
        )]
#endif
        #endregion
        [SerializeField]
        private float time;

        [SerializeField]
        private string finishTrigger;

        private float timer;
        private bool finished;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            finished = false;
        }

        public override void OnStateUpdate(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(fsm, stateInfo, layerIndex);

            if (!finished)
            {
                timer += Time.deltaTime;
                if (timer > time)
                {
                    finished = true;
                    fsm.SetTrigger(finishTrigger);
                }
            }
        }
    }
}