using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TicTacRog.Presentation.Animation
{
    public sealed class AnimationQueue : MonoBehaviour
    {
        private readonly Queue<IAnimationEvent> _queue = new();
        private bool _isPlaying = false;
        private Coroutine _currentCoroutine;
        private IAnimationEvent _currentEvent;

        public void Enqueue(IAnimationEvent animationEvent)
        {
            _queue.Enqueue(animationEvent);
            
            if (!_isPlaying)
            {
                _currentCoroutine = StartCoroutine(PlayQueueCoroutine());
            }
        }

        public void Clear()
        {
            _queue.Clear();
            
            if (_currentEvent != null)
            {
                _currentEvent.StopAnimation();
                _currentEvent = null;
            }
            
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }
            
            _isPlaying = false;
        }

        public bool IsPlaying => _isPlaying;

        public event Action OnQueueCompleted;

        public event Action<IAnimationEvent> OnEventStarted;

        public event Action<IAnimationEvent> OnEventCompleted;

        private IEnumerator PlayQueueCoroutine()
        {
            _isPlaying = true;

            while (_queue.Count > 0)
            {
                var animEvent = _queue.Dequeue();
                _currentEvent = animEvent;
                
                OnEventStarted?.Invoke(animEvent);
                
                yield return animEvent.PlayAnimation();
                
                _currentEvent = null;
                OnEventCompleted?.Invoke(animEvent);
            }

            _isPlaying = false;
            _currentCoroutine = null;
            _currentEvent = null;
            OnQueueCompleted?.Invoke();
        }
    }
}

