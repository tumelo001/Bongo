using Bongo.Data;

namespace Bongo.Models.ViewModels
{
    public class GroupsViewModel
    {
        public List<Lecture> GroupedLectures { get; set; }
        public string[] Sessions { get; set; }
        public string[] SameGroups { get; set; }
        public string[] Ignore{ get; set; }
        public bool CheckDisabled(string s) => s.Contains("disabled");
    }
}