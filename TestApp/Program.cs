using System;
using TestApp;

DataSimulator.PopulateStudentInfo();
DataSimulator.PopulateStudentRankings();
DataSimulator.PopulateCourseInfo();
DataSimulator.RunScheduling();

ReportTest.RunTest();

Console.WriteLine("Done");