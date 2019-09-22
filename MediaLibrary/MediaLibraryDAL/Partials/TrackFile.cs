﻿using MediaLibraryDAL.Models;
using MediaLibraryDAL.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaLibraryDAL.Enums.TransactionEnums;

namespace MediaLibraryDAL.DbContexts
{
    public partial class TrackFile: BaseModel
    {
        public TrackFile() { }

        public TrackFile(byte[] data, string type, int trackId)
        {
            Data = data;
            Type = type;
            TrackId = trackId;
        }
    }
}