﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using PusherServer.Exceptions;
using PusherServer.RestfulClient;
using PusherServer.Tests.Helpers;

namespace PusherServer.Tests.UnitTests
{
    [TestFixture]
    public class When_Triggering_an_Event
    {
        private IPusher _pusher;
        private IPusherRestClient _subPusherClient;
        private IApplicationConfig _config;

        private readonly string _channelName = "my-channel";
        private readonly string _eventName = "my_event";
        private readonly object _eventData = new { hello = "world" };

        private HttpResponseMessage _v7ProtocolSuccessfulResponse;
        private HttpResponseMessage _v8ProtocolSuccessfulResponse;
        
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            _v7ProtocolSuccessfulResponse = Substitute.For<HttpResponseMessage>();
            _v7ProtocolSuccessfulResponse.Content = new StringContent("{}");
            _v7ProtocolSuccessfulResponse.StatusCode = HttpStatusCode.OK;

            _v8ProtocolSuccessfulResponse = Substitute.For<HttpResponseMessage>();
            _v8ProtocolSuccessfulResponse.Content = new StringContent(TriggerResultHelper.TRIGGER_RESPONSE_JSON);
            _v8ProtocolSuccessfulResponse.StatusCode = HttpStatusCode.OK;
        }

        [SetUp]
        public void Setup()
        {
            _subPusherClient = Substitute.For<IPusherRestClient>();

            IPusherOptions options = new PusherOptions()
            {
                RestClient = _subPusherClient
            };

            _config = new ApplicationConfig
            {
                AppId = "test-app-id",
                AppKey = "test-app-key",
                AppSecret = "test-app-secret",
            };

            _pusher = new Pusher(_config.AppId, _config.AppKey, _config.AppSecret, options);

            _subPusherClient.ExecutePostAsync(Arg.Any<IPusherRestRequest>()).Returns(Task.FromResult(new TriggerResult(_v8ProtocolSuccessfulResponse, TriggerResultHelper.TRIGGER_RESPONSE_JSON)));
        }

        [Test]
        public async Task url_resource_is_in_expected_format()
        {
            await _pusher.TriggerAsync(_channelName, _eventName, _eventData);

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(
#pragma warning restore 4014
                Arg.Is<IPusherRestRequest>(
                    x => x.ResourceUri.StartsWith("/apps/" + _config.AppId + "/events?")
                )
            );
        }

        [Test]
        public async Task post_payload_contains_channelName_eventName_and_eventData()
        {
            await _pusher.TriggerAsync(_channelName, _eventName, _eventData);

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(
#pragma warning restore 4014
                Arg.Is<IPusherRestRequest>(
                    x => CheckRequestContainsPayload(x, _channelName, _eventName, _eventData)
                )
            );
        }

        [Test]
        public async Task with_async_and_a_single_channel_the_request_is_made()
        {
            await _pusher.TriggerAsync(_channelName, _eventName, _eventData);

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(Arg.Any<IPusherRestRequest>());
#pragma warning restore 4014
        }

        [Test]
        public async Task with_async_and_a_single_channel_and_trigger_options_the_request_is_made()
        {
            await _pusher.TriggerAsync(_channelName, _eventName, _eventData);

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(Arg.Any<IPusherRestRequest>());
#pragma warning restore 4014
        }

        [Test]
        public async Task with_async_and_multiple_channels_the_request_is_made()
        {
            await _pusher.TriggerAsync(new[] { "fish", "pie" }, _eventName, _eventData);

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(Arg.Any<IPusherRestRequest>());
#pragma warning restore 4014
        }

        [Test]
        public async Task with_async_and_multiple_channels_and_trigger_options_the_request_is_made()
        {
            await _pusher.TriggerAsync(new[] { "fish", "pie" }, _eventName, _eventData);

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(Arg.Any<IPusherRestRequest>());
#pragma warning restore 4014
        }

        [Test]
        public async Task on_a_single_channel_the_socket_id_parameter_should_be_present_in_the_querystring()
        {
            var expectedSocketId = "123.098";
            
            await _pusher.TriggerAsync(_channelName, _eventName, _eventData, new TriggerOptions()
                    {
                        SocketId = expectedSocketId
                    });

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(
#pragma warning restore 4014
                Arg.Is<IPusherRestRequest>(
                    x => CheckRequestContainsSocketIdParameter(x, expectedSocketId)
                )
            );
        }

        [Test]
        public async Task on_a_single_channel_the_socket_id_parameter_should_be_present_in_the_querystring_async()
        {
            var expectedSocketId = "123.098";

            await _pusher.TriggerAsync(
                _channelName,
                _eventName,
                _eventData,
                new TriggerOptions()
                {
                    SocketId = expectedSocketId
                }
            );

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(Arg.Is<IPusherRestRequest>(
#pragma warning restore 4014
                x => CheckRequestContainsSocketIdParameter(x, expectedSocketId)
                )
            );
        }

        [Test]
        public async Task on_a_multiple_channels_the_socket_id_parameter_should_be_present_in_the_querystring()
        {
            var expectedSocketId = "123.456";

            await _pusher.TriggerAsync(new[]{ "my-channel", "my-channel-2" }, _eventName, _eventData, new TriggerOptions()
                    {
                        SocketId = expectedSocketId
                    });

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(
#pragma warning restore 4014
                Arg.Is<IPusherRestRequest>(
                    x => CheckRequestContainsSocketIdParameter(x, expectedSocketId)
                )
            );
        }

        [Test]
        public async Task on_a_multiple_channels_the_socket_id_parameter_should_be_present_in_the_querystring_async()
        {
            var expectedSocketId = "123.456";

            await _pusher.TriggerAsync(
                    new[] { "my-channel", "my-channel-2" },
                    _eventName,
                    _eventData,
                    new TriggerOptions()
                    {
                        SocketId = expectedSocketId
                    }
                );

#pragma warning disable 4014
            _subPusherClient.Received().ExecutePostAsync(
#pragma warning restore 4014
                Arg.Is<IPusherRestRequest>(
                    x => CheckRequestContainsSocketIdParameter(x, expectedSocketId)
                )
            );
        }

        [Test]
        public async Task socket_id_cannot_contain_colon_prefix()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId(":444.444");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \":444.444\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task socket_id_cannot_contain_colon_suffix()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId("444.444:");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \"444.444:\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task socket_id_cannot_contain_letters_suffix()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId("444.444a");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \"444.444a\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task socket_id_must_contain_a_period_point()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId("444");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \"444\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task socket_id_must_not_contain_newline_prefix()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId("\n444.444");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \"\n444.444\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task socket_id_must_not_contain_newline_suffix()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId("444.444\n");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \"444.444\n\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task socket_id_must_not_be_empty_string()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithSocketId(string.Empty);
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("socket_id \"\" was not in the form: \\A\\d+\\.\\d+\\z", caughtException.Message);
        }

        [Test]
        public async Task channel_must_not_have_trailing_colon()
		{
            FormatException caughtException = null;

            try
		    {
		        await TriggerWithChannelName("test_channel:");
		    }
		    catch (FormatException ex)
		    {
		        caughtException = ex;
		    }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("channel name \"test_channel:\" was not in the form: \\A[a-zA-Z0-9_=@,.;\\-]+\\z", caughtException.Message);
        }

		[Test]
		public async Task channel_name_must_not_have_leading_colon()
		{
            FormatException caughtException = null;

            try
		    {
		        await TriggerWithChannelName(":test_channel");
		    }
		    catch (FormatException ex)
		    {
                caughtException = ex;
		    }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("channel name \":test_channel\" was not in the form: \\A[a-zA-Z0-9_=@,.;\\-]+\\z", caughtException.Message);
        }
		
        [Test]
		public async Task channel_name_must_not_have_leading_colon_newline()
        {
            FormatException caughtException = null;

            try
            {
                await TriggerWithChannelName(":\ntest_channel");
            }
            catch (FormatException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("channel name \":\ntest_channel\" was not in the form: \\A[a-zA-Z0-9_=@,.;\\-]+\\z", caughtException.Message);
        }
		
        [Test]
        public async Task channel_name_must_not_have_trailing_colon_newline()
        {
            FormatException caughtException = null;

		    try
		    {
		        await TriggerWithChannelName("test_channel\n:");
		    }
		    catch (FormatException ex)
		    {
		        caughtException = ex;
		    }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("channel name \"test_channel\n:\" was not in the form: \\A[a-zA-Z0-9_=@,.;\\-]+\\z", caughtException.Message);
        }
		
		[Test]
		public async Task channel_names_in_array_must_be_validated()
		{
		    FormatException caughtException = null;

		    try
		    {
		        await _pusher.TriggerAsync(new[] { "this_one_is_okay", "test_channel\n:" }, _eventName, _eventData);
		    }
		    catch (FormatException ex)
		    {
		        caughtException = ex;
		    }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase("channel name \"test_channel\n:\" was not in the form: \\A[a-zA-Z0-9_=@,.;\\-]+\\z", caughtException.Message);
        }

        [Test]
        public async Task channel_names_must_not_exceed_allowed_length()
        {
            ArgumentOutOfRangeException caughtException = null;

            try
            {
                var channelName = new string('a', ValidationHelper.CHANNEL_NAME_MAX_LENGTH + 1);
                await TriggerWithChannelName(channelName);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase($"The length of the channel name was greater than the allowed 164 characters{Environment.NewLine}Parameter name: channelName", caughtException.Message);
        }

        [Test]
        public async Task event_arrays_must_not_exceed_allowed_length()
        {
            EventBatchSizeExceededException caughtException = null;

            try
            {
                var events = DataHelper.CreateEvents(numberOfEvents: 11);

                await TriggerWithBatch(events.ToArray());
            }
            catch (EventBatchSizeExceededException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase($"Only 10 events permitted per batch.{Environment.NewLine}Parameter name: events{Environment.NewLine}Actual value was 11.", caughtException.Message);
        }

        [Test]
        public async Task event_arrays_will_be_rejected_if_a_channel_name_is_too_long()
        {
            ArgumentOutOfRangeException caughtException = null;

            try
            {
                var events = DataHelper.CreateEvents(numberOfEvents: 9);
                events.Add(new Event {Channel = new string('a', ValidationHelper.CHANNEL_NAME_MAX_LENGTH + 1)});

                await TriggerWithBatch(events.ToArray());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                caughtException = ex;
            }

            Assert.IsNotNull(caughtException);
            StringAssert.AreEqualIgnoringCase($"The length of the channel name was greater than the allowed 164 characters{Environment.NewLine}Parameter name: channelName", caughtException.Message);
        }

        private bool CheckRequestContainsPayload(IPusherRestRequest request, string channelName, string eventName, object eventData)
        {
            var expectedBody = new TriggerBody()
            {
                name = eventName,
                channels = new[] { channelName },
                data = DefaultSerializer.Default.Serialize(eventData)
            };

            var expected = DefaultSerializer.Default.Serialize(expectedBody);

            return request.GetContentAsJsonString().Contains(expected);
        }

        private async Task<ITriggerResult> TriggerWithSocketId(string socketId)
        {
            var response = await _pusher.TriggerAsync(_channelName, _eventName, _eventData, new TriggerOptions()
            {
                SocketId = socketId
            });

            return response;
        }

        private async Task<ITriggerResult> TriggerWithChannelName(string channelName)
        {
            var response = await _pusher.TriggerAsync(channelName, _eventName, _eventData);

            return response;
        }

        private async Task<ITriggerResult> TriggerWithBatch(Event[] events)
        {
            var response = await _pusher.TriggerAsync(events);

            return response;
        }

        private static bool CheckRequestContainsSocketIdParameter(IPusherRestRequest request, string expectedSocketId)
        {
            var parameter = request.GetContentAsJsonString();
            return parameter.Contains("socket_id") &&
                   parameter.Contains(expectedSocketId);
        }
    }
}

