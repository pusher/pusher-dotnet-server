﻿using NSubstitute;
using NUnit.Framework;
using RestSharp;
using RestSharp.Serializers;
using System;

namespace PusherServer.Tests.UnitTests
{
    [TestFixture]
    public class When_Triggering_an_Event
    {
        IPusher _pusher;
        IRestClient _subClient;

        string channelName = "my-channel";
        string eventName = "my_event";
        object eventData = new { hello = "world" };

        [SetUp]
        public void Setup()
        {
            _subClient = Substitute.For<IRestClient>();
            IPusherOptions options = new PusherOptions()
            {
                RestClient = _subClient
            };

            Config.AppId = "test-app-id";
            Config.AppKey = "test-app-key";
            Config.AppSecret = "test-app-secret";

            _pusher = new Pusher(Config.AppId, Config.AppKey, Config.AppSecret, options);
        }

        [Test]
        public void url_resource_is_in_expected_format()
        {
            ITriggerResult result =
                _pusher.Trigger(
                    channelName,
                    eventName,
                    eventData
                );

            _subClient.Received().Execute(
                Arg.Is<IRestRequest>(
                    x => CheckRequestHasExpectedUrl(x)
                )
            );
        }

        private bool CheckRequestHasExpectedUrl(IRestRequest req)
        { 
            return req.Resource.StartsWith( "/apps/" + Config.AppId + "/events?");
        }

        [Test]
        public void post_payload_contains_channelName_eventName_and_eventData()
        {
            ITriggerResult result =
                _pusher.Trigger(
                    channelName,
                    eventName,
                    eventData
                );

            _subClient.Received().Execute(
                Arg.Is<IRestRequest>(
                    x => CheckRequestContainsPayload(x, channelName, eventName, eventData)
                )
            );
        }

        private bool CheckRequestContainsPayload(IRestRequest request, string channelName, string eventName, object eventData)
        {
            var serializer = new JsonSerializer();
            var expectedBody = new TriggerBody() {
                name = eventName,
                channels = new string[]{channelName},
                data = serializer.Serialize(eventData)
            };
            var expected = serializer.Serialize(expectedBody);

            return request.Parameters[0].Type == ParameterType.RequestBody &&
                request.Parameters[0].ToString().Contains( expected );
        }

        [Test]
        public void on_a_single_channel_the_socket_id_parameter_should_be_present_in_the_querystring()
        {
            var expectedSocketId = "123.098";
            
            ITriggerResult result =
                _pusher.Trigger(
                    channelName,
                    eventName,
                    eventData,
                    new TriggerOptions()
                    {
                        SocketId = expectedSocketId
                    }
                );

            _subClient.Received().Execute(
                Arg.Is<IRestRequest>(
                    x => CheckRequestContainsSocketIdParameter(x, expectedSocketId)
                )
            );
        }

        [Test]
        public void on_a_multiple_channels_the_socket_id_parameter_should_be_present_in_the_querystring()
        {
            var expectedSocketId = "123.456";

            ITriggerResult result =
                _pusher.Trigger(
                    new string[]{ "my-channel", "my-channel-2" },
                    eventName,
                    eventData,
                    new TriggerOptions()
                    {
                        SocketId = expectedSocketId
                    }
                );

            _subClient.Received().Execute(
                Arg.Is<IRestRequest>(
                    x => CheckRequestContainsSocketIdParameter(x, expectedSocketId)
                )
            );
        }

        [Test]
        public void libary_name_header_is_set_with_trigger_request()
        {
            ITriggerResult result =_pusher.Trigger(channelName, eventName, eventData
                );

            _subClient.Received().Execute(
                Arg.Is<IRestRequest>(
                    x => CheckRequestContainsHeaderParameter(x, "Pusher-Library-Name", "pusher-http-dotnet")
                )
            );
        }

        [Test]
        [ExpectedException]
        public void socket_id_cannot_contain_colon_prefix()
        {
            TriggerWithSocketId(":444.444");
        }

        [Test]
        [ExpectedException]
        public void socket_id_cannot_contain_colon_suffix()
        {
            TriggerWithSocketId("444.444:");
        }

        [Test]
        [ExpectedException]
        public void socket_id_cannot_contain_letters_suffix()
        {
            TriggerWithSocketId("444.444a");
        }

        [Test]
        [ExpectedException]
        public void socket_id_must_contain_a_period_point()
        {
            TriggerWithSocketId("444");
        }

        [Test]
        [ExpectedException]
        public void socket_id_must_not_contain_newline_prefix()
        {
            TriggerWithSocketId("\n444.444");
        }

        [Test]
        [ExpectedException]
        public void socket_id_must_not_contain_newline_suffix()
        {
            TriggerWithSocketId("444.444\n");
        }

        [Test]
        [ExpectedException]
        public void socket_id_must_not_be_empty_string()
        {
            TriggerWithSocketId(string.Empty);
        }

        [Test]
        [ExpectedException]
        public void channel_must_not_have_trailing_colon()
		{
			TriggerWithChannelName("test_channel:");
		}
		[Test]
        [ExpectedException]
		public void channel_name_must_not_have_leading_colon()
		{
			TriggerWithChannelName(":test_channel");
		}
		
        [Test]
        [ExpectedException]
		public void channel_name_must_not_have_leading_colon_newline()
        {
			TriggerWithChannelName(":\ntest_channel");
		}
		
        [Test]
        [ExpectedException]
		public void channel_name_must_not_have_trailing_colon_newline()
		{
			TriggerWithChannelName("test_channel\n:");
		}
		
		[Test]
        [ExpectedException]
		public void channel_names_in_array_must_be_validated()
		{
			_pusher.Trigger(new string[] { "this_one_is_okay", "test_channel\n:" }, eventName, eventData);
        }

        [Test]
        [ExpectedException]
        public void channel_names_must_not_exceed_allowed_length()
        {
            var channelName = new String('a', ValidationHelper.CHANNEL_NAME_MAX_LENGTH + 1);
            TriggerWithChannelName(channelName);
        }

        private void TriggerWithSocketId(string socketId)
        {
            _pusher.Trigger(channelName, eventName, eventData, new TriggerOptions()
            {
                SocketId = socketId
            });
        }

        private void TriggerWithChannelName(string channelName)
        {
            _pusher.Trigger(channelName, eventName, eventData);
        }

        private static bool CheckRequestContainsSocketIdParameter(IRestRequest request, string expectedSocketId) {
            var parameter = request.Parameters[0];
            return parameter.Type == ParameterType.RequestBody &&
                parameter.ToString().Contains("socket_id");
        }

        private static bool CheckRequestContainsHeaderParameter(IRestRequest request, string headerName, string headerValue)
        {
            bool found = false;
            foreach(Parameter p in request.Parameters)
            {
                if(p.Type == ParameterType.HttpHeader &&
                   p.Name == headerName &&
                   p.Value.ToString() == headerValue)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}
