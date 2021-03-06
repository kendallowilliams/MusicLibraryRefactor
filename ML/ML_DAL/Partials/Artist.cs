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
    public partial class Artist: IDataModel, IArtistJSON
    {
        public Artist(string name): base()
        {
            Name = name;
        }

        public Artist(int id, string name): base()
        {
            Id = id;
            Name = name;
        }
    }
}
