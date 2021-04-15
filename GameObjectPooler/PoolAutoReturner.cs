using UnityEngine;

namespace Phuntasia
{
    public class PoolAutoReturner : BaseBehaviour
    {
        [SerializeField] float _duration = 0.75f;

        protected override void OnEnable()
        {
            base.OnEnable();

            SetTimeout(_duration, Despawn);
        }

        void Despawn()
        {
            PoolManager.Return(this);
        }
    }
}