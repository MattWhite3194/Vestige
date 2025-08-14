using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vestige.Game.UI;
using Vestige.Game.UI.Containers;
using Vestige.Game.WorldMap;

namespace Vestige.Game.Menus
{
    public class MiniMapMenu : UIContainer
    {
        private Map _map;
        private Rectangle _mapSourceRect;
        private Vector2 _worldSize;
        public MiniMapMenu(Map map, Vector2 position, Vector2 size, Anchor anchor = Anchor.TopRight) : base(position: position, size: size, anchor: anchor)
        {
            _map = map;
            _worldSize = new Vector2(map.MapRenderTarget.Width, map.MapRenderTarget.Height);
        }
        public override void Update(double delta)
        {
            Vector2 playerPosition = (Main.GetCameraPosition() / 16) + (Vestige.NativeResolution.ToVector2() / 32);
            Vector2 topLeft = Vector2.Clamp(playerPosition - (Size / 2), Vector2.Zero, _worldSize - (Size / 2));
            _mapSourceRect = new Rectangle(topLeft.ToPoint(), Size.ToPoint());
            base.Update(delta);
        }
        protected override void DrawComponents(SpriteBatch spriteBatch)
        {
            //Utilities.DrawFilledRectangle(spriteBatch, new Rectangle(Point.Zero, Size.ToPoint()), Color.Black);
            spriteBatch.Draw(_map.MapRenderTarget, new Rectangle(Point.Zero, Size.ToPoint()), _mapSourceRect, Color.White);
        }
    }
}
