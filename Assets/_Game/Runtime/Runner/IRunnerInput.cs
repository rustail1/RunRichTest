namespace RunRich.Runtime.Runner
{
    public interface IRunnerInput
    {
        bool StartRequested { get; }
        bool IsPressed { get; }
        bool PressedThisFrame { get; }

        /// <summary>Horizontal drag from PointerDown, normalized by Screen.height like the XAPK.</summary>
        float DragFromPointerDown { get; }

        /// <summary>
        /// Raw horizontal pointer movement in pixels since the previous input sample.
        /// It returns to zero while the pointer is stationary or released.
        /// </summary>
        float DeltaX { get; }
    }
}
