using Modelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ClienteDAO : Conexao
    {
        public void Inserir(Cliente cliente)
        {
            try
            {
                SqlCommand comando = CriarComando("Insert into Cliente (Nome, DataNascimento, CPF, Endereco) values (@nome, @datanascimento, @cpf, @endereco)");
                //Insert into Cliente (Nome, DataNascimento, CPF, Endereco) values (@nome, @datanascimento, @cpf, @endereco)
                comando.Parameters.AddWithValue("@nome", cliente.Nome);
                comando.Parameters.AddWithValue("@datanascimento", cliente.DataNascimento);
                comando.Parameters.AddWithValue("@cpf", cliente.Cpf);
                comando.Parameters.AddWithValue("@endereco", cliente.Endereco);
                comando.ExecuteNonQuery();
                comando.Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Cliente> BuscarTodos()
        {
            try
            {
                List<Cliente> clientes = new List<Cliente>();
                SqlCommand comando = CriarComando("Select * From Cliente Where  Excluido is NULL OR Excluido = 0");//Select * From Compra Where Excluido is NULL OR Excluido = 0
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Cliente c = new Cliente();
                        c.Codigo = (int)reader["Codigo"];
                        c.Nome = (string)reader["Nome"];
                        c.DataNascimento = (DateTime)reader["DataNascimento"];
                        c.Cpf = (string)reader["CPF"];
                        c.Endereco = (string)reader["Endereco"];
                        clientes.Add(c);
                    }
                }
                comando.Connection.Close();
                return clientes;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
