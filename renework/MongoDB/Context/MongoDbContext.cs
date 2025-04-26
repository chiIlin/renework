// MongoDB/Context/MongoDbContext.cs
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using renework.MongoDB.Collections;

namespace renework.MongoDB.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _db;
        public MongoDbContext(
            IOptions<MongoSettings> settings,
            IMongoClient client)
        {
            _db = client.GetDatabase(settings.Value.DatabaseName);

            //unique users
            var users = Users;
            var indexModel = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true });
            users.Indexes.CreateOne(indexModel);
        }

        public IMongoCollection<User> Users => _db.GetCollection<User>("Users");
        public IMongoCollection<Course> Courses => _db.GetCollection<Course>("Courses");
        public IMongoCollection<AppliedCourse> AppliedCourses => _db.GetCollection<AppliedCourse>("AppliedCourses");
        public IMongoCollection<Application> Applications => _db.GetCollection<Application>("Applications");
        public IMongoCollection<CourseReview> CourseReviews => _db.GetCollection<CourseReview>("CourseReviews");
        public IMongoCollection<BusinessData> BusinessData => _db.GetCollection<BusinessData>("BusinessData");
    }
}
