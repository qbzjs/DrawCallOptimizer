﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Data.Perks;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.StaticData.Items;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.PerksPanel
{
    public class PerkList : MonoBehaviour, IProgressReader
    {
        [SerializeField] private Transform _container;
        [SerializeField] private PerkView perkView;

        private IEnumerable<PerkTypeId> _perkTypeIds = Enum.GetValues(typeof(PerkTypeId)).Cast<PerkTypeId>();

        private Dictionary<PerkTypeId, PerkView> _activePerks;

        private IPlayerProgressService _playerProgressService;
        private PlayerProgress _progress;

        public void Construct()
        {
            _activePerks = new Dictionary<PerkTypeId, PerkView>(_perkTypeIds.Count());
            _playerProgressService = AllServices.Container.Single<IPlayerProgressService>();
            ConstructPerks();
            _playerProgressService.Progress.PerksData.NewPerkAdded += AddNewPerk;
        }

        private void ConstructPerks()
        {
            foreach (PerkItemData perkData in _playerProgressService.Progress.PerksData.Perks)
            {
                if (perkData.LevelTypeId != LevelTypeId.None)
                    AddNewPerk(perkData);
            }
        }

        private void AddNewPerk(PerkItemData perkItemData)
        {
            PerkView value = Instantiate(perkView, _container);
            value.Construct(perkItemData);
            _activePerks.Add(perkItemData.PerkTypeId, value);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            Construct();
        }
    }
}