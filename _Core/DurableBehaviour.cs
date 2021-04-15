using UnityEngine;

namespace Phuntasia
{
    public class DurableBehaviour : BaseBehaviour
    {
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }
    }
}