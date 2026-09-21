using System;
using System.Collections.Generic;

namespace RunRich.Runtime.Bootstrap
{
    public sealed class GameLoop
    {
        private readonly List<ITickable> _tickables = new();

        public void Register(ITickable tickable)
        {
            if (tickable == null)
            {
                throw new ArgumentNullException(nameof(tickable));
            }

            if (!_tickables.Contains(tickable))
            {
                _tickables.Add(tickable);
            }
        }

        public void Unregister(ITickable tickable)
        {
            _tickables.Remove(tickable);
        }

        public void Tick(float deltaTime)
        {
            for (var index = 0; index < _tickables.Count; index++)
            {
                _tickables[index].Tick(deltaTime);
            }
        }
    }
}
