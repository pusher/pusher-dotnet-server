﻿namespace PusherServer
{
    /// <summary>
    /// Interface for Authenticaton Data
    /// </summary>
    public interface IAuthenticationData
    {
        /// <summary>
        /// Gets the Authetication String
        /// </summary>
        string auth { get; }

        /// <summary>
        /// Double encoded JSON containing presence channel user information.
        /// </summary>
        string channel_data { get; }

        /// <summary>
        /// Returns a Json representation of the authentication data.
        /// </summary>
        /// <returns></returns>
        string ToJson();
    }
}
