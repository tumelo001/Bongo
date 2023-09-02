namespace Bongo.Data
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly AppDbContext _appDbContext;

        private ITimetableRepository timetable;
        private IModuleColorRepository moduleColor;
        private IColorRepository color;
        private IUserReviewRepository userReview;


        public RepositoryWrapper(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public ITimetableRepository Timetable
        {
            get
            {
                if (timetable == null)
                    timetable = new TimetableRepository(_appDbContext);
                return timetable;
            }
        }
        public IModuleColorRepository ModuleColor
        {
            get
            {
                if (moduleColor == null)
                    moduleColor = new ModuleColorRepository(_appDbContext);
                return moduleColor;
            }
        }
        public IColorRepository Color
        {
            get
            {
                if (color == null)
                    color = new ColorRepository(_appDbContext);
                return color;
            }
        }
        public IUserReviewRepository UserReview
        {
            get
            {
                if (userReview == null)
                    userReview = new UserReviewRepository(_appDbContext);
                return userReview;
            }
        }

        public void SaveChanges()
        {
            _appDbContext.SaveChanges();
        }
    }
}