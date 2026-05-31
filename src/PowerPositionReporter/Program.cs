using PowerPositionReporter;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<PowerPositionOptions>()
    .Bind(builder.Configuration.GetSection(PowerPositionOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.OutputFolder), "OutputFolder is required.")
    .Validate(options => options.IntervalMinutes >= 1, "IntervalMinutes must be greater than or equal to 1.")
    .ValidateOnStart();

builder.Services.AddSingleton<Axpo.IPowerService, Axpo.PowerService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(LondonTimeZone.GetLondonTimeZone());
builder.Services.AddSingleton<PowerPositionAggregator>();
builder.Services.AddSingleton<CsvReportWriter>();
builder.Services.AddSingleton<PowerPositionReportService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
