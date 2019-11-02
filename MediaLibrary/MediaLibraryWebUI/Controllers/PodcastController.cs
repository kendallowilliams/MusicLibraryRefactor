﻿using MediaLibraryBLL.Services.Interfaces;
using MediaLibraryDAL.DbContexts;
using MediaLibraryDAL.Services.Interfaces;
using MediaLibraryWebUI.ActionResults;
using MediaLibraryWebUI.Models;
using MediaLibraryWebUI.Models.Configurations;
using MediaLibraryWebUI.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static MediaLibraryWebUI.Enums;

namespace MediaLibraryWebUI.Controllers
{
    [Export("Podcast", typeof(IController)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class PodcastController : BaseController
    {
        private readonly IPodcastUIService podcastUIService;
        private readonly IDataService dataService;
        private readonly PodcastViewModel podcastViewModel;
        private readonly IPodcastService podcastService;

        [ImportingConstructor]
        public PodcastController(IPodcastUIService podcastUIService, IDataService dataService, PodcastViewModel podcastViewModel,
                                 IPodcastService podcastService)
        {
            this.podcastUIService = podcastUIService;
            this.dataService = dataService;
            this.podcastViewModel = podcastViewModel;
            this.podcastService = podcastService;
        }

        public async Task<ActionResult> Index()
        {
            ActionResult result = null;
            Configuration configuration = await dataService.GetAsync<Configuration>(item => item.Type == nameof(MediaPages.Podcasts));

            if (configuration != null)
            {
                podcastViewModel.Configuration = JsonConvert.DeserializeObject<PodcastConfiguration>(configuration.JsonData);
            }

            if (podcastViewModel.Configuration.SelectedPodcastPage == PodcastPages.Podcast)
            {
                result = await Get(podcastViewModel.Configuration.SelectedPodcastId);
            }
            else
            {
                podcastViewModel.PodcastGroups = await podcastUIService.GetPodcastGroups(podcastViewModel.Configuration.SelectedPodcastSort);
                result = View(podcastViewModel);
            }

            return result;
        }

        public async Task<ActionResult> AddPodcast(string rssFeed)
        {
            Podcast podcast = await podcastService.AddPodcast(rssFeed);
            
            podcastViewModel.SelectedPodcast = podcast;

            return View("Podcast", podcastViewModel);
        }

        public async Task RemovePodcast(int id)
        {
            Configuration configuration = await dataService.GetAsync<Configuration>(item => item.Type == nameof(MediaPages.Podcasts));

            await dataService.Delete<Podcast>(id);

            if (configuration != null)
            {
                PodcastConfiguration podcastConfiguration = JsonConvert.DeserializeObject<PodcastConfiguration>(configuration.JsonData);

                podcastConfiguration.SelectedPodcastPage = PodcastPages.Index;
                configuration.JsonData = JsonConvert.SerializeObject(podcastConfiguration);
                await dataService.Update(configuration);
            }
        }

        public async Task<ActionResult> Get(int id)
        {
            podcastViewModel.SelectedPodcast = await dataService.GetAsync<Podcast, ICollection<PodcastItem>>(podcast => podcast.Id == id, podcast => podcast.PodcastItems);

            return View("Podcast", podcastViewModel);
        }

        public async Task<ActionResult> DownloadPodcastItem(int id)
        {
            await podcastService.AddPodcastFile(null, id);

            return View("Podcast", podcastViewModel);
        }

        public async Task RefreshPodcast(int id)
        {
            await podcastService.RefreshPodcast(await dataService.GetAsync<Podcast>(item => item.Id == id));
        }

        [AllowAnonymous]
        public async Task<ActionResult> File(int id)
        {
            PodcastFile file = dataService.Get<PodcastFile>(item => item.PodcastItemId == id);
            ActionResult result = null;

            if (file != null)
            {
                result = new RangeFileContentResult(file?.Data, null, file.Type);
            }
            else
            {
                PodcastItem podcastItem = await dataService.GetAsync<PodcastItem>(item => item.Id == id);

                if (podcastItem != null)
                {
                    result = new RedirectResult(podcastItem.Url);
                }
                else
                {
                    result = new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
            }

            return result;
        }

        public async Task UpdateConfiguration(PodcastConfiguration podcastConfiguration)
        {
            if (ModelState.IsValid)
            {
                Configuration configuration = await dataService.GetAsync<Configuration>(item => item.Type == nameof(MediaPages.Podcasts));

                if (configuration == null)
                {
                    configuration = new Configuration() { Type = nameof(MediaPages.Podcasts), JsonData = JsonConvert.SerializeObject(podcastConfiguration) };
                    await dataService.Insert(configuration);
                }
                else
                {
                    configuration.JsonData = JsonConvert.SerializeObject(podcastConfiguration);
                    await dataService.Update(configuration);
                }
            }
        }
    }
}