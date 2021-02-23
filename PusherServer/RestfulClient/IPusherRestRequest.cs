﻿namespace PusherServer.RestfulClient
{
    /// <summary>
    /// The contract for a REST request to be made to the Pusher API
    /// </summary>
    public interface IPusherRestRequest
    {
        /// <summary>
        /// Gets or sets the type of RESTful call to make
        /// </summary>
        PusherMethod Method { get; set; }

        /// <summary>
        /// Gets the Resource Uri for this request
        /// </summary>
        string ResourceUri { get; }

        /// <summary>
        /// Gets or sets the content that will be sent with the request
        /// </summary>
        object Body { get; set; }

        /// <summary>
        /// Gets the current body as a Json String
        /// </summary>
        /// <returns></returns>
        string GetContentAsJsonString();
    }
}