using UnityEngine;

namespace MyFirstARGame
{
    /// <summary>
    /// Provides events for user input based on <see cref="PressInputBase"/>.
    /// </summary>
    public class PressEventsProvider : PressInputBase
    {
        public delegate void PressEventHandler(Vector3 position);

        public event PressEventHandler PressBegan;
        public event PressEventHandler Pressed;
        public event PressEventHandler PressCancelled;

        protected override void OnPressBegan(Vector3 position)
        {
            base.OnPressBegan(position);

            this.PressBegan?.Invoke(position);
        }

        protected override void OnPress(Vector3 position)
        {
            base.OnPress(position);

            this.Pressed?.Invoke(position);
        }

        protected override void OnPressCancel()
        {
            base.OnPressCancel();

            this.PressCancelled?.Invoke(Vector3.zero);
        }
    }
}