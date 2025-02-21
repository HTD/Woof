# Woof.NPOI

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2022 by CodeDog, All rights reserved.

---

## About

**The easiest multi-platform "export to Excel" feature.**

Uses **Apache NPOI** to create an **Excel workbook**.
This library allows any collection to be exported to Excel in **one simple method**.

Built in **sane defaults** and **automatics** allow creating decent looking,
**clean** and **readable** spreasheets out of raw data without any tweaking done.

Column widths are **automatic** by default, can be defined manually in data adnotations.
Data formats are automatic too, but can be defined in the same way.

No need for creating specialized objects, manually creating rows or columns.
It works **full auto**. Just `data.ToExcel("myFileName.xlsx");`, done and done.

You want to download Excel files in a web application?
No problem, it has `XWorkbookResut` type exactly for that purpose.
User's browser will know it's an Excel file and will open it in Excel,
LibreOffice or any other compatible app.

Woof.NPOI is to NPOI like HTML2PDF are to just plain PDF generators.
A PDF generator is a DRAWING API, you can't just convert some formatted text
to a document. You must draw each text block and graphical element.

NPOI library is an Excel file generator. You have to format and place each cell.
Woof.NPOI is a converter. It takes any collection of objects and treats
each object as a row. Its properties are values for the subsequent columns.

Woof.NPOI also does some formatting. It makes the first row the headers row.
Without additional hints it will use the property names for headers.
But of course using annotations you can define any custom names for the columns.

**The header row is frozen**, so you can scroll the sheet and the header row stays in place.

**Automatic summary** is provided. You can sum one or more columns,
show summary at the bottom or at the top.

Please see the provided example test project to see how it's done
and use built in XML documentation.

As always, report any bugs found as GitHub issues.

Works and tested both on Windows and Linux. Produced files were
tested both on Microsoft Excel and LibreOffice Calc.

## Usage

1. Add `using` directive:

    ```cs
    using Woof.NPOI;
    ```

2. [Optional] Create export type by setting `XSheet` and `XCell` attributes.

3. Convert directly to Excel file with `ToExcel()` extension method, convert
   to `XSSFWorkbook` with `ToXSSFWorkbook()` extension method or return new
   `XWorkbookResult` created from collection in your web application controller.

See the attached [unit test](https://github.com/HTD/Woof/blob/master/Tests/UnitTests/NPOITest.cs) for an example usage.
The sample data for this test is defined [here](https://github.com/HTD/Woof/blob/master/Tests/UnitTests/Types/NPOI/TestRow.cs).

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.