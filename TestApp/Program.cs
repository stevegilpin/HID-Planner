using System;
using Data;
using TestApp;

// DataManager.ClearApplicationData();
DataSimulator.RunScheduling();
DataSimulator.CreateReports();
DataSimulator.DumpResultsFromLatestRun();

Console.WriteLine("Done");