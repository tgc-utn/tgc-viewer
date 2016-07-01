using TGC.Examples.Engine2D.Core;

namespace TGC.Examples.Engine2D
{
    public abstract class GameObject
    {
        public abstract void Update(float elapsedTime);

        public abstract void Render(float elapsedTime, Drawer2D drawer);
    }
}