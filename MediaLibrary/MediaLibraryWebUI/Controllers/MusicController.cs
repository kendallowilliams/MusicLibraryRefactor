﻿using MediaLibraryBLL.Services.Interfaces;
using MediaLibraryDAL.DbContexts;
using MediaLibraryDAL.Services.Interfaces;
using MediaLibraryWebUI.ActionResults;
using MediaLibraryWebUI.Models;
using MediaLibraryWebUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using MediaLibraryWebUI.Attributes;
using MediaLibraryWebUI.DataContracts;
using static MediaLibraryDAL.Enums.TransactionEnums;
using Newtonsoft.Json;
using static MediaLibraryWebUI.Enums;
using MediaLibraryWebUI.Models.Configurations;
using MediaLibraryWebUI.Models.Data;
using System.Web;
using System.IO;
using IO_File = System.IO.File;
using Fody;

namespace MediaLibraryWebUI.Controllers
{
    [ConfigureAwait(false)]
    [Export(nameof(MediaPages.Music), typeof(IController)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class MusicController : BaseController
    {
        private readonly IDataService dataService;
        private readonly IMusicUIService musicService;
        private readonly MusicViewModel musicViewModel;
        private readonly ITrackService trackService;
        private readonly IFileService fileService;
        private readonly IControllerService controllerService;
        private readonly ITransactionService transactionService;

        [ImportingConstructor]
        public MusicController(IDataService dataService, IMusicUIService musicService, MusicViewModel musicViewModel,
                               ITrackService trackService, IFileService fileService, IControllerService controllerService,
                               ITransactionService transactionService)
        {
            this.dataService = dataService;
            this.musicService = musicService;
            this.musicViewModel = musicViewModel;
            this.trackService = trackService;
            this.fileService = fileService;
            this.controllerService = controllerService;
            this.transactionService = transactionService;
        }

        [CompressContent]
        public async Task<ActionResult> Index()
        {
            ActionResult result = null;
            Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaPages.Music));

            if (configuration != null)
            {
                musicViewModel.Configuration = JsonConvert.DeserializeObject<MusicConfiguration>(configuration.JsonData) ?? new MusicConfiguration();
            }

            if (musicViewModel.Configuration.SelectedMusicPage == MusicPages.Album &&
                await dataService.Exists<Album>(album => album.Id == musicViewModel.Configuration.SelectedAlbumId))
            {
                result = await GetAlbum(musicViewModel.Configuration.SelectedAlbumId);
            }
            else if (musicViewModel.Configuration.SelectedMusicPage == MusicPages.Artist &&
                     await dataService.Exists<Artist>(artist => artist.Id == musicViewModel.Configuration.SelectedArtistId))
            {
                result = await GetArtist(musicViewModel.Configuration.SelectedArtistId);
            }
            else
            {
                musicViewModel.SongGroups = await musicService.GetSongGroups(musicViewModel.Configuration.SelectedSongSort);
                musicViewModel.ArtistGroups = await musicService.GetArtistGroups(musicViewModel.Configuration.SelectedArtistSort);
                musicViewModel.AlbumGroups = await musicService.GetAlbumGroups(musicViewModel.Configuration.SelectedAlbumSort);
                musicViewModel.Albums = await musicService.Albums();
                musicViewModel.Artists = await musicService.Artists();
                musicViewModel.Songs = await musicService.Songs();
                musicViewModel.Playlists = await dataService.GetList<Playlist>();
                result = PartialView(musicViewModel);
            }

            return result;
        }

        [CompressContent]
        public async Task<ActionResult> GetSongGroup(string key)
        {
            IGrouping<string, Track> group = musicViewModel.SongGroups.FirstOrDefault(item => item.Key == key);
            int playlistCount = await dataService.Count<Playlist>();

            return PartialView("~/Views/Music/SongGroup.cshtml", (Group: group, PlaylistCount: playlistCount));
        }

#if !DEBUG && !DEV
        [AllowAnonymous]
#endif
        public async Task<ActionResult> File(int id)
        {
            Track track = await dataService.Get<Track, TrackPath>(item => item.Id == id, item => item.TrackPath);
            ActionResult result = null;

            if (track != null && IO_File.Exists(Path.Combine(track.TrackPath.Location, track.FileName)))
            {
                result = new FileRangeResult(Path.Combine(track.TrackPath.Location, track.FileName),
                                             Request.Headers["Range"], 
                                             MimeMapping.GetMimeMapping(track.FileName));
            }
            else
            {
                result = new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return result;
        }

        public async Task AddTrackToPlaylist(int itemId, int playlistId)
        {
            PlaylistTrack item = new PlaylistTrack() { PlaylistId = playlistId, TrackId = itemId };
            Transaction transaction = null;

            try
            {
                transaction = await transactionService.GetNewTransaction(TransactionTypes.AddPlaylistSong);
                await dataService.Insert(item);
                await transactionService.UpdateTransactionCompleted(transaction, $"Playlist: {playlistId}, Track: {itemId}");
            }
            catch (Exception ex)
            {
                await transactionService.UpdateTransactionErrored(transaction, ex);
            }
        }

        public async Task AddArtistToPlaylist(int itemId, int playlistId)
        {
            Transaction transaction = null;

            try
            {
                IEnumerable<Track> tracks = null;
                IEnumerable<PlaylistTrack> items = null;

                transaction = await transactionService.GetNewTransaction(TransactionTypes.AddPlaylistArtist);
                tracks = await dataService.GetList<Track>(track => track.ArtistId == itemId);
                items = tracks.Select(track => new PlaylistTrack { TrackId = track.Id, PlaylistId = playlistId });
                await dataService.Insert(items);
                await transactionService.UpdateTransactionCompleted(transaction, $"Playlist: {playlistId}, Artist: {itemId}");
            }
            catch (Exception ex)
            {
                await transactionService.UpdateTransactionErrored(transaction, ex);
            }
        }

        public async Task AddAlbumToPlaylist(int itemId, int playlistId)
        {
            Transaction transaction = null;

            try
            {
                IEnumerable<Track> tracks = null;
                IEnumerable<PlaylistTrack> items = null;

                transaction = await transactionService.GetNewTransaction(TransactionTypes.AddPlaylistAlbum);
                tracks = await dataService.GetList<Track>(track => track.AlbumId == itemId);
                items = tracks.Select(track => new PlaylistTrack { TrackId = track.Id, PlaylistId = playlistId });
                await dataService.Insert(items);
                await transactionService.UpdateTransactionCompleted(transaction, $"Playlist: {playlistId}, Album: {itemId}");
            }
            catch (Exception ex)
            {
                await transactionService.UpdateTransactionErrored(transaction, ex);
            }
        }

        private async Task<ActionResult> GetAlbum(int id)
        {
            Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaPages.Music));

            musicViewModel.Configuration = JsonConvert.DeserializeObject<MusicConfiguration>(configuration.JsonData) ?? new MusicConfiguration();
            musicViewModel.SelectedAlbum = await dataService.Get<Album, IEnumerable<Track>>(album => album.Id == id, album => album.Tracks);
            musicViewModel.SelectedAlbum.Tracks = musicViewModel.SelectedAlbum.Tracks?.OrderBy(song => song.Position).ThenBy(song => song.Title).ToList();

            return PartialView("Album", musicViewModel);
        }

        private async Task<ActionResult> GetArtist(int id)
        {
            Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaPages.Music));

            musicViewModel.Configuration = JsonConvert.DeserializeObject<MusicConfiguration>(configuration.JsonData) ?? new MusicConfiguration();
            musicViewModel.SelectedArtist = await dataService.Get<Artist, IEnumerable<Album>>(artist => artist.Id == id, artist => artist.Albums);

            return PartialView("Artist", musicViewModel);
        }

        public async Task<ActionResult> Scan(ScanDirectoryRequest request)
        {
            Transaction transaction = null;
            HttpStatusCodeResult result = new HttpStatusCodeResult(HttpStatusCode.Accepted);
            string message = string.Empty;

            try
            {
                Transaction existingTransaction = await transactionService.GetActiveTransactionByType(TransactionTypes.Read);
#if DEBUG
                request = new ScanDirectoryRequest()
                {
                    Path = System.Configuration.ConfigurationManager.AppSettings["MediaLibraryRoot_DEBUG"],
                    Recursive = true
                };
#endif
                transaction = await transactionService.GetNewTransaction(TransactionTypes.Read);

                if (request.IsValid())
                {
                    TrackPath path = await dataService.Get<TrackPath>(item => item.Location.Equals(request.Path, StringComparison.CurrentCultureIgnoreCase));

                    if (path != null)
                    {
                        message = $"'{path.Location}' has already been added. Run {nameof(MusicController)} -> {nameof(Refresh)} instead.";
                        result = new HttpStatusCodeResult(HttpStatusCode.Conflict, message);
                        await transactionService.UpdateTransactionCompleted(transaction, message);
                    }
                    else if (existingTransaction == null)
                    {
                        await controllerService.QueueBackgroundWorkItem(ct => fileService.ReadDirectory(transaction, request.Path, request.Recursive).ContinueWith(task => musicService.ClearData()),
                                                                              transaction);
                    }
                    else
                    {
                        message = $"{nameof(TransactionTypes.Read)} is already running. See transaction #{existingTransaction.Id}";
                        result = new HttpStatusCodeResult(HttpStatusCode.Conflict, message);
                        await transactionService.UpdateTransactionCompleted(transaction, message);
                    }
                }
                else
                {
                    message = $"{nameof(HttpStatusCode.BadRequest)}: {JsonConvert.SerializeObject(request)}";
                    result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
                    await transactionService.UpdateTransactionErrored(transaction, new Exception(message));
                }
            }
            catch(Exception ex)
            {
                await transactionService.UpdateTransactionErrored(transaction, ex);
                result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        public async Task<ActionResult> Refresh()
        {
            Transaction transaction = null;
            HttpStatusCodeResult result = new HttpStatusCodeResult(HttpStatusCode.Accepted);

            try
            {
                Task workItem = null;

                transaction = await transactionService.GetNewTransaction(TransactionTypes.RefreshMusic);
                workItem = fileService.CheckForMusicUpdates(transaction).ContinueWith(task => musicService.ClearData());
                await controllerService.QueueBackgroundWorkItem(ct => workItem, transaction);
            }
            catch (Exception ex)
            {
                await transactionService.UpdateTransactionErrored(transaction, ex);
                result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }

            return result;
        }

        public async Task UpdateConfiguration(MusicConfiguration musicConfiguration)
        {
            if (ModelState.IsValid)
            {
                Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaPages.Music));

                if (configuration == null)
                {
                    configuration = new Configuration() { Type = nameof(MediaPages.Music), JsonData = JsonConvert.SerializeObject(musicConfiguration) };
                    await dataService.Insert(configuration);
                }
                else
                {
                    configuration.JsonData = JsonConvert.SerializeObject(musicConfiguration);
                    await dataService.Update(configuration);
                }
            }
        }

        public async Task<JsonResult> GetSong(int id)
        {
            Track track = await dataService.Get<Track, Album, Artist, Genre>(item => item.Id == id,
                                                                                  item => item.Album,
                                                                                  item => item.Artist,
                                                                                  item => item.Genre);
            Song song = new Song
            {
                Id = track.Id,
                Title = track.Title,
                Album = track?.Album.Title,
                Artist = track?.Artist.Name,
                Genre = track?.Genre.Name
            };

            return new JsonResult { Data = song, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public async Task UpdateSong(Song song)
        {
            if (ModelState.IsValid)
            {
                Track track = await dataService.Get<Track>(item => item.Id == song.Id);
                Album album = await dataService.Get<Album>(item => item.Title == song.Album.Trim());
                Artist artist = await dataService.Get<Artist>(item => item.Name == song.Artist.Trim());
                Genre genre = await dataService.Get<Genre>(item => item.Name == song.Genre.Trim());
                
                if (track != null)
                {
                    if (artist == null)
                    {
                        artist = new Artist(song.Artist.Trim());
                        await dataService.Insert(artist);
                    }

                    if (genre == null)
                    {
                        genre = new Genre(song.Genre.Trim());
                        await dataService.Insert(genre);
                    }

                    if (album == null)
                    {
                        album = new Album(song.Album.Trim()) { ArtistId = artist.Id, GenreId = genre.Id };
                        await dataService.Insert(album);
                    }
                    
                    track.Title = song.Title;
                    track.AlbumId = album.Id;
                    track.ArtistId = artist.Id;
                    track.GenreId = genre.Id;
                    await dataService.Update(track);
                    musicService.ClearData();
                }
            }
        }

        public async Task Upload(HttpPostedFileBase file)
        {
            if (file != null)
            {
                string dtFormat = "yyyyMMddHHmmss",
                       newFile = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.ToString(dtFormat)}{Path.GetExtension(file.FileName)}",
                       filePath = Path.Combine(fileService.MusicFolder, newFile);

                Directory.CreateDirectory(fileService.MusicFolder);
                file.SaveAs(filePath);
                await fileService.ReadMediaFile(filePath);
                musicService.ClearData();
            }
        }

        [CompressContent]
        public async Task<ActionResult> GetAlbums()
        {
            if (musicViewModel.AlbumGroups == null) /*then*/ musicViewModel.AlbumGroups = await musicService.GetAlbumGroups(musicViewModel.AlbumSort);
            musicViewModel.Playlists = await dataService.GetList<Playlist>();

            return PartialView("Albums", musicViewModel);
        }

        [CompressContent]
        public async Task<ActionResult> GetArtists()
        {
            if (musicViewModel.ArtistGroups == null) /*then*/ musicViewModel.ArtistGroups = await musicService.GetArtistGroups(musicViewModel.ArtistSort);
            musicViewModel.Playlists = await dataService.GetList<Playlist>();

            return PartialView("Artists", musicViewModel);
        }

        [CompressContent]
        public async Task<ActionResult> GetSongs()
        {
            if (musicViewModel.SongGroups == null) /*then*/ musicViewModel.SongGroups = await musicService.GetSongGroups(musicViewModel.SongSort);
            musicViewModel.Playlists = await dataService.GetList<Playlist>();

            return PartialView("Songs", musicViewModel);
        }

        public async Task<ActionResult> MusicConfiguration()
        {
            Configuration configuration = await dataService.Get<Configuration>(item => item.Type == nameof(MediaPages.Music));

            if (configuration != null)
            {
                musicViewModel.Configuration = JsonConvert.DeserializeObject<MusicConfiguration>(configuration.JsonData) ?? new MusicConfiguration();
            }

            return PartialView($"~/Views/Shared/Configurations/{nameof(MusicConfiguration)}.cshtml", musicViewModel.Configuration);
        }
    }
}