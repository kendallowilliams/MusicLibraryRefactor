﻿using MediaLibraryWebUI.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MediaLibraryWebUI.Enums;

namespace MediaLibraryWebUI.Models.Configurations
{
    public class PodcastConfiguration : IConfiguration
    {
        public PodcastConfiguration()
        {
            SelectPage = MediaPages.Podcasts;
        }

        public int SelectedPodcastId { get; set; }
        public PodcastPages SelectedPodcastPage { get; set; }
        public PodcastSort SelectedPodcastSort { get; set; }
        public MediaPages SelectPage { get; set; }
    }
}