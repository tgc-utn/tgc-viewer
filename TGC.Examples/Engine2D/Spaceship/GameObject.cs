using TGC.Examples.Engine2D.Spaceship.Core;

namespace TGC.Examples.Engine2D.Spaceship
{
    public abstract class GameObject
    {
        public abstract void Update(float elapsedTime);

        public abstract void Render(float elapsedTime, Drawer2D drawer);
    }
}