// See https://aka.ms/new-console-template for more information

var app = new simplecrudapp.SimpleCrudApp(args);

try
{
    app.RegisterDependencies();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("{0} throws an exception: {1}", nameof(simplecrudapp.SimpleCrudApp), ex.Message);
}
finally
{
    app.UnregisterDependencies();
}