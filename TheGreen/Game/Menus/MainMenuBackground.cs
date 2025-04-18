using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Drawables;
using TheGreen.Game.UI.Containers;
using TheGreen.Game.UIComponents;

namespace TheGreen.Game.Menus
{
    public class MainMenuBackground : UIComponentContainer
    {
        private ParallaxManager parallaxManager;
        private Vector2 parallaxOffset;
        public MainMenuBackground()
        {
            this.Anchor = Anchor.ScreenScale;
            parallaxOffset = new Vector2(0, TheGreen.NativeResolution.Y);
            parallaxManager = new ParallaxManager();
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.MountainsBackground, new Vector2(2f, 0), parallaxOffset, TheGreen.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFarthestBackground, new Vector2(30f, 1), parallaxOffset, TheGreen.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesFartherBackground, new Vector2(35f, 1), parallaxOffset, TheGreen.NativeResolution.Y + 50, -1));
            parallaxManager.AddParallaxBackground(new ParallaxBackground(ContentLoader.TreesBackground, new Vector2(40f, 1), parallaxOffset, TheGreen.NativeResolution.Y + 50, -1));
        }
        public override void Update(double delta)
        {
            parallaxOffset.X += (float)delta;
            parallaxManager.Update(delta, parallaxOffset);
        }
        public override void Draw(SpriteBatch spritebatch)
        {
            parallaxManager.Draw(spritebatch, Color.White);
        }
    }
}
