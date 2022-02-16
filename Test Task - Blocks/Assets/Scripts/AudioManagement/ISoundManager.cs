namespace RFTestTaskBlocks
{
    public interface ISoundManager : IGameService
    {
        void PreloadSFXList(params string[] addresses);
        void PlaySFX(string soundAddress);
    }
}