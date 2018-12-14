﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Fody;
using MusicLibraryBLL.Models;
using MusicLibraryBLL.Services.Interfaces;

namespace MusicLibraryBLL.Services
{
    [ConfigureAwait(false)]
    [Export(typeof(IWebService))]
    public class WebService : IWebService
    {
        [ImportingConstructor]
        public WebService()
        {
        }

        public async Task<byte[]> DownloadData(string address)
        {
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();

            using (WebClient client = new WebClient())
            {
                try
                {
                    Uri uri = new Uri(address);

                    client.DownloadDataCompleted += (sender, args) =>
                    {
                        if (args.Error == null) { tcs.SetResult(args.Result); }
                        else { tcs.SetException(args.Error); }
                    };
                    client.DownloadDataAsync(uri);
                }
                catch(Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            return await tcs.Task;
        }
    }
}