namespace simplecrudapp
{
    /// <summary>
    /// Constant key of the allowed command in the application.
    /// </summary>
    public static class CmdKey
    {
        public const string C = "-c";
        public const string R = "-r";
        public const string U = "-u";
        public const string D = "-d";
        public const string Exit = "-exit";
        public const string ResetDatabase = "-dbreset";
        public const string Create = "-create";
        public const string Read = "-read";
        public const string Update = "-update";
        public const string Delete = "-delete";

        /// <summary>
        /// Use argument with two `-`.
        /// e.g. --id, --name, --arg
        /// </summary>
        public const string Id = "id";
        public const string Name = "name";
        public const string Birth = "birth";
        public const string RowCount = "rowcount";
    }
}
