﻿using UnityEngine;

namespace CodeBase.Projectiles.Movement
{
    public class BulletMovement : ProjectileMovement
    {
        private float _speed;
        private Vector3 _target;

        private void Update()
        {
            if (IsMove)
                transform.position += transform.forward * _speed * Time.deltaTime;
        }

        public void Construct(float speed, Transform parent, float lifeTime)
        {
            _speed = speed * 1f;
            base.Construct(parent, lifeTime);
        }

        public void SetTargetPosition(Vector3 target)
        {
            _target = target;
            transform.LookAt(_target);
        }

        public override void Launch()
        {
            StartCoroutine(LaunchTime());
            IsMove = true;
        }

        public override void Stop()
        {
            SetInactive();
            IsMove = false;
        }
    }
}