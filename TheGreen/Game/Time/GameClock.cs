using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGreen.Game.Time
{
    public class GameClock
    {
        /// <summary>
        /// The light level of an empty tile at the current time. Does not affect stored tile light, only the final draw light
        /// </summary>
        public byte GlobalLight = 255;
        /// <summary>
        /// The time value of the current day in the world
        /// </summary>
        private double _gameTime;

        /// <summary>
        /// The total time in seconds of a game day
        /// </summary>
        public int TotalDayCycleTime;

        /// <summary>
        /// A mapping gradient of the current day time to the current Global lighting value.
        /// </summary>
        private List<(int, byte)> _timeToLightGradient;

        /// <summary>
        /// Should only be called at the start of the game
        /// </summary>
        /// <param name="time"></param><param name="totalDayCycleTime">The total time in a game day in seconds</param>
        public void StartGameClock(int currentTime, int totalDayCycleTime)
        {
            _gameTime = currentTime;
            TotalDayCycleTime = totalDayCycleTime;
            _timeToLightGradient = [
                (0, 40),
                (totalDayCycleTime/4 - totalDayCycleTime/8, 40),
                (totalDayCycleTime/4 + totalDayCycleTime/8, 255),

                (totalDayCycleTime/2 + totalDayCycleTime/4 - totalDayCycleTime/8, 255),
                (totalDayCycleTime/2 + totalDayCycleTime/4 + totalDayCycleTime/8, 40),
                (totalDayCycleTime, 40)
            ];
        }

        public void Update(double delta)
        {
            //wrap time between 0 and totalTimeInDay
            _gameTime += delta;
            _gameTime = (_gameTime + TotalDayCycleTime) % TotalDayCycleTime;
            //calculate gradient light value
            GlobalLight = (byte)Lerp(0.0, 1.0, _gameTime);
            for (int i = 0; i < _timeToLightGradient.Count; i++)
            {
                var (x1, y1) = _timeToLightGradient[i];
                var (x2, y2) = _timeToLightGradient[(i + 1) % _timeToLightGradient.Count];

                if (_gameTime >= x1 && _gameTime <= x2)
                {
                    GlobalLight = (byte)Lerp(y1, y2, (_gameTime - x1) / (x2 - x1));
                }
            }
        }
        public int GetGameTime()
        {
            return (int)_gameTime;
        }

        private double Lerp(double y1, double y2, double t)
        {
            return y1 + (y2 - y1) * t;
        }
    }
}
