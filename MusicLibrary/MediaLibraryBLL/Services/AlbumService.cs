﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fody;
using MediaLibraryDAL.Models;
using MediaLibraryBLL.Services.Interfaces;
using MediaLibraryDAL.Services.Interfaces;
using System.Linq.Expressions;

namespace MediaLibraryBLL.Services
{
    [ConfigureAwait(false)]
    [Export(typeof(IAlbumService))]
    public class AlbumService : IAlbumService
    {
        private readonly IDataService dataService;

        [ImportingConstructor]
        public AlbumService(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public async Task<int?> AddAlbum(Album album)
        {
            int? id = default(int?);

            if (album != null)
            {
                Album dbAlbum = dataService.Get<Album>(item => item.Title == album.Title);

                if (dbAlbum != null) { id = dbAlbum.Id; }
                else { id = await dataService.Insert(album); }
            }

            return id;
        }

        public IEnumerable<Album> GetAlbums(Expression<Func<Album, bool>> expression = null) => dataService.GetList(expression);

        public Album GetAlbum(Expression<Func<Album, bool>> expression = null) =>  dataService.Get(expression);

        public async Task<int> InsertAlbum(Album album) => await dataService.Insert<Album>(album);

        public async Task<int> DeleteAlbum(int id) => await dataService.Delete<Album>(id);

        public async Task<int> DeleteAlbum(Album album) => await dataService.Delete(album);

        public async Task DeleteAllAlbums() => await dataService.DeleteAll<Album>();

        public async Task<int> UpdateAlbum(Album album) => await dataService.Update(album);
    }
}