using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using Vestige.Game.Input;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;
using Vestige.Game.WorldMap;

namespace Vestige.Game.Menus
{
    internal class MapMenu : UIContainer
    {
        private Button _zoomInButton;
        private Button _zoomOutButton;
        private float _defaultZoom;
        private float _userZoom = 1.0f;
        private float _maxZoom;
        private Vector2 _offset = Vector2.Zero;
        private Vector2 _grabPosition;
        private bool _mouseDown = false;
        private Vector2 _mapPosition;
        private Vector2 _mapPositionOnGrab;
        private Vector2 _mapOrigin;
        private Map _map;
        public MapMenu(Map map)
        {
            _map = map;
            _defaultZoom = Vestige.NativeResolution.X / (float)map.MapRenderTarget.Width;
            _maxZoom = 2.0f / _defaultZoom;
            _maxZoom = (float)Math.Round(_maxZoom, 1);
            _zoomInButton = new Button(Vector2.Zero, "Zoom In", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            _zoomOutButton = new Button(Vector2.Zero, "Zoom Out", Vector2.Zero, color: Color.White, clickedColor: Vestige.SelectedTextColor, hoveredColor: Vestige.HighlightedTextColor, maxWidth: 60);
            _offset = Vector2.Zero;
            _mapPosition = Vector2.Zero;
            _mapOrigin = new Vector2(map.MapRenderTarget.Width / 2, map.MapRenderTarget.Height / 2);
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonDown)
            {
                _mouseDown = true;
                _grabPosition = InputManager.GetMouseWindowPosition();
                _mapPositionOnGrab = _mapPosition;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.LeftMouse && @event.EventType == InputEventType.MouseButtonUp)
            {
                _mouseDown = false;
                InputManager.MarkInputAsHandled(@event);
            }
            else if (@event.InputButton == InputButton.MiddleMouse)
            {
                if (@event.EventType == InputEventType.MouseButtonDown)
                {
                    _userZoom = Math.Min(_userZoom + 0.05f * _userZoom, _maxZoom);
                }
                else
                {
                    _userZoom = Math.Max(_userZoom - 0.05f * _userZoom, 0.5f);
                }
            }
            base.HandleInput( @event );
        }
        public override void Update(double delta)
        {
            if (_mouseDown)
            {
                _offset = InputManager.GetMouseWindowPosition() - _grabPosition;
                _mapPosition = (_mapPositionOnGrab + _offset / (_userZoom * _defaultZoom));
            }
            float mapWidth = _map.MapRenderTarget.Width * (_userZoom * _defaultZoom) * (parentSize.X / Vestige.NativeResolution.X);
            if (mapWidth > parentSize.X)
            {
                _mapPosition = Vector2.Clamp(_mapPosition, new Vector2(-(mapWidth - parentSize.X) / 2.0f / (_userZoom * _defaultZoom), _mapPosition.Y), new Vector2((mapWidth - parentSize.X) / 2.0f / (_userZoom * _defaultZoom), _mapPosition.Y));
            }
            else
            {
                _mapPosition = Vector2.Clamp(_mapPosition, new Vector2((-parentSize.X + mapWidth) / 2.0f / (_userZoom * _defaultZoom), _mapPosition.Y), new Vector2((parentSize.X - mapWidth) / 2.0f / (_userZoom * _defaultZoom), _mapPosition.Y));
            }
            float mapHeight = _map.MapRenderTarget.Height * (_userZoom * _defaultZoom) * (parentSize.X / Vestige.NativeResolution.X);
            if (mapHeight > parentSize.Y)
            {
                _mapPosition = Vector2.Clamp(_mapPosition, new Vector2(_mapPosition.X, -(mapHeight - parentSize.Y) / 2.0f / (_userZoom * _defaultZoom)), new Vector2(_mapPosition.X, (mapHeight - parentSize.Y) / 2.0f / (_userZoom * _defaultZoom)));
            }
            else
            {
                _mapPosition = Vector2.Clamp(_mapPosition, new Vector2(_mapPosition.X, (-parentSize.Y + mapHeight) / 2.0f / (_userZoom * _defaultZoom)), new Vector2(_mapPosition.X, (parentSize.Y - mapHeight) / 2.0f / (_userZoom * _defaultZoom)));
            }
        }
        public override void Draw(SpriteBatch spriteBatch, RasterizerState rasterizerState = null)
        {
            //Draw this without an anchor matrix
            spriteBatch.Begin();
            Utilities.DrawFilledRectangle(spriteBatch, new Rectangle(0, 0, (int)parentSize.X, (int)parentSize.Y), Color.DarkBlue);
            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(parentSize.X / Vestige.NativeResolution.X) * Matrix.CreateTranslation(new Vector3(parentSize / 2.0f + _mapPosition * (_userZoom * _defaultZoom), 0)));
            spriteBatch.Draw(_map.MapRenderTarget, Vector2.Zero, null, Color.White, 0.0f, _mapOrigin, _userZoom * _defaultZoom, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }
    }
}
