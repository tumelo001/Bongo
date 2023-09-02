using Bongo.Models;
using System.Text.RegularExpressions;

namespace Bongo.Data
{
   
    public class TimetableMaker
    {
        #region Private Lists
        public static List<Session> lstSessions;
        public static List<string> timetableLines;
        public static List<ModuleData> modulesData;
        public static List<Lecture> Lectures;
        #endregion

        public TimetableMaker()
        {
            lstSessions = new List<Session>();
            timetableLines = new List<string>();
            modulesData = new List<ModuleData>();
            Lectures = new List<Lecture>();
            
        }

        public List<Session> Create(string text, out List<Lecture> lectures, out List<string> moduleCodes)
        {
            ReadTextToList(text);
            ExtractModules();
            ExtractModuleData();
            ExtractLectures();
            ExtractSessions();

            lectures = Lectures;
            moduleCodes = new List<string>(modulesData.Select(md => md.ModuleCode));
            return lstSessions;
        }
        private static void ReadTextToList(string text)
        {
            //make a list of the lines in the text file
            timetableLines = text.Split('\n').ToList();

        }
        public static void ExtractModules()
        {
            //Get the list of modulesData
            Regex modulepattern = new Regex(@"[A-Z]{4}[\d]{4}|CLASH!![\d]");
            for (int i = 0; i < timetableLines.Count; i++)
            {
                Match match = modulepattern.Match(timetableLines[i]);
                if (match.Success)
                    modulesData.Add(new ModuleData { ModuleCode = match.Value });
            }
        }
        public static void ExtractModuleData()
        {
            //Add the sessions details
            for (int i = 0; i < modulesData.Count; i++)
            {
                int startIndex = timetableLines.IndexOf(modulesData[i].ModuleCode) + 1;
                int endIndex = i == modulesData.Count - 1 ? timetableLines.Count - 1 : timetableLines.IndexOf(modulesData[i + 1].ModuleCode);
                for (int j = startIndex; j < endIndex; j++)
                {
                    modulesData[i].moduleData.Add(timetableLines[j]);
                }
                modulesData[i].moduleData = modulesData[i].moduleData.Distinct().ToList();
            }
        }
        public static void ExtractLectures()
        {
            //Get the list of lectures
            Regex lecturepattern = new Regex(@"Lecture [0-9]?|Tutorial [0-9]?|Practical [0-9]?");
            foreach (ModuleData module in modulesData)
                for (int i = 0; i < module.moduleData.Count; i++)
                {
                    Match match = lecturepattern.Match(module.moduleData[i]);
                    if (match.Success)
                        Lectures.Add(new Lecture { ModuleCode = module.ModuleCode, LectureDesc = match.Value });
                }
        }
        private static void ExtractSessions()
        {
            Regex timepattern = new Regex(@"[0-9]{2}:[0-9]{2} [0-9]{2}:[0-9]{2}");
            Regex daypattern = new Regex(@"Monday|Tuesday|Wednesday|Thursday|Friday");
            Regex lecturepattern = new Regex(@"Lecture [0-9]?|Tutorial [0-9]?|Practical [0-9]?");

            foreach (Lecture lect in Lectures)
            {
                ModuleData module = modulesData.FirstOrDefault(m => m.ModuleCode == lect.ModuleCode);
                int startIndex = module.moduleData.IndexOf(lect.LectureDesc);
                for (int i = startIndex + 1; i < module.moduleData.Count; i++)
                {
                    Match match = lecturepattern.Match(module.moduleData[i]);
                    if (!match.Success)
                    {
                        Match timeMatch = timepattern.Match(module.moduleData[i]);
                        Match dayMatch = daypattern.Match(module.moduleData[i]);

                        if(timeMatch.Success && dayMatch.Success)
                        {
                            Session session = new Session
                            {
                                ModuleCode = lect.ModuleCode,
                                sessionType = lect.LectureDesc,
                                sessionInPDFValue = module.moduleData[i],
                                Venue = module.moduleData[i].Substring(0, timeMatch.Index - 1),
                                Period = Periods.GetPeriod(timeMatch.Value, dayMatch.Value)
                            };

                            lect.sessions.Add(session);
                        }
                    }
                    else
                        break;
                }
            }
        }   
    }
    public static class Periods
    {
        public static int[] GetPeriod(string time, string day)
        {
            int[] period = new int[2];

            switch (day.ToUpper())
            {
                case "MONDAY": period[0] = 1; break;
                case "TUESDAY": period[0] = 2; break;
                case "WEDNESDAY": period[0] = 3; break;
                case "THURSDAY": period[0] = 4; break;
                case "FRIDAY": period[0] = 5; break;
            }
            period[1] = int.Parse(time.Substring(0, 2)) - 6;

            return period;
        }
    }
    public class ModuleData
    {
        public string ModuleCode { get; set; }
        public List<string> moduleData { get; set; } = new List<string>();
    }
    public class Lecture
    {
        public string ModuleCode { get; set; }
        public string LectureDesc { get; set; }
        public bool isFirstSemester => (int.Parse(ModuleCode.Substring(6, 1)) == 0 || (int)ModuleCode[6] % 2 == 1);
        public bool isSecondSemester => (int)ModuleCode[6] % 2 == 0;
        public List<Session> sessions { get; set; } = new List<Session>();
    }

}
