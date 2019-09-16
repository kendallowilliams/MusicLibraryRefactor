﻿using MediaLibraryBLL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MediaLibraryBLL.Models
{
    public class TrackPath : BaseModel
    {
        public TrackPath() { }

        public TrackPath(string location)
        {
            Location = location;
        }

        public string Location { get; set; }
        public DateTime LastScanDate { get; set; } = DateTime.Now;
    }
}