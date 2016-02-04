# Pusher .NET HTTP API library

[![Build Status](https://travis-ci.org/pusher/pusher-http-dotnet.svg?branch=master)](https://travis-ci.org/pusher/pusher-http-dotnet)

This is a .NET library for interacting with the Pusher HTTP API.

Registering at <http://pusher.com> and use the application credentials within your app as shown below.

Comprehensive documenation can be found at <http://pusher.com/docs/>.

## Installation

### NuGet Package
```
Install-Package PusherServer
```

## How to use

### Constructor

```cs
var Pusher = new Pusher(APP_ID, APP_KEY, APP_SECRET);
```

### Publishing/Triggering events

To trigger an event on one or more channels use the trigger function.

#### A single channel

```cs
var result = pusher.Trigger( "channel-1", "test_event", new { message = "hello world" } );
```

or asynchronously

```cs
pusher.TriggerAsync( "channel-1", "test_event", new { message = "hello world" }, (ITriggerResult result) => 
{
});
```

#### Multiple channels

```cs
var result = pusher.Trigger( new string[]{ "channel-1", "channel-2" ], "test_event", new { message: "hello world" } );
```

or asynchronously

```
pusher.TriggeAsync( new string[]{ "channel-1", "channel-2" ], "test_event", new { message: "hello world" }, (ITriggerResult result) => 
{
});
```

#### Event Buffer

Version 3.0.0 of the library introduced support for event buffering. The purpose of this functionality is
to ensure that events that are triggered whilst a client is offline for a short period of time will
still be delivered upon reconnection.

*Note: this requires your Pusher application to be on a cluster that has the Event Buffer capability*

As part of this the `trigger` function now returns a set of `event_id` values for each event triggered on a channel.
These can then be used by the client to tell the Pusher service the last event it has received. If additional events 
have been triggered after that event ID the service has the opportunity to provide the client with those IDs.

For detailed information please see the [Event Buffer Documentation **TODO**](#).

The event ID values are accessed via a `ITriggerResult` that is returned from the `trigger` call.

```cs
// Trigger on single channel
var triggerResult = pusher.Trigger("ch1", "my-event", new {some = "data"});
var eventId = triggerResult.EventIds["ch1"];

// Trigger on multiple channels
var channels = new string[]{"ch1", "ch2", "ch3"};
ITriggerResult multiChannelTriggerResult = pusher.Trigger(channels, "my_event", new { hello = "world" });
var ch1EventId = multiChannelTriggerResult.EventIds["ch1"];
var ch2EventId = multiChannelTriggerResult.EventIds["ch2"];
var ch3EventId = multiChannelTriggerResult.EventIds["ch3"];
```

### Excluding event recipients

In order to avoid the person that triggered the event also receiving it the `trigger` function can take an optional `ITriggerOptions` parameter which has a `SocketId` property. For more informaiton see: <http://pusher.com/docs/publisher_api_guide/publisher_excluding_recipients>.

```cs
var result = pusher.Trigger(channel, event, data, new TriggerOptions() { SocketId = "1234.56" } );
```

### Authenticating Private channels

To authorise your users to access private channels on Pusher, you can use the `Authenticate` function:

```
var auth = pusher.Authenticate( channelName, socketId );
var json = auth.ToJson();
```

The `json` can then be returned to the client which will then use it for validation of the subscription with Pusher.

For more information see: <http://pusher.com/docs/authenticating_users>

### Authenticating Presence channels

Using presence channels is similar to private channels, but you can specify extra data to identify that particular user:

```cs
var channelData = new PresenceChannelData() {
	user_id: "unique_user_id",
	user_info: new {
	  name = "Phil Leggetter"
	  twitter_id = "@leggetter"
	}
};
var auth = pusher.Authenticate( channelName, socketId, channelData );
var json = auth.ToJson();
```

The `json` can then be returned to the client which will then use it for validation of the subscription with Pusher.

For more information see: <http://pusher.com/docs/authenticating_users>

### Application State

It is possible to query the state of your Pusher application using the generic `Pusher.Get( resource )` method and overloads.

For full details see: <http://pusher.com/docs/rest_api>

#### List channels

You can get a list of channels that are present within your application:

```cs
IGetResult<ChannelsList> result = pusher.Get<ChannelsList>("/channels");
```

You can provide additional parameters to filter the list of channels that is returned.

```cs
IGetResult<ChannelsList> result = pusher.Get<ChannelsList>("/channels", new { filter_by_prefix = "presence-" } );
```

#### Fetch channel information

Retrive information about a single channel:

```cs
IGetResult<object> result = pusher.Get<object>("/channels/my_channel" );
```

*Note: `object` has been used above because as yet there isn't a defined class that the information can be serialized on to*

#### Fetch a list of users on a presence channel

Retrive a list of users that are on a presence channel:

```cs
IGetResult<object> result = pusher.Get<object>("/channels/presence-channel/users" );
```

*Note: `object` has been used above because as yet there isn't a defined class that the information can be serialized on to*

### WebHooks

Pusher will trigger WebHooks based on the settings you have for your application. You can consume these and use them
within your application as follows.

For more information see <https://pusher.com/docs/webhooks>.

```cs
// How you get these depends on the framework you're using

// HTTP_X_PUSHER_SIGNATURE from HTTP Header
var receivedSignature = "value";

// Body of HTTP request
var receivedBody = "value;

var pusher = new Pusher(...);
var webHook = pusher.ProcessWebHook(receivedSignature, receivedBody);
if(webHook.IsValid)
{
  // The WebHook validated
  // Dictionary<string,string>[]
  var events = webHook.Events;

  foreach(var webHookEvent in webHook.Events)
  {
    var eventType = webHookEvent["name"];
    var channelName = webHookEvent["channel"];

    // depending on the type of event (eventType)
    // there may be other values in the Dictionary<string,string>
  }

}
else {
  // Log the validation errors to work out what the problem is
  // webHook.ValidationErrors
}
```

## Development Notes

* Developed using Visual Studio Community 2013
* PusherServer acceptance tests depends on [PusherClient](https://github.com/pusher/pusher-dotnet-client).
* The NUnit test framework is used for testing, your copy of Visual Studio needs the "NUnit test adapter" installed from Tools -> Extensions and Updates if you wish to run the test from the IDE.

### Running Tests

In order to run the tests copy `PusherServer.Tests/App.example.config` to `PusherServer.Tests/App.config`
and replace the configuration values with Pusher application credtials. Then run the tests in Visual Studio.

### Publish to NuGet

You should be familiar with [creating an publishing NuGet packages](http://docs.nuget.org/docs/creating-packages/creating-and-publishing-a-package).

From the `pusher-dotnet-server` directory:

1. Update `PusherServer/Properties/AssemblyInfo.cs` with new version number.
2. Check and change any info required in `PusherServer/PusherServer.nuspec`.
3. Run `package.cmd` to pack a package to deploy to NuGet.
3. Run `tools/nuget.exe push PusherServer.{VERSION}.nupkg'.

## License

This code is free to use under the terms of the MIT license.
