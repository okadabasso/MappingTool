namespace ConsoleApp1.Filters;

using ConsoleAppFramework;
using Microsoft.Extensions.Logging;

internal class LoggingFilter(ConsoleAppFilter next) : ConsoleAppFilter(next) // ctor needs `ConsoleAppFilter next` and call base(next)
{
    // implement InvokeAsync as filter body
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        // You can access the logger from the context
        ConsoleApp.Log("start");        
        try
        {
            /* on before */
            await Next.InvokeAsync(context, cancellationToken); // invoke next filter or command body
            /* on after */
        }
        catch
        {
            /* on error */
            throw;
        }
        finally
        {
            /* on finally */
        }
    }
}