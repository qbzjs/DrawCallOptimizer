using UnityEngine;

namespace CodeBase.Enemy
{
    public class Aggro : MonoBehaviour
    {
        [SerializeField] private TriggerObserver _triggerObserver;
        [SerializeField] private Follow _follow;
        [SerializeField] private RotateToHero _rotateToHero;

        // private float _cooldown;

        private bool _hasAggroTarget;

        // private WaitForSeconds _switchFollowOffAfterCooldown;
        private Coroutine _aggroCoroutine;

        private void Start()
        {
            // _switchFollowOffAfterCooldown = new WaitForSeconds(_cooldown);

            _triggerObserver.TriggerEnter += TriggerEnter;
            // _triggerObserver.TriggerExit += TriggerExit;

            if (_follow != null)
            {
                _follow.Stop();
                _follow.enabled = false;
                _rotateToHero.enabled = false;
            }
        }

        // public void Construct(float cooldown) =>
        //     _cooldown = cooldown;

        private void OnDestroy()
        {
            _triggerObserver.TriggerEnter -= TriggerEnter;
            // _triggerObserver.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Collider obj)
        {
            if (_hasAggroTarget) return;

            StopAggroCoroutine();

            SwitchFollowOn();
        }

        // private void TriggerExit(Collider obj)
        // {
        //     if (!_hasAggroTarget) return;
        //
        //     _aggroCoroutine = StartCoroutine(SwitchFollowOffAfterCooldown());
        // }

        public void Construct(float radius) =>
            _triggerObserver.GetComponent<SphereCollider>().radius = radius;

        private void StopAggroCoroutine()
        {
            if (_aggroCoroutine == null) return;

            StopCoroutine(_aggroCoroutine);
        }

        // private IEnumerator SwitchFollowOffAfterCooldown()
        // {
        //     yield return _switchFollowOffAfterCooldown;
        //
        //     SwitchFollowOff();
        // }

        private void SwitchFollowOn()
        {
            if (_follow != null)
            {
                _hasAggroTarget = true;
                _follow.Move();
                _rotateToHero.enabled = true;
                _follow.enabled = true;
            }
        }

        // private void SwitchFollowOff()
        // {
        //     if (_follow != null)
        //     {
        //         // _follow.enabled = false;
        //         _follow.Stop();
        //         _hasAggroTarget = false;
        //         _rotateToHero.enabled = false;
        //     }
        // }
    }
}