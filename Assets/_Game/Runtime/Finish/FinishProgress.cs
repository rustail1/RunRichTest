using System;

namespace RunRich.Runtime.Finish
{
    public sealed class FinishProgress
    {
        public event Action<int> Changed;

        public int Multiplier { get; private set; } = 1;

        public void Reach(int multiplier)
        {
            if (multiplier <= Multiplier)
            {
                return;
            }

            Multiplier = multiplier;
            Changed?.Invoke(Multiplier);
        }
    }
}
