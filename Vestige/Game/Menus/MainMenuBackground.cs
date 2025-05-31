using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.Drawables;
using Vestige.Game.UI.Containers;
using Vestige.Game.UI;

namespace Vestige.Game.Menus
{
    public class MainMenuBackground : UIContainer
    {
        private ParallaxManager parallaxManager;
        private Vector2 parallaxOffset;
        public MainMenuBackground() : base(Vector2.Zero, Vestige.NativeResolution.ToVector2(), Anchor.MiddleMiddle)
        {
            parallaxOffset = new Vector2(0, Vestige.NativeResolution.Y);
            parallaxManager = new ParallaxManager();
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(2f, 0), parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(30f, 1), parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(35f, 1), parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(40f, 1), parallaxOffset, Vestige.NativeResolution.Y + 50, -1));
        }
        public override void Update(double delta)
        {
            parallaxOffset.X += (float)delta;
            parallaxManager.Update(delta, parallaxOffset);
        }
        protected override void DrawComponents(SpriteBatch spritebatch)
        {
            parallaxManager.Draw(spritebatch, Color.White);
        }
    }
}
