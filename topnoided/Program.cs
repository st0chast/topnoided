using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace top_noided
{
    internal class Program
    {
        private static void Main()
        {
            // Game loop pattern
            while (true)
            {
                // Builds a table where all relevant processes' data is stored
                ProcessesTable PT = new();
                PT.Table = new DataTable("ProcTable");
                // Column creation is wrapped into a method
                AddColumn(PT.Table, $"{typeof(int)}", "PID");
                AddColumn(PT.Table, $"{typeof(string)}", "Process Name");
                // Working Set Memory
                AddColumn(PT.Table, $"{typeof(int)}", "WS Memory");
                AddColumn(PT.Table, $"{typeof(float)}", "CPU%");
                // Gets all processes' data
                Process[] processes = Process.GetProcesses();
                // Removes data for Session 0 processes
                processes = processes.Where(x => x.SessionId != 0).ToArray();
                // ProcessData includes CPU and working set memory performance counters
                ProcessData[] rawTable = new ProcessData[processes.Length];
                // Translates data from Processes[] to ProcessData[] and creates performance counters
                GetRawData(processes, rawTable);
                // Recommended time for good performance counter data is 1s, 2s is used for ergonomics
                Thread.Sleep(2000);
                // Writes performance counters' data to ProcessData[]
                UpdateRawData(rawTable);
                // Prepares ProcessData[] for output
                FillRows(PT.Table, rawTable);
                // Prints processes data descending by CPU%
                ShowTable(PT.Table);
            }
        }

        private class ProcessesTable
        {
            // Stores PID, Process Name, WS Memory, CPU%
            // Enables sorting
            public DataTable Table { get; set; }
        }

        private class ProcessData
        {
            public int PID { get; set; }
            public string Name { get; set; }
            public PerformanceCounter WSMem { get; set; }
            public float WSMemUsage { get; set; }
            public PerformanceCounter CPU { get; set; }
            public float CPUUsage { get; set; }
        }

        private static void AddColumn(DataTable table, string type, string name)
        {
            // For example: AddColumn(PT.Table, $"{typeof(int)}", "PID");
            var column = new DataColumn
            {
                DataType = Type.GetType(type),
                ColumnName = name
            };
            table.Columns.Add(column);
        }

        private static void GetRawData(Process[] processes, ProcessData[] rawTable)
        {
            int i = 0;
            foreach (var process in processes)
            {
                ProcessData PD = new();
                // The warning that PerformanceCounter is only supported on Windows is irrelevant
                // The whole app is for Windows only, Net5 is just nicer to write in
                PD.CPU = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                PD.CPUUsage = PD.CPU.NextValue();
                PD.PID = process.Id;
                PD.Name = process.ProcessName;
                PD.WSMem = new PerformanceCounter("Process", "Working Set", PD.Name);
                PD.WSMemUsage = PD.WSMem.NextValue();
                rawTable[i] = PD;
                i++;
            }
        }

        private static void UpdateRawData(ProcessData[] rawTable)
        {
            foreach (var process in rawTable)
            {
                // This is how you get CPU usage data over time.
                process.CPUUsage = process.CPU.NextValue();
            }
        }

        private static void FillRows(DataTable table, ProcessData[] rawTable)
        {
            int processorCount = Environment.ProcessorCount;
            foreach (var process in rawTable)
            {
                // The purpose of topnoided is to take a peek at top processes
                if (process.CPUUsage == 0) { continue; }
                DataRow row;
                row = table.NewRow();
                row["PID"] = process.PID.ToString();
                row["Process Name"] = process.Name;
                // Converts it into MB
                row["WS Memory"] = process.WSMemUsage / 1024 / 1024;
                // Without this your CPU% might not match the one in Task Manager
                row["CPU%"] = Math.Round(process.CPUUsage / processorCount, 4);
                table.Rows.Add(row);
            }
        }

        public static string Shape(DataRow r, int pos, int space)
        {
            // Wraps creation of lines that are easy to read in a terminal
            var sb = new StringBuilder();
            // Translates a cell into a string
            string uncut = String.Format($"{r[pos]}");
            // Cuts the string if it's longer than the alotted space
            sb.Append($"{uncut.Substring(0, Math.Min(space, uncut.Length))}");
            // If it's shorter, fill it with spaces for smoother display
            // topnoided resets the console cursor to (0,0) and overwrites everything to avoid flickers
            var delta = space - sb.Length;
            while (delta > 0)
            {
                sb.Append(' ');
                delta--;
            }

            return String.Format($"{sb}");
        }

        private static void ShowTable(DataTable table)
        {
            int lines = 0;
            var sb = new StringBuilder();
            DataView processView = table.DefaultView;
            processView.Sort = "CPU% desc";
            DataTable processViewCPUUsageDesc = processView.ToTable();
            sb.AppendLine("PID     WSMem   CPU%    Name");
            foreach (DataRow r in processViewCPUUsageDesc.Rows)
            {
                sb.Append(Shape(r, 0, 8));
                sb.Append(Shape(r, 2, 8));
                sb.Append(Shape(r, 3, 8));
                sb.Append(Shape(r, 1, 64));
                sb.AppendLine();
                lines++;
                if (lines >= 20) { break; }
            }

            // topnoided resets the console cursor to (0,0) and overwrites everything to avoid flickers
            while (lines < 20)
            {
                // 8+8+8+64 spaces
                sb.AppendLine("                                                                                        ");
                lines++;
            }
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(sb);
        }
    }
}
