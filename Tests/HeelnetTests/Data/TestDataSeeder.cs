using HeelnetTests.Utils;

namespace HeelnetTests.Data
{
    /// <summary>
    /// Zet de database vóór iedere test terug naar een bekende begintoestand.
    /// Werkwijze: 1) de testdata verwijderen, in de juiste volgorde i.v.m. foreign keys;
    /// 2) de testdata opnieuw invoegen.
    /// Wordt alleen uitgevoerd als er een databaseverbinding is ingesteld.
    /// </summary>
    public static class TestDataSeeder
    {
        private static readonly Lazy<MsSqlDatabaseHelper> db = new(() => new MsSqlDatabaseHelper());
        private static MsSqlDatabaseHelper Db => db.Value;

        public static void Seed()
        {
            // TODO: invullen zodra de tabellen van HeelNet bestaan (zie Server/Migrations). Voorbeeld:
            //
            // Db.ExecuteQuery("DELETE FROM Users WHERE Email LIKE '%@test.heelnet';");
            //
            // foreach (var user in TestData.GetTestUsers())
            // {
            //     Db.ExecuteQuery("INSERT INTO Users (Email, Role) VALUES (@email, @role);",
            //         ("@email", user.Email), ("@role", user.Role));
            // }

            _ = Db;
            Log.Logger.Debug("Testdata geseed");
        }
    }
}
