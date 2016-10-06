﻿using NUnit.Framework;
using RestSharp.Serializers;
using System;

namespace PusherServer.Tests.UnitTests
{
    [TestFixture]
    public class When_authenticating_a_private_channel
    {
        IPusher _pusher = new Pusher(Config.AppId, Config.AppKey, Config.AppSecret);

        [Test]
        public void the_auth_response_is_valid()
        {
            string channelName = "my-channel";
            string socketId = "123.456";

            string expectedAuthString = Config.AppKey + ":" + CreateSignedString(channelName, socketId);

            IAuthenticationData result = _pusher.Authenticate(channelName, socketId);
            Assert.AreEqual(expectedAuthString, result.auth);
        }

        [Test]
        public void socket_id_cannot_contain_colon_prefix()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", ":444.444");
            });
        }

        [Test]
        public void socket_id_cannot_contain_colon_suffix()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", "444.444:");
            });
        }

        [Test]
        public void socket_id_cannot_contain_letters_suffix()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", "444.444a");
            });
        }

        [Test]
        public void socket_id_must_contain_a_period_point()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", "444");
            });
        }

        [Test]
        public void socket_id_must_not_contain_newline_prefix()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", "\n444.444");
            });
        }

        [Test]
        public void socket_id_must_not_contain_newline_suffix()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", "444.444\n");
            });
        }

        [Test]
        public void socket_id_must_not_be_empty_string()
        {
            Assert.Throws<FormatException>(() =>
            {
                _pusher.Authenticate("private-test", string.Empty);
            });
        }

        [Test]
        public void channel_must_not_have_trailing_colon()
        {
            Assert.Throws<FormatException>(() =>
            {
                AuthWithChannelName("private-channel:");
            });
        }
        [Test]
        public void channel_name_must_not_have_leading_colon()
        {
            Assert.Throws<FormatException>(() =>
            {
                AuthWithChannelName(":private-channel");
            });
        }

        [Test]
        public void channel_name_must_not_have_leading_colon_newline()
        {
            Assert.Throws<FormatException>(() =>
            {
                AuthWithChannelName(":\nprivate-channel");
            });
        }

        [Test]
        public void channel_name_must_not_have_trailing_colon_newline()
        {
            Assert.Throws<FormatException>(() =>
            {
                AuthWithChannelName("private-channel\n:");
            });
        }

        [Test]
        public void channel_names_must_not_exceed_allowed_length()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var channelName = new String('a', ValidationHelper.CHANNEL_NAME_MAX_LENGTH + 1);
                AuthWithChannelName(channelName);
            });
        }

        private void AuthWithChannelName(string channelName)
        {
            _pusher.Authenticate(channelName, "123.456");
        }

        private string CreateSignedString(string channelName, string socketId)
        {
            // null for presence data
            var stringToSign = socketId + ":" + channelName;
            return CryptoHelper.GetHmac256(Config.AppSecret, stringToSign);
        }
    }

    [TestFixture]
    public class When_authenticating_a_presence_channel
    {
        IPusher _pusher = new Pusher(Config.AppId, Config.AppKey, Config.AppSecret);

        [Test]
        public void null_presence_data_throw_Exception()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                string channelName = "my-channel";
                string socketId = "some_socket_id";

                PresenceChannelData data = null;
                _pusher.Authenticate(channelName, socketId, data);
            });
        }

        [Test]
        public void the_auth_response_is_valid()
        {
            string channelName = "my-channel";
            string socketId = "123.456";

            var serializer = new JsonSerializer();

            PresenceChannelData data = new PresenceChannelData()
            {
                user_id = "unique_user_id",
                user_info = new { twitter_id = "@leggetter" }
            };
            string presenceJSON = serializer.Serialize(data);

            string expectedAuthString = Config.AppKey + ":" + CreateSignedString(channelName, socketId, presenceJSON);

            IAuthenticationData result = _pusher.Authenticate(channelName, socketId, data);
            Assert.AreEqual(expectedAuthString, result.auth);
        }

        [Test]
        public void channel_data_is_encoded_as_JSON()
        {
            string channelName = "my-channel";
            string socketId = "123.456";

            var serializer = new JsonSerializer();

            PresenceChannelData data = new PresenceChannelData()
            {
                user_id = "unique_user_id",
                user_info = new { twitter_id = "@leggetter" }
            };

            string expectedChannelData = serializer.Serialize(data); ;

            IAuthenticationData result = _pusher.Authenticate(channelName, socketId, data);
            Assert.AreEqual(expectedChannelData, result.channel_data);
        }

        private string CreateSignedString(string channelName, string socketId, string presenceJSON)
        {
            var stringToSign = socketId + ":" + channelName + ":" + presenceJSON;
            return CryptoHelper.GetHmac256(Config.AppSecret, stringToSign);
        }
    }
}
