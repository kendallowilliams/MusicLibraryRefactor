﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
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
    [Export(typeof(ITrackService))]
    public class TrackService : ITrackService
    {
        private readonly IDataService dataService;

         [ImportingConstructor]
        public TrackService(IDataService dataService)
        {
            this.dataService = dataService;
        }

        public async Task<IEnumerable<Track>> GetTracks(Expression<Func<Track, bool>> expression = null) => await dataService.GetList(expression);

        public async Task<Track> GetTrack(Expression<Func<Track, bool>> expression = null) => await dataService.Get(expression);

        public async Task<int> InsertTrack(Track track) => await dataService.Insert(track);

        public async Task<int> DeleteTrack(int id) => await dataService.Delete<Track>(id);

        public async Task<int> DeleteTrack(Track track) => await dataService.Delete(track);

        public async Task DeleteAllTracks() => await dataService.DeleteAll<Track>();

        public async Task<int> UpdateTrack(Track track) => await dataService.Update(track);

        public async Task<int?> AddPath(string location)
        {

            int? id = default(int?);

            if (!string.IsNullOrWhiteSpace(location))
            {
                object parameters = new { location };
                TrackPath path = new TrackPath(location),
                          dbPath = await dataService.Get<TrackPath>(item => item.Location == location);

                if (dbPath != null) { id = dbPath.Id; }
                else
                {
                    await dataService.Insert(path);
                    id = path.Id;
                }
            }

            return id;
        }

        public async Task<int?> AddTrackFile(int trackId)
        {
            Track track = await GetTrack(item => item.Id == trackId);
            TrackPath path = await dataService.Get<TrackPath>(trackPath => trackPath.Id == track.PathId);
            TrackFile trackFile = null;
            string filePath = Path.Combine(path.Location, track.FileName);
            byte[] data = File.ReadAllBytes(filePath);

            trackFile = new TrackFile(data, MimeMapping.GetMimeMapping(track.FileName), trackId);
            await dataService.Insert(trackFile);

            return trackFile.Id;
        }

        public async Task<TrackFile> GetTrackFile(int id)
        {
            TrackFile file = await dataService.Get<TrackFile>(item => item.Id == id);

            if (file == null)
            {
                Track track = await dataService.Get<Track>(item => item.Id == id);

                if (track != null)
                {
                    TrackPath path = await dataService.Get<TrackPath>(trackPath => trackPath.Id == track.PathId);
                    string fileName = Path.Combine(path.Location, track.FileName);
                    byte[] data = File.ReadAllBytes(fileName);

                    file = new TrackFile { Data = data, Type = MimeMapping.GetMimeMapping(track.FileName) };
                }
            }

            return file;
        }
    }
}