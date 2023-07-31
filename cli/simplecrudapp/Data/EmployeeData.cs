using SQLite;

#nullable disable

namespace simplecrudapp.Data
{
    [Table(TableName)]
    public class EmployeeData
    {
        public const string TableName = "Employee";

        [PrimaryKey]
        public uint Id { get; set; }

        [NotNull, Unique]
        public string FullName { get; set; }

        [NotNull]
        public string BirthDate { get; set; }
    }
}
