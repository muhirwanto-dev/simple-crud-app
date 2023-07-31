using Microsoft.Extensions.Configuration;
using Nancy.TinyIoc;

namespace simplecrudapp
{
    public class SimpleCrudApp
    {
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
            var sqliteDb = TinyIoCContainer.Current.Resolve<SQLiteDb>();

            if (_args.Contains(CmdKey.ResetDatabase))
            {
                try
                {
                    sqliteDb.DropAllTable();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to drop SQLite tables due to: {0}", ex.Message);

                    // Keep application running even we failed to drop all tables.
                }
            }

            try
            {
                sqliteDb.CreateAllTable();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create SQLite tables has exception: {0}", ex.Message);

                // Exit the application if database can't be created.
                return;
            }

            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                string[] inputs = input.Split(' ');

                if (inputs.Contains(CmdKey.Exit))
                {
                    break;
                }

                IConfigurationRoot cmd = new ConfigurationBuilder().AddCommandLine(inputs).Build();

                try
                {
                    ValidateCrud(cmd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    continue;
                }

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
        }

        private void ValidateCrud(IConfigurationRoot config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("Command-line argument: null");
            }

            _command = ECommand.Unknown;

            var c = config[CmdKey.C] != null || config[CmdKey.Create] != null;
            if (c)
            {
                _command = ECommand.Create;
            }

            var r = config[CmdKey.R] != null || config[CmdKey.Read] != null;
            if (r)
            {
                _command = ECommand.Read;
            }

            var u = config[CmdKey.U] != null || config[CmdKey.Update] != null;
            if (u)
            {
                _command = ECommand.Update;
            }

            var d = config[CmdKey.D] != null || config[CmdKey.Delete] != null;
            if (d)
            {
                _command = ECommand.Delete;
            }

            if (_command == ECommand.Unknown)
            {
                throw new ArgumentException("No valid CRUD argument found!");
            }
        }
    }
}
