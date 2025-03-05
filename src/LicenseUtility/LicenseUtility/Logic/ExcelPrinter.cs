using NugetUtility.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;

namespace NugetUtility.Logic
{
    internal class ExcelPrinter
    {
        public static void PrintToExcel(Dictionary<string, Package> packages, string componentName, string outputPath)
        {
            var fi = new FileInfo(Path.Combine(outputPath, "packages.xlsx"));
            using (var p = new ExcelPackage(fi))
            {
                var sheet = p.Workbook.Worksheets.Add("Components");
                AddHeader(sheet, componentName);
                int rowCounter = 3;
                foreach (var package in packages.Values)
                {
                    AddEntry(sheet, rowCounter, package.Metadata);
                    rowCounter++;
                }
                FormatWorksheet(sheet, rowCounter - 1);
                p.Save();
            }
        }

        private static void AddHeader(ExcelWorksheet sheet, string componentName)
        {
            sheet.Cells[1, 1].Value = $"TruTops Boost – Third Party Components - {componentName}";
            sheet.Cells[1, 1, 1, 7].Merge = true; //Merge columns start and end range
            sheet.Cells[1, 1, 1, 7].Style.Font.Bold = true; //Font should be bold
            sheet.Cells[1, 1, 1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Alignment is center
            sheet.Cells[1, 1, 1, 7].Style.Font.Size = 18;
            sheet.Cells[1, 1, 1, 7].Style.Fill.PatternType = ExcelFillStyle.LightGray;
            sheet.Cells[1, 1, 1, 7].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            sheet.Cells[2, 1].Value = "Component";
            sheet.Cells[2, 1].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 2].Value = "Version";
            sheet.Cells[2, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 3].Value = "Author / Company";
            sheet.Cells[2, 3].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 4].Value = "Url";
            sheet.Cells[2, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 5].Value = "Description";
            sheet.Cells[2, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 6].Value = "License";
            sheet.Cells[2, 6].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 7].Value = "License Text";
            sheet.Cells[2, 7].Style.Border.BorderAround(ExcelBorderStyle.Medium);
            sheet.Cells[2, 1, 2, 7].Style.Font.Bold = true;
            sheet.Cells[2, 1, 2, 7].Style.Fill.PatternType = ExcelFillStyle.LightGray;
        }

        private static void AddEntry(ExcelWorksheet sheet, int row, Metadata package)
        {
            sheet.Cells[row, 1].Value = package.Id;
            sheet.Cells[row, 2].Value = package.Version;
            sheet.Cells[row, 3].Value = package.Authors;
            sheet.Cells[row, 4].Value = package.ProjectUrl;
            sheet.Cells[row, 5].Value = package.Description;
            sheet.Cells[row, 6].Value = package.License.Text;
            sheet.Cells[row, 7].Value = package.License.LicenseText;
        }

        private static void FormatWorksheet(ExcelWorksheet sheet, int numberOfRows)
        {
            sheet.Cells[3, 1, numberOfRows, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            sheet.Cells[3, 1, numberOfRows, 6].AutoFitColumns();
            sheet.Column(5).Width = 50;
            sheet.Cells[3, 5, numberOfRows, 5].Style.WrapText = true;
            sheet.Column(7).Width = 100;
            sheet.Cells[3, 7, numberOfRows, 7].Style.WrapText = true;
        }
    }
}
