
using Experimental1.Data;
using Experimental1.Commands;
using Experimental1.Filters;
using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using DryIoc;
using Experimental1.Samples;
using DryIoc.Microsoft.DependencyInjection;


// create a new Console Application
var app = ConsoleApp.Create()
    .ConfigureDefaultConfiguration()
    .ConfigureLogging((config, logging) =>
    {
        // LogManager.Configuration.
        logging.ClearProviders();
        logging.AddNLog();
    })
    .ConfigureServices((context, services) =>
    {
        var container = BuildContainer();
        services.AddSingleton<IContainer>(container);
        // add services
    })

;

// コンソールアプリケーションのフィルターを登録
app.UseFilter<LoggingFilter>();
// コマンドライン引数を解析し、コンソールアプリケーションを実行
app.Run(args);

IContainer BuildContainer()
{
    var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

    var services = new ServiceCollection();
    services
    .AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddNLog();
    })
    .AddSingleton<IConfiguration>(configuration)
    ;
    var container = new Container(rules => rules.With(propertiesAndFields: PropertiesAndFields.Auto))
        .WithDependencyInjectionAdapter(services)
        ;

    // add own services
    container.Register<Sample1>(reuse: Reuse.Transient);

    return container;
}