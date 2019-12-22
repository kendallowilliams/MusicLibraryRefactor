﻿using MediaLibraryWebUI.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static MediaLibraryWebUI.Enums;

namespace MediaLibraryWebUI.Models.Configurations
{
    public class TelevisionConfiguration : BaseConfiguration
    {
        public TelevisionConfiguration()
        {
        }
        public int SelectedSeriesId { get; set; }
        public TelevisionPages SelectedTelevisionPage { get; set; }
        public SeriesSort SelectedSeriesSort { get; set; }
    }
}