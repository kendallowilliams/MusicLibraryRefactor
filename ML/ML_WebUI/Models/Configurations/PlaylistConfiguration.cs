﻿using MediaLibraryWebUI.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MediaLibraryWebUI.UIEnums;

namespace MediaLibraryWebUI.Models.Configurations
{
    public class PlaylistConfiguration : BaseConfiguration
    {
        public PlaylistConfiguration()
        {
        }

        public int SelectedPlaylistId { get; set; }
        public PlaylistPages SelectedPlaylistPage { get; set; }
        public PlaylistSort SelectedMusicPlaylistSort { get; set; }
        public PlaylistSort SelectedPodcastPlaylistSort { get; set; }
        public PlaylistSort SelectedTelevisionPlaylistSort { get; set; }
        public PlaylistTabs SelectedPlaylistTab { get; set; }
    }
}