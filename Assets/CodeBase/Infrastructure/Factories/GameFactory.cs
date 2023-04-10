﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Data;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Pool;
using CodeBase.Services.Registrator;
using CodeBase.Services.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Factories
{
    public class GameFactory : IGameFactory
    {
        private const float Yaddition = 0.5f;
        private readonly IAssets _assets;
        private readonly IPlayerProgressService _progressService;
        private readonly IStaticDataService _staticData;
        private readonly IRegistratorService _registratorService;
        private GameObject _heroGameObject;
        private IPoolService _poolService;

        public List<IProgressReader> ProgressReaders { get; set; } = new List<IProgressReader>();
        public List<IProgressSaver> ProgressWriters { get; set; } = new List<IProgressSaver>();

        public GameFactory(IAssets assets, IPoolService poolService, IPlayerProgressService progressService, IStaticDataService staticData,
            IRegistratorService registratorService)
        {
            _poolService = poolService;
            _assets = assets;
            _progressService = progressService;
            _staticData = staticData;
            _registratorService = registratorService;

            SetProgressReadersWriters(registratorService);
        }

        public GameObject GetHero() =>
            _heroGameObject;

        private void SetProgressReadersWriters(IRegistratorService registratorService)
        {
            ProgressReaders = registratorService.ProgressReaders;
            ProgressWriters = registratorService.ProgressWriters;
        }

        public async Task WarmUp()
        {
            _assets.Initialize();
            _poolService.GenerateObjects();
        }

        public async Task<GameObject> CreateHero(Vector3 at)
        {
            // if (_heroGameObject == null)
            _heroGameObject = await _registratorService.InstantiateRegisteredAsync(AssetAddresses.Hero, at.AddY(Yaddition));
            // else
            //     _heroGameObject.transform.position = _heroGameObject.transform.position.AddY(Yaddition);

            return _heroGameObject;
        }

        public void CleanUp()
        {
            ProgressReaders.Clear();
            ProgressWriters.Clear();

            _assets.CleanUp();
        }
    }
}