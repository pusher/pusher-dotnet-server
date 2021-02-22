﻿using PusherServer.Exceptions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PusherServer
{
    /// <summary>
    /// Helps validation of channel names and socket_id values.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// A regular expression to check that a channel name is in a format allowed and accepted by Pusher.
        /// </summary>
        public static Regex CHANNEL_NAME_REGEX = new Regex(@"\A[a-zA-Z0-9_=@,.;\-]+\z", RegexOptions.Singleline);

        /// <summary>
        /// The maximum length of a channel name allowed by Pusher.
        /// </summary>
        public static int CHANNEL_NAME_MAX_LENGTH = 164;

        /// <summary>
        /// A regular expression to check that a socket_id is in a format allowed and accepted by Pusher.
        /// </summary>
        public static Regex SOCKET_ID_REGEX = new Regex(@"\A\d+\.\d+\z", RegexOptions.Singleline);

        /// <summary>
        /// The maximum event batch size accepted by Pusher
        /// </summary>
        public static int MAX_BATCH_SIZE = 10;

        /// <summary>
        /// Validate a <paramref name="socketId"/> value.
        /// </summary>
        /// <param name="socketId">The value to be checked.</param>
        /// <exception cref="SocketIdFormatException">If the <paramref name="socketId"/> is not in the allowed format.</exception>
        internal static void ValidateSocketId(string socketId)
        {
            if (socketId != null && SOCKET_ID_REGEX.IsMatch(socketId) == false)
            {
                throw new SocketIdFormatException(actualValue: socketId);
            }
        }

        /// <summary>
        /// Validate a <paramref name="channelName"/>.
        /// </summary>
        /// <param name="channelName">The channel name to be checked.</param>
        /// <exception cref="ChannelNameLengthExceededException">If the length of the <paramref name="channelName"/> is longer than expected.</exception>
        /// <exception cref="ChannelNameFormatException">If the <paramref name="channelName"/> is not in the allowed format.</exception>
        internal static void ValidateChannelName(string channelName)
        {
            if(channelName.Length > CHANNEL_NAME_MAX_LENGTH)
            {
                throw new ChannelNameLengthExceededException(nameof(channelName), channelName.Length);
            }

            if (CHANNEL_NAME_REGEX.IsMatch(channelName) == false)
            {
                throw new ChannelNameFormatException(actualValue: channelName);
            }
        }

        /// <summary>
        /// Validate an array of channel names
        /// </summary>
        /// <param name="channelNames">The array of channel names</param>
        /// <exception cref="ChannelNameLengthExceededException">If the length of any channel name is longer than expected.</exception>
        /// <exception cref="ChannelNameFormatException">If any channel names are not in the allowed format.</exception>
        internal static void ValidateChannelNames(IEnumerable<string> channelNames)
        {
            foreach(string name in channelNames)
            {
                ValidateChannelName(name);
            }
        }

        /// <summary>
        /// Validates a batch of events.
        /// </summary>
        /// <param name="events">The batch of events to validate.</param>
        /// <exception cref="ChannelNameLengthExceededException">If the length of any channel name is longer than expected.</exception>
        /// <exception cref="ChannelNameFormatException">If any channel names are not in the allowed format.</exception>
        /// <exception cref="EventBatchSizeExceededException">If the size of the batch is greater than 10.</exception>
        internal static void ValidateBatchEvents(Event[] events)
        {
            if (events.Length > MAX_BATCH_SIZE)
            {
                throw new EventBatchSizeExceededException(nameof(events), events.Length);
            }

            foreach (Event e in events)
            {
                ValidateChannelName(e.Channel);
                ValidateSocketId(e.SocketId);
            }
        }

        /// <summary>
        /// Validates the size of the data field for a batch event.
        /// </summary>
        /// <param name="data">The data filed to validate.</param>
        /// <param name="options">The current <see cref="IPusherOptions"/>.</param>
        /// <exception cref="EventDataSizeExceededException">If the size of <paramref name="data"/> is greater than the expected.</exception>
        /// <remarks>Note: the size of the data field is only validated if the <c>IPusherOption.BatchEventDataSizeLimit</c> is specified.</remarks>
        internal static void ValidateBatchEventData(string data, IPusherOptions options)
        {
            if (options != null && options.BatchEventDataSizeLimit.HasValue)
            {
                if (data != null && data.Length > options.BatchEventDataSizeLimit.Value)
                {
                    throw new EventDataSizeExceededException(options.BatchEventDataSizeLimit.Value, data.Length);
                }
            }
        }
    }
}
