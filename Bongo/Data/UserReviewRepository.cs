using Bongo.Models.User;

namespace Bongo.Data
{
    public class UserReviewRepository : RepositoryBase<UserReview>, IUserReviewRepository
    {
        public UserReviewRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }
    }
}
