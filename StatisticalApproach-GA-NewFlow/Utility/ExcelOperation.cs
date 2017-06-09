using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticalApproach.Utility
{
    static class ExcelOperation
    {
        static public void dataTableListToExcel(List<DataTable> dtList, bool _IsHeaderIncluded, string FileFullPath)
        {
            object missing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.Visible = false;
            excel.DisplayAlerts = false;
            try
            {
                Microsoft.Office.Interop.Excel.Workbook wb = excel.Application.Workbooks.Add(Type.Missing);

                foreach (DataTable dt in dtList)
                {
                    Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.Worksheets.Add();
                    int iCol = 0;

                    if (_IsHeaderIncluded == true)
                    {
                        foreach (DataColumn c in dt.Columns)
                        {
                            iCol++;
                            ws.Cells[1, iCol] = c.ColumnName;
                        }
                    }

                    int iRow = 0;
                    foreach (DataRow r in dt.Rows)
                    {
                        iRow++;
                        Console.WriteLine("Row {0} is writing into excel ...", iRow);

                        iCol = 0;
                        foreach (DataColumn c in dt.Columns)
                        {
                            iCol++;
                            if (_IsHeaderIncluded == true)
                            {
                                ws.Cells[iRow + 1, iCol] = r[c.ColumnName];
                            }
                            else
                            {
                                ws.Cells[iRow, iCol] = r[c.ColumnName];
                            }
                        }

                    }
                }
                wb.SaveAs(FileFullPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing);
                ((Microsoft.Office.Interop.Excel._Application)excel).Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ((Microsoft.Office.Interop.Excel._Application)excel).Quit();
            }
        }
    }
}
