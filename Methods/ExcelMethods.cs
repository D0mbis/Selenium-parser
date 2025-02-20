﻿using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Selenium
{
    internal class ExcelMethods : IDisposable
    {

        private string path;

        public ExcelMethods()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            path = System.Configuration.ConfigurationManager.AppSettings["path"];

        }
        public void GetCorrectlyPath()
        {
            while (true)
            {
                if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    MessageBox.Show("File path contains ivalid char!\nTry again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SaveAs();
                    continue;
                }
                else if (string.IsNullOrEmpty(path))
                {
                    var result = MessageBox.Show("Please provide a path to save the file:", "Save results file", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.OK)
                    {
                        SaveAs();
                        continue;
                    }
                    else
                    {
                        var result2 = MessageBox.Show($"Are you sure? \nPress \"Yes\" end results will be lost.", "Attention!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result2 == MessageBoxResult.Yes)
                        {
                            break;
                        }
                        else continue;
                    }
                }
                if (File.Exists(path))
                {
                    try
                    {
                        var result = MessageBox.Show($"File: {path} will be overwrite. \nAre you sure?", "Worning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            File.Delete(path);
                            break;
                        }
                        else
                        {
                            SaveAs();
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                else if (!File.Exists(path))
                {
                    var result = MessageBox.Show($"File: {path} does not exist \nDo you want to create a new file?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        using (FileStream stream = new FileStream(path, FileMode.Create))
                            break;
                    }
                    else
                    {
                        SaveAs();
                        continue;
                    }
                }
            }
        }

        public string SaveAs()
        {
            while (true)
            {
                try
                {
                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "Книга Excel 97-2003 (*.xlsx) | *.xlsx"
                    };
                    if (saveDialog.ShowDialog() == true)
                    {
                        path = saveDialog.FileName.ToString();
                    }
                    return path;
                }
                catch
                {
                    continue;
                }
            }
        }

        public string SaveExcelFileNew(Dictionary<string, List<string>> dictionaryOfResults, DateTime timeStart)
        {
            var timeEnd = DateTime.Now;
            GetCorrectlyPath();
            using (CheckUserInput check = new CheckUserInput()) { check.SaveAppConfig(path); }
            using (ExcelPackage package = new ExcelPackage(path))
            {
                var _worksheet = package.Workbook.Worksheets.Add("Results");
                var cells = _worksheet.Cells;
                int rowNumber = 2, columnNumber;
                foreach (var item in dictionaryOfResults)
                {
                    columnNumber = 1;
                    cells[rowNumber, columnNumber].Value = item.Key;
                    foreach (var item2 in item.Value)
                    {
                        columnNumber++;
                        try
                        {
                            cells[rowNumber, columnNumber].Value = item2;
                        }
                        catch { }
                    }
                    rowNumber++;
                }
                try
                {
                    AddStylesToWorkSheet(_worksheet);
                    package.Save();
                    var allTime = timeEnd.Subtract(timeStart).ToString("mm\\:ss");
                    MessageBox.Show($"Results were saved: {dictionaryOfResults.Count}\nTime spent: {allTime} minutes", "Successfully completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    return path;
                }
                catch
                {
                    MessageBox.Show("Folder does not exist, select an existing folder.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    path = SaveAs();
                    package.SaveAs(path);
                    return path;
                }
            }
        }

        public void AddStylesToWorkSheet(ExcelWorksheet _worksheet)
        {
                string[] headers = { "Name", "Type of activity", "Phone", "Adress", "Website" };
                int column = 1;
                foreach (string item in headers)
                {
                    var cell = _worksheet.Cells[1, column];
                    cell.Value = item;
                    cell.Style.Font.Size = 15;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                    cell.Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    column++;
                }
                _worksheet.Cells.AutoFitColumns(0,75);
                _worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        public void Dispose()
        {

        }
    }
}