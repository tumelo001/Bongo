using System.Text.RegularExpressions;

namespace Bongo.Models.ViewModels
{
    public class IndexViewModel
    {
        public Session[,] Sessions { get; set; }
        private int setLatest;
        public int latestPeriod
        {
            get
            {
                if (setLatest != 0)
                {
                    return setLatest;
                }

                int latest = 7;
                Regex timepattern = new Regex(@"[\d]{2}:[\d]{2} [\d]{2}:[\d]{2}");
                foreach (Session session in Sessions)
                {
                    if (session != null)
                    {
                        Match timeMatch = timepattern.Match(session.sessionInPDFValue);
                        int endTime = int.Parse(timeMatch.Value.Substring(6, 2));
                        if (endTime > latest)
                            latest = endTime;
                    }
                }
                return latest - 7;
            }
            set
            {
                setLatest = value;
            }
        }
    }
}
