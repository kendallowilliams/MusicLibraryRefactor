﻿using MediaLibraryDAL.Models;
using MediaLibraryDAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaLibraryDAL.Enums;

namespace MediaLibraryDAL.DbContexts
{
    public partial class TrackPath: IDataModel
    {
        public TrackPath(string location) : base()
        {
            Location = location;
            LastScanDate = DateTime.Now;
        }
    }
}
