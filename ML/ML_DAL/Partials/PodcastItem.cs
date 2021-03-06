﻿using MediaLibraryDAL.Models;
using MediaLibraryDAL.Models.Interfaces;
using MediaLibraryDAL.Partials.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaLibraryDAL.Enums;

namespace MediaLibraryDAL.DbContexts
{
    public partial class PodcastItem: IPlayableItem, IPodcastItemJSON
    {
        public PodcastItem(string title, string description, string url, long length, DateTime publishDate, int podcastId) : base()
        {
            Title = title;
            Url = url;
            Description = description;
            Length = (int)length;
            PublishDate = publishDate;
            PodcastId = podcastId;
        }
    }
}
