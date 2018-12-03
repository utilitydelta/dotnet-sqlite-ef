using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UtilityDelta.Data
{
    public class ContextManager : IDisposable
    {
        private const long CurrentVersion = 2;

        public ContextManager(string databaseFilePath)
        {
            Context = new DataContext(databaseFilePath);
            Connection = Context.Database.GetDbConnection();
            Connection.Open();
            UpdateSchemaIfRequired();
        }

        public DataContext Context { get; set; }
        public DbConnection Connection { get; set; }

        public void Dispose()
        {
            Connection.Close();
            Context?.Dispose();
        }

        private void UpdateSchemaIfRequired()
        {
            if (!HasSchemaTable()) UpgradeToLatestVersion(0, 1);

            //Upgrade schema to latest version
            var existingVersion = GetExistingVersion();
            if (existingVersion > 0 && existingVersion > CurrentVersion)
                throw new Exception(
                    $"This database has been created with a newer version: '{existingVersion}'. It cannot be opened as your CAD only supports up to version '{CurrentVersion}'.");

            if (existingVersion < CurrentVersion) UpgradeToLatestVersion(existingVersion, CurrentVersion);
        }

        /// <summary>
        ///     Check the database to see if any version is present
        /// </summary>
        /// <returns></returns>
        private bool HasSchemaTable()
        {
            var hasSchemaTable = false;
            using (var command = Connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'SchemaHistory'";
                var result = (long) command.ExecuteScalar();
                hasSchemaTable = result > 0;
            }

            return hasSchemaTable;
        }

        private static string GetScript(bool createScript, long version)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var path = assembly.FullName.Split(',')[0] + (createScript ? ".CreateScripts." : ".UpgradeScripts.") +
                       version + ".sql";

            try
            {
                using (var stream = assembly.GetManifestResourceStream(path))
                using (var reader =
                    new StreamReader(
                        stream ?? throw new InvalidOperationException(
                            "Could not retrieve upgrade script from assembly")))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Unable to load {(createScript ? "create" : "upgrade")} script version: '{version}'. {ex}");
            }
        }

        /// <summary>
        ///     Run upgrade sql files one by one, up to and including the latest version
        /// </summary>
        /// <param name="existingVersion"></param>
        /// <param name="upgradeTo"></param>
        private void UpgradeToLatestVersion(long existingVersion, long upgradeTo)
        {
            using (var trans = Context.Database.BeginTransaction())
            {
                try
                {
                    existingVersion++;
                    while (existingVersion <= upgradeTo)
                    {
                        var script = GetScript(false, existingVersion);
                        Context.Database.ExecuteSqlCommand(script);
                        InsertIntoSchemaHistory(script, existingVersion);
                        existingVersion++;
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Could not execute upgrade database scripts from version '{existingVersion}' to '{upgradeTo}'. {ex}");
                }
            }
        }

        private long GetExistingVersion()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT max(Version) FROM SchemaHistory";
                return (long) command.ExecuteScalar();
            }
        }

        private void InsertIntoSchemaHistory(string script, long version)
        {
            Context.Database.ExecuteSqlCommand(
                "INSERT INTO SchemaHistory (Version, DateApplied, ScriptApplied) VALUES (@Version, @DateApplied, @ScriptApplied)",
                new SqliteParameter("@Version", version),
                new SqliteParameter("@DateApplied", DateTime.Now),
                new SqliteParameter("@ScriptApplied", script));
        }
    }
}