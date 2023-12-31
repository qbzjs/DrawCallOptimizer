﻿using CodeBase.Data;
using CodeBase.StaticData.Items.Shop.Ammo;
using CodeBase.UI.Windows.Common;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.UI.Windows.Gifts.Items
{
    public class AmmoGiftItem : AmmoItemBase
    {
        private GiftsGenerator _generator;
        private Transform _heroTransform;

        public void Construct(Transform heroTransform, AmmoItem ammoItem, PlayerProgress progress,
            GiftsGenerator generator)
        {
            _heroTransform = heroTransform;
            _generator = generator;
            base.Construct(ammoItem, progress);
        }

        protected override void Clicked()
        {
            int value = _shopAmmoStaticData.Count.GetHashCode();
            Progress.WeaponsData.WeaponsAmmoData.AddAmmo(_ammoItem.WeaponTypeId, value);
            ClearData();
            _generator.Clicked();
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.AmmoGotten),
                transform: _heroTransform, Volume, AudioSource);
        }
    }
}