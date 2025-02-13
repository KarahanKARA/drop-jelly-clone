using _TheGame._Scripts.Block;
using _TheGame._Scripts.Systems;

namespace _TheGame._Scripts.References
{
    public class SystemReferences : Singleton<SystemReferences>
    {
        public RgbColorSystem rgbColorSystem;
        public BlockCreatorSystem blockCreatorSystem;
    }
}
