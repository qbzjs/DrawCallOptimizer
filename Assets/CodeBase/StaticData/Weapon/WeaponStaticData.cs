using CodeBase.StaticData.ProjectileTrace;
using UnityEngine;

namespace CodeBase.StaticData.Weapon
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "StaticData/Weapon")]
    public class WeaponStaticData : ScriptableObject
    {
        public WeaponTypeId WeaponTypeId;
        public ProjectileTraceTypeId ProjectileTraceTypeId;

        [Range(1, 30)] public int Damage;

        [Range(1, 10)] public int RotationSpeed;

        [Range(1, 5)] public int Cooldown;

        [Range(1, 50)] public int Range;

        [Range(1, 30)] public int ProjectileSpeed;

        [Range(1f, 5f)] public float MuzzleVfxLifeTime;

        public GameObject MuzzleVfx;
        public GameObject BlastVfx;
        public GameObject WeaponPrefab;
        public GameObject ProjectilePrefab;
    }
}