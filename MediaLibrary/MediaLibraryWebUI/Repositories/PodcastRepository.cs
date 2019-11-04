﻿using MediaLibraryBLL.Models;
using MediaLibraryBLL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MediaLibraryWebUI.Enums;

namespace MediaLibraryWebUI.Repositories
{
    public static class PodcastRepository
    {
        public static IEnumerable<IListItem<object, PodcastSort>> GetPodcastSortItems()
        {
            yield return new ListItem<object, PodcastSort>(null, "Date added", PodcastSort.DateAdded);
            yield return new ListItem<object, PodcastSort>(null, "A to Z", PodcastSort.AtoZ);
            yield return new ListItem<object, PodcastSort>(null, "Date updated", PodcastSort.LastUpdateDate);
        }
    }
}