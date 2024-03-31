using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Npgsql;

namespace APICSharp
{
    public class DBAction
    {
        //private SqlConnection Conn;
        private NpgsqlConnection Conn;
        private string ConnStr;
        //private SqlCommand cmd;
        private NpgsqlCommand cmd;
        private SqlDataReader sqlreader;
        private DataSet dataSet;

        public DBAction()
        {
            //ConnStr = "Host=your_server_address;Port=5432;Username=your_username;Password=your_password;Database=your_database_name;";
            ConnStr = "";
          
          if (GlobalVar.ServerName != ""  & GlobalVar.SUserName != "" & GlobalVar.SUserPwd != "")
          {
            if (GlobalVar.ServerName.IndexOf("SQLEXPRESS") > -1)
            {
                //ConnStr = "Server=" + GlobalVar.ServerName + ";" + "User ID=" + GlobalVar.SUserName + ";" + "Password=" + GlobalVar.SUserPwd + ";" + "Initial Catalog=" + "ZKBioSecurity_db";
                ConnStr = "Host=" + GlobalVar.ServerName + ";" + "Port=5432" + "Username=" + GlobalVar.SUserName + ";" + "Password=" + GlobalVar.SUserPwd + ";" + "Database=" + "ZKBioSecurity_db";
            }
            else 
              {
                  ConnStr = "Data Source=" + GlobalVar.ServerName + ";" + "User ID=" + GlobalVar.SUserName + ";" + "Password=" + GlobalVar.SUserPwd + ";" + "Initial Catalog=" + "ZKBioSecurity_db";
              }
          }
          else
          {
            if (GlobalVar.ServerName.IndexOf("SQLEXPRESS") > -1)
                ConnStr = "Server=" + GlobalVar.ServerName + ";" + "Database=" + "ZKBioSecurity_db" + ";" + "Trusted_Connection=True;";
            else
                ConnStr = "Data Source=" + GlobalVar.ServerName + ";" + "Database=" + "ZKBioSecurity_db" + ";" + "Trusted_Connection=True;";
          }
            
            //Conn = new SqlConnection(ConnStr);
            NpgsqlConnection Conn = new NpgsqlConnection(ConnStr);

        }

        

        public int ExecuteSQL(string SQLstr)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                Conn.Open();
                int rows = cmd.ExecuteNonQuery();
                Conn.Close();
                return rows;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog("execute SQL error=" + ex.Message + " detail=" + ex.StackTrace);
                return -1;
            }
        }

        public int ExecuteTrans(string SQLstr)
        {
            //SqlTransaction sqlTrans = Conn.BeginTransaction();
            NpgsqlTransaction sqlTrans = Conn.BeginTransaction();
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            cmd.Transaction = sqlTrans;
            try
            {
                Conn.Open();
                int rows = cmd.ExecuteNonQuery();
                sqlTrans.Commit(); 
                Conn.Close();
                return rows;
            }
            catch (Exception ex)
            {
                sqlTrans.Rollback();
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog("Execute Transaction error=" + ex.Message + " detail=" + ex.StackTrace);
                return -1;
            }
        }

        public int ExecuteStoreProc(string StoreProcName, string ParamName,string ParamValue)
        {
            //cmd = new SqlCommand(StoreProcName, Conn);
            cmd = new NpgsqlCommand(StoreProcName, Conn);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                Conn.Open();
                // ParamName="@ClassName"
                cmd.Parameters.Add(ParamName, SqlDbType.VarChar, 50).Value = ParamValue;
                cmd.Parameters.Add(ParamName, NpgsqlDbType.Varchar, 50) = ParamValue;
                
                int rows = cmd.ExecuteNonQuery();
                Conn.Close();
                return rows;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog("Execute StoreProc error=" + ex.Message + " detail=" + ex.StackTrace);
                return -1;
            }
        }

        public int RecordCount(string SQLstr)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                Conn.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar());
                Conn.Close();
                Console.WriteLine("row number=" +rows);
                return rows;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog("RecordCount error=" + ex.Message + " detail=" + ex.StackTrace);
                return -1;
            }
        }

        public DataSet DisplaySetDGV(string SQLstr)
        {
            try
            {
                Conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(SQLstr, Conn);
                dataSet =  new DataSet();
                adapter.Fill(dataSet);
                return dataSet;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog(" DisplayDGV error=" + ex.Message + " detail=" + ex.StackTrace);
                return dataSet;
            }
        }

        public SqlDataReader DisplayDGV(string SQLstr)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                Conn.Open();
                sqlreader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
               // Conn.Close();
                return sqlreader;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog(" DisplayDGV error=" + ex.Message + " detail=" + ex.StackTrace);
                return sqlreader;
            }
        }

        public string QueryID(string SQLstr, string ColName)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                string rv = "";
                Conn.Open();
                sqlreader = cmd.ExecuteReader();
                if (sqlreader.HasRows)
                {

                    int c1 = sqlreader.GetOrdinal(ColName);
                    while (sqlreader.Read())
                    {
                        rv = sqlreader[c1].ToString();
                    } 
                    sqlreader.Close(); 
                    Conn.Close();
                }
                else
                {
                    sqlreader.Close();
                    Conn.Close();
                }
               // Console.WriteLine("rv=" + rv);
                return rv;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog(" QueryID error=" + ex.Message + " detail=" + ex.StackTrace);
                return "-1";
            }
        }

        public string AddListData(string SQLstr, string ColName, ListBox lst1)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                string rv = "";
                Conn.Open();
                sqlreader = cmd.ExecuteReader();
                if (sqlreader.HasRows)
                {
                    lst1.Items.Clear(); 
                    int c1 = sqlreader.GetOrdinal(ColName);
                    while (sqlreader.Read())
                    {
                        rv = sqlreader[c1].ToString();
                        lst1.Items.Add(rv); 
                    }
                    sqlreader.Close();
                    Conn.Close();
                }
                else
                {
                    sqlreader.Close();
                    Conn.Close();
                }
                return rv;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog(" QueryID error=" + ex.Message + " detail=" + ex.StackTrace);
                return "-1";
            }
        }

        public string AddCmbData(string SQLstr, string ColName, ComboBox lst1)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                string rv = "";
                Conn.Open();
                //sqlreader = cmd.ExecuteReader();
                NpgsqlDataReader sqlreader = cmd.ExecuteReader();

                lst1.Items.Clear(); 
                if (sqlreader.HasRows)
                {
                    int c1 = sqlreader.GetOrdinal(ColName);
                    while (sqlreader.Read())
                    {
                        rv = sqlreader[c1].ToString();
                        lst1.Items.Add(rv);
                    }
                    sqlreader.Close();
                    Conn.Close();
                }
                else
                {
                    sqlreader.Close();
                    Conn.Close();
                }
                return rv;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog(" QueryID error=" + ex.Message + " detail=" + ex.StackTrace);
                return "-1";
            }
        }

        public string AddCmbData2(string SQLstr, string ColName, ComboBox lst1)
        {
            //cmd = new SqlCommand(SQLstr, Conn);
            cmd = new NpgsqlCommand(SQLstr, Conn);
            try
            {
                string rv = "";
                Conn.Open();
                sqlreader = cmd.ExecuteReader();
                lst1.Items.Clear();
                if (sqlreader.HasRows)
                {
                    int c1 = sqlreader.GetOrdinal(ColName);
                    while (sqlreader.Read())
                    {
                        rv = sqlreader[c1].ToString();
                        lst1.Items.Add(rv);
                    }
                    sqlreader.Close();
                    Conn.Close();
                }
                else
                {
                    sqlreader.Close();
                    Conn.Close();
                }

                for (int i = 0; i < lst1.Items.Count; i++)
                {
                    rv = lst1.Items[i].ToString();
                    if (rv.Length == 1)
                    {
                            lst1.Items.Remove(rv);
                            i = i - 1;
                    }
                }
               return rv;
            }
            catch (Exception ex)
            {
                Conn.Close();
                LogClass LogA = new LogClass();
                LogA.writeErrorLog(" QueryID error=" + ex.Message + " detail=" + ex.StackTrace);
                return "-1";
            }
        }

        public void Close()
        {
            Conn.Close();
        }
    }
}
