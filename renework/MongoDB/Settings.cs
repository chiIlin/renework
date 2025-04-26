namespace renework.MongoDB
{
    // a simple POCO to bind your settings:
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

}
