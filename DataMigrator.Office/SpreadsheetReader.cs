﻿using System;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DataMigrator.Office
{
    ///<summary>
    ///Provides the base functionality around Spreadsheets as a set of static functions
    ///</summary>
    public class SpreadsheetReader : AbstractReader
    {
        //Blocks the constructor as this is intended to be a static library
        private SpreadsheetReader()
        {
        }

        /// <summary>
        /// Returns a new spreadsheet document as a stream from the blank spreadsheet template.
        ///</summary>
        public static MemoryStream Create()
        {
            return GetEmbeddedResourceStream("Templates\\blank.xlsx");
        }

        ///<summary>
        ///Returns a worksheet part from a spreadsheet by name
        ///</summary>
        public static WorksheetPart GetWorksheetPartByName(SpreadsheetDocument document, string worksheetName)
        {
            var sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == worksheetName);

            if (sheets.Count() == 0) return null; // The specified worksheet does not exist.

            return (WorksheetPart)document.WorkbookPart.GetPartById(sheets.First().Id);
        }

        /// <summary>
        /// Returns the name of a column offset by the supplied distance.
        /// </summary>
        public static string GetColumnName(string start, int increment)
        {
            int total = GetColumnIndex(start);

            //Increment and turn back into AB notation
            total += increment;

            string result = "";
            int remainder;

            while (total > 0)
            {
                total = Math.DivRem(total, 26, out remainder);
                if (remainder == 0)
                {
                    result = "Z" + result;
                    total -= 1;
                }
                else
                {
                    result = (char)(64 + remainder) + result;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the column name supplied.
        /// </summary>
        public static int GetColumnIndex(string column)
        {
            //Convert column to numeric
            char[] base26 = column.ToCharArray();
            int total = 0;
            int place = 0;

            //Turn start into a base 26 number
            for (int i = (base26.Length - 1); i >= 0; i += -1)
            {
                char chr = char.ToUpper(base26[i]);
                int asc = chr - 64;

                if (asc < 1 || asc > 26) throw new ApplicationException(string.Format("Invalid column name '{0}'.", column));

                if (place == 0)
                {
                    total = asc;
                    place = 26;
                }
                else
                {
                    total += (asc * place);
                    place *= 26;
                }
            }

            //Increment and turn back into AB notation
            return total;
        }

        /// <summary>
        /// Returns the column from a cell reference
        ///</summary>
        public static string ColumnFromReference(string cellReference)
        {
            cellReference = cellReference.Replace("$", "");
            return string.Join(null, System.Text.RegularExpressions.Regex.Split(cellReference, "[^\\D]"));
        }

        /// <summary>
        /// Returns the column from a cell reference
        ///</summary>
        public static UInt32Value RowFromReference(string cellReference)
        {
            string numberString = string.Join(null, System.Text.RegularExpressions.Regex.Split(cellReference, "[^\\d]"));
            uint result;
            var ret = new UInt32Value();

            if (uint.TryParse(numberString, out result))
            {
                ret.Value = result;
            }

            return ret;
        }

        /// <summary>
        /// Returns the first reference from a range that may include a sheet.
        ///</summary>
        public static string ReferenceFromRange(string range)
        {
            string references = range;
            if (range.Contains("!")) references = range.Split("!".ToCharArray())[1];

            string reference = references;
            if (references.Contains(":")) reference = references.Split(":".ToCharArray())[0];

            return reference;
        }

        /// <summary>
        /// Gets a copy of the default style information
        /// </summary>
        public static SpreadsheetStyle GetDefaultStyle(SpreadsheetDocument spreadsheet)
        {
            var styles = GetWorkbookStyles(spreadsheet);

            var cellStyleFormats = styles.Stylesheet.CellStyleFormats;
            var cellStyleFormat = (CellFormat)cellStyleFormats.FirstChild;

            var font = (Font)styles.Stylesheet.Fonts.ChildElements[(int)cellStyleFormat.FontId.Value];
            var fill = (Fill)styles.Stylesheet.Fills.ChildElements[(int)cellStyleFormat.FillId.Value];
            var border = (Border)styles.Stylesheet.Borders.ChildElements[(int)cellStyleFormat.BorderId.Value];

            NumberingFormat numberFormat = null;
            if (styles.Stylesheet.NumberingFormats != null && cellStyleFormat.NumberFormatId.Value > 0)
            {
                //Loop through and see if there is a matching border style
                foreach (var formatElement in styles.Stylesheet.NumberingFormats)
                {
                    var format = (NumberingFormat)formatElement;

                    if (cellStyleFormat.NumberFormatId.Value == format.NumberFormatId.Value)
                    {
                        numberFormat = format;
                        break;
                    }
                }
            }

            return new SpreadsheetStyle(font, fill, border, null, numberFormat);
        }

        /// <summary>
        /// Returns the workbook styles part from a spreadsheet document.
        /// </summary>
        public static WorkbookStylesPart GetWorkbookStyles(SpreadsheetDocument spreadsheet)
        {
            return spreadsheet.WorkbookPart.GetPartsOfType<WorkbookStylesPart>().First();
        }

        /// <summary>
        /// Returns a DefinedName matching the name supplied.
        /// </summary>
        public static DefinedName GetDefinedName(SpreadsheetDocument spreadsheet, string rangeName)
        {
            var parts = spreadsheet.WorkbookPart.Workbook.Descendants<DefinedNames>();

            if (parts.Count() > 0)
            {
                foreach (var name in parts.First().Elements<DefinedName>())
                {
                    if (name.Name == rangeName) return name;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the internal sheet id from the sheet name
        /// </summary>
        public static UInt32Value GetSheetId(SpreadsheetDocument spreadsheet, string worksheetName)
        {
            var sheets = spreadsheet.WorkbookPart.Workbook.Descendants<Sheets>().First();

            //Loop through the sheets and get the id
            foreach (Sheet sheet in sheets)
            {
                if (sheet.Name == worksheetName) return sheet.SheetId;
            }

            throw new ApplicationException(string.Format("A sheet id for name {0} was not found.", worksheetName));
        }
    }
}