using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
//using MySql.Data.MySqlClient;

namespace FingerPrintModule.DAO
{
    public class DataAccess : IDisposable
    {
        private IDbConnection sql_con;
        //private IDbCommand sql_cmd;
        private IDbDataAdapter dbDataAdapter;
        readonly string path = String.Format(@"{0}\connection_string.txt",
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", ""));

        public DataAccess()
        {
            //var connectionInfo = GetConnectionStringFromFile();
            var connectionString = GetConnectionString();
            sql_con = new SqlConnection(connectionString);
        }

        public DataAccess(string connectionString)
        {
            sql_con = new SqlConnection(connectionString);
        }

        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();

        private DataTable GetData(IDbCommand CommandText)
        {
            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();

            dbDataAdapter = new SqlDataAdapter((SqlCommand)CommandText);
            DS.Reset();
            dbDataAdapter.Fill(DS);
            DT = DS.Tables[0];
            sql_con.Close();
            return DT;
        }

        public string RetrieveSingleRecord(string txtQuery)
        {
            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();

            using (IDbCommand sql_cmd = new SqlCommand(txtQuery, (SqlConnection)sql_con))
            {
                var rd = sql_cmd.ExecuteReader();


                while (rd.Read())
                {
                    return Convert.ToString(rd[0]);
                }
                return null;

                //sql_cmd.ExecuteNonQuery();
               // sql_con.Close();
            }
        }

        public void ExecuteQuery(string txtQuery)
        {
            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();

            using (IDbCommand sql_cmd = new SqlCommand(txtQuery, (SqlConnection)sql_con))
            {
                sql_cmd.ExecuteNonQuery();
                sql_con.Close();
            }
        }

        public int ExecuteScalar(string txtQuery)
        {
            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();

            using (IDbCommand sql_cmd = new SqlCommand(txtQuery, (SqlConnection)sql_con))
            {
                var counts = Convert.ToInt32(sql_cmd.ExecuteScalar());
                sql_con.Close();
                return counts;
            }
        }

        public List<FingerPrintInfo> GetBiometricinfo(int staffId = 0)
        {
            var _list = new List<FingerPrintInfo>();

            IDbCommand sql_cmd = new SqlCommand
            {
                Connection = (SqlConnection)sql_con
            };

            var sqlQuery = "";

            if (staffId != 0)
            {
                sqlQuery = "SELECT StaffId, template, imageWidth, imageHeight, imageDPI,  imageQuality, " +
                    "fingerPosition, serialNumber, model, manufacturer, date_created, creator " +
                    "FROM biometricInfo where StaffId = @StaffId";
                sql_cmd.CommandText = sqlQuery;
                var param = sql_cmd.CreateParameter();
                param.ParameterName = "@StaffId";
                param.DbType = DbType.Int32;
                param.Value = staffId;
                sql_cmd.Parameters.Add(param);
            }
            else
            {
                sqlQuery = "SELECT StaffId, template, imageWidth, imageHeight, imageDPI,  imageQuality, fingerPosition, serialNumber, model, manufacturer, date_created, creator FROM biometricInfo";
                sql_cmd.CommandText = sqlQuery;
            }

            var dt = GetData(sql_cmd);

            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    var fposition = dr.Field<string>("fingerPosition");
                    Enum.TryParse(fposition, out FingerPositions position);

                    _list.Add(new FingerPrintInfo
                    {
                        staffId = dr.Field<string>("StaffId"),
                        Template = dr.Field<string>("template"),
                        ImageWidth = dr.Field<int>("imageWidth"),
                        ImageHeight = dr.Field<int>("imageHeight"),
                        ImageDPI = dr.Field<int>("imageDPI"),
                        ImageQuality = dr.Field<int>("imageQuality"),
                        FingerPositions = position,
                        SerialNumber = dr.Field<string>("serialNumber"),
                        Model = dr.Field<string>("model"),
                        Manufacturer = dr.Field<string>("manufacturer"),

                    });
                }
                catch (Exception ex)
                {
                    throw new Exception("some reason to rethrow", ex);
                }
            }

            return _list;
        }

        public void WriteConnectionToFile(ConnectionString connectionString)
        {
            using (StreamWriter sw = (File.Exists(path)) ? new StreamWriter(path, false) : File.CreateText(path))
            {
                sw.WriteLine(connectionString.FullConnectionString);
                sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(connectionString));
            }
        }

        public static string GetConnectionString()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["EJIntegralConnection"].ConnectionString;
        }

        //public ConnectionString GetConnectionStringFromFile()
        //{
        //    ConnectionString connectionString = null;
        //    if (File.Exists(path))
        //    {
        //        var lines = File.ReadLines(path).ToArray();
        //        connectionString = Newtonsoft.Json.JsonConvert.DeserializeObject<ConnectionString>(lines[1]);
        //    }
        //    return connectionString;
        //}

        public ResponseModel SaveToDatabase(List<FingerPrintInfo> fingerPrintList)
        {
            try
            {
                var db = new DataAccess();
                foreach (var f in fingerPrintList)
                {
                    db.Save(f);
                }
                return new ResponseModel
                {
                    ErrorMessage = "Saved successfully",
                    IsSuccessful = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    ErrorMessage = ex.Message,
                    IsSuccessful = false
                };
            }
        }

        public void Save(FingerPrintInfo fingerPrint)
        {
            var insertSQL = string.Format("insert into biometricInfo(StaffId, template, imageWidth, imageHeight, imageDPI,  imageQuality, fingerPosition, serialNumber, model, manufacturer, creator, date_created)");
            insertSQL += string.Format("Values('{0}','{1}',{2},{3},{4},{5},'{6}','{7}','{8}','{9}','{10}', GETDATE())", fingerPrint.staffId, fingerPrint.Template, fingerPrint.ImageWidth, fingerPrint.ImageHeight, fingerPrint.ImageDPI, fingerPrint.ImageQuality, fingerPrint.FingerPositions, fingerPrint.SerialNumber, fingerPrint.Model, fingerPrint.Manufacturer, fingerPrint.Creator);
            ExecuteQuery(insertSQL);
        }

        public string RetrievePatientNameByUniqueId(string patientuniqueId)
        {
            var sql = string.Format("SELECT CONCAT(given_name,' ',family_name) AS patient_name, pid.patient_id FROM person_name pn " +
                                 "INNER JOIN patient_identifier pid ON pn.person_id=pid.patient_id " +
                                 "WHERE pid.identifier_type=4 AND pid.identifier= @patientuniqueId;");

            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();

            IDbCommand sql_cmd = new SqlCommand(sql, (SqlConnection)sql_con);

            var param = sql_cmd.CreateParameter();
            param.ParameterName = "@patientuniqueId";
            param.DbType = DbType.String;
            param.Value = patientuniqueId;
            sql_cmd.Parameters.Add(param);

            using (var rd = sql_cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    return string.Format("{0}|{1}", rd[0], rd[1]);
                    //break;
                }
                return null;
            }
            //sql_cmd = null;
            //sql_con.Close();
        }

        /*
         public string RetrievePatientNameByPatientId(int staffId)
         {
             string sql = string.Format("SELECT CONCAT(given_name,' ',family_name) AS patient_name, pid.identifier FROM person_name pn " +
                 "INNER JOIN patient_identifier pid ON pn.person_id = pid.patient_id " +
                 "WHERE pid.identifier_type = 4 AND pid.patient_id = @patientId;");

             if (sql_con.State != ConnectionState.Open)
                 sql_con.Open();

             IDbCommand sql_cmd = new SqlCommand(sql, (SqlConnection)sql_con);

             var param = sql_cmd.CreateParameter();
             param.ParameterName = "@patientId";
             param.DbType = DbType.Int32;
             param.Value = staffId;
             sql_cmd.Parameters.Add(param);

             using (var rd = sql_cmd.ExecuteReader())
             {
                 while (rd.Read())
                 {
                     return string.Format("{0}|{1}", rd[0], rd[1]);
                     break;
                 }
                 return null;
             }
             sql_cmd = null;
             sql_con.Close();
         }
         */

        public string RetrieveStaffIdByID(string StaffId)
        {
            var sql = string.Format("SELECT CONCAT([FirstName], ' ',[MiddleName], ' ',[LastName]) as Fullname, [StaffId]  FROM [dbo].[staff_details] where StaffId = @StaffId;");

            if (sql_con.State != ConnectionState.Open)
                sql_con.Open();

            using (IDbCommand sql_cmd = new SqlCommand(sql, (SqlConnection)sql_con))
            {
                var param = sql_cmd.CreateParameter();
                param.ParameterName = "@StaffId";
                param.DbType = DbType.String;
                param.Value = StaffId;
                sql_cmd.Parameters.Add(param);

                using (var rd = sql_cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        return string.Format("{0}|{1}", rd[0], rd[1]);
                    }
                    return null;
                }
            }
        }

        public void Dispose()
        {
            DS.Dispose();
            DT.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}

  
