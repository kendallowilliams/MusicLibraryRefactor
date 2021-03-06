﻿using MediaLibraryBLL.Services.Interfaces;
using MediaLibraryDAL.DbContexts;
using MediaLibraryDAL.Services.Interfaces;
using MediaLibraryWebUI.Models;
using MediaLibraryWebUI.Models.Configurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static MediaLibraryDAL.Enums;
using static MediaLibraryWebUI.UIEnums;

namespace MediaLibraryWebUI.Controllers
{
    [Export("MediaLibrary", typeof(IController)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class MediaLibraryController : BaseController
    {
        private readonly Lazy<MediaLibraryViewModel> lazyMediaLibraryViewModel;
        private readonly Lazy<IDataService> lazyDataService;
        private readonly Lazy<ILogService> lazyLogService;
        private MediaLibraryViewModel mediaLibraryViewModel => lazyMediaLibraryViewModel.Value;
        private IDataService dataService => lazyDataService.Value;
        private ILogService logService => lazyLogService.Value;

        [ImportingConstructor]
        public MediaLibraryController(Lazy<MediaLibraryViewModel> mediaLibraryViewModel, Lazy<IDataService> dataService, Lazy<ILogService> logService)
        {
            this.lazyMediaLibraryViewModel = mediaLibraryViewModel;
            this.lazyDataService = dataService;
            this.lazyLogService = logService;
        }

        public async Task<ActionResult> Index()
        {
            Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaLibraryController).Replace(nameof(Controller), string.Empty));

            if (configuration != null)
            {
                mediaLibraryViewModel.Configuration = JsonConvert.DeserializeObject<MediaLibraryConfiguration>(configuration.JsonData) ?? new MediaLibraryConfiguration();
            }

            mediaLibraryViewModel.Playlists = await dataService.GetList<Playlist>();

            return View(mediaLibraryViewModel);
        }

        public async Task UpdateConfiguration(MediaLibraryConfiguration mediaLibraryConfiguration)
        {
            if (ModelState.IsValid)
            {
                Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaLibraryController).Replace(nameof(Controller), string.Empty));

                if (configuration == null)
                {
                    configuration = new Configuration()
                    {
                        Type = nameof(MediaLibraryController).Replace(nameof(Controller), string.Empty),
                        JsonData = JsonConvert.SerializeObject(mediaLibraryConfiguration)
                    };
                    await dataService.Insert(configuration);
                }
                else
                {
                    configuration.JsonData = JsonConvert.SerializeObject(mediaLibraryConfiguration);
                    await dataService.Update(configuration);
                }
            }
        }

        public async Task<ActionResult> MediaLibraryConfiguration()
        {
            Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaLibraryController).Replace(nameof(Controller), string.Empty));

            if (configuration != null)
            {
                mediaLibraryViewModel.Configuration = JsonConvert.DeserializeObject<MediaLibraryConfiguration>(configuration.JsonData) ?? new MediaLibraryConfiguration();
            }

            return Json(mediaLibraryViewModel.Configuration, JsonRequestBehavior.AllowGet);
        }

        public async Task Log(TransactionTypes transactionType, string message) => await logService.Log(transactionType, message);
    }
}