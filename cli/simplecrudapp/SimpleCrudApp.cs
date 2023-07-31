using Microsoft.Extensions.Configuration;
using Nancy.TinyIoc;

namespace simplecrudapp
{
    public class SimpleCrudApp
    {
        public const string CmdKeyC = "c";
        public const string CmdKeyR = "r";
        public const string CmdKeyU = "u";
        public const string CmdKeyD = "d";
        public const string CmdKeyCreate = "create";
        public const string CmdKeyRead = "read";
        public const string CmdKeyUpdate = "update";
        public const string CmdKeyDelete = "delete";

        /// <summary>
        /// Command line arguments.
        /// </summary>
        private readonly string[] _args;

        private ECommand _command;

        public SimpleCrudApp(string[] args)
        {
            _args = args;
        }

        /// <summary>
        /// Register all instances into dependency container.
        /// </summary>
        public void RegisterDependencies()
        {
            var container = TinyIoCContainer.Current;

            container.Register<SQLiteDb>().AsSingleton();
        }

        /// <summary>
        /// Unregister all instances into dependency container.
        /// </summary>
        public void UnregisterDependencies()
        {
            var container = TinyIoCContainer.Current;

            container.Unregister<SQLiteDb>();
        }

        public void Run()
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddCommandLine(_args).Build();

            ValidateArgs(config);

            switch (_command)
            {
                case ECommand.Create:
                {
                    break;
                }
                case ECommand.Read:
                {
                    break;
                }
                case ECommand.Update:
                {
                    break;
                }
                case ECommand.Delete:
                {
                    break;
                }
            }
        }

        private void ValidateArgs(IConfigurationRoot config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("Command-line argument: null");
            }

            _command = ECommand.Unknown;

            var c = config.GetSection(CmdKeyC) != null || config.GetSection(CmdKeyCreate) != null;
            if (c)
            {
                _command = ECommand.Create;
            }

            var r = config.GetSection(CmdKeyR) != null || config.GetSection(CmdKeyRead) != null;
            if (r)
            {
                _command = ECommand.Read;
            }

            var u = config.GetSection(CmdKeyU) != null || config.GetSection(CmdKeyUpdate) != null;
            if (u)
            {
                _command = ECommand.Update;
            }

            var d = config.GetSection(CmdKeyD) != null || config.GetSection(CmdKeyDelete) != null;
            if (d)
            {
                _command = ECommand.Delete;
            }

            if (_command == ECommand.Unknown)
            {
                throw new ArgumentException("No valid argument found!");
            }
        }
    }
}
