using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Great2.Utils
{
    public class ExcelHelper
    {
        private static bool IsPrinting = false;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static WorksheetPart GetWorksheetPartByName(SpreadsheetDocument doc, string sheetName)
        {
            IEnumerable<Sheet> sheets = doc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == sheetName);

            if (sheets.Count() == 0)
                return null;

            string relationshipId = sheets.First().Id.Value;
            WorksheetPart worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(relationshipId);
            return worksheetPart;
        }

        public static string GetCellValue(SpreadsheetDocument doc, string sheet, string address)
        {
            string value = null;

            if (doc == null)
                return null;

            WorksheetPart wspart = GetWorksheetPartByName(doc, sheet);

            if (wspart == null)
                return null;

            // Use its Worksheet property to get a reference to the cell 
            // whose address matches the address you supplied.
            Cell theCell = wspart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == address).FirstOrDefault();

            // If the cell does not exist, return an empty string.
            if (theCell.InnerText.Length > 0)
            {
                value = theCell.InnerText;

                // If the cell represents an integer number, you are done. 
                // For dates, this code returns the serialized value that 
                // represents the date. The code handles strings and 
                // Booleans individually. For shared strings, the code 
                // looks up the corresponding value in the shared string 
                // table. For Booleans, the code converts the value into 
                // the words TRUE or FALSE.
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                                value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;

                            break;

                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;

                                default:
                                    value = "TRUE";
                                    break;
                            }
                            break;
                    }
                }
            }

            return value;
        }

        public static bool SetCellValue(SpreadsheetDocument doc, string sheet, string address, object value)
        {
            if (doc == null)
                return false;

            WorksheetPart wspart = GetWorksheetPartByName(doc, sheet);

            if (wspart == null)
                return false;

            Cell cell = wspart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == address).FirstOrDefault();

            switch (value)
            {
                case string s:
                    cell.DataType = CellValues.SharedString;

                    if (!doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Any())
                        doc.WorkbookPart.AddNewPart<SharedStringTablePart>();

                    var sharedStringTablePart = doc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();

                    if (sharedStringTablePart.SharedStringTable == null)
                        sharedStringTablePart.SharedStringTable = new SharedStringTable();

                    //Iterate through shared string table to check if the value is already present.
                    foreach (SharedStringItem ssItem in sharedStringTablePart.SharedStringTable.Elements<SharedStringItem>())
                    {
                        if (ssItem.InnerText == s)
                        {
                            cell.CellValue = new CellValue(ssItem.ElementsBefore().Count().ToString());
                            return true;
                        }
                    }

                    // The text does not exist in the part. Create the SharedStringItem.
                    var item = sharedStringTablePart.SharedStringTable.AppendChild(new SharedStringItem(new Text(s)));
                    cell.CellValue = new CellValue(item.ElementsBefore().Count().ToString());
                    break;

                case long i:
                    cell.CellValue = new CellValue(i.ToString());
                    cell.DataType = CellValues.Number;
                    break;

                case int i:
                    cell.CellValue = new CellValue(i.ToString());
                    cell.DataType = CellValues.Number;
                    break;

                case float f:
                    cell.CellValue = new CellValue(f.ToString());
                    cell.DataType = CellValues.Number;
                    break;

                case double d:
                    cell.CellValue = new CellValue(d.ToString(CultureInfo.InvariantCulture));
                    cell.DataType = CellValues.Number;
                    break;

                case DateTime dt:
                    cell.DataType = CellValues.Number;
                    //cell.StyleIndex = Convert.ToUInt32(_dateStyleIndex);

                    double oaValue = dt.ToOADate();
                    cell.CellValue = new CellValue(oaValue.ToString(CultureInfo.InvariantCulture));
                    break;

                default:
                    throw new ArgumentException("Unsupported type for parameter value!");
            }

            // Update cell formulas
            doc.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
            doc.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

            // Save the worksheet.
            wspart.Worksheet.Save();

            return true;
        }

        public static bool Print(string filepath)
        {
            if (IsPrinting)
            {
                MetroMessageBox.Show("Printing is already in progress!", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }

            IsPrinting = true;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Excel.Application excelApp = new Excel.Application();
                    
                    // Open the Workbook:
                    Excel.Workbook wb = excelApp.Workbooks.Open(filepath);

                    // Get the first worksheet.
                    // (Excel uses base 1 indexing, not base 0.)
                    Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets[1];

                    Excel.Dialog dialog = excelApp.Dialogs[Excel.XlBuiltInDialog.xlDialogPrint];
                    BringExcelWindowToFront(excelApp);
                    dialog.Show();

                    // Cleanup:
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    Marshal.FinalReleaseComObject(ws);

                    wb.Close(false);
                    Marshal.FinalReleaseComObject(wb);

                    excelApp.Quit();
                    Marshal.FinalReleaseComObject(excelApp);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    IsPrinting = false;
                }
            });

            return true;
        }

        private static void BringExcelWindowToFront(Excel.Application xlApp)
        {
            string caption = xlApp.Caption;
            IntPtr handler = FindWindow(null, caption);
            SetForegroundWindow(handler);
        }
    }
}
