﻿using MusicLibraryBLL.Models;
using MusicLibraryBLL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MusicLibraryBLL.Services
{
    [Export(typeof(IFileService))]
    public class FileService : IFileService
    {
        private readonly IId3Service id3Service;
        private readonly IArtistService artistService;
        private readonly IAlbumService albumService;
        private readonly IGenreService genreService;
        private readonly ITrackService trackService;

         [ImportingConstructor]
        public FileService(IId3Service id3Service, IArtistService artistService, IAlbumService albumService,
                           IGenreService genreService, ITrackService trackService)
        {
            this.id3Service = id3Service;
            this.artistService = artistService;
            this.albumService = albumService;
            this.genreService = genreService;
            this.trackService = trackService;
        }

        public async Task<IEnumerable<string>> EnumerateDirectories(string path, string searchPattern = null)
        {
            return await Task.Run(() => string.IsNullOrWhiteSpace(searchPattern) ? Directory.EnumerateDirectories(path) :
                                                                                   Directory.EnumerateDirectories(path, searchPattern));
        }

        public async Task<IEnumerable<string>> EnumerateFiles(string path, string searchPattern = null)
        {
            return await Task.Run(() => string.IsNullOrWhiteSpace(searchPattern) ? Directory.EnumerateFiles(path) :
                                                                                   Directory.EnumerateFiles(path, searchPattern));
        }

        public async Task Write(string path, byte[] data)
        {
            await Task.Run(() => File.WriteAllBytes(path, data));
        }

        public async Task Write(string path, string data)
        {
            await Task.Run(() => File.WriteAllText(path, data));
        }

        public async Task<bool> Exists(string path) => await Task.Run(() => File.Exists(path));

        public async Task Delete(string path) => await Task.Run(() => File.Delete(path));

        public async Task ReadDirectory(string path, bool recursive = true, bool copyFiles = false)
        {
            IEnumerable<string> directories = await EnumerateDirectories(path);
            IEnumerable<string> fileTypes = ConfigurationManager.AppSettings["FileTypes"].Split(new[] { ',' })
                                                                                         .Select(fileType => fileType.ToLower());

            foreach(string directory in directories.AsParallel())
            {
                IEnumerable<string> files = (await EnumerateFiles(directory)).Where(file => fileTypes.Contains(Path.GetExtension(file).ToLower()));
                foreach (string file in files) { await ReadMediaFile(file, copyFiles); }
                if (recursive) { await ReadDirectory(directory, recursive, copyFiles); }
            }
        }

        public async Task ReadMediaFile(string path, bool copyFiles = false)
        {
            MediaData data = await id3Service.ProcessFile(path);
            int? genreId = await genreService.AddGenre(data.Genres),
                artistId = await artistService.AddArtist(data.Artists),
                albumId = await albumService.AddAlbum(new Album(data, artistId, genreId)),
                pathId = await trackService.AddPath(Path.GetFullPath(path));
            Track track = new Track(data, pathId, genreId, albumId, artistId);
            await trackService.InsertTrack(track);
        }
    }
}