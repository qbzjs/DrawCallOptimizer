using System;
using CodeBase.Services.Input.Types;
using UnityEngine;

namespace CodeBase.Services.Input.Platforms
{
    public class MobilePlatformInputService : PlatformInputService
    {
        private readonly TouchScreenInputType _touchScreenInputType;

        public override event Action<Vector2> Moved;
        public override event Action Shot;
        public override event Action ChoseWeapon1;
        public override event Action ChoseWeapon2;
        public override event Action ChoseWeapon3;
        public override event Action ChoseWeapon4;

        public MobilePlatformInputService(TouchScreenInputType touchScreenInputType)
        {
            _touchScreenInputType = touchScreenInputType;

            SubscribeEvents();
        }

        protected override void SubscribeEvents()
        {
        }

        protected override void UnsubscribeEvents()
        {
        }

        protected override void MoveTo(Vector2 direction) =>
            Moved?.Invoke(direction);

        protected override void ShootTo() =>
            Shot?.Invoke();
    }
}