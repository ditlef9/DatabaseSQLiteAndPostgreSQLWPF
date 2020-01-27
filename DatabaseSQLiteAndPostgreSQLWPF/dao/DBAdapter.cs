using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DatabaseSQLiteAndPostgreSQLWPF.dao
{
    class DBAdapter
    {
        public NpgsqlConnection npgsqlCon; // Connection for PostgreSQL
        public SQLiteConnection sqliteCon; // Connection for SQLite
        public string sqliteConnectionString;

        String databasePath = ""; //  SQLite only (appData + "\\" + projectName + "\\" + "db")
        String databaseFile = ""; //  SQLite only (mydb.db)
        String databaseType = ""; //  postgresql
        String databaseHost = ""; // localhost
        String databasePort = ""; // 5432
        String databaseUser = ""; // postgres
        String databasePassword = ""; // postgres
        String databaseName = ""; // csharp

        /*- Constructor ------------------------------------------------------------------------------------------- */
        public DBAdapter(String dbType, String dbPath, String dbFile,  String dbHost, String dbPort, String dbUser, String dbPassword, String dbName)
        {
            // Assign variables
            this.databaseType = dbType;
            this.databasePath = dbPath;
            this.databaseFile = dbFile;
            this.databaseHost = dbHost;
            this.databasePort = dbPort;
            this.databaseUser = dbUser;
            this.databasePassword = dbPassword;
            this.databaseName = dbName;


            // Database connection string
            if (dbType.Equals("postgresql")) { 
                string connstring = String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                                databaseHost, databasePort, databaseUser, databasePassword, databaseName);
                // Making connection with Npgsql provider
                npgsqlCon = new NpgsqlConnection(connstring);
            } // Postgre
            else
            {
                // Database connection string
                createSQLiteDatabase(); // Make sure database exists. If it doesnt, then create it
                sqliteConnectionString = @"URI=file:" + databasePath + "\\" + databaseFile;

                // Connect to database
                sqliteCon = new SQLiteConnection(sqliteConnectionString);

            } // sqlite
        } // Constructor

        /*- Create SQLite Database --------------------------------------------------------------- */
        public void createSQLiteDatabase()
        {
            // Empty dbPath or file, set standard
            if (this.databasePath.Equals("")) {
                this.databasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MyProgram"; 
            }
            if (this.databaseFile.Equals("")) {
                this.databaseFile = Assembly.GetCallingAssembly().GetName().Name;
                this.databaseFile = this.databaseFile + ".db";
            }
     
            if (!(Directory.Exists(this.databasePath)))
            {
                Directory.CreateDirectory(this.databasePath);
            }

            String fullPathFile = this.databasePath + "\\" + this.databaseFile;
            if (!(File.Exists(fullPathFile)))
            {
                File.WriteAllText(fullPathFile, "");
            }
        } // createSQLiteDatabase

        /*- Open ---------------------------------------------------------------------------------- */
        public void open()
        {
            if (this.databaseType.Equals("postgresql")) {
                npgsqlCon.Open();
            }
            else
            {
                sqliteCon.Open();
            }
        } // open

        /*- Close --------------------------------------------------------------------------------- */
        public void close()
        {
            if (this.databaseType.Equals("postgresql"))
            {
                npgsqlCon.Close();
            }
            else
            {
                sqliteCon.Close();
            }
        } // close


        /*- Table exists -------------------------------------------------------------------------- */
        public bool tableExists(String tableName)
        {
            bool exists = false;

            if (this.databaseType.Equals("postgresql")) { 
                String query = @"SELECT * FROM information_schema.tables WHERE table_name = '" + tableName + "'";
                NpgsqlCommand command = new NpgsqlCommand(query, npgsqlCon);
                NpgsqlDataReader rdr = command.ExecuteReader();
                if (rdr != null && rdr.HasRows)
                {
                    exists = true;
                }
                else
                {
                    exists = false;
                }
                rdr.Close();
            }
            return exists;
        }
        /*- Liquidbase table exists -------------------------------------------------------------------------- */
        public bool liquidbaseTableExists()
        {
            bool exists = false;

            if (this.databaseType.Equals("postgresql"))
            {
                String query = @"SELECT * FROM information_schema.tables WHERE table_name='liquidbase_postgresql'";
                NpgsqlCommand command = new NpgsqlCommand(query, npgsqlCon);
                NpgsqlDataReader rdr = command.ExecuteReader();
                if (rdr != null && rdr.HasRows)
                {
                    exists = true;
                }
                else
                {
                    exists = false;
                }
                rdr.Close();
            }
            return exists;
        }
        /*- Create liquidbase --------------------------------------------------------------------- */
        public void createLiquidbase()
        {
            if (this.databaseType.Equals("postgresql"))
            {
                // Create sequence
                String query = @"CREATE SEQUENCE liquidbase_id_seq";
                this.command(query);

                // Create table
                query = @"CREATE TABLE liquidbase_postgresql(
                    liquidbase_id INTEGER NOT NULL default nextval('liquidbase_id_seq'),
                    liquidbase_directory TEXT,
                    liquidbase_script TEXT,
                    liquidbase_datetime TEXT)";
                this.command(query);

                // Alter sequence
                query = @"ALTER SEQUENCE liquidbase_id_seq owned by liquidbase_postgresql.liquidbase_id;";
                this.command(query);
            }
            else
            {
                // Create db
                String query = @"CREATE TABLE liquidbase_sqlite(
                    liquidbase_id INTEGER PRIMARY KEY,
                    liquidbase_directory TEXT,
                    liquidbase_script TEXT,
                    liquidbase_datetime DATETIME)";
                this.command(query);

            }
        }
        /*- Run liquidbase ------------------------------------------------------------------------ */
        /* This will loop trough dao/liquidbase and execute sql scripts */
        internal void runLiquidbase()
        {

            if (this.databaseType.Equals("postgresql"))
            {
                String path = "dao/liquidbase";
                string[] files = Directory.GetFiles(path);
                for (int x = 0; x < files.Length; x++)
                {
                    // Get scriptname. Check if it has been run
                    String scriptPath = files[x];
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine("runLiquidbase() postgresql scriptName=" + scriptPath);

                    String query = "SELECT liquidbase_id FROM liquidbase_postgresql WHERE liquidbase_script='" + scriptPath + "'";
                    NpgsqlCommand command = new NpgsqlCommand(query, npgsqlCon);
                    NpgsqlDataReader reader = command.ExecuteReader();
                    bool hasRows = reader.HasRows;
                    reader.Close();
                    if (!(hasRows))
                    {

                        // Insert 
                        String inpScriptSQL = "'" + scriptPath + "'";

                        DateTime datetime = DateTime.Now;
                        string datetimeSQL = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                        datetimeSQL = "'" + datetimeSQL + "'";

                        query = @"INSERT INTO liquidbase_postgresql(liquidbase_directory, liquidbase_script, liquidbase_datetime)
                                    VALUES(" +
                                        "NULL, " +
                                        inpScriptSQL + ", " +
                                        datetimeSQL + ")";

                        Console.WriteLine("-------------------------------");
                        Console.WriteLine("query=" + query);
                        this.command(query);

                        // Read script and execute it
                        string readScript = System.IO.File.ReadAllText(scriptPath);
                        readScript = readScript.Replace("_id INTEGER PRIMARY KEY,", "_id SERIAL PRIMARY KEY,");
                        readScript = readScript.Replace("DATETIME", "TEXT");
                        this.command(readScript);
                    }
                }
            } // postgresql
            else
            {
                String path = "dao/liquidbase";
                string[] files = Directory.GetFiles(path);
                for (int x = 0; x < files.Length; x++)
                {
                    // Get scriptname. Check if it has been run
                    String scriptPath = files[x];
                    // Console.WriteLine("-------------------------------");
                    // Console.WriteLine("scriptName=" + scriptPath);

                    String query = "SELECT liquidbase_id FROM liquidbase_sqlite WHERE liquidbase_script='" + scriptPath + "'";
                    string stm = query;
                    SQLiteCommand cmd = new SQLiteCommand(stm, sqliteCon);
                    SQLiteDataReader rdr = cmd.ExecuteReader();
                    bool hasRows = rdr.HasRows;
                    rdr.Close();
                    if (!(hasRows))
                    {
                        // Insert 
                        String inpScriptSQL = "'" + scriptPath + "'";

                        DateTime datetime = DateTime.Now;
                        string datetimeSQL = datetime.ToString("yyyy-MM-dd HH:mm:ss");
                        datetimeSQL = "'" + datetimeSQL + "'";

                        query = @"INSERT INTO liquidbase_sqlite(liquidbase_id, liquidbase_directory, liquidbase_script, liquidbase_datetime)
                                    VALUES(" +
                                        "NULL, " +
                                        "NULL, " +
                                        inpScriptSQL + ", " +
                                        datetimeSQL + ")";
                        this.command(query);

                        // Read script and execute it
                        string readScript = System.IO.File.ReadAllText(scriptPath);
                        this.command(readScript);
                    }

                }
            } // sqlite
        } // runLiquidbase
        /*- Count rows ---------------------------------------------------------------------------- */
        public int countRows(String query)
        {
            int numberOfRows = -1;

            if (this.databaseType.Equals("postgresql"))
            {
                NpgsqlCommand command = new NpgsqlCommand(query, npgsqlCon);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    numberOfRows = reader.GetInt32(0);
                }
                reader.Close();
            }

            return numberOfRows;
        } // countRows

        /*- Quote smart ------------------------------------------------------------------------- */
        public String quoteSmart(String value)
        {

            // Escapes special characters in a string for use in an SQL statement
            if (value != null && value.Length > 0)
            {
                value = value.Replace("\\", "\\\\");
                value = value.Replace("'", "\\'");
                value = value.Replace("\0", "\\0");
                value = value.Replace("\n", "\\n");
                value = value.Replace("\r", "\\r");
                value = value.Replace("\"", "\\\"");
                value = value.Replace("\\x1a", "\\Z");
                value = value.Replace("'", "&#39;");
            }

            value = "'" + value + "'";

            // Remove 'null'
            value = value.Replace("'null'", "NULL");

            return value;
        }
        /*- QeuryRows ---------------------------------------------------------------------------- */
        public NpgsqlDataReader queryRowsPostgres(String query)
        {
            // Postgres
            Console.WriteLine("NpgsqlDataReader queryRowsPostgres=" + query);
            NpgsqlCommand command = new NpgsqlCommand(query, npgsqlCon);
            NpgsqlDataReader npgsqlReader = command.ExecuteReader();
            return npgsqlReader;
          
        } // queryRows
        public SQLiteDataReader queryRowsSQLite(String query)
        {
           // SQLite 
            SQLiteCommand cmd = new SQLiteCommand(query, sqliteCon);
            SQLiteDataReader sqliteReader = cmd.ExecuteReader();
            return sqliteReader;
        } // queryRows

        /*- Command ------------------------------------------------------------------------------- */
        public void command(String query)
        {

            Console.WriteLine("DBAdapterPostgreSQL command(): " + query);
            if (this.databaseType.Equals("postgresql"))
            {
                try
                {
                    var command = new NpgsqlCommand(query, npgsqlCon);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Something made the code crash
                    // PostgreSQL doesnt need the first row (the ID row when inserting data)
                    // Check if the statement is "insert" and first row is "id" and value is "NULL"
                    // Example: INSERT INTO cases(case_id, case_number, case_evidence_number) VALUES(NULL, 12345, 6);
                    if (query.StartsWith("INSERT INTO"))
                    {
                        if (query.Contains("VALUES(NULL,"))
                        {
                            // Remove VALUES(NULL,
                            String columnsAndValues = query.Substring(query.IndexOf('(') + 1);
                            int index = columnsAndValues.IndexOf(',');
                            String firstColumn    = columnsAndValues.Substring(0, index);
                            String firstColumnComma = firstColumn + ", ";

                            String postgreQuery = query.Replace("VALUES(NULL, ", "VALUES(");
                            postgreQuery = postgreQuery.Replace(firstColumnComma, "");

                            try { 
                                var command = new NpgsqlCommand(postgreQuery, npgsqlCon);
                                command.ExecuteNonQuery();
                            } catch(Exception exf)
                            {
                                Console.WriteLine("DBAdapterPostgreSQL: " + postgreQuery + " -> "  + exf.Message);

                            }


                        }
                    }
                    Console.WriteLine("DBAdapterPostgreSQL: " + ex.Message);
                }
            } // postgresql
            else
            {
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand(sqliteCon);
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine("public void insert(String query): " + e.ToString());
                }

            } // sqlite
        } // command
    }
}
