using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ProdutoDAO : Conexao
    {
        public void Inserir(Modelo.Produto modeloproduto)
        {
            try
            {
                SqlCommand comando = CriarComando("Insert into Produto (Descricao, ValorUnitario) values (@descricao, @valorunitario)");
                //"Insert into Produto (Descricao, ValorUnitario) values (@descricao, @valorunitario)";
                comando.Parameters.AddWithValue("@descricao", modeloproduto.Descricao);
                comando.Parameters.AddWithValue("@valorunitario", modeloproduto.ValorUnitario);
                comando.ExecuteNonQuery();
                comando.Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Modelo.Produto BuscarPorId(int id)
        {
            Modelo.Produto resultado = new Modelo.Produto();
            SqlCommand comando = CriarComando("Select * From Produto Where Codigo= @id");
            comando.Parameters.AddWithValue("@id", id);

            using (SqlDataReader reader = comando.ExecuteReader())
            {
                if (reader.Read())
                {                    
                    resultado.Codigo = (int)reader["Codigo"];
                    resultado.DataCriacao = (DateTime)reader["DataCriacao"];
                    resultado.Descricao = (string)reader["Descricao"];
                    resultado.ValorUnitario = (decimal)reader["ValorUnitario"];
                    resultado.Excluido = (bool)reader["Excluido"];
                    resultado.DataExcluido = reader["DataExclusao"].ToString() == String.Empty ? resultado.DataExcluido : (DateTime)reader["DataExclusao"];  
                    
                }
            }
            comando.Connection.Close();
            return resultado;
        }

        public List<Modelo.Produto> BuscarTodos()
        {
            try
            {
                List<Modelo.Produto> produtos = new List<Modelo.Produto>();
                SqlCommand comando = CriarComando("Select * From Produto Where Excluido = 0");
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Modelo.Produto c = new Modelo.Produto();
                        c.Codigo = (int)reader["Codigo"];
                        c.DataCriacao = (DateTime)reader["DataCriacao"];
                        c.Descricao = (string)reader["Descricao"];
                        c.ValorUnitario = (decimal)reader["ValorUnitario"];
                        produtos.Add(c);
                    }
                }
                comando.Connection.Close();
                return produtos;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

