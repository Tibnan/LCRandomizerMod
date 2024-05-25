namespace LCRandomizerMod
{
    public interface ICustomValue
    {
        void ReloadStats();

        void SyncStatsWithClients();

        void SaveOnExit();
    }
}
