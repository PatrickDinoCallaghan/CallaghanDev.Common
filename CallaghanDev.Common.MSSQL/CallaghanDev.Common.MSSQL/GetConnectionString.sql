DECLARE @ServerName NVARCHAR(128);
DECLARE @DatabaseName NVARCHAR(128);
DECLARE @ConnectionString NVARCHAR(512);

-- Get the server name
SET @ServerName = @@SERVERNAME;

-- Get the current database name
SET @DatabaseName = DB_NAME();

-- Construct the connection string
SET @ConnectionString = 'Data Source=' + @ServerName + ';Initial Catalog=' + @DatabaseName + ';Integrated Security=True;';

-- Return the connection string
SELECT @ConnectionString AS ConnectionString;

DECLARE @UserName NVARCHAR(128);

-- Get the server name
SET @ServerName = @@SERVERNAME;

-- Get the current database name
SET @DatabaseName = DB_NAME();

-- Get the current user name
SET @UserName = SUSER_SNAME();

-- Construct the connection string with the user name
SET @ConnectionString = 'Data Source=' + @ServerName + ';Initial Catalog=' + @DatabaseName + ';User ID=' + @UserName + ';Password=YourPasswordHere;';

-- Return the connection string
SELECT @ConnectionString AS ConnectionString;
