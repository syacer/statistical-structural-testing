using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GADEApproach
{
        static class ExcelOperation
        {
            public static DataSet ReadExcelFile(string excelPath)
            {
                DataSet ds = new DataSet();

                string connectionString = GetConnectionString(excelPath);

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.Connection = conn;

                    // Get all Sheets in Excel File
                    DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    // Loop through all Sheets to get data
                    foreach (DataRow dr in dtSheet.Rows)
                    {
                        string sheetName = dr["TABLE_NAME"].ToString();

                        if (!sheetName.EndsWith("$"))
                            continue;

                        // Get all rows from the Sheet
                        cmd.CommandText = "SELECT * FROM [" + sheetName + "]";

                        DataTable dt = new DataTable();
                        dt.TableName = sheetName;

                        OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                        da.Fill(dt);
                        ds.Tables.Add(dt);
                    }

                    cmd = null;
                    conn.Close();
                }

                return ds;
            }

            static private string GetConnectionString(string excelPath)
            {
                Dictionary<string, string> props = new Dictionary<string, string>();

                // XLSX - Excel 2007, 2010, 2012, 2013
                props["Provider"] = "Microsoft.ACE.OLEDB.16.0;";
                props["Extended Properties"] = "Excel 12.0 XML";
                props["Data Source"] = excelPath;

                // XLS - Excel 2003 and Older
                //props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
                //props["Extended Properties"] = "Excel 8.0";
                //props["Data Source"] = "C:\\MyExcel.xls";

                StringBuilder sb = new StringBuilder();

                foreach (KeyValuePair<string, string> prop in props)
                {
                    sb.Append(prop.Key);
                    sb.Append('=');
                    sb.Append(prop.Value);
                    sb.Append(';');
                }

                return sb.ToString();
            }
            static public void dataTableListToExcel(List<DataTable> dtList, bool _IsHeaderIncluded, string FileFullPath, bool newFile)
            {
                object missing = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.DisplayAlerts = false;
                Microsoft.Office.Interop.Excel.Workbook wb = null;
                try
                {
                    if (newFile == true)
                {
                    File.Create(FileFullPath).Close();
                    wb = excel.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                }
                else
                {
                    wb = excel.Workbooks.Open(FileFullPath);
                }

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
