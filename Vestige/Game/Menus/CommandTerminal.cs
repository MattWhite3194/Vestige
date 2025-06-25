using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Vestige.Game.Entities;
using Vestige.Game.Input;
using Vestige.Game.Items;
using Vestige.Game.UI;
using Vestige.Game.UI.Components;
using Vestige.Game.UI.Containers;

namespace Vestige.Game.Menus
{
    internal class CommandTerminal : GridContainer
    {
        private TextBox _terminalInput;
        public event Action OnExitTerminal;
        private Dictionary<string, Action<string[]>> _commands;
        private Action<string> outputMessage;
        public CommandTerminal(Action<string> outputMessage) : base(1, 5, Vector2.Zero, Vector2.Zero, anchor: Anchor.BottomLeft)
        {
            _terminalInput = new TextBox(Vector2.Zero, "", Vector2.Zero, -1, "Enter a command", 0, TextAlign.Left);
            AddComponentChild(_terminalInput);
            _terminalInput.OnEnterPressed += () => FindCommand(_terminalInput.GetText());
            InitializeCommands();
            this.outputMessage = outputMessage;
        }
        public override void HandleInput(InputEvent @event)
        {
            if (@event.EventType == InputEventType.MouseButtonDown && @event.InputButton == InputButton.LeftMouse && !_terminalInput.MouseInside)
            {
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            if (@event.EventType == InputEventType.KeyDown && @event.InputButton == InputButton.Options)
            {
                _terminalInput.SetText("");
                OnExitTerminal?.Invoke();
                InputManager.MarkInputAsHandled(@event);
                return;
            }
            base.HandleInput(@event);
        }
        private void FindCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                //Do Nothing, Stinky
            }
            else if (input[0] != '/')
            {
                outputMessage?.Invoke(input);
            }
            else
            {
                string[] tokens = input.Trim().Split(' ');
                string commandToken = tokens[0].ToLower().Replace("/", "");
                string[] args = tokens.Length > 1 ? tokens[1..] : [];
                if (_commands.TryGetValue(commandToken, out Action<string[]> command))
                {
                    command.Invoke(args);
                }
            }
            _terminalInput.SetText("");
            OnExitTerminal?.Invoke();
        }
        public void SetFocused(bool focused)
        {
            _terminalInput.SetFocused(focused);
        }
        private void InitializeCommands()
        {
            _commands = new Dictionary<string, Action<string[]>>
            {
                { "summon", (args) => {
                    if (args.Length != 3)
                        return;
                    try
                    {
                        int type = int.Parse(args[0]);
                        Vector2 position = new Vector2(float.Parse(args[1]), float.Parse(args[2]));
                        Main.EntityManager.CreateEnemy(type, position);
                    }
                    catch (Exception ex)
                    {
                        outputMessage?.Invoke("Incorrect Format: summon <enemyID> <X> <Y>");
                    }
                }},

                { "position", (args) => {
                    if (args.Length != 1)
                        return;
                    Player player = Main.EntityManager.GetPlayerByName(args[0]);
                    if (player != null)
                    {
                        outputMessage?.Invoke(player.Position.ToString());
                    }
                }},
                { "item", (args) => {
                    try
                    {
                        int itemID = int.Parse(args[0]);
                        int totalQuantity = 1;
                        if (args.Length > 2)
                        {
                            totalQuantity = int.Parse(args[2]);
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
                            Player player = Main.EntityManager.GetPlayerByName(args[1]);
                            player?.Inventory.AddItemToPlayerInventory(item);
                        } while (totalQuantity > 0);
                    }
                    catch (Exception ex)
                    {
                        outputMessage?.Invoke("Incorrect format: item <id> <playerName> --quantity");
                    }
                }}
            };
        }
    }
}
