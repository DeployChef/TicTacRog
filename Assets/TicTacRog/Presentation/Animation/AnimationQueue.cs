using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacRog.Presentation.Animation
{
    /// <summary>
    /// Очередь анимаций для последовательного проигрывания
    /// Домен работает мгновенно, UI проигрывает события из очереди
    /// </summary>
    public sealed class AnimationQueue : MonoBehaviour
    {
        private readonly Queue<IAnimationEvent> _queue = new();
        private bool _isPlaying = false;

        /// <summary>
        /// Добавляет событие в очередь
        /// </summary>
        public void Enqueue(IAnimationEvent animationEvent)
        {
            _queue.Enqueue(animationEvent);
            
            // Если не играет, начинаем
            if (!_isPlaying)
            {
                StartCoroutine(PlayQueueCoroutine());
            }
        }

        /// <summary>
        /// Очищает очередь
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
        }

        /// <summary>
        /// Проигрывается ли сейчас анимация
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Подписка на окончание проигрывания очереди
        /// </summary>
        public event Action OnQueueCompleted;

        /// <summary>
        /// Подписка на начало проигрывания события
        /// </summary>
        public event Action<IAnimationEvent> OnEventStarted;

        /// <summary>
        /// Подписка на окончание проигрывания события
        /// </summary>
        public event Action<IAnimationEvent> OnEventCompleted;

        private IEnumerator PlayQueueCoroutine()
        {
            _isPlaying = true;

            while (_queue.Count > 0)
            {
                var animEvent = _queue.Dequeue();
                
                OnEventStarted?.Invoke(animEvent);
                
                // Проигрываем анимацию
                yield return animEvent.PlayAnimation();
                
                OnEventCompleted?.Invoke(animEvent);
            }

            _isPlaying = false;
            OnQueueCompleted?.Invoke();
        }
    }
}

