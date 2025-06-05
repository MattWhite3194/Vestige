using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.UI;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Menus.InGame
{
    internal class InGameTerminal : GridContainer
    {
        private TextBox _terminalInput;
        public EventHandler OnExitTerminal;
        private Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>()
        {
            {"summon", (string[] args) => 
            {
                if (args.Length != 3)
                    return;
                try
                {
                    int type = int.Parse(args[0]);
                    Vector2 position = new Vector2(float.Parse(args[1]), float.Parse(args[2]));
                    Main.EntityManager.CreateEnemy(type, position);
                }
                catch (FormatException ex) 
                {
                    Debug.WriteLine(ex.Message);
                }
            }},
            {"position", (string[] args) => {
                Debug.WriteLine(Main.EntityManager.GetPlayer().Position);
            }},
            {"item", (string[] args) => 
            { 
                try
                {
                    int itemID = int.Parse(args[0]);
                    int totalQuantity = 1;
                    if (args.Length > 1)
                    {
                        totalQuantity = int.Parse(args[1]);
                    }
                    do
                    {
                        Item item = Item.InstantiateItemByID(itemID);
                        int newItemQuantity = totalQuantity;
                        if (totalQuantity > item.MaxStack)
                        {
                            newItemQuantity = item.MaxStack;
                        }
                        totalQuantity -= newItemQuantity;
                        item.Quantity = newItemQuantity;
                        Main.EntityManager.GetPlayer().Inventory.AddItemToPlayerInventory(item);
                    } while (totalQuantity > 0);
                }
                catch (FormatException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }}
        };
        public InGameTerminal() : base(1, 5, Vector2.Zero, Vector2.Zero, anchor: Anchor.BottomLeft)
        {
            _terminalInput = new TextBox(Vector2.Zero, "", Vector2.Zero, -1, "Enter a command", 0, TextAlign.Left);
            AddComponentChild(_terminalInput);
            _terminalInput.OnEnterPressed += (sender, e) => FindCommand(_terminalInput.GetText());
            _terminalInput.OnEscapePressed += (sender, e) =>
            {
                _terminalInput.SetText("");
                OnExitTerminal.Invoke(this, EventArgs.Empty);
            };
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event.EventType == InputEventType.MouseButtonDown && @event.InputButton == InputButton.LeftMouse && !_terminalInput.MouseInside)
            {
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            base.HandleInput(@event);
        }
        private void FindCommand(string input)
        {
            string[] tokens = input.ToLower().Trim().Split(' ');
            if (tokens.Length == 0) 
                return;
            string commandToken = tokens[0];
            string[] args = tokens.Length > 1 ? tokens[1..] : null;
            if (_commands.TryGetValue(commandToken, out var command)) 
            {
                command.Invoke(args);
            }
            _terminalInput.SetText("");
            OnExitTerminal?.Invoke(this, EventArgs.Empty);
        }
        public void SetFocused(bool focused)
        {
            _terminalInput.SetFocused(focused);
        }
    }
}
