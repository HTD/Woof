# Woof.Data.DbCommandAPI

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

A package for stored procedures data access scenarios.

Stored procedures API gives following advantages:
- additional isolation layer between the database and the application,
- ability to use optimized, database-specific queries for bulk data operations,
- legacy applications / databases support,
- can be used side by side with EF Core API.

Using the `DbCommand` directly to fetch data is very tedious and requires writing a large amounts
of very repetitive code. This module removes all the boilerplate and makes the whole process as
easy as it gets.

This API can be used to create a very basic ORM. If a good ORM is really needed, you should
use an ORM like `Entity Framework`. Also, the application can take advantage of both worlds,
using `Entity Framework` and this, for some special tasks like bulk data operations.

## Usage

The main class of the API is `DbModel<T>`, where `T` is the database-specific `DbParameter` type.
The extending class can execute stored procedures both synchronously and asynchronously,
optionally using transactions.

The fetched data are converted automatically to objects and collections.

Example MSSQL database model can look like this:
```cs
class MyModel : DbModel<SqlParameter> {

    public MyModel() : base(new SqlConnection("my-connection-string"));

    // database operations...

}
```

Executing the procedures is as simple as providing the procedure name and an anonymous object
with the parameters. Like this:

```cs
var items = await QueryAsync<MyItem>("GetItems", new { date: DateTime.Today });
```

Of course in real world scenarios the application will provide the connection for the model
and will take care of it's being properly disposed.

For more details refer to the code XML documentation.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.