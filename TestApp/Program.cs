using System;
using System.IO;
using TestApp;

DataSimulator.PopulateCourseInfo();
DataSimulator.PopulateStudentInfo(); // todo find sample data with student IDs included
DataSimulator.PopulateStudentRankings();

Console.WriteLine("Done");