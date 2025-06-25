using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Vestige.Game.IO
{
    public class Settings
    {
        private string _path;
        private Dictionary<string, object> _data;
        public Settings(string path)
        {
            _data = new Dictionary<string, object>();
            _path = path;
        }
        public void Load()
        {
            if (File.Exists(_path))
            {
                FileStream stream = File.OpenRead(_path);
                Dictionary<string, JsonElement> rawData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stream);
                foreach (KeyValuePair<string, JsonElement> kvp in rawData)
                {
                    JsonElement elem = kvp.Value;

                    object value = elem.ValueKind switch
                    {
                        JsonValueKind.Number when elem.TryGetInt32(out int i) => i,
                        JsonValueKind.Number when elem.TryGetSingle(out float f) => f,
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => null
                    };

                    _data[kvp.Key] = value;
                }
            }
            else
            {
                _data = new Dictionary<string, object>
                {
                    {"screen-width", GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width},
                    {"screen-height", GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height},
                    {"fullscreen", false },
                    {"ui-scale", 1.0f },
                    {"smooth-lighting", true }
                };
            }
        }
        public void Save()
        {
            string settingsJson = JsonSerializer.Serialize(_data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_path, settingsJson);
        }
        public object Get(string key)
        {
            return _data[key];
        }
        public void Set(string key, object value)
        {
            _data[key] = value;
        }
    }
}
