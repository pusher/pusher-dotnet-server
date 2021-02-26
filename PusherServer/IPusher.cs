﻿using System;
using System.Threading.Tasks;

namespace PusherServer
{
    /// <summary>
    /// Provides access to functionality within the Pusher service such as Trigger to trigger events
    /// and authenticating subscription requests to private and presence channels.
    /// </summary>
    public interface IPusher
    {
        /// <summary>
        /// Triggers an event on the specified channels in the background.
        /// </summary>
        /// <param name="channelName">The name of the channel to trigger the event on</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="data">The data to be sent with the event. The event payload.</param>
        /// <param name="options">Additional options to be used when triggering the event. See <see cref="ITriggerOptions" />.</param>
        /// <returns>The result of the call to the REST API</returns>
        Task<ITriggerResult> TriggerAsync(string channelName, string eventName, object data, ITriggerOptions options = null);

        /// <summary>
        /// Triggers an event on the specified channels in the background.
        /// </summary>
        /// <param name="channelNames">The channels to trigger the event on</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="data">The data to be sent with the event. The event payload.</param>
        /// <param name="options">(Optional)Additional options to be used when triggering the event. See <see cref="ITriggerOptions" />.</param>
        /// <returns>The result of the call to the REST API</returns>
        Task<ITriggerResult> TriggerAsync(string[] channelNames, string eventName, object data, ITriggerOptions options = null);

        /// <summary>
        /// Triggers the events in the passed in array asynchronously
        /// </summary>
        /// <param name="events">The events to trigger</param>
        /// <returns>The result of the call to the REST API</returns>
        Task<ITriggerResult> TriggerAsync(Event[] events);

        /// <summary>
        /// Authenticates the subscription request for a private channel.
        /// </summary>
        /// <param name="channelName">Name of the channel to be authenticated.</param>
        /// <param name="socketId">The socket id which uniquely identifies the connection attempting to subscribe to the channel.</param>
        /// <returns>Authentication data where the required authentication token can be accessed via <see cref="IAuthenticationData.auth"/></returns>
        IAuthenticationData Authenticate(string channelName, string socketId);

        /// <summary>
        /// Authenticates the subscription request for a presence channel.
        /// </summary>
        /// <param name="channelName">Name of the channel to be authenticated.</param>
        /// <param name="socketId">The socket id which uniquely identifies the connection attempting to subscribe to the channel.</param>
        /// <param name="data">Information about the user subscribing to the presence channel.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
        /// <returns>Authentication data where the required authentication token can be accessed via <see cref="IAuthenticationData.auth"/></returns>
        IAuthenticationData Authenticate(string channelName, string socketId, PresenceChannelData data);

        /// <summary>
        /// Makes an asynchronous GET request to the specified resource. Authentication is handled as part of the call. The data returned from the request is deserizlized to the object type defined by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource">The resource.</param>
        /// <param name="parameters">Additional parameters to be sent as part of the request query string.</param>
        /// <returns>The result of the GET request</returns>
        Task<IGetResult<T>> GetAsync<T>(string resource, object parameters = null);

        /// <summary>
        /// Handle an incoming WebHook and validate it.
        /// </summary>
        /// <param name="signature">The signature of the incoming WebHook</param>
        /// <param name="body">The body of the incoming Webhook request</param>
        /// <returns>A WebHook helper.</returns>
        IWebHook ProcessWebHook(string signature, string body);

        /// <summary>
        /// Queries the Pusher API for the Users of a Presence Channel asynchronously
        /// </summary>
        /// <typeparam name="T">The type of object that will be returned by the API</typeparam>
        /// <param name="channelName">The name of the channel to query</param>
        /// <returns>The result of the Presence Channel Users query</returns>
        Task<IGetResult<T>> FetchUsersFromPresenceChannelAsync<T>(string channelName);

        /// <summary>
        /// Asynchronously queries the Pusher API for the state of a Channel
        /// </summary>
        /// <typeparam name="T">The type of object that will be returned by the API</typeparam>
        /// <param name="channelName">The name of the channel to query</param>
        /// <param name="info">An object containing a list of attributes to include in the query</param>
        /// <returns>The result of the Channel State query</returns>
        Task<IGetResult<T>> FetchStateForChannelAsync<T>(string channelName, object info = null);

        /// <summary>
        /// Queries the Pusher API for the state of all channels based upon the info object
        /// </summary>
        /// <typeparam name="T">The type of object that will be returned by the API</typeparam>
        /// <param name="info">An object containing a list of attributes to include in the query</param>
        Task<IGetResult<T>> FetchStateForChannelsAsync<T>(object info);
    }
}