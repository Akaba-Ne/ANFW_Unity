using System;
using System.Collections.Generic;

namespace ANFW
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        /// <summary>
        /// 指定したイベント型を購読する。返り値を Dispose することで購読を解除できる
        /// </summary>
        /// <param name="handler">イベント受信時に呼ばれるハンドラー</param>
        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);

            return new Subscription(() => Unsubscribe<TEvent>(handler));
        }

        /// <summary>
        /// 指定したイベントを発火する
        /// </summary>
        /// <param name="eventData">発火するイベント</param>
        public static void Emit<TEvent>(TEvent eventData)
        {
            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var list)) return;

            foreach (var handler in list.ToArray())
                ((Action<TEvent>)handler)(eventData);
        }

        private static void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        private sealed class Subscription : IDisposable
        {
            private Action _onDispose;

            public Subscription(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose?.Invoke();
                _onDispose = null;
            }
        }
    }
}
