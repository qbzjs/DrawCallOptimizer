﻿using System.Collections.Generic;
using CodeBase.Hero;
using CodeBase.Projectiles.Movement;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class DrawProjection : MonoBehaviour
    {
        [SerializeField] private WeaponRotation _weaponRotation;
        [SerializeField] private BombMovement _bombMovement;
        [SerializeField] private MortarBehavior _mortarBehavior;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private LayerMask _collidableLayers;
        [SerializeField] [Range(3, 100)] private int _lineSegmentCount;
        [SerializeField] private float _forceMultiplier;

        private float _timeBetweenPoints = 0.1f;
        private List<Vector3> _linePoints = new List<Vector3>();
        private Vector3? _target = null;
        private float _bombMovementSpeed;
        private RaycastHit[] _results;
        private float _rigidbodyMass;

        private void Awake() =>
            _weaponRotation.GotTarget += SetTarget;

        private void SetTarget(Vector3 target) =>
            _target = target;

        // private void Update()
        // {
        //     if ((BombMovement)_mortarBehavior.GetMovement() != null)
        //     {
        //         _lineRenderer.positionCount = _lineSegmentCount;
        //
        //         // if (_mortarBehavior.ProjectilesRespawns.Length == 1)
        //         // {
        //             Vector3 startingPosition = _mortarBehavior.ProjectilesRespawns[0].position;
        //             BombMovement bombMovement = ((BombMovement)_mortarBehavior.GetMovement());
        //             Vector3 startingVelocity = _mortarBehavior.ProjectilesRespawns[0].forward * bombMovement.Power;
        //
        //             for (float i = 0; i < _lineSegmentCount; i += _timeBetweenPoints)
        //             {
        //                 Vector3 newPoint = startingPosition + i * startingVelocity;
        //                 newPoint.y = startingPosition.y + startingVelocity.y * i + Physics.gravity.y / 2f * i * i;
        //                 _linePoints.Add(newPoint);
        //
        //                 if (Physics.OverlapSphere(newPoint, 2, _collidableLayers).Length > 0)
        //                 {
        //                     _lineRenderer.positionCount = _linePoints.Count;
        //                     bombMovement.SetTargetPosition(newPoint);
        //                     // _mortarBehavior.SetTarget(newPoint);
        //                     break;
        //                 }
        //             }
        //
        //             _lineRenderer.SetPositions(_linePoints.ToArray());
        //         // }
        //     }
        // }

        public void UpdateTrajectory(BombMovement bombMovement)
        {
            if (_mortarBehavior.ProjectilesRespawns[0] == null || _target == null)
                return;

            Vector3 target = (Vector3)_target;
            Debug.Log($"DrawProjection target {target}");

            if (_bombMovementSpeed == 0f)
            {
                _bombMovementSpeed = bombMovement.Speed;
                _rigidbodyMass = bombMovement.Rigidbody.mass;
            }

            Vector3 aim = target - _mortarBehavior.ProjectilesRespawns[0].position;
            float lenght = Vector3.Distance(target, _mortarBehavior.ProjectilesRespawns[0].position);
            float time = lenght / _bombMovementSpeed;
            float antiGravity = -Physics.gravity.y * time / 2;
            float deltaY = (target.y - transform.position.y) / time;
            Vector3 bombSpeed = aim.normalized * _bombMovementSpeed;

            Vector3 velocity = (bombSpeed / _rigidbodyMass) * Time.fixedDeltaTime;
            float flightDuration = (2 * velocity.y) / Physics.gravity.y;

            if (flightDuration < 0)
                flightDuration *= -1;

            float stepTime = flightDuration / _lineSegmentCount;
            _linePoints.Clear();

            Debug.Log($"DrawProjection aim {aim}");
            Debug.Log($"DrawProjection lenght {lenght}");
            Debug.Log($"DrawProjection velocity {velocity}");

            for (int i = 0; i < _lineSegmentCount; i++)
            {
                float stepTimePassed = stepTime * i;

                Vector3 movementVector = new Vector3(
                    velocity.x * stepTimePassed,
                    velocity.y * stepTimePassed - 0.5f * Physics.gravity.y * stepTimePassed * stepTimePassed,
                    velocity.z * stepTimePassed);

                _results = new RaycastHit[1];

                int count = Physics.RaycastNonAlloc(_mortarBehavior.ProjectilesRespawns[0].position, -movementVector, _results, movementVector.magnitude,
                    _collidableLayers);

                if (count > 0)
                    break;

                _linePoints.Add(-movementVector + _mortarBehavior.ProjectilesRespawns[0].position);
            }

            _lineRenderer.positionCount = _linePoints.Count;
            _lineRenderer.SetPositions(_linePoints.ToArray());
        }

        public void ShowTrajectory(BombMovement bombMovement)
        {
            if (_bombMovementSpeed == 0f)
            {
                _bombMovementSpeed = bombMovement.Speed;
                _rigidbodyMass = bombMovement.Rigidbody.mass;
            }

            Vector3 target = (Vector3)_target;
            Vector3 speed = (target - _mortarBehavior.ProjectilesRespawns[0].position) * _bombMovementSpeed;

            Vector3[] points = new Vector3[100];
            _lineRenderer.positionCount = points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                float time = i * _timeBetweenPoints;
                Vector3 position = _mortarBehavior.ProjectilesRespawns[0].position + speed * time + Physics.gravity * time * time / 2f;
                
                points[i] = position;
            }

            _lineRenderer.SetPositions(points);
        }
    }
}