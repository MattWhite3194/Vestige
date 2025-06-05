using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Menus
{
    internal class InGameOptionsMenu : PanelContainer
    {
        private GridContainer _optionsGrid;
        private Button _saveAndQuitButton;
        public InGameOptionsMenu(GraphicsDevice graphicsDevice) : base(new Vector2(0, 40), new Vector2(288, 150), Vestige.UIPanelColor, new Color(0, 0, 0, 255), 20, 1, 10, graphicsDevice, anchor: UI.Anchor.TopMiddle)
        { 
            _optionsGrid = new GridContainer(1, size: new Vector2(288, 150));

            _saveAndQuitButton = new Button(new Vector2(0, 140), "Save and Quit", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 288);

            _optionsGrid.AddComponentChild(_saveAndQuitButton);
            AddContainerChild(_optionsGrid);
        }
        public void AssignSaveAndQuitAction(Action saveAndQuitAction)
        {
            _saveAndQuitButton.OnButtonPress += () => saveAndQuitAction();
        }
        public override void HandleInput(InputEvent @event)
        {
            base.HandleInput(@event);
            InputManager.MarkInputAsHandled(@event);
        }
    }
}
