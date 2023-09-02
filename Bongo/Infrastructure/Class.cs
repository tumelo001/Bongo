using Bongo.Data;
using Bongo.Models;
using System.Linq.Expressions;

namespace Bongo.Infrastructure
{
    public class GetTimeTable
    {
        private static Session[,] timetableArray;
        private static List<Session> lstSessions;
        private static List<Session> lstCleanSessions;
        public static List<Lecture> Lectures;
        private static List<Lecture> groupedLectures;
        private static List<string> ModuleCodes;//for Control

        public GetTimeTable(string text, bool isForFirstSemester)
        {
            timetableArray = new Session[5, 16];
            groupedLectures = new List<Lecture>();
            TimetableMaker maker = new TimetableMaker();
            lstSessions = maker.Create(text, out Lectures, out ModuleCodes);
            Filter(isForFirstSemester);
        }

        public Session[,] Get(out List<List<Session>> Clashes, out List<Lecture> GroupedLectures)
        {
            Clashes = GetClashes();
            GroupedLectures = GetGroups();

            foreach (Session session in lstCleanSessions)
            {
                timetableArray[session.Period[0] - 1, session.Period[1] - 1] = session;
            }
            return timetableArray;
        }
        public List<Lecture> GetGroups(bool ignoreChosen = false, bool ignoreIgnored = false)
        {
            List<Lecture> GroupedLectures = new();
            for (int i = 0; i < groupedLectures.Count; i++)
            {
                GroupedLectures.Add(groupedLectures[i]);
            }

            List<Lecture> ignoredLectures = new();
            if (!ignoreIgnored)
                foreach (Lecture lect in groupedLectures)
                    foreach (Session s in lect.sessions)
                        if (s.sessionInPDFValue.Contains("ignored"))
                        {
                            GroupedLectures.Remove(lect);
                            ignoredLectures.Add(lect);
                        }

            if (!ignoreChosen)
                foreach (Lecture lect in groupedLectures)
                    if (!ignoredLectures.Contains(lect))
                        foreach (Session s in lect.sessions)
                            if (s.sessionInPDFValue.Contains("selectedGroup"))
                            {
                                timetableArray[s.Period[0] - 1, s.Period[1] - 1] = s;
                                GroupedLectures.Remove(lect);
                            }

            return GroupedLectures;

        }
        public List<List<Session>> GetClashes(bool ignoreChosen = false)
        {
            List<List<Session>> clashes = new List<List<Session>>();
            List<Session> Sessions = new List<Session>(lstSessions);
            List<Session> checkedSessions = new List<Session>();

            for (int i = 0; i < lstSessions.Count; i++)
            {
                if (checkedSessions.Contains(lstSessions[i]))
                    continue;

                List<Session> list = new List<Session>() { lstSessions[i] };
                checkedSessions.Add(lstSessions[i]);

                for (int j = i + 1; j < lstSessions.Count; j++)
                {
                    if (checkedSessions.Contains(lstSessions[j]))
                        continue;

                    if (Extensions.ContainsClashes(new string[] { lstSessions[i].sessionInPDFValue, lstSessions[j].sessionInPDFValue }))
                    {
                        list.Add(lstSessions[j]);
                        checkedSessions.Add(lstSessions[j]);
                    }
                }

                if (list.Count > 1 && (ignoreChosen || !HasSelection(list, Sessions)))
                {
                    clashes.Add(list);
                }

            }

            lstCleanSessions = new List<Session>(Sessions);
            return clashes;
        }
        public List<List<Session>> GetSpecificClashes(string session)
        {
            List<List<Session>> allClashes = GetClashes(true);
            foreach (List<Session> list in allClashes)
            {
                List<string> sessions = list.Select(s => s.sessionInPDFValue).ToList();
                if (sessions.Contains(session))
                    return new List<List<Session>> { list };
            }
            return default;
        }
        private static bool HasSelection(List<Session> list, List<Session> sessions)
        {
            bool hasSelection = false;
            foreach (Session s in list)
                if (s.sessionInPDFValue.Contains("selectedClass"))
                {
                    hasSelection = true;
                }
                else
                    sessions.Remove(s);

            return hasSelection;
        }
        private static void Filter(bool isForFirstSemester)
        {
            Expression<Func<Lecture, bool>> required = isForFirstSemester ? lect => lect.isFirstSemester : lect => lect.isSecondSemester;
            Lectures = Lectures.Where(required.Compile()).ToList();

            //Add Sessions to sessions list and clashing lectures to clashings list
            Expression<Func<Lecture, bool>> ungrouped = lect => lect.sessions.Count == 1;
            lstSessions = Lectures.Where(ungrouped.Compile()).Select(l => l.sessions[0]).ToList();

            Expression<Func<Lecture, bool>> grouped = lect => lect.sessions.Count > 1 && !lect.sessions[0].sessionInPDFValue.Contains("CLASH");
            groupedLectures = Lectures.Where(grouped.Compile()).ToList();

            MoveAllClashingToSessions();

        }
        private static void MoveAllClashingToSessions()
        {
            List<Lecture> GroupedLectures = new List<Lecture>(groupedLectures);
            foreach (Lecture lect in groupedLectures)
            {
                List<Session> checkedSessions = new List<Session>();
                bool AllSameTime = true;
                for (int i = 0; i < lect.sessions.Count; i++)
                {
                    if (checkedSessions.Contains(lect.sessions[i]))
                        continue;
                    checkedSessions.Add(lect.sessions[i]);

                    for (int j = i + 1; j < lect.sessions.Count; j++)
                    {
                        if (checkedSessions.Contains(lect.sessions[j]))
                            continue;
                        checkedSessions.Add(lect.sessions[j]);

                        if (!Extensions.ContainsClashes(new string[] { lect.sessions[i].sessionInPDFValue, lect.sessions[j].sessionInPDFValue }))
                        {
                            AllSameTime = false;
                            break;
                        }
                    }

                    if (AllSameTime)
                    {
                        Session clashing = lstSessions.FirstOrDefault(s => Extensions.ContainsClashes(new string[] { s.sessionInPDFValue, lect.sessions[0].sessionInPDFValue }));
                        if (clashing != null)
                        {
                            lstSessions.AddRange(lect.sessions);
                            GroupedLectures.Remove(lect);
                        }
                    }

                }
            }
            groupedLectures = new List<Lecture>(GroupedLectures);
        }
    }

}
