using Microsoft.Extensions.Configuration;
using Nancy.TinyIoc;
using simplecrudapp.Data;

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

                    // Keep application running even failed to drop all tables.
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

                if (input.Contains("cls"))
                {
                    Console.Clear();

                    continue;
                }

                try
                {
                    ValidateCrud(inputs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    // Invalid CRUD command, retry the input.
                    continue;
                }

                IConfigurationRoot cmd = new ConfigurationBuilder().AddCommandLine(inputs).Build();

                switch (_command)
                {
                    case ECommand.Create:
                    {
                        if (!uint.TryParse(cmd[CmdKey.Id], out uint id))
                        {
                            Console.WriteLine("Failed to parse id: {0}", cmd[CmdKey.Id]);

                            continue;
                        }

                        try
                        {
                            sqliteDb.Insert(new EmployeeData
                            {
                                Id = id,
                                FullName = cmd[CmdKey.Name],
                                BirthDate = cmd[CmdKey.Birth],
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to create data with id: {0}, msg: {1}", id, ex.Message);
                        }

                        break;
                    }
                    case ECommand.Read:
                    {
                        break;
                    }
                    case ECommand.Update:
                    {
                        if (!uint.TryParse(cmd[CmdKey.Id], out uint id))
                        {
                            Console.WriteLine("Failed to parse id: {0}", cmd[CmdKey.Id]);

                            continue;
                        }

                        string? name = cmd[CmdKey.Name];
                        string? birth = cmd[CmdKey.Birth];

                        var oldData = sqliteDb.GetRow<EmployeeData>(row => row.Id == id);
                        if (oldData != null)
                        {
                            oldData.FullName = name;
                            oldData.BirthDate = birth;

                            try
                            {
                                sqliteDb.Update(oldData);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to update data with id: {0}, msg: {1}", id, ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Data with id: {0} doesn't exist, create one instead!");

                            try
                            {
                                sqliteDb.Insert(new EmployeeData
                                {
                                    Id = id,
                                    FullName = name,
                                    BirthDate = birth,
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to create data with id: {0}, msg: {1}", id, ex.Message);
                            }
                        }

                        break;
                    }
                    case ECommand.Delete:
                    {
                        if (!uint.TryParse(cmd[CmdKey.Id], out uint id))
                        {
                            Console.WriteLine("Failed to parse id: {0}", cmd[CmdKey.Id]);

                            continue;
                        }

                        try
                        {
                            sqliteDb.Delete(new EmployeeData
                            {
                                Id = id,
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete data with id: {0}, msg: {1}", id, ex.Message);
                        }

                        break;
                    }
                }
            }
        }

        private void ValidateCrud(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("Command-line argument: null");
            }

            _command = ECommand.Unknown;

            var c = args.Contains(CmdKey.C) || args.Contains(CmdKey.Create);
            if (c)
            {
                _command = ECommand.Create;
            }

            var r = args.Contains(CmdKey.R) || args.Contains(CmdKey.Read);
            if (r)
            {
                _command = ECommand.Read;
            }

            var u = args.Contains(CmdKey.U) || args.Contains(CmdKey.Update);
            if (u)
            {
                _command = ECommand.Update;
            }

            var d = args.Contains(CmdKey.D) || args.Contains(CmdKey.Delete);
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
