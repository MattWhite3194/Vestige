using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace TheGreen.Game.IO
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
        private Point[] _resolutions = [
                new Point(960, 640),
                new Point(1024, 768),
                new Point(1136, 640),
                new Point(1280, 720),
                new Point(1280, 800),
                new Point(1366, 768),
                new Point(1440, 900),
                new Point(1600, 900),
                new Point(1680, 1050),
                new Point(1920, 1080),
                new Point(1920, 1200),
                new Point(2048, 1280),
                new Point(2160, 1350),
                new Point(2560, 1440),
                new Point(2560, 1600),
                new Point(2880, 1800),
                new Point(3008, 1692),
                new Point(3072, 1920),
                new Point(3200, 1800),
                new Point(3840, 2160),
                ];
        public void load()
        {
            if (File.Exists(_path))
            {
                FileStream stream = File.OpenRead(_path);
                Dictionary<string, JsonElement> rawData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(stream);
                foreach (var kvp in rawData)
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
                    {"fullscreen", false }
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
        public Point GetNextResolution()
        {
            Point currentResolution = TheGreen.ScreenResolution;
            int closestResolutionIndex = 0;
            int minDiff = _resolutions[0].X + _resolutions[0].Y;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                int resolutionDiff = Math.Abs((_resolutions[i].X + _resolutions[i].Y) - (currentResolution.X + currentResolution.Y));
                if (resolutionDiff < minDiff)
                {
                    minDiff = resolutionDiff;
                    closestResolutionIndex = i;
                }
                if (_resolutions[i].X == currentResolution.X && _resolutions[i].Y == currentResolution.Y)
                {
                    Debug.WriteLine("here");
                    return _resolutions[(i + 1) % _resolutions.Length];
                }
            }
            return _resolutions[closestResolutionIndex];
        }
    }
}
