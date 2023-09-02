using Bongo.Data;
using Bongo.Infrastructure;
using Bongo.Models;
using Bongo.Models.ViewModels;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace Bongo.Controllers
{
    [Authorize]
    public class SessionController : Controller
    {
        private IRepositoryWrapper _repository;
        private static Timetable table;
        private List<List<Session>> ClashesList;
        private List<Lecture> GroupedList;
        private static bool firstSemester;
        private static GetTimeTable timetableGetter;
        private static Session[,] data;

        public SessionController(IRepositoryWrapper repository)
        {
            _repository = repository;
        }

        private void Initialise(bool isForFirstSemester)
        {
            firstSemester = isForFirstSemester;
            table = _repository.Timetable.GetUserTimetable(User.Identity.Name);
            timetableGetter = new GetTimeTable(table.TimetableText, isForFirstSemester);
            data = timetableGetter.Get(out ClashesList, out GroupedList);
        }

        public IActionResult Index()
        {
            bool isForFirstSemester = Request.Cookies["isForFirstSemester"] == "true";
            Initialise(isForFirstSemester);
            MarkDisabled();

            if (ClashesList.Count > 0)
                return View("Clashes", ClashesList);
            if (GroupedList.Count > 0)
                return View("Groups", new GroupsViewModel { GroupedLectures = GroupedList });

            TempData["isForFirstSemester"] = isForFirstSemester;
            return RedirectToAction("Index", "Home", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
        }
        [HttpPost]
        public IActionResult Clashes(string[] Sessions)
        {
            RemoveSelectedWhereNecessary("class");

            foreach (string session in Sessions)
            {
                if (Extensions.ContainsClashes(Sessions))
                {
                    ModelState.AddModelError("", "You have selected clashing sessions");
                    return View(timetableGetter.GetClashes());
                }

                if (session is not null)
                {
                    if (!session.Contains("Tap to handle") || (session.Contains("Tap to handle") && table.TimetableText.Contains(session)))
                        table.TimetableText = table.TimetableText.
                            Replace(session, session + "selectedClass");
                    else
                    {
                        Regex timepattern = new Regex(@"[\d]{2}:[\d]{2} [\d]{2}:[\d]{2}");
                        Regex daypattern = new Regex(@"Monday|Tuesday|Wednesday|Thursday|Friday");
                        Match timeMatch = timepattern.Match(session);
                        string startTime = timeMatch.Value.Substring(0, 5);
                        string endTime = timeMatch.Value.Replace(startTime + " ", "");
                        int semesterNo = Request.Cookies["isForFirstSemester"] == "true" ? 1 : 2;
                        AddNewSession(new AddSessionViewModel
                        {
                            ModuleCode = $"CCCC12{semesterNo}3",
                            SessionType = "Lecture",
                            SessionNumber = 1,
                            startTime = startTime,
                            endTime = endTime,
                            Day = $"{daypattern.Match(session).Value}selectedClass",
                            Venue = "Tap to handle"

                        });
                    }

                }
            }

            UpdateAndSave();

            return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
        }

        [HttpPost]
        public IActionResult Groups(GroupsViewModel model)
        {
            List<Lecture> grouped = timetableGetter.GetGroups();
            List<string> sessions = new List<string>(model.Sessions);
            string[] strings = GetClashing(model.Sessions);
            if (strings.Count() > 0)
            {
                foreach (string item in strings)
                {
                    sessions.Remove(item);
                }
            }

            List<Lecture> clashing = new();
            List<Lecture> unclashing = new();

            if (strings.Length > 0)
            {

                foreach (var session in strings)
                {
                    foreach (var lect in grouped)
                    {
                        if (lect.sessions.Select(l => l.sessionInPDFValue).Contains(session))
                        {
                            clashing.Add(lect);
                        }
                        else
                            unclashing.Add(lect);

                    }
                }
            }

            RemoveSelectedWhereNecessary("group", unclashing);
            RemoveSelectedWhereNecessary("ignored", unclashing);

            if (model.Ignore != null)
                foreach (string s in model.Ignore)
                {
                    table.TimetableText = table.TimetableText.Replace(s, s + "ignored");
                    sessions.Remove(s);
                }

            foreach (string session in sessions)
            {
                if (session is not null)
                {
                    Lecture sessionLecture = grouped.FirstOrDefault(lect => lect.sessions.Select(s => s.sessionInPDFValue).Contains(session));
                    if (model.SameGroups != null && model.SameGroups.Contains($"{sessionLecture.ModuleCode} {sessionLecture.LectureDesc}"))
                    {
                        Regex groupPattern = new Regex(@"Group [A-Z]{1,2}[\d]?");
                        foreach (Session s in sessionLecture.sessions)
                            if (groupPattern.Match(s.sessionInPDFValue).Success)
                                table.TimetableText = table.TimetableText.Replace(s.sessionInPDFValue, s.sessionInPDFValue + "selectedGroup");
                    }
                    else
                        table.TimetableText = table.TimetableText.Replace(session, session + "selectedGroup");
                }
            }

            UpdateAndSave();
            if (strings.Length > 0)
            {
                ModelState.AddModelError("", "You have selected clashing sessions. Please ensure you don't select sessions that are clashing.");
                return View(new GroupsViewModel { GroupedLectures = clashing });
            }

            return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });

        }

        private string[] GetClashing(string[] sessions)
        {
            List<string> checkedSessions = new List<string>();
            List<string> strings = new List<string>();

            for (int i = 0; i < sessions.Length; i++)
            {
                if (checkedSessions.Contains(sessions[i]))
                    continue;

                List<string> list = new List<string>() { sessions[i] };
                checkedSessions.Add(sessions[i]);

                for (int j = i + 1; j < sessions.Length; j++)
                {
                    if (checkedSessions.Contains(sessions[j]))
                        continue;

                    if (Extensions.ContainsClashes(new string[] { sessions[i], sessions[j] }))
                    {
                        list.Add(sessions[j]);
                        checkedSessions.Add(sessions[j]);
                    }
                }
                if (list.Count > 1)
                    strings.AddRange(list);
            }

            return strings.ToArray();
        }

        public IActionResult ManageModules()
        {
            PopulateColorDLL();
            var colors = _repository.Color.FindAll();
            var moduleColors = _repository.ModuleColor.GetByCondition(m => m.Username == User.Identity.Name);
            var x = moduleColors.Where(m =>
                 Request.Cookies["isForFirstSemester"] == "true" ? (int.Parse(m.ModuleCode.Substring(6, 1)) == 0 || int.Parse(m.ModuleCode.Substring(6, 1)) % 2 == 1)
                                : int.Parse(m.ModuleCode.Substring(6, 1)) % 2 == 0);
            return View("EditColors", new ModulesColorsViewModel()
            {
                ModuleColors = x,
                Colors = colors
            });
        }

        [HttpPost]
        public IActionResult DeleteModule(string ModuleCode)
        {
            int moduleIndex = table.TimetableText.IndexOf(ModuleCode);
            if (moduleIndex != -1)
            {
                Regex modPattern = new Regex(@"[A-Z]{4}[\d]{4}");
                Match nextModule = modPattern.Match(table.TimetableText.Substring(moduleIndex + ModuleCode.Length));
                if (nextModule.Success)
                {
                    int nextModuleIndex = moduleIndex + nextModule.Index;
                    string whole = table.TimetableText.Substring(moduleIndex, nextModuleIndex - moduleIndex);
                    table.TimetableText = table.TimetableText.Replace(whole, "");
                }
                else
                    table.TimetableText = table.TimetableText.Replace(table.TimetableText.Substring(moduleIndex), "");

                var moduleColor = _repository.ModuleColor.FindAll().FirstOrDefault(mc => mc.Username == User.Identity.Name && mc.ModuleCode == ModuleCode);
                _repository.ModuleColor.Delete(moduleColor);

                UpdateAndSave();
                TempData["Message"] = "Module removed successfuly💃🏿";
            }
            return RedirectToAction("ManageModules");
        }
        public IActionResult EditClashes(bool firstSemester, string session = "")
        {
            firstSemester = Request.Cookies["isForFirstSemester"] == "true";
            Initialise(firstSemester);
            TempData["isForFirstSemester"] = firstSemester;
            List<List<Session>> clashes = session == "" ? timetableGetter.GetClashes(true) : timetableGetter.GetSpecificClashes(session);
            return View("Clashes", clashes);
        }
        public IActionResult EditGroups(bool firstSemester)
        {
            firstSemester = Request.Cookies["isForFirstSemester"] == "true";
            Initialise(firstSemester);
            TempData["isForFirstSemester"] = firstSemester;
            return View("Groups", new GroupsViewModel { GroupedLectures = timetableGetter.GetGroups(true, true) });
        }
        public IActionResult TimeTableFileUpload()
        {
            table = _repository.Timetable.GetUserTimetable(User.Identity.Name);
            if (table != null)
                return RedirectToAction("Index", new { isForFirstSemester = true });
            return View();
        }

        [HttpPost]
        public IActionResult TimeTableFileUpload(IFormFile file, string[] startBlank, bool isFirstSemester = true)
        {
            if (file != null)
            {
                if (file.Length < 500000) //revise
                {
                    if (file.ContentType == "application/pdf")
                    {
                        string text = "";
                        try
                        {
                            using (Stream stream = file.OpenReadStream())
                            {
                                //Read the text from a pdf file
                                using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(stream)))
                                {

                                    // Extract text from each page
                                    for (int pageNumber = 1; pageNumber <= pdfDocument.GetNumberOfPages(); pageNumber++)
                                    {
                                        // Get the page using the page number
                                        PdfPage page = pdfDocument.GetPage(pageNumber);

                                        // Create a new SimpleTextExtractionStrategy to extract text from the page
                                        LocationTextExtractionStrategy strategy = new LocationTextExtractionStrategy();

                                        // Extract text from the page
                                        string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

                                        // Append the extracted text from the page to the result
                                        text += pageText;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {

                            ModelState.AddModelError("", "Something went wrong while uploading TimeTable. " +
                                "\n Please make sure your have uploaded the correct file.");
                            return View();
                        }

                        //Remove unwanted text
                        Regex patternTop = new Regex(@"(\d{4}) CLASS TIMETABLE\n(\d{10})");
                        Match match = patternTop.Match(text);
                        if (match.Success)
                        {
                            Regex pattern = new Regex(@"205 Nelson Mandela Drive  \|  Park West, Bloemfontein 9301 \| South Africa\nP\.O\. Box 339  \|  Bloemfontein 9300  \|  South Africa \| www\.ufs\.ac\.za|\nVenue Start End Day From To|Venue Start End Day From To\n");//|\(Group [A-Z]{1,2}\)|
                            text = pattern.Replace(text, String.Empty);

                            Timetable newTimetale = new Timetable { TimetableText = text, Username = User.Identity.Name };
                            _repository.Timetable.Update(newTimetale);
                            Extensions.AddNewUserModuleColor(ref _repository, User.Identity.Name, newTimetale.TimetableText);
                            _repository.SaveChanges();


                            SetCookie("isForFirstSemester", isFirstSemester.ToString().ToLower());

                            return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
                        }
                        else
                            ModelState.AddModelError("", "Something went wrong while uploading timetable. " +
                                "\n Please make sure your have uploaded your personal timetable");
                    }
                    else
                        ModelState.AddModelError("", "Only pdf files allowed");
                }
                else
                    ModelState.AddModelError("", "Chosen file is too large");
            }
            else
            {
                if (startBlank.Count() != 0)
                    return StartBlank();
                else
                    ModelState.AddModelError("", "No file was uploaded. Please upload file or select to continue without uploading.");
            }

            return View();
        }
        private IActionResult StartBlank()
        {
            Timetable newTimetale = new Timetable { TimetableText = "", Username = User.Identity.Name };
            _repository.Timetable.Update(newTimetale);
            _repository.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteSession(string session, bool firstSemester)
        {
            if (session != null)
            {
                //table.TimetableText.Trim();
                string[] timeLines = table.TimetableText.Split("\n");

                List<string> rem = new List<string>();


                int x = 0;
                for (int i = 0; i < timeLines.Length; i++)
                {
                    if (timeLines[i] == session)
                    {
                        x = i; break;
                    }
                }

                Regex lecturepattern = new Regex(@"Lecture [0-9]?|Tutorial [0-9]?|Practical [0-9]?");
                Regex modulepattern = new Regex(@"[A-Z]{4}[\d]{4}");

                for (int i = x; i > 0; i--)
                {
                    Match match = lecturepattern.Match(timeLines[i]);
                    if (match.Success)
                    {
                        //timeLines[i] = "";
                        for (int j = i + 1; j < timeLines.Length; j++)
                        {
                            Match matchAfter = lecturepattern.Match(timeLines[j]);
                            Match matchMod = modulepattern.Match(timeLines[j]);
                            matchMod.Equals(timeLines[j]);
                            if ((matchAfter.Success || matchMod.Success) && match.Value != timeLines[j])
                            {
                                break;
                            }
                            else
                            {
                                if (timeLines[j] != "")
                                {
                                    table.TimetableText = table.TimetableText.Replace(timeLines[j], "");
                                    rem.Add(timeLines[j]);
                                }
                            }
                        }
                        break;
                    }
                }
                UpdateAndSave();
                TempData["Message"] = "Removed successfully";
            }
            return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
        }

        [HttpGet]
        public IActionResult AddSession(string day, string time, string moduleCode = "", string Venue = "")
        {
            PopulateEndTimeDLL(time, getAvailablePeriodCount(time, day));
            return View(new AddSessionViewModel { ModuleCode = moduleCode, Venue = Venue, Day = day, startTime = time });
        }

        [HttpPost]
        public IActionResult AddSession(AddSessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                IActionResult result = AddNewSession(model);
                if (result != default)
                    return result;
                UpdateAndSave();
                return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
            }

            PopulateEndTimeDLL(model.startTime, getAvailablePeriodCount(model.startTime, model.Day));
            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmGroup(AddSessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                AddNewSession(model, true);
                UpdateAndSave();
                return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
            }

            PopulateEndTimeDLL(model.startTime, getAvailablePeriodCount(model.startTime, model.Day));
            return View("AddSession", model);
        }

        [HttpGet]
        public IActionResult SessionDetails(string session)
        {
            if (session != null)
            {
                Session _session;
                bool isB = Request.Cookies["isForFirstSemester"] == "true";

                Initialise(Request.Cookies["isForFirstSemester"] == "true");
                var arr = data;
                for (int i = 0; i < arr.GetLength(0); i++)
                {
                    for (int j = 0; j < arr.GetLength(1); j++)
                    {
                        if (arr[i, j] != null && arr[i, j].sessionInPDFValue == session)
                        {
                            _session = arr[i, j];

                            ModuleColor moduleColor = _repository.ModuleColor
                                .GetModuleColorWithColorDetails(User.Identity.Name, _session.ModuleCode);

                            PopulateColorDLL(moduleColor.ColorId);
                            return View(new SessionModuleColorViewModel
                            {
                                ModuleColor = moduleColor,
                                Session = _session,
                                Colors = _repository.Color.FindAll()
                            });
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { isForFirstSemester = Request.Cookies["isForFirstSemester"] });
        }

        [HttpPost]
        public IActionResult UpdateModuleColor(SessionModuleColorsUpdate model)
        {
            if (model.ColorId.Count() > 0)
            {
                for (int i = 0; i < model.ColorId.Count(); i++)
                {
                    Color color = _repository.Color.GetById(model.ColorId[i]);
                    ModuleColor moduleColor = _repository.ModuleColor.GetById(model.ModuleColorId[i]);
                    moduleColor.Color = color;
                    _repository.ModuleColor.Update(moduleColor);
                }
                if (model.ColorId.Count() == 1 && model.View == "Details")
                {
                    Regex timePattern = new Regex(@"(\d{2}:\d{2}) (\d{2}:\d{2})");
                    string newSessionInPDFValue = model.oldSessionInPDFValue.Replace(model.oldSessionInPDFValue.
                        Substring(0, timePattern.Match(model.oldSessionInPDFValue).Index), $"{model.Venue} ");
                    table.TimetableText = table.TimetableText.Replace(model.oldSessionInPDFValue, newSessionInPDFValue);
                }
                UpdateAndSave();
                TempData["Message"] = "Changes saved successfuly💃🏿";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditColors()
        {
            PopulateColorDLL();
            return View(new ModulesColorsViewModel()
            {
                ModuleColors = _repository.ModuleColor.GetByCondition(m => m.Username == User.Identity.Name).Where(m =>
                 Request.Cookies["isForFirstSemester"] == "true" ? (int.Parse(m.ModuleCode.Substring(6, 1)) == 0 || int.Parse(m.ModuleCode.Substring(6, 1)) % 2 == 1)
                                : int.Parse(m.ModuleCode.Substring(6, 1)) % 2 == 0),
                Colors = _repository.Color.FindAll()
            });
        }

        [HttpGet]
        public IActionResult RandomColorEdit(string activeAction)
        {
            var lstModuleColor = _repository.ModuleColor.GetByCondition(m => m.Username == User.Identity.Name).ToList();
            int colorId = 1;
            foreach (var moduleColor in lstModuleColor)
            {
                moduleColor.ColorId = colorId++;
                _repository.ModuleColor.Update(moduleColor);
                colorId = colorId > 14 ? 1 : colorId + 0;
            }
            _repository.SaveChanges();
            return RedirectToAction(activeAction == "EditColors" ? "EditColors" : "ManageModules");
        }

        [HttpPost]
        public IActionResult ClearTable(int id)
        {
            table = _repository.Timetable.GetUserTimetable(User.Identity.Name);
            var moduleColor = _repository.ModuleColor.GetByCondition(m => m.Username == User.Identity.Name);
            if (table != null)
            {
                _repository.Timetable.Delete(table);

            }
            if (moduleColor != null)
            {
                foreach (var item in moduleColor)
                {
                    _repository.ModuleColor.Delete(item);
                }
            }
            _repository.SaveChanges();
            if (id == 0)
                return StartBlank();
            return RedirectToAction("TimeTableFileUpload");
        }
        private void PopulateEndTimeDLL(string startTime, int periodCount)
        {
            List<SelectListItem> endTimes = new List<SelectListItem>();
            int start = int.Parse(startTime.Substring(0, 2)) + 1;
            for (int i = 0; i <= periodCount; i++)
            {
                string value = start < 10 ? $"0{start}:00" : $"{start}:00";
                endTimes.Add(new SelectListItem { Value = value, Text = value });
                start++;
            }
            ViewBag.endTimes = endTimes;
        }
        private int getAvailablePeriodCount(string startTime, string Day)
        {
            int day = 0;

            switch (Day)
            {
                case "Monday": day = 0; break;
                case "Tuesday": day = 1; break;
                case "Wednesday": day = 2; break;
                case "Thursday": day = 3; break;
                case "Friday": day = 4; break;
            }

            Initialise(Request.Cookies["isForFirstSemester"] == "true");
            int period = int.Parse(startTime.Substring(0, 2)) - 6;
            int count = 0;
            while (period < 16 && data[day, period] is null)
            {
                count++;
                period++;
            }

            return count;
        }
        private void PopulateColorDLL(object selectedColor = null)
        {
            ViewBag.Colors = new SelectList(_repository.Color.FindAll()
                .OrderBy(g => g.ColorName),
                "ColorId", "ColorName", selectedColor);
        }
        private void MarkDisabled()
        {
            foreach (Lecture lect in GroupedList)
            {
                foreach (Session session in lect.sessions)
                {
                    if (data[session.Period[0] - 1, session.Period[1] - 1] != null)
                        session.sessionInPDFValue += session.sessionInPDFValue.Contains(" disabled") ? "" : " disabled";
                }
            }
        }
        private void RemoveSelectedWhereNecessary(string type, List<Lecture> groups = default)
        {

            if (type.ToLower() == "class")
            {
                List<List<Session>> clashes = timetableGetter.GetClashes(true);
                foreach (List<Session> list in clashes)
                    foreach (Session s in list)
                        table.TimetableText = table.TimetableText.Replace(s.sessionInPDFValue, s.sessionInPDFValue.Replace("selectedClass", ""));
            }
            else
            {

                foreach (Lecture lect in groups)
                    foreach (Session s in lect.sessions)
                        if (type.ToLower() == "group")
                            table.TimetableText = table.TimetableText.Replace(s.sessionInPDFValue, s.sessionInPDFValue.Replace("selectedGroup", ""));
                        else
                            table.TimetableText = table.TimetableText.Replace(s.sessionInPDFValue, s.sessionInPDFValue.Replace("ignored", ""));
            }
        }
        private IActionResult AddNewSession(AddSessionViewModel model, bool groupConfirmed = false)
        {
            string moduleCode = model.ModuleCode.ToUpper();
            int moduleIndex = table.TimetableText.IndexOf(moduleCode);
            if (moduleIndex != -1)
            {
                int sessionTypeIndex = moduleIndex + table.TimetableText.Substring(moduleIndex).IndexOf($"{model.SessionType} {model.SessionNumber}");
                if (sessionTypeIndex != moduleIndex - 1)
                {
                    string text = table.TimetableText.Substring(moduleIndex + 8, sessionTypeIndex - moduleIndex);
                    Regex modulepattern = new Regex(@"[A-Z]{4}[\d]{4}|CLASH!![\d]");
                    Match match = modulepattern.Match(text);
                    if (!match.Success)
                    {
                        if (groupConfirmed)
                        {
                            int index = sessionTypeIndex + $"{model.SessionType} {model.SessionNumber}".Length;
                            string newSessionText = $"{model.SessionType} {model.SessionNumber}\n{model.Venue} {model.startTime} {model.endTime} {model.Day}selectedGroup";
                            Regex breakPoint = new Regex(@"[A-Z]{4}[\d]{4}|Lecture [0-9]?|Tutorial [0-9]?|Practical [0-9]?");
                            Match breakMatch = breakPoint.Match(table.TimetableText.Substring(index));

                            if (breakMatch.Success)
                            {
                                string oldSessionText = table.TimetableText.Substring(sessionTypeIndex + $"{model.SessionType} {model.SessionNumber}".Length, breakMatch.Index);
                                string newText = newSessionText.Replace($"{model.SessionType} {model.SessionNumber}", "") + oldSessionText.Replace("selectedGroup", "");
                                table.TimetableText = table.TimetableText.Replace(oldSessionText, newText);
                            }
                            else
                                table.TimetableText = table.TimetableText.Replace(table.TimetableText.Substring(sessionTypeIndex),
                                    $"{newSessionText}\n{table.TimetableText.Substring(sessionTypeIndex + $"{model.SessionType + model.SessionNumber}".Length + 1)}\n");
                        }
                        else
                            return View("ConfirmGroup", model);
                    }
                    else
                    {
                        table.TimetableText = table.TimetableText.Replace(table.TimetableText.Substring(moduleIndex), $"{moduleCode}\n" +
                        $"{model.SessionType} {model.SessionNumber}\n{model.Venue} {model.startTime} {model.endTime} {model.Day}\n" +
                        table.TimetableText.Substring(moduleIndex + 8) + "\n");
                    }
                }
                else
                {
                    table.TimetableText = table.TimetableText.Replace(table.TimetableText.Substring(moduleIndex), $"{moduleCode}\n" +
                        $"{model.SessionType} {model.SessionNumber}\n{model.Venue} {model.startTime} {model.endTime} {model.Day}\n" +
                        table.TimetableText.Substring(moduleIndex + 8) + "\n");
                }
            }
            else
            {
                table.TimetableText = $"{table.TimetableText}{moduleCode}" +
                    $"\n{model.SessionType} {model.SessionNumber}\n{model.Venue} {model.startTime} {model.endTime} {model.Day}\n";
                _repository.ModuleColor.Create(new ModuleColor
                {
                    ColorId = _repository.Color.GetByName("no-color").ColorId,
                    Username = User.Identity.Name,
                    ModuleCode = moduleCode

                });
            }
            return default;
        }
        private void UpdateAndSave()
        {
            _repository.Timetable.Update(table);
            _repository.SaveChanges();
        }
        private void SetCookie(string key, string value)
        {
            CookieOptions cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(90) };
            Response.Cookies.Append(key, value == null ? "" : value, cookieOptions);
        }
    }
}
