using Spectre.Console.Cli;

namespace make_fwd
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp<MakeForwardedCommand>();
            return app.Run(args);
        }
    }
}
