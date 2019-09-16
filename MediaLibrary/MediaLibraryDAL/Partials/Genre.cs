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
    public partial class Genre: BaseModel
    {
        public Genre() { }

        public Genre(string name)
        {
            Name = name;
        }

        public Genre(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
