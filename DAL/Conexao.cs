using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public abstract class Conexao
    {
        private const string CONNECTIONSTRING = "Password=123@cetec;Persist Security Info=True;User ID=usr_heitor;Initial Catalog=TreinamentoH;Data Source=bd.ceteclins.com.br";

        protected SqlCommand CriarComando(String sql, CommandType cmdTipo = CommandType.Text)
        {
            SqlConnection conexao = new SqlConnection(CONNECTIONSTRING);
            conexao.Open();
            SqlCommand comando = conexao.CreateCommand();
            //string sql = "SELECT * FROM Users WHERE Name=@name";
            comando.CommandText = sql;            
            comando.CommandType = cmdTipo;
            return comando;
        }
    }
}
