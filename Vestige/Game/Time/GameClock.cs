using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Vestige.Game.Time
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
        private bool _dayTime;
        /// <summary>
        /// Should only be called at the start of the game
        /// </summary>
        /// <param name="time"></param><param name="totalDayCycleTime">The total time in a game day in seconds</param>
        public void SetGameClock(int currentTime, int totalDayCycleTime)
        {
            //TODO: possibly create an actual gradient array. Memory over performance
            _gameTime = currentTime;
            TotalDayCycleTime = totalDayCycleTime;
            _timeToLightGradient = [
                (0, 40),
                ((totalDayCycleTime/4) - (totalDayCycleTime/16), 40),
                ((totalDayCycleTime/4) + (totalDayCycleTime/16), 255),

                ((totalDayCycleTime/2) + (totalDayCycleTime/4) - (totalDayCycleTime/16), 255),
                ((totalDayCycleTime/2) + (totalDayCycleTime/4) + (totalDayCycleTime/16), 40),
                (totalDayCycleTime, 40)
            ];
        }

        public void Update(double delta)
        {
            _gameTime += delta;
            _gameTime = _gameTime % TotalDayCycleTime;
            for (int i = 0; i < _timeToLightGradient.Count; i++)
            {
                (int x1, byte y1) = _timeToLightGradient[i];
                (int x2, byte y2) = _timeToLightGradient[(i + 1) % _timeToLightGradient.Count];

                if (_gameTime >= x1 && _gameTime <= x2)
                {
                    GlobalLight = (byte)MathHelper.Lerp(y1, y2, (float)(_gameTime - x1) / (x2 - x1));
                    _dayTime = x1 > TotalDayCycleTime / 4 || x2 < (TotalDayCycleTime / 2) + (TotalDayCycleTime / 4);
                    break;
                }
            }
        }
        public double GetGameTime()
        {
            return _gameTime;
        }
        public double GetCycleTime()
        {
            return (_gameTime + (TotalDayCycleTime / 4)) % (TotalDayCycleTime / 2);
        }
        public bool DayTime()
        {
            return _dayTime;
        }
    }
}
