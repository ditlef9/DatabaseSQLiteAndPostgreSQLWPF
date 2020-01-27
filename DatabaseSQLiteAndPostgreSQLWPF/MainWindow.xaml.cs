using DatabaseSQLiteAndPostgreSQLWPF.dao;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatabaseSQLiteAndPostgreSQLWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*- Database */
        String dbType = "postgresql"; // sqlite or postgresql
        String dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MyProgram";
        String dbFile = "mydb.db";
        String dbHost = "localhost";
        String dbPort = "5432";
        String dbUser = "postgres";
        String dbPassword = "root";
        String dbName = "csharp";


        public MainWindow()
        {
            InitializeComponent();
            printDatabaseInformation();
            runLiquidbase();

            insertExampleRow();
            readRowsExample();
        }
        /*- Print database ---------------------------------------------------------------------------------------- */
        public void printDatabaseInformation()
        {
            // Update text
            textBoxDatabase.Text = "printDatabase()\n" +
                "dbType=" + dbType + "\n" +
                "dbPath=" + dbPath + "\n" +
                "dbFile=" + dbFile + "\n" +
                "dbHost=" + dbHost + "\n" +
                "dbPort=" + dbPort + "\n" +
                "dbUser=" + dbUser + "\n" +
                "dbPassword=" + dbPassword + "\n" +
                "dbName=" + dbName;

        } // printDatabase

        /*- Run liquidbase ---------------------------------------------------------------------------------------- */
        public void runLiquidbase()
        {
            // Update text
            textBoxLiquidbase.Text = "runLiquidbase()";

            // Database
            DBAdapter dbAdapter = new DBAdapter(dbType, dbPath, dbFile, dbHost, dbPort, dbUser, dbPassword, dbName);
            dbAdapter.open();


            // Check that liquidbase exists, if it doesnt, then create it
            bool exists = dbAdapter.liquidbaseTableExists();
            if (exists == false)
            {
                textBoxLiquidbase.Text = textBoxLiquidbase.Text + "\n" + "Creating liquidbase";
                dbAdapter.createLiquidbase();
            }


            // Run liquidbase 
            textBoxLiquidbase.Text = textBoxLiquidbase.Text + "\n" + "Run liquidbase with dbAdapter.runLiquidbase()";
            dbAdapter.runLiquidbase();

            // Close database
            dbAdapter.close();
        } // runLiquidbase

        /*- Insert example row ------------------------------------------------------------------------------------- */
        public void insertExampleRow()
        {
            // Update text
            textBoxInsert.Text = "insertExampleRow()";

            // Database
            DBAdapter dbAdapter = new DBAdapter(dbType, dbPath, dbFile, dbHost, dbPort, dbUser, dbPassword, dbName);
            dbAdapter.open();

            // Case number
            Random random = new Random();
            int caseNumber = random.Next();
            String caseNumberSQL = dbAdapter.quoteSmart(caseNumber.ToString());

            // Evidence number
            DateTime datetime = DateTime.Now;
            String year = datetime.ToString("yyyy");
            int journal = random.Next();
            int district = random.Next();
            int item = random.Next();
            String title = randomString(10, false);
            String evidenceNumber = year + "/" + journal + "-" + district + "-" + item + " " + title;
            String evidenceNumberSQL = dbAdapter.quoteSmart(evidenceNumber);


            // Check if exists
            String query = "SELECT case_id FROM cases WHERE case_number=" + caseNumberSQL + " AND case_evidence_number=" + evidenceNumberSQL;
            bool isDuplicate = false;
            if (dbType.Equals("postgresql"))
            {
                NpgsqlDataReader reader = dbAdapter.queryRowsPostgres(query);
                if (reader.HasRows)
                {
                    isDuplicate = true;
                }
                reader.Close();
            }
            else
            {
                SQLiteDataReader reader = dbAdapter.queryRowsSQLite(query);
                if (reader.HasRows)
                {
                    isDuplicate = true;
                }
                reader.Close();
            }



            // Insert into database
            if (!(isDuplicate))
            {
                String queryInsert = @"INSERT INTO cases(case_id, case_number, case_evidence_number)
                            VALUES(" +
                                    "NULL, " +
                                    caseNumberSQL + ", " +
                                    evidenceNumberSQL + ")";
                dbAdapter.command(queryInsert);
                textBoxInsert.Text = textBoxInsert.Text + "\n" + "Insert caseNumber=" + caseNumber + " evidenceNumber=" + evidenceNumber;
            }

            // Database close
            dbAdapter.close();
        } // insertExampleRow
        /*- Read rows example ------------------------------------------------------------------------------------- */
        public void readRowsExample()
        {
            // Update text
            textBoxRead.Text = "readRowsExample()";

            // Database
            DBAdapter dbAdapter = new DBAdapter(dbType, dbPath, dbFile, dbHost, dbPort, dbUser, dbPassword, dbName);
            dbAdapter.open();


            // Read
            String query = "SELECT case_id, case_number, case_evidence_number FROM cases ORDER BY case_id DESC LIMIT 15 offset 0";
            if (dbType.Equals("postgresql"))
            {
                NpgsqlDataReader reader = dbAdapter.queryRowsPostgres(query);
                while (reader.Read())
                {
                    int caseId = reader.GetInt32(0);
                    String caseNumber = reader.GetString(1);
                    String caseEvidenceNumber = reader.GetString(2);
                    textBoxRead.Text = textBoxRead.Text + "\n" + caseId + " | " + caseNumber + "|" + caseEvidenceNumber;
                }
                reader.Close();
            }
            else
            {
                SQLiteDataReader reader = dbAdapter.queryRowsSQLite(query);
                while (reader.Read())
                {
                    int caseId = reader.GetInt32(0);
                    String caseNumber = reader.GetString(1);
                    String caseEvidenceNumber = reader.GetString(2);
                    textBoxRead.Text = textBoxRead.Text + "\n" + caseId + " | " + caseNumber + "|" + caseEvidenceNumber;
                }
                reader.Close();
            }

            // Database close
            dbAdapter.close();
        } // readRowsExample

        /*- Random string ------------------------------------------------------------------ */
        public string randomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

    }
}
