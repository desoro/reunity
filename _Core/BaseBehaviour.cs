using System;
using System.Collections;
using UnityEngine;

namespace Phuntasia
{
    public class BaseBehaviour : MonoBehaviour
    {
        bool _isQuitting;
        WaitForFixedUpdate _fixedWait = new WaitForFixedUpdate();

        public RectTransform rectTransform => transform as RectTransform;

        protected virtual void OnValidate() { }

        protected virtual void Reset() { }

        protected virtual void Awake() { }

        protected virtual void Start() { }

        protected virtual void OnEnable() { }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDisable()
        {
            if (!_isQuitting)
            {
                OnDisableSafe();
            }
        }

        protected virtual void OnDestroy() { }

        protected virtual void OnDisableSafe() { }

        public void NextFrame(Action callback)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            StartCoroutine(Routine());

            IEnumerator Routine()
            {
                yield return null;

                callback?.Invoke();
            }
        }

        public void NextFixedFrame(Action callback)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            StartCoroutine(Routine());

            IEnumerator Routine()
            {
                yield return _fixedWait;

                callback?.Invoke();
            }
        }

        public Coroutine SetTimeout(float timeout, Action callback)
        {
            if (!gameObject.activeInHierarchy)
            {
                return null;
            }

            return StartCoroutine(TimeoutRoutine());

            IEnumerator TimeoutRoutine()
            {
                yield return new WaitForSeconds(timeout);

                callback?.Invoke();
            }
        }

        public void ClearTimeout(Coroutine timeout)
        {
            if (this != null && timeout != null)
            {
                StopCoroutine(timeout);
            }
        }

        public Coroutine SetInterval(float interval, Action callback, bool immediateFirstInterval = false)
        {
            if (!gameObject.activeInHierarchy)
            {
                return null;
            }

            if (immediateFirstInterval)
            {
                callback?.Invoke();
            }

            return StartCoroutine(IntervalRoutine());

            IEnumerator IntervalRoutine()
            {
                var wait = new WaitForSeconds(interval);

                while (true)
                {
                    yield return wait;

                    callback?.Invoke();
                }
            }
        }

        public Coroutine SetFrameInterval(Action callback, bool immediateFirstInterval = false)
        {
            if (!gameObject.activeInHierarchy)
            {
                return null;
            }

            if (immediateFirstInterval)
            {
                callback?.Invoke();
            }

            return StartCoroutine(FrameIntervalRoutine());

            IEnumerator FrameIntervalRoutine()
            {
                while (true)
                {
                    yield return null;

                    callback?.Invoke();
                }
            }
        }

        public Coroutine SetFixedInterval(Action callback, bool immediateFirstInterval = false)
        {
            if (!gameObject.activeInHierarchy)
            {
                return null;
            }

            if (immediateFirstInterval)
            {
                callback?.Invoke();
            }

            return StartCoroutine(FixedIntervalRoutine());

            IEnumerator FixedIntervalRoutine()
            {
                while (true)
                {
                    yield return _fixedWait;

                    callback?.Invoke();
                }
            }
        }

        public void ClearInterval(Coroutine interval)
        {
            if (this != null && interval != null)
            {
                StopCoroutine(interval);
            }
        }
    }
}