using DiscordRPC;

namespace Zeyadstrap.Integrations
{
    public sealed class LauncherRichPresence : IDisposable
    {
        private const string LOG_IDENT = "LauncherRichPresence";

        private readonly DiscordRpcClient? _rpcClient;
        private readonly Timer? _refreshTimer;
        private readonly DiscordRPC.RichPresence _presence;

        private bool _ready;

        public static LauncherRichPresence? TryCreate(string details, string? state = null)
        {
            if (!App.Settings.Prop.ShowZeyadstrapRichPresence)
                return null;

            if (String.IsNullOrWhiteSpace(App.DiscordApplicationId))
            {
                App.Logger.WriteLine(LOG_IDENT, "Discord application ID is not configured, skipping launcher presence");
                return null;
            }

            return new LauncherRichPresence(details, state);
        }

        private LauncherRichPresence(string details, string? state)
        {
            _presence = new DiscordRPC.RichPresence
            {
                Details = details,
                State = state,
                Timestamps = new Timestamps { Start = DateTime.UtcNow },
                Assets = new Assets
                {
                    LargeImageKey = App.DiscordDefaultIconAsset,
                    LargeImageText = App.ProjectName
                }
            };

            _rpcClient = new DiscordRpcClient(App.DiscordApplicationId, autoEvents: true);

            _rpcClient.OnReady += (_, e) =>
            {
                _ready = true;
                App.Logger.WriteLine(LOG_IDENT, $"Received ready from user {e.User} ({e.User.ID})");
                UpdatePresence();
            };

            _rpcClient.OnClose += (_, e) =>
            {
                _ready = false;
                App.Logger.WriteLine(LOG_IDENT, $"Lost connection to Discord RPC - {e.Reason} ({e.Code})");
            };

            _rpcClient.OnError += (_, e) =>
                App.Logger.WriteLine(LOG_IDENT, $"An RPC error occurred - {e.Message}");

            if (!_rpcClient.Initialize())
                App.Logger.WriteLine(LOG_IDENT, "Discord RPC initialization failed");

            _refreshTimer = new Timer(_ => UpdatePresence(true), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
        }

        public void SetActivity(string details, string? state = null)
        {
            _presence.Details = details;
            _presence.State = state;
            UpdatePresence();
        }

        private void UpdatePresence(bool refresh = false)
        {
            if (_rpcClient is null || !_ready)
                return;

            try
            {
                if (!refresh)
                    App.Logger.WriteLine(LOG_IDENT, "Updating launcher presence");

                _rpcClient.SetPresence(_presence);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public void Dispose()
        {
            _refreshTimer?.Dispose();

            if (_rpcClient is not null && _ready)
            {
                try
                {
                    _rpcClient.ClearPresence();
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            _rpcClient?.Dispose();
        }
    }
}