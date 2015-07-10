using FluentMigrator;

namespace TestMigrations
{
    [Migration(1000000000000000000)]
    public class Migration1 : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Invoice")
                .WithColumn("Id").AsInt64().Identity().PrimaryKey().NotNullable()
                .WithColumn("DueDate").AsDate();
        }
    }
}