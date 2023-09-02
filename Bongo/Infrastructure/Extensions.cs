using System.Text.RegularExpressions;
using Bongo.Data;
using Bongo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bongo.Infrastructure
{
    public class Extensions
    {
        public static bool ContainsClashes(string[] sessions)
        {
            // Create a List to store the session time ranges
            List<(DateTime StartTime, DateTime EndTime, string Day)> sessionTimes = new List<(DateTime, DateTime, string)>();

            Regex timePattern = new Regex(@"(\d{2}:\d{2}) (\d{2}:\d{2})");
            Regex dayPattern = new Regex(@"Monday|Tuesday|Wednesday|Thursday|Friday");

            foreach (string session in sessions)
            {
                Match timeMatch = timePattern.Match(session);
                Match dayMatch = dayPattern.Match(session);

                if (timeMatch.Success && dayMatch.Success)
                {
                    DateTime startTime = DateTime.Parse(timeMatch.Groups[1].Value);
                    DateTime endTime = DateTime.Parse(timeMatch.Groups[2].Value);
                    string Day = dayMatch.Value;
                    // Check for overlapping sessions
                    foreach (var sessionTime in sessionTimes)
                    {
                        if (Day == sessionTime.Day && ((startTime >= sessionTime.StartTime && startTime < sessionTime.EndTime) ||
                            (endTime > sessionTime.StartTime && endTime <= sessionTime.EndTime) ||
                            (startTime <= sessionTime.StartTime && endTime >= sessionTime.EndTime)))
                        {
                            return true; // Clash detected
                        }
                    }

                    sessionTimes.Add((startTime, endTime, Day));
                }
            }
            return false;
        }
        public static int GetInterval(string sessionPdfValue)
        {
            Regex timePattern = new Regex(@"(\d{2}:\d{2}) (\d{2}:\d{2})");
            Match timeMatch = timePattern.Match(sessionPdfValue);
            int startTime = int.Parse(timeMatch.Groups[1].Value.Substring(0, 2));
            int endTime = int.Parse(timeMatch.Groups[2].Value.Substring(0, 2));
            return (endTime - startTime);
        }

        public static void AddNewUserModuleColor(ref IRepositoryWrapper _repo, string Username, string text)
        {
            List<string> moduleCodes;
            List<Lecture> lects;//for Conrtol
            new TimetableMaker().Create(text, out lects, out moduleCodes);
            foreach (string moduleCode in moduleCodes)
            {
                _repo.ModuleColor.Update(new ModuleColor
                {
                    ColorId = _repo.Color.GetByName("no-color").ColorId,
                    Username = Username,
                    ModuleCode = moduleCode
                });
            }
        }

        public static bool HasGroups(List<Session> sessions)
        {
            Regex groupPattern = new Regex(@"Group [A-Z]{1,2}[\d]?");
            List<string> groups = new List<string>();
            foreach (var session in sessions)
            {
                Match groupMatch = groupPattern.Match(session.sessionInPDFValue);
                if (groupMatch.Success)
                {
                    groups.Add(groupMatch.Value);
                }
            }
            return groups.Distinct().Count() < groups.Count;
        }

    }

}
