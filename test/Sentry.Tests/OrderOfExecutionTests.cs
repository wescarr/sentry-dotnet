using Sentry.Testing;

namespace Sentry.Tests;

[UsesVerify]
public class OrderOfExecutionTests
{
    [Fact]
    public async Task Event()
    {
        var events = new List<string>();
        var options = GetOptions();
        AddGlobalCapture(options, events);

        var hub = SentrySdk.InitHub(options);
        hub.CaptureEvent(new());

        await Verify(events);
    }

    [Fact]
    public async Task EventWithScope()
    {
        var events = new List<string>();
        var options = GetOptions();
        AddGlobalCapture(options, events);

        var hub = SentrySdk.InitHub(options);
        hub.CaptureEvent(new(), _ => AddScopedCapture(_, events));

        await Verify(events);
    }

    [Fact]
    public async Task Transaction()
    {
        var events = new List<string>();
        var options = GetOptions();
        AddGlobalCapture(options, events);

        var hub = SentrySdk.InitHub(options);
        var transaction = hub.StartTransaction("name", "operation");
        hub.ConfigureScope(scope => scope.Transaction = transaction);
      //  hub.CaptureMessage("TheMessage");
        transaction.Finish();

        await Verify(events);
    }

    [Fact]
    public async Task Exception()
    {
        var events = new List<string>();
        var options = GetOptions();
        AddGlobalCapture(options, events);

        var hub = SentrySdk.InitHub(options);
        var transaction = hub.StartTransaction("name", "operation");
        hub.ConfigureScope(scope =>
        {
            scope.Transaction = transaction;
            AddScopedCapture(scope, events);
        });
        hub.CaptureException(new());
        transaction.Finish();
        await Verify(events);
    }
    public async Task RunTest(Action )
    {
        var events = new List<string>();
        var options = GetOptions();
        AddGlobalCapture(options, events);

        var hub = SentrySdk.InitHub(options);
        var transaction = hub.StartTransaction("name", "operation");
        hub.ConfigureScope(scope =>
        {
            scope.Transaction = transaction;
            AddScopedCapture(scope, events);
        });
        hub.CaptureException(new());
        transaction.Finish();
        await Verify(events);
    }

    private static SentryOptions GetOptions()
    {
        var options = new SentryOptions
        {
            TracesSampleRate = 1,
            Transport = new FakeTransport(),
            Dsn = ValidDsn,
        };
        return options;
    }

    static void AddScopedCapture(Scope scope, List<string> events)
    {
        var capture = new Capture("scoped", events);
        scope.AddExceptionProcessor(capture);
        scope.AddEventProcessor(capture);
        scope.AddTransactionProcessor(capture);
    }

    static void AddGlobalCapture(SentryOptions options, List<string> events)
    {
        options.BeforeSend += _ =>
        {
            events.Add("global BeforeSend");
            return _;
        };
        options.TracesSampler += _ =>
        {
            events.Add("global TracesSampler");
            return null;
        };
        var capture = new Capture("global", events);
        options.AddExceptionFilter(capture);
        options.AddExceptionProcessor(capture);
        options.AddEventProcessor(capture);
        options.AddTransactionProcessor(capture);
    }

    class Capture :
        IExceptionFilter,
        ISentryEventProcessor,
        ISentryTransactionProcessor, ISentryEventExceptionProcessor
    {
        string context;
        List<string> events;

        public Capture(string context, List<string> events)
        {
            this.context = context;
            this.events = events;
        }

        bool IExceptionFilter.Filter(Exception ex)
        {
            AddEvent("ExceptionFilter");
            return false;
        }

        SentryEvent ISentryEventProcessor.Process(SentryEvent @event)
        {
            AddEvent("EventProcessor");
            return @event;
        }

        Transaction ISentryTransactionProcessor.Process(Transaction transaction)
        {
            AddEvent("TransactionProcessor");
            return transaction;
        }

        void ISentryEventExceptionProcessor.Process(Exception exception, SentryEvent sentryEvent) =>
            AddEvent("EventExceptionProcessor");

        void AddEvent(string name)
        {
            events.Add($"{context} {name}");
        }
    }
}
