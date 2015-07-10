using FluentMigrator;

namespace TestMigrations
{
    [Migration(0)]
    public class Migration0 : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Person")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey().NotNullable()
                .WithColumn("Value").AsString();
        }
    }
}