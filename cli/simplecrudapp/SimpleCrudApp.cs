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

        /// <summary>
        /// Run the cli application, use -exit to ended the program.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
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

                // Exit the program with `-exit`
                if (inputs.Contains(CmdKey.Exit))
                {
                    break;
                }

                // Clear the input using `cls`
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
                                EmployeeId = id,
                                FullName = cmd[CmdKey.Name],
                                BirthDate = cmd[CmdKey.Birth],
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("SQLite failed to create data with id: {0}, msg: {1}", id, ex.Message);
                        }

                        break;
                    }
                    case ECommand.Read:
                    {
                        bool all = !inputs.Contains($"--{CmdKey.Id}");
                        if (!all)
                        {
                            if (!uint.TryParse(cmd[CmdKey.Id], out uint id))
                            {
                                Console.WriteLine("Failed to parse id: {0}", cmd[CmdKey.Id]);

                                continue;
                            }

                            EmployeeData? employee = null;

                            try
                            {
                                employee = sqliteDb.GetRow<EmployeeData>(row => row.EmployeeId == id);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("SQLite failed to update data with id: {0}, msg: {1}", id, ex.Message);
                            }

                            if (employee != null)
                            {
                                PrintTable(new List<EmployeeData>{ employee }, maxRows: 1);

                                continue;
                            }

                            Console.WriteLine("Selected id ({0}) not found, show all data instead!", id);
                        }

                        var employees = sqliteDb.GetRows<EmployeeData>();
                        if (employees == null)
                        {
                            throw new NullReferenceException("Oops, something went wrong in the database, contact the developer!");
                        }

                        if (int.TryParse(cmd[CmdKey.RowCount], out int count))
                        {
                            PrintTable(employees.ToList(), maxRows: count);
                        }
                        else
                        {
                            PrintTable(employees.ToList());
                        }

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

                        var oldData = sqliteDb.GetRow<EmployeeData>(row => row.EmployeeId == id);
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
                                Console.WriteLine("SQLite failed to update data with id: {0}, msg: {1}", id, ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Data with id: {0} doesn't exist, create one instead!");

                            try
                            {
                                sqliteDb.Insert(new EmployeeData
                                {
                                    EmployeeId = id,
                                    FullName = name,
                                    BirthDate = birth,
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("SQLite failed to create data with id: {0}, msg: {1}", id, ex.Message);
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
                                EmployeeId = id,
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("SQLite failed to delete data with id: {0}, msg: {1}", id, ex.Message);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Validate CRUD command, throw an exception if there's no valid command.
        /// </summary>
        /// <param name="args">Arguments passed inside the main loop</param>
        /// <exception cref="ArgumentNullException">Exception occured if argument is null</exception>
        /// <exception cref="ArgumentException">Exception occured if no valid CRUD</exception>
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

        /// <summary>
        /// Print data on the db table.
        /// </summary>
        /// <param name="data">Selected data to be printed</param>
        /// <param name="maxRows">Maximum data allowed to print</param>
        private void PrintTable(List<EmployeeData> data, int maxRows = -1)
        {
            // Verify allowed maximum row value.
            if (maxRows > data.Count)
            {
                maxRows = data.Count;
            }

            // Setup config (hard coded).
            int[] columnWidth = { 10, 20, 9};

            Action newLine = () =>
            {
                Console.WriteLine();
            };

            // Print entire row with specifi character (fill):
            // |-----------------...|
            Action<char, bool> printOneRow = (fill, midBorder) =>
            {
                Console.Write("|");

                foreach (var cwidth in columnWidth)
                {
                    for (int i = 0; i < cwidth; i++)
                    {
                        Console.Write(fill);
                    }

                    if (cwidth != columnWidth.Last())
                    {
                        Console.Write(midBorder ? '|' : fill);
                    }
                }

                Console.Write("|");
            };

            // Print column with specific character width.
            Action<string, int> printColumn = (value, width) =>
            {
                while (value.Length < width)
                {
                    value += ' ';
                }

                Console.Write(value);
            };

            // Print header
            newLine();
            printOneRow('-', false);
            newLine();
            Console.Write('|');
            printColumn(nameof(EmployeeData.EmployeeId), columnWidth[0]);
            Console.Write('|');
            printColumn(nameof(EmployeeData.FullName), columnWidth[1]);
            Console.Write('|');
            printColumn(nameof(EmployeeData.BirthDate), columnWidth[2]);
            Console.Write('|');
            newLine();
            printOneRow('-', false);

            if (maxRows > 0)
            {
                data.RemoveRange(maxRows, data.Count - maxRows);
            }

            if (data.Any())
            {
                foreach (var row in data)
                {
                    newLine();
                    Console.Write('|');
                    printColumn(row.EmployeeId.ToString(), columnWidth[0]);
                    Console.Write('|');
                    printColumn(row.FullName, columnWidth[1]);
                    Console.Write('|');
                    printColumn(row.BirthDate, columnWidth[2]);
                    Console.Write('|');
                }
            }
            else
            {
                newLine();
                printOneRow(' ', true);
            }

            newLine();
            printOneRow('-', false);
            newLine();
        }
    }
}
