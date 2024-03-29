﻿using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OJTTraining.Service
{
    public static class DownloadService
    {
        public static byte[] GenerateExcel<T>(IEnumerable<T> list)
        {
            using var stream = new MemoryStream();
            using var package = new ExcelPackage(stream);
            var workSheet = package.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells.LoadFromCollection(list, true);

            var properties = typeof(T).GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].PropertyType == typeof(DateTime) || properties[i].PropertyType == typeof(DateTime?))
                {
                    var dateTimeString = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    dateTimeString = $"[$-{CultureInfo.CurrentCulture.Name}]{dateTimeString.Replace("tt", "AM/PM")}";
                    workSheet.Column(i + 1).Style.Numberformat.Format = dateTimeString;
                }
            }

            return package.GetAsByteArray();
        }

        public static byte[] GenerateCSV<T>(IEnumerable<T> list)
        {
            var delimiter = ',';

            var properties = typeof(T).GetProperties()
                 .Where(n =>
                 n.PropertyType == typeof(string)
                 || n.PropertyType == typeof(bool)
                 || n.PropertyType == typeof(char)
                 || n.PropertyType == typeof(byte)
                 || n.PropertyType == typeof(decimal)
                 || n.PropertyType == typeof(int)
                 || n.PropertyType == typeof(DateTime)
                 || n.PropertyType == typeof(DateTime?));

            using var sw = new StringWriter();

            var header = properties
            .Select(n => n.Name)
            .Aggregate((a, b) => a + delimiter + b);
            sw.WriteLine(header);

            foreach (var item in list)
            {
                var row = properties
                    .Select(n =>
                    {
                        var value = n.GetValue(item, null);
                        if (value == null)
                        {
                            return "null";
                        }
                        else if (n.PropertyType == typeof(DateTime))
                        {
                            DateTime valueDateTime1 = (DateTime)value;
                            return valueDateTime1.ToString("g", CultureInfo.CurrentCulture);
                        }
                        else if (n.PropertyType == typeof(DateTime?))
                        {
                            DateTime? valueDateTime2 = (DateTime?)value;
                            return valueDateTime2?.ToString("g", CultureInfo.CurrentCulture);
                        }
                        else if (n.PropertyType == typeof(string))
                        {
                            return "\"" + value.ToString() + "\"";
                        }
                        else
                        {
                            return value.ToString();
                        }
                    })
                    .Aggregate((a, b) => a + delimiter + b);
                sw.WriteLine(row);
            }

            return Encoding.UTF8.GetBytes(sw.ToString());
        }
    }
}
