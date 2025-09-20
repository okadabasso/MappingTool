
using ConsoleApp1.Data;
using ConsoleApp1.Commands;
using ConsoleApp1.Filters;
using ConsoleAppFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
// create a new Console Application
var app = ConsoleApp.Create()
    .ConfigureDefaultConfiguration()
    .ConfigureLogging((config, logging) =>
    {
        NLog.LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("nlog"));
        // LogManager.Configuration.
        logging.ClearProviders();
        logging.AddNLog();
    })
    .ConfigureServices((services) =>
    {
        // add services
    })

;

// コンソールアプリケーションのフィルターを登録
app.UseFilter<LoggingFilter>();
// コマンドライン引数を解析し、コンソールアプリケーションを実行
app.Run(args);