﻿using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.WeaponsPanel
{
    public class WeaponsCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _grenadeLaucher;
        [SerializeField] private TextMeshProUGUI _rpg;
        [SerializeField] private TextMeshProUGUI _rocketLaucher;
        [SerializeField] private TextMeshProUGUI _mortar;

        private IPlayerProgressService _progressService;

        private void Awake()
        {
            _progressService = AllServices.Container.Single<IPlayerProgressService>();

            _progressService.Progress.WeaponsData.WeaponsAmmoData.GrenadeLauncherAmmoChanged += ChangeGrenadeLauncherAmmo;
            _progressService.Progress.WeaponsData.WeaponsAmmoData.RpgAmmoChanged += ChangeRpgAmmo;
            _progressService.Progress.WeaponsData.WeaponsAmmoData.RocketLauncherAmmoChanged += ChangeRocketLauncherAmmo;
            _progressService.Progress.WeaponsData.WeaponsAmmoData.MortarAmmoChanged += ChangeMortarAmmo;
        }

        private void OnDisable()
        {
            _progressService.Progress.WeaponsData.WeaponsAmmoData.GrenadeLauncherAmmoChanged -= ChangeGrenadeLauncherAmmo;
            _progressService.Progress.WeaponsData.WeaponsAmmoData.RpgAmmoChanged -= ChangeRpgAmmo;
            _progressService.Progress.WeaponsData.WeaponsAmmoData.RocketLauncherAmmoChanged -= ChangeRocketLauncherAmmo;
            _progressService.Progress.WeaponsData.WeaponsAmmoData.MortarAmmoChanged -= ChangeMortarAmmo;
        }

        private void ChangeGrenadeLauncherAmmo(int ammo) =>
            _grenadeLaucher.text = $"{ammo}";

        private void ChangeRpgAmmo(int ammo) =>
            _rpg.text = $"{ammo}";

        private void ChangeRocketLauncherAmmo(int ammo) =>
            _rocketLaucher.text = $"{ammo}";

        private void ChangeMortarAmmo(int ammo) =>
            _mortar.text = $"{ammo}";
    }
}