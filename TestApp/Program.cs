using System;
using Database;
using Database.Objects;
using HIDLib.DataImport;

var rows = CsvReader.ReadCourseInfo(@"C:\sql\CourseInfoSample.csv", "2024");
using var ctx = DbConnection.GetDbContext();
ctx.CourseInfo.AddRange(rows);
ctx.SaveChanges();