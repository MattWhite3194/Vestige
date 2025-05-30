using Microsoft.Xna.Framework;
using System.ComponentModel;
using TheGreen.Game.UI.Components;

namespace TheGreen.Game.UI.Containers
{
    public class GridContainer : UIContainer
    {
        private int _cols, _margin;

        public GridContainer(int cols, int margin = 5, Vector2 position = default, Vector2 size = default, Anchor anchor = Anchor.MiddleMiddle) : base(position, size, anchor: anchor)
        {
            _cols = cols;
            _margin = margin;
        }

        public override void AddComponentChild(UIComponent component)
        {
            int i = (ComponentCount + ContainerCount) % _cols;
            int j = (ComponentCount + ContainerCount) / _cols;
            component.Position = new Vector2(_margin * i + component.Size.X * i, _margin * j + component.Size.Y * j);
            Size = Vector2.Max(Size, component.Position + component.Size);
            base.AddComponentChild(component);
        }
        public override void AddContainerChild(UIContainer container)
        {
            int i = (ComponentCount + ContainerCount) % _cols;
            int j = (ComponentCount + ContainerCount) / _cols;
            container.Position = new Vector2(_margin * i + container.Size.X * i, _margin * j + container.Size.Y * j);
            Size = Vector2.Max(Size, container.Position + container.Size);
            base.AddContainerChild(container);
        }
    }
}
