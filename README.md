# simple-crud-app
This is a simple CRUD (Create Read Update Delete) application of employee data.

# using CLI
To try the program directly in windows, do the following steps below:
- go to 'cli\pub\'
- open command prompt here
- call `simplecrudapp.exe`, or `simplecrudapp.exe -dbreset` to reset the database
- new blank line will be ready to receive input

# input command
1. Create new table data
	`-create --id 1001 --name Ardi --birth 12-Aug-54`, or
	`-c --id 1001 --name Ardi --birth 12-Aug-54`
	
2. Read table data
	`-read`, or `-r` to show all data in the table
	`-read --rowcount 3` to show max 3 rows of table data
	`-read --id 1001` to show data only for selected id, if not exist then show all instead
	
3. Update existing table data
	`-update --id 1001 --name Ardi --birth 12-Aug-54`, or
	`-u --id 1001 --name Ardi --birth 12-Aug-54` to update data regarding to id 1001, if not exist, create new one instead
	
4. Delete existing table data
	`-delete --id 1001`, or  `-d --id 1001`
	
# data validation
We're using SQLite table with
EmployeeId as PrimaryKey, it can't be duplicated
FullName with Attribute [NotNull, Unique], it can't be null or duplicated
BirthDate with [NotNull], it can't be null but can be duplicated