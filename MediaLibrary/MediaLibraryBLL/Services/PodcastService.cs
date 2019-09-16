﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Fody;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using MediaLibraryBLL.Services.Interfaces;
using MediaLibraryDAL.Services.Interfaces;
using System.Linq.Expressions;
using MediaLibraryDAL.DbContexts;

namespace MediaLibraryBLL.Services
{
    [ConfigureAwait(false)]
    [Export(typeof(IPodcastService))]
    public class PodcastService : IPodcastService
    {
        private readonly IDataService dataService;
        private readonly IWebService webService;
        private readonly ITransactionService transactionService;

         [ImportingConstructor]
        public PodcastService(IDataService dataService, IWebService webService, ITransactionService transactionService)
        {
            this.dataService = dataService;
            this.webService = webService;
            this.transactionService = transactionService;
        }

        public async Task<Podcast> AddPodcast(string url) => await ParseRssFeed(new Podcast { Url = url });

        public async Task<IEnumerable<Podcast>> GetPodcasts(Expression<Func<Podcast, bool>> expression = null) => await dataService.GetList(expression);

        public async Task<Podcast> GetPodcast(Expression<Func<Podcast, bool>> expression = null) =>  await dataService.Get(expression);

        public async Task<IEnumerable<PodcastItem>> GetPodcastItems(int podcastId) => await dataService.GetList<PodcastItem>(item => item.PodcastId == podcastId);

        public async Task<int> InsertPodcast(Podcast podcast) => await dataService.Insert(podcast);

        public async Task<int> DeletePodcast(int id) => await dataService.Delete<Podcast>(id);

        public async Task DeleteAllPodcasts() => await dataService.DeleteAll<Podcast>();

        public async Task<int> UpdatePodcast(Podcast podcast) => await dataService.Update(podcast);

        public async Task<Podcast> RefreshPodcast(Podcast podcast) => await ParseRssFeed(podcast, true);

        public async Task<Podcast> ParseRssFeed(Podcast podcastData, bool isUpdate = false)
        {
            string title = string.Empty,
                   imageUrl = string.Empty,
                   description = string.Empty,
                   author = string.Empty;
            DateTime pubDate = DateTime.MinValue,
                     lastUpdateDate = DateTime.MinValue;
            List<ISyndicationItem> items = new List<ISyndicationItem>();
            IEnumerable<PodcastItem> podcastItems = Enumerable.Empty<PodcastItem>();
            Podcast podcast = null;

            using (var xmlReader = XmlReader.Create(podcastData.Url, new XmlReaderSettings { Async = true }))
            {
                var feedReader = new RssFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    switch(feedReader.ElementType)
                    {
                        case SyndicationElementType.Category:
                            ISyndicationCategory category = await feedReader.ReadCategory();
                            break;
                        case SyndicationElementType.Content:
                            ISyndicationContent content = await feedReader.ReadContent();
                            if (content.Name == "title") { title = content.Value; }
                            if (content.Name == "description") { description = content.Value; }
                            if (content.Name == "author") { author = content.Value; }
                            break;
                        case SyndicationElementType.Image:
                            ISyndicationImage image = await feedReader.ReadImage();
                            imageUrl = image.Url?.AbsoluteUri;
                            break;
                        case SyndicationElementType.Item:
                            ISyndicationItem item = await feedReader.ReadItem();
                            items.Add(item);
                            break;
                        case SyndicationElementType.Link:
                            ISyndicationLink link = await feedReader.ReadLink();
                            break;
                        case SyndicationElementType.Person:
                            ISyndicationPerson person = await feedReader.ReadPerson();
                            break;
                        case SyndicationElementType.None:
                        default:
                            break;
                    }
                }
                
                pubDate = items.Max(item => item.Published.DateTime);

                if (isUpdate)
                {
                    lastUpdateDate = podcastData.LastUpdateDate;
                    podcastData.Author = author;
                    podcastData.Title = title;
                    podcastData.ImageUrl = imageUrl;
                    podcastData.Description = description;
                    podcastData.LastUpdateDate = pubDate;
                    podcast = podcastData;
                    await UpdatePodcast(podcast);
                }
                else
                {
                    podcast = new Podcast(title, podcastData.Url, imageUrl, description, author) { LastUpdateDate = pubDate == DateTime.MinValue ? DateTime.Now : pubDate };
                    await InsertPodcast(podcast);
                }

                podcastItems = items.Select(item => new
                {
                    item.Title,
                    item.Description,
                    Enclosure = item.Links.FirstOrDefault(linkItem => linkItem.RelationshipType == "enclosure"),
                    PublishDate = item.Published.DateTime

                }).Select(data => new PodcastItem(data.Title, data.Description, data.Enclosure.Uri.OriginalString,
                                                  data.Enclosure.Length, data.PublishDate, podcast.Id))
                  .Where(item => item.PublishDate >= lastUpdateDate);

                podcast.PodcastItems = podcastItems.ToList();
                await UpdatePodcast(podcast);
            }

            return podcast;
        }

        public async Task<int?> AddPodcastFile(Transaction transaction, int podcastItemId)
        {
            PodcastFile podcastFile = null;

            try
            {
                byte[] data = null;
                string fileName = string.Empty;
                PodcastItem podcastItem = null;

                podcastItem = await dataService.Get<PodcastItem>(item => item.Id == podcastItemId);
                data = await webService.DownloadData(podcastItem.Url);
                fileName = Path.GetFileName((new Uri(podcastItem.Url)).LocalPath);
                podcastFile = new PodcastFile(data, MimeMapping.GetMimeMapping(fileName), podcastItem.PodcastId, podcastItem.Id);
                await dataService.Insert(podcastFile);
                await transactionService.UpdateTransactionCompleted(transaction);
            }
            catch(Exception ex)
            {
                await transactionService.UpdateTransactionErrored(transaction, ex);
            }

            return podcastFile?.Id;
        }

        public async Task<PodcastFile> GetPodcastFile(int id) => await dataService.Get<PodcastFile>(file => file.Id == id);
    }
}