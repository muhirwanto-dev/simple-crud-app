// See https://aka.ms/new-console-template for more information

var app = new simplecrudapp.SimpleCrudApp(args);

try
{
    app.RegisterDependencies();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"{nameof(simplecrudapp.SimpleCrudApp)} throws an exception: {ex.Message}");
}
finally
{
    app.UnregisterDependencies();
}