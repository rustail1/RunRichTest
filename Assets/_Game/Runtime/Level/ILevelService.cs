namespace RunRich.Runtime.Level
{
    public interface ILevelService
    {
        LevelBindings CurrentLevel { get; }
        void Initialize();
        void Restart();
        void LoadNext();
    }
}
