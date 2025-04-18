using Microsoft.Xna.Framework;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.UI.Containers
{
    public class GridContainer : UIComponentContainer
    {
        private int _cols, _margin;

        public GridContainer(int cols, int margin = 5, Vector2 position = default, Vector2 size = default) : base(position, size)
        {
            _cols = cols;
            _margin = margin;
        }

        public override void AddUIComponent(UIComponent component)
        {
            int i = ComponentCount % _cols;
            int j = ComponentCount / _cols;
            component.Position = new Vector2(_margin * i + component.Size.X * i, _margin * j + component.Size.Y * j);
            base.AddUIComponent(component);
        }
    }
}
