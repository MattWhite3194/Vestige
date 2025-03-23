using Microsoft.Xna.Framework;
using System.Collections.Generic;
namespace TheGreen.Game.Drawables
{
    public class AnimationComponent
    {
        private List<AnimationFrames> _animations = new List<AnimationFrames>();
        private int _currentAnimation;
        /// <summary>
        /// The speed of the animation in FPS.
        /// </summary>
        private float _animationSpeed;
        public Rectangle AnimationRectangle;
        private double _elapsedAnimationTime;
        private Vector2 _size;
        private class AnimationFrames
        {
            public int StartFrame;
            public int EndFrame;
            public int CurrentFrame;

            public AnimationFrames(int startFrame, int endFrame)
            {
                StartFrame = startFrame;
                EndFrame = endFrame;
                CurrentFrame = 0;
            }
        }

        public AnimationComponent(Vector2 size, List<(int, int)> animationFrames)
        {
            _size = size;
            foreach (var animation in animationFrames)
            {
                AddAnimation(animation.Item1, animation.Item2);
            }
            SetCurrentAnimationRect(0);
        }

        public void Update(double delta)
        {
            if (_animations.Count == 0)
                return;
            _elapsedAnimationTime += delta;
            if (_animationSpeed == 0)
                return;
            if (_animations[_currentAnimation].EndFrame - _animations[_currentAnimation].StartFrame == 0)
                return;
            if (_elapsedAnimationTime >= 1.0f / _animationSpeed)
            {
                _elapsedAnimationTime = 0.0;
                _animations[_currentAnimation].CurrentFrame = (_animations[_currentAnimation].CurrentFrame + 1) % (_animations[_currentAnimation].EndFrame - _animations[_currentAnimation].StartFrame);
                SetCurrentAnimationRect(
                    _animations[_currentAnimation].CurrentFrame + _animations[_currentAnimation].StartFrame
                );
            }
        }

        private void SetCurrentAnimationRect(int frame)
        {
            AnimationRectangle = new Rectangle(0, frame * (int)_size.Y, (int)_size.X, (int)_size.Y);
        }
        private void AddAnimation(int startFrame, int endFrame)
        {
            _animations.Add(new AnimationFrames(startFrame, endFrame));
        }
        public void SetCurrentAnimation(int animation)
        {
            _currentAnimation = animation;
            SetCurrentAnimationRect(_animations[animation].CurrentFrame + _animations[_currentAnimation].StartFrame);
        }

        public void SetAnimationSpeed(float speed)
        {
            _animationSpeed = speed;
        }
    }
}
