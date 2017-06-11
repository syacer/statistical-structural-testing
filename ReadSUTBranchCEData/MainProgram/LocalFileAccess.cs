﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainProgram
{
    public class LocalFileAccess
    {
        public LocalFileAccess()
        {

        }

        public int StoreLinesToList(string filePath, List<string> list)
        {
            try
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        list.Add(line);
                    }
                    r.Close();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
        public int StoreListToLines(string filePath, List<string> list)
        {
            try
            {
                using (System.IO.StreamWriter newTask = new System.IO.StreamWriter(filePath, append: true))
                {
                    foreach (string str in list)
                    {
                        newTask.WriteLine(str);
                    }
                    newTask.Close();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public int StoreListToLinesAppend(string filePath, List<string> list)
        {
            try
            {
                using (System.IO.StreamWriter newTask = File.AppendText(filePath))
                {
                    foreach (string str in list)
                    {
                        newTask.WriteLine(str);
                    }
                    newTask.Close();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}