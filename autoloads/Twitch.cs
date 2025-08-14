using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Api;
using TwitchLib.Api.Core;

public class TwitchIntegration {
	private static TwitchAPI API;

	public void Init() {
		API = new TwitchAPI();
		API.Settings.ClientId = "";
	}
};