﻿using System;
using System.Net;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using YoutubeGameBarWidget.Utilities;

namespace YoutubeGameBarWidget.Search
{
    /// <summary>
    /// Implements a Data Retriever using Youtube GameBar Search Server's service.
    /// 
    /// For more API details, see: https://github.com/MarconiGRF/YoutubeGameBarSearchServer
    /// </summary>
    class Search
    {
        private WebClient client;
        private string ytgbssEndPoint;
        public ListItems parsedResults;
        public event EventHandler FinishedFetchingResults;
        public event EventHandler FailedFetchingResults;
        public Visibility ThumbnailsVisibility;

        /// <summary>
        /// The FinishedFetchingResults event method manager.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFinishedFetchingResults(EventArgs e)
        {
            EventHandler handler = FinishedFetchingResults;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// The FailedFetchingResults event method manager.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFailedFetchingResults(EventArgs e)
        {
            EventHandler handler = FailedFetchingResults;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// A simple constructor setting the common parameters for every search request.
        /// </summary>
        public Search()
        {
            this.ytgbssEndPoint = String.Format(Constants.Endpoints.SSBase, Utils.GetVar(Constants.Vars.SSAddress), Utils.GetVar(Constants.Vars.SSPort));
            this.DetermineThumbnailVisibility();

            this.client = new WebClient();
            this.client.Headers.Add(HttpRequestHeader.ContentType, Constants.Headers.Json);
            this.client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(ParseResults);
        }

        /// <summary>
        /// Determines wheter thumbnails are visible or not on each ListItem based on stored setting value.
        /// </summary>
        private void DetermineThumbnailVisibility()
        {
            bool thumbsMustBeShown = bool.Parse((string)Utils.GetSettingValue(Constants.Settings.ShowThumbnails["varname"]));
            if (thumbsMustBeShown)
            {
                this.ThumbnailsVisibility = Visibility.Visible;
            } 
            else
            {
                this.ThumbnailsVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Performs a search (GET) request on YTGBSS by the given term, raising events when the raw data is ready.
        /// </summary>
        /// <param name="givenTerm">The term to compose the request.</param>
        /// <returns></returns>
        public async Task ByTerm(string givenTerm)
        {
            this.client.DownloadStringAsync(new Uri(ytgbssEndPoint + givenTerm));
        }

        /// <summary>
        /// Parses the raw data into a ListItems object, raising FinishedFetchingResults event when finished.
        /// In case of failure, it FailedFetchingResults event will be raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParseResults(Object sender, DownloadStringCompletedEventArgs e)
        {
            this.parsedResults = new ListItems();
            ThemeResources colorResources = Painter.GetTheme();

            try
            {
                JsonArray jArray = JsonArray.Parse((string)e.Result);
                foreach (JsonValue jValue in jArray)
                {
                    JsonObject jObject = jValue.GetObject();
                    ListItem resultItem = new ListItem(
                            jObject.GetNamedString("mediaType"),
                            jObject.GetNamedString("mediaTitle"),
                            jObject.GetNamedString("channelTitle"),
                            jObject.GetNamedString("mediaUrl"),
                            jObject.GetNamedString("thumbnail"),
                            this.ThumbnailsVisibility);
                    resultItem.ColorResources = colorResources;
                    this.parsedResults.Add(resultItem);
                }

                this.OnFinishedFetchingResults(EventArgs.Empty);
            }
            catch
            {
                this.OnFailedFetchingResults(EventArgs.Empty);
            }
            
        }

        /// <summary>
        /// Returns the previously parsed result list from the last search by term.
        /// </summary>
        /// <returns></returns>
        public ListItems Retreive()
        {
            return this.parsedResults;
        }
    }
}
