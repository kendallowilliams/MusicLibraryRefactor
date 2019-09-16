﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaLibraryDAL.Models;
using TagLib;

namespace MediaLibraryBLL.Services.Interfaces
{
    public interface IId3Service
    {
        Task<MediaData> ProcessFile(string path);
    }
}