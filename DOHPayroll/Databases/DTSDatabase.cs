using DOHPayroll.DTRModels;
using DOHPayroll.Models;
using DOHPayroll.Models.PostModels;
using DOHPayroll.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DOHPayroll.Databases
{
    public sealed class DTSDatabase
    {
        public static string CONNECTION_STRING { get; set; }

        private static DTSDatabase INSTANCE;

        private DTSDatabase() { }

        public static DTSDatabase Instance
        {
            get
            {
                if(INSTANCE == null)
                {
                    INSTANCE = new DTSDatabase();
                    INSTANCE.Initialize(1);
                }
                return INSTANCE;
            }
        }

        public void Initialize(int index)
        {
            CONNECTION_STRING += "SERVER=" + ConnectionString.Servers[index] + ";";
            CONNECTION_STRING += "DATABASE=dts;";
            CONNECTION_STRING += "UID=" + ConnectionString.UIDs[index] + ";";
            CONNECTION_STRING += "PASSWORD=" + ConnectionString.Password[index] + ";";
            CONNECTION_STRING += "pooling = false; SslMode=none; convert zero datetime=True";
        }

        #region GET DIVISION
        public async Task<SelectList> GetDivisionAsync()
        {
            var query = "SELECT * FROM dts.division";

            var division = new List<SelectListModel>();

            using (MySqlConnection connection = new MySqlConnection(CONNECTION_STRING))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query.ToString(), connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        division.Add(new SelectListModel
                        {
                            value = reader["id"].ToString(),
                            text = reader["description"].ToString()
                        });
                    }
                }
                connection.Close();
            }

            var selectList = new SelectList(division, "value", "text");

            return selectList;
        }
        #endregion
    }
}
