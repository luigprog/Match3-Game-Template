using UnityEngine;

namespace StateMachineBehaviours
{
    /// <summary>
    /// Apply the gravity effect to all tiles, and fires the finishTrigger when all tiles finished
    /// the application of the gravity effect.
    /// </summary>
    public class ApplyGravityEffectToTiles : StateMachineBehaviour
    {
        #region Inspector Description Attribute
#if UNITY_EDITOR
        [Description(
            "Apply the gravity effect to all tiles, and fires the finishTrigger when all tiles finished\n" +
            "the application of the gravity effect."
        )]
#endif
        #endregion
        [SerializeField]
        private string finishTrigger;

        private Animator fsm;

        public override void OnStateEnter(Animator fsm, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(fsm, stateInfo, layerIndex);

            this.fsm = fsm;

            TileManager.Instance.OnGravityEffectDoneForAllTiles += OnGravityEffectDoneForAllTiles;
            TileManager.Instance.ApplyGravityEffectToTiles();
        }

        private void OnGravityEffectDoneForAllTiles()
        {
            TileManager.Instance.OnGravityEffectDoneForAllTiles -= OnGravityEffectDoneForAllTiles;
            fsm.SetTrigger(finishTrigger);
        }
    }
}