using System;
using TestApp;

DataSimulator.PopulateStudentInfo();
DataSimulator.PopulateStudentRankings();
DataSimulator.PopulateCourseInfo();
DataSimulator.RunScheduling();

Console.WriteLine("Done");