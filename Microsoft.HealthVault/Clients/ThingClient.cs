// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.HealthVault.Clients.Deserializers;
using Microsoft.HealthVault.Connection;
using Microsoft.HealthVault.Exceptions;
using Microsoft.HealthVault.Helpers;
using Microsoft.HealthVault.Thing;
using Microsoft.HealthVault.Transport;

namespace Microsoft.HealthVault.Clients
{
    /// <summary>
    /// An interface for the HealthVault thing client
    /// </summary>
    internal class ThingClient : IThingClient
    {
        private readonly IHealthVaultConnection _connection;
        private readonly IThingDeserializer _thingDeserializer;

        public ThingClient(
            IHealthVaultConnection connection,
            IThingDeserializer thingDeserializer)
        {
            _connection = connection;
            _thingDeserializer = thingDeserializer;
        }

        public Guid? CorrelationId { get; set; }

        public async Task<T> GetThingAsync<T>(Guid recordId, Guid thingId)
            where T : IThing
        {
            Validator.ThrowIfArgumentNull(recordId, nameof(recordId), Resources.NewItemsNullItem);

            // Create a new searcher to get the item.
            HealthRecordAccessor accessor = new HealthRecordAccessor(_connection, recordId);
            HealthRecordSearcher searcher = new HealthRecordSearcher(accessor);

            ThingQuery query = new ThingQuery();
            query.ItemIds.Add(thingId);
            query.View.Sections = ThingSections.Default;
            query.CurrentVersionOnly = true;

            HealthServiceResponseData result = await GetRequestWithParameters(recordId, searcher, new[] { query });

            IReadOnlyCollection<ThingCollection> resultSet = _thingDeserializer.Deserialize(result, searcher);

            // Check in case HealthVault returned invalid data.
            if (resultSet.Count == 0)
            {
                return default(T);
            }

            if (resultSet.Count > 1 || resultSet.ElementAt(0).Count > 1)
            {
                throw new MoreThanOneThingException(Resources.GetSingleThingTooManyResults);
            }

            if (resultSet.Count == 1)
            {
                ThingCollection resultGroup = resultSet.ElementAt(0);

                if (resultGroup.Count == 1)
                {
                    return (T)resultGroup[0];
                }
            }

            return default(T);
        }

        public async Task<ThingCollection> GetThingsAsync(Guid recordId, ThingQuery query)
        {
            Validator.ThrowIfGuidEmpty(recordId, nameof(recordId));
            Validator.ThrowIfArgumentNull(query, nameof(query));

            IReadOnlyCollection<ThingCollection> resultSet = await this.GetThingsAsync(recordId, new[] { query }).ConfigureAwait(false);
            return resultSet.FirstOrDefault();
        }

        public async Task<IReadOnlyCollection<ThingCollection>> GetThingsAsync(Guid recordId, IEnumerable<ThingQuery> queries)
        {
            Validator.ThrowIfGuidEmpty(recordId, nameof(recordId));

            List<ThingQuery> queriesList = queries.ToList();
            Validator.ThrowIfCollectionNullOrEmpty(queriesList, nameof(queries));

            HealthRecordAccessor accessor = new HealthRecordAccessor(_connection, recordId);
            HealthRecordSearcher searcher = new HealthRecordSearcher(accessor);

            HealthServiceResponseData response = await GetRequestWithParameters(recordId, searcher, queriesList).ConfigureAwait(false);
            IReadOnlyCollection<ThingCollection> resultSet = _thingDeserializer.Deserialize(response, searcher);

            return resultSet;
        }

        public async Task<IReadOnlyCollection<T>> GetThingsAsync<T>(Guid recordId, ThingQuery query = null)
            where T : IThing
        {
            Validator.ThrowIfArgumentNull(recordId, nameof(recordId), Resources.NewItemsNullItem);

            // Ensure that we have a query that requests the correct type
            T thing = (T)Activator.CreateInstance(typeof(T));
            query = query ?? new ThingQuery();
            query.TypeIds.Clear();
            query.TypeIds.Add(thing.TypeId);

            ThingCollection results = await GetThingsAsync(recordId, query);

            IList<T> things = new Collection<T>();
            foreach (IThing resultThing in results)
            {
                if (resultThing is T)
                {
                    things.Add((T)resultThing);
                }
            }

            return new ReadOnlyCollection<T>(things);
        }

        public async Task CreateNewThingsAsync<T>(Guid recordId, ICollection<T> things)
            where T : IThing
        {
            Validator.ThrowIfArgumentNull(recordId, nameof(recordId), Resources.NewItemsNullItem);
            Validator.ThrowIfArgumentNull(things, nameof(things), Resources.NewItemsNullItem);

            StringBuilder infoXml = new StringBuilder();
            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;

            using (XmlWriter infoXmlWriter =
                XmlWriter.Create(infoXml, settings))
            {
                foreach (IThing thing in things)
                {
                    Validator.ThrowIfArgumentNull(thing, nameof(thing), Resources.NewItemsNullItem);

                    thing.WriteItemXml(infoXmlWriter);
                }

                infoXmlWriter.Flush();
            }

            HealthServiceResponseData responseData = await _connection.ExecuteAsync(
                HealthVaultMethods.PutThings,
                2,
                infoXml.ToString(),
                recordId,
                CorrelationId);

            // Now update the Id for the new item
            XPathNodeIterator thingIds =
                responseData.InfoNavigator.Select(
                    GetThingIdXPathExpression(responseData.InfoNavigator));

            int thingIndex = 0;
            foreach (XPathNavigator thingIdNav in thingIds)
            {
                var thing = things.ElementAt(thingIndex) as ThingBase;
                if (thing != null)
                {
                    thing.Key =
                        new ThingKey(
                            new Guid(thingIdNav.Value),
                            new Guid(thingIdNav.GetAttribute(
                                    "version-stamp", string.Empty)));
                }

                thingIndex++;
            }
        }

        public async Task UpdateThingsAsync<T>(Guid recordId, ICollection<T> things)
            where T : IThing
        {
            Validator.ThrowIfArgumentNull(things, nameof(things), Resources.UpdateItemNull);

            StringBuilder infoXml = new StringBuilder(128);
            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;

            bool somethingRequiresUpdate = false;

            using (XmlWriter infoXmlWriter =
                XmlWriter.Create(infoXml, settings))
            {
                foreach (IThing thing in things)
                {
                    Validator.ThrowIfArgumentNull(thing, nameof(things), Resources.UpdateItemsArgumentNull);

                    if (thing.Key == null)
                    {
                        throw new ArgumentException(Resources.UpdateThingWithNoId, nameof(things));
                    }

                    if ((thing as ThingBase)?.WriteItemXml(infoXmlWriter, false) == true)
                    {
                        somethingRequiresUpdate = true;
                    }
                }

                infoXmlWriter.Flush();
            }

            if (somethingRequiresUpdate)
            {
                HealthServiceResponseData response = await _connection.ExecuteAsync(
                    HealthVaultMethods.PutThings,
                    2,
                    infoXml.ToString(),
                    recordId,
                    correlationId: CorrelationId);

                XPathNodeIterator thingIds =
                    response.InfoNavigator.Select(
                        GetThingIdXPathExpression(response.InfoNavigator));

                using (var enumerator = things.GetEnumerator())
                {
                    foreach (XPathNavigator thingIdNav in thingIds)
                    {
                        ThingBase thingBase = enumerator.Current as ThingBase;
                        if (thingBase != null)
                        {
                            thingBase.Key = new ThingKey(
                                new Guid(thingIdNav.Value),
                                new Guid(thingIdNav.GetAttribute(
                                    "version-stamp", string.Empty)));
                            thingBase.ClearDirtyFlags();
                        }

                        enumerator.MoveNext();
                    }
                }
            }
        }

        public async Task RemoveThingsAsync<T>(Guid recordId, ICollection<T> things)
            where T : IThing
        {
            StringBuilder parameters = new StringBuilder();
            foreach (IThing item in things)
            {
                parameters.Append("<thing-id version-stamp=\"");
                parameters.Append(item.Key.VersionStamp);
                parameters.Append("\">");
                parameters.Append(item.Key.Id);
                parameters.Append("</thing-id>");
            }

            await _connection.ExecuteAsync(HealthVaultMethods.RemoveThings, 1, parameters.ToString(), recordId, correlationId: CorrelationId);
        }

        internal static string GetParametersXml(HealthRecordSearcher searcher)
        {
            if (searcher.Filters.Count == 0)
            {
                HealthServiceResponseError error = new HealthServiceResponseError
                {
                    Message = Resources.HealthRecordSearcherNoFilters
                };

                HealthServiceException e =
                    HealthServiceExceptionHelper.GetHealthServiceException(
                        HealthServiceStatusCode.InvalidFilter, error);
                throw e;
            }

            StringBuilder parameters = new StringBuilder(128);

            XmlWriterSettings settings = SDKHelper.XmlUnicodeWriterSettings;

            using (XmlWriter writer = XmlWriter.Create(parameters, settings))
            {
                foreach (ThingQuery filter in searcher.Filters)
                {
                    // Add all filters
                    filter.AddFilterXml(writer);
                }

                writer.Flush();
            }

            return parameters.ToString();
        }

        private async Task<HealthServiceResponseData> GetRequestWithParameters(Guid recordId, HealthRecordSearcher searcher, IEnumerable<ThingQuery> queries)
        {
            foreach (ThingQuery query in queries)
            {
                searcher.Filters.Add(query);
            }

            return await _connection.ExecuteAsync(HealthVaultMethods.GetThings, 3, GetParametersXml(searcher), recordId, correlationId: CorrelationId).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the XPathExpression for the ThingId from the supplied XPathNavigator
        /// </summary>
        internal static XPathExpression GetThingIdXPathExpression(XPathNavigator infoNav)
        {
            XmlNamespaceManager infoXmlNamespaceManager =
                new XmlNamespaceManager(infoNav.NameTable);

            infoXmlNamespaceManager.AddNamespace(
                    "wc",
                    "urn:com.microsoft.wc.methods.response.PutThings");

            XPathExpression infoThingIdPathClone = XPathExpression.Compile("/wc:info/thing-id");

            infoThingIdPathClone.SetContext(infoXmlNamespaceManager);

            return infoThingIdPathClone;
        }
    }
}