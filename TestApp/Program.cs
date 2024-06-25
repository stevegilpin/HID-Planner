using System;
using TestApp;

DataSimulator.PopulateCourseInfo();
DataSimulator.PopulateStudentInfo();
DataSimulator.PopulateStudentRankings();
DataSimulator.PopulateCourseSchedule();
DataSimulator.RunScheduling();

Console.WriteLine("Done");