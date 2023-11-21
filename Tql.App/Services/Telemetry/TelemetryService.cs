using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Tql.App.Support;

namespace Tql.App.Services.Telemetry;

internal class TelemetryService : IDisposable
{
    private readonly TelemetryConfiguration _configuration;
    private readonly TelemetryClient _client;
    private volatile bool _enableExceptions;
    private volatile bool _enableMetrics;

    public TelemetryService(Settings settings)
    {
        // Right now we're generating a random user ID for every device. Once
        // we setup (some kind of) sync, we can sync this user ID across to
        // other devices.
        var userId = (settings.UserId ??= Guid.NewGuid().ToString());
        var deviceId = (settings.DeviceId ??= Guid.NewGuid().ToString());

        LoadSettings(settings);

        TelemetryDebugWriter.IsTracingDisabled = true;

        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.TelemetryProcessorChainBuilder.Use(next => new PIITelemetryProcessor(next));
        _configuration.TelemetryProcessorChainBuilder.Build();
#if DEBUG
        _configuration.ConnectionString = Constants.ApplicationInsightsDevelopmentConnectionString;
        _configuration.TelemetryChannel.DeveloperMode = true;
#else
        _configuration.ConnectionString = Constants.ApplicationInsightsConnectionString;
#endif

        _client = new TelemetryClient(_configuration);
        _client.Context.Component.Version = GetType().Assembly.GetName().Version.ToString();
        _client.Context.Device.Id = deviceId;
        _client.Context.Session.Id = Guid.NewGuid().ToString();
        _client.Context.User.Id = userId;

        TrackEvent("Application Start");
    }

    private void LoadSettings(Settings settings)
    {
        DoLoadSettings();

        settings.AttachPropertyChanged(
            nameof(settings.EnableExceptionTelemetry),
            (_, _) => DoLoadSettings()
        );
        settings.AttachPropertyChanged(
            nameof(settings.EnableMetricsTelemetry),
            (_, _) => DoLoadSettings()
        );

        void DoLoadSettings()
        {
            _enableExceptions =
                settings.EnableExceptionTelemetry ?? Settings.DefaultEnableExceptionTelemetry;
            _enableMetrics =
                settings.EnableMetricsTelemetry ?? Settings.DefaultEnableMetricsTelemetry;
        }
    }

    [MustUseReturnValue]
    public IPageViewTelemetry CreatePageView(string name)
    {
        if (!_enableMetrics)
            return PageViewTelemetrySink.Instance;

        return new PageViewTelemetryImpl(this, name);
    }

    [MustUseReturnValue]
    public IRequestTelemetry CreateRequest(string name)
    {
        if (!_enableMetrics)
            return RequestTelemetrySink.Instance;

        return new RequestTelemetryImpl(this, name);
    }

    [MustUseReturnValue]
    public IDependencyTelemetry CreateDependency(
        string name,
        string? target = null,
        string? data = null,
        string? type = null
    )
    {
        if (!_enableMetrics)
            return DependencyTelemetrySink.Instance;

        return new DependencyTelemetryImpl(this, name, target, data, type);
    }

    public void TrackException(Exception exception)
    {
        if (!_enableExceptions)
            return;

        _client.TrackException(exception);
    }

    public void TrackEvent(string name)
    {
        if (!_enableMetrics)
            return;

        _client.TrackEvent(name);
    }

    [MustUseReturnValue]
    public IEventTelemetry CreateEvent(string name)
    {
        if (!_enableMetrics)
            return EventTelemetrySink.Instance;

        return new EventTelemetryImpl(this, name);
    }

    public void Dispose()
    {
        TrackEvent("Application Shutdown");

        _client.Flush();
        _configuration.Dispose();
    }

    private class TelemetrySink : ITelemetry
    {
        public bool IsEnabled => false;

        public void AddProperty(string name, string value) { }

        public void Dispose() { }
    }

    private abstract class TelemetryImpl : ITelemetry
    {
        public bool IsEnabled => true;
        protected List<(string Name, string Value)> Properties { get; } = new();

        public void AddProperty(string name, string value)
        {
            Properties.Add((name, value));
        }

        public abstract void Dispose();
    }

    private class PageViewTelemetrySink : TelemetrySink, IPageViewTelemetry
    {
        public static readonly PageViewTelemetrySink Instance = new();
    }

    private class PageViewTelemetryImpl(TelemetryService owner, string name)
        : TelemetryImpl,
            IPageViewTelemetry
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public override void Dispose()
        {
            var telemetry = new PageViewTelemetry { Name = name, Duration = _stopwatch.Elapsed };

            foreach (var property in Properties)
            {
                telemetry.Properties[property.Name] = property.Value;
            }

            owner._client.TrackPageView(telemetry);
        }
    }

    private class RequestTelemetrySink : TelemetrySink, IRequestTelemetry
    {
        public static readonly RequestTelemetrySink Instance = new();

        public bool IsSuccess
        {
            get => false;
            set { }
        }

        public string? RequestCode
        {
            get => null;
            set { }
        }
    }

    private class RequestTelemetryImpl(TelemetryService owner, string name)
        : TelemetryImpl,
            IRequestTelemetry
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public bool IsSuccess { get; set; }
        public string? RequestCode { get; set; }

        public override void Dispose()
        {
            var telemetry = new RequestTelemetry
            {
                Success = IsSuccess,
                Name = name,
                ResponseCode = RequestCode,
                Duration = _stopwatch.Elapsed
            };

            foreach (var property in Properties)
            {
                telemetry.Properties[property.Name] = property.Value;
            }

            owner._client.TrackRequest(telemetry);
        }
    }

    private class DependencyTelemetrySink : TelemetrySink, IDependencyTelemetry
    {
        public static readonly DependencyTelemetrySink Instance = new();

        public bool IsSuccess
        {
            get => false;
            set { }
        }

        public string? ResultCode
        {
            get => null;
            set { }
        }
    }

    private class DependencyTelemetryImpl(
        TelemetryService owner,
        string name,
        string? target,
        string? data,
        string? type
    ) : TelemetryImpl, IDependencyTelemetry
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public bool IsSuccess { get; set; }
        public string? ResultCode { get; set; }

        public override void Dispose()
        {
            var telemetry = new DependencyTelemetry
            {
                Success = IsSuccess,
                Name = name,
                ResultCode = ResultCode,
                Target = target,
                Data = data,
                Type = type,
                Duration = _stopwatch.Elapsed
            };

            foreach (var property in Properties)
            {
                telemetry.Properties[property.Name] = property.Value;
            }

            owner._client.TrackDependency(telemetry);
        }
    }

    private class EventTelemetrySink : TelemetrySink, IEventTelemetry
    {
        public static readonly EventTelemetrySink Instance = new();
    }

    private class EventTelemetryImpl(TelemetryService owner, string name)
        : TelemetryImpl,
            IEventTelemetry
    {
        public override void Dispose()
        {
            var telemetry = new EventTelemetry { Name = name };

            foreach (var property in Properties)
            {
                telemetry.Properties[property.Name] = property.Value;
            }

            owner._client.TrackEvent(telemetry);
        }
    }
}
