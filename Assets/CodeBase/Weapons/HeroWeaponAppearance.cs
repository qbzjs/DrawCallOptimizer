﻿using System.Collections;
using System.Collections.Generic;
using CodeBase.Hero;
using CodeBase.Projectiles;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.StaticData.ProjectileTraces;
using CodeBase.StaticData.Weapons;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class HeroWeaponAppearance : BaseWeaponAppearance
    {
        private HeroWeaponSelection _heroWeaponSelection;
        private List<ProjectileBlast> _blasts;
        private HeroWeaponTypeId _weaponTypeId;
        private float _blastRange;
        private GameObject _blastVfxPrefab;

        public void Construct(HeroWeaponSelection heroWeaponSelection)
        {
            _heroWeaponSelection = heroWeaponSelection;
            _heroWeaponSelection.WeaponSelected += InitializeSelectedWeapon;
            _blasts = new List<ProjectileBlast>(_projectilesRespawns.Length);
        }

        private void InitializeSelectedWeapon(GameObject weaponPrefab, HeroWeaponStaticData weaponStaticData,
            ProjectileTraceStaticData projectileTraceStaticData)
        {
            base.Construct(weaponStaticData.MuzzleVfx, weaponStaticData.MuzzleVfxLifeTime, weaponStaticData.Cooldown, weaponStaticData.ProjectileSpeed,
                weaponStaticData.MovementLifeTime, weaponStaticData.Damage, projectileTraceStaticData);

            _weaponTypeId = weaponStaticData.WeaponTypeId;
            _blastRange = weaponStaticData.BlastRange;
            _blastVfxPrefab = weaponStaticData.blastVfxPrefab;

            CreateShotVfx();
            CreateProjectiles();
        }

        protected override void CreateProjectiles()
        {
            for (int i = 0; i < _projectilesRespawns.Length; i++)
            {
                var projectileObject = CreateProjectileObject(i);

                CreateProjectileMovement(projectileObject);

                CreateProjectileTrace(projectileObject);

                CreateProjectileBlast(projectileObject);
            }

            SetPosition(CurrentProjectileIndex, transform);
            SetInitialVisibility();
        }

        private void CreateProjectileBlast(GameObject projectileObject)
        {
            ProjectileBlast projectileBlast = projectileObject.GetComponentInChildren<ProjectileBlast>();
            SetBlast(ref projectileBlast);
            _blasts.Add(projectileBlast);
        }

        protected override void CreateProjectileMovement(GameObject projectileObject)
        {
            ProjectileMovement projectileMovement = projectileObject.GetComponent<ProjectileMovement>();
            SetMovementType(ref projectileMovement);
            ProjectileMovements.Add(projectileMovement);
        }

        private void SetMovementType(ref ProjectileMovement movement)
        {
            switch (_weaponTypeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    SetGrenadeMovement(ref movement);
                    break;

                case HeroWeaponTypeId.Mortar:
                    SetBombMovement(ref movement);
                    break;

                case HeroWeaponTypeId.RPG:
                    SetBulletMovement(ref movement);
                    break;

                case HeroWeaponTypeId.RocketLauncher:
                    SetBulletMovement(ref movement);
                    break;
            }
        }

        private void SetGrenadeMovement(ref ProjectileMovement movement) =>
            (movement as GrenadeMovement)?.Construct(ProjectileSpeed, transform, MovementLifeTime);

        private void SetBombMovement(ref ProjectileMovement movement) =>
            (movement as BombMovement)?.Construct(ProjectileSpeed, transform, MovementLifeTime);

        private void SetBlast(ref ProjectileBlast blast) =>
            blast.Construct(_blastVfxPrefab, _blastRange, Damage);

        public void ShootTo(Vector3 enemyPosition)
        {
            if (CanShoot)
            {
                for (int i = 0; i < _projectilesRespawns.Length; i++)
                    StartCoroutine(CoroutineShootTo(enemyPosition));

                for (int i = 0; i < _muzzlesRespawns.Length; i++)
                    LaunchShotVfx();
            }
        }

        private IEnumerator CoroutineShootTo(Vector3 targetPosition)
        {
            int index = -1;

            bool found = GetIndexNotActiveProjectile(ref index);

            if (found)
            {
                CanShoot = false;
                SetPosition(index, null);
                ProjectileObjects[index].SetActive(true);

                (ProjectileMovements[index] as BombMovement)?.SetTargetPosition(targetPosition);

                ProjectileMovements[index].Launch();
                Debug.Log($"index: {index}");
                Debug.Log("Launched");
                ProjectileObjects[index].GetComponent<ProjectileTrace>().CreateTrace();

                LaunchShotVfx();

                yield return LaunchProjectileCooldown;

                SetNextProjectileReady(index);
                CanShoot = true;
            }
        }

        private void SetNextProjectileReady(int index)
        {
            if (GetIndexNotActiveProjectile(ref index))
            {
                SetPosition(index, transform);
                ProjectileObjects[index].SetActive(_showProjectiles);
            }
        }

        private bool GetIndexNotActiveProjectile(ref int index)
        {
            for (int i = 0; i < ProjectileObjects.Count; i++)
            {
                if (ProjectileObjects[i].GetComponent<ProjectileMovement>().IsMove == false)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }
    }
}