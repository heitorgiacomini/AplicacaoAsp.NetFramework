using Modelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CompraDAO : Conexao
    {

        public void DesativarCompra(Modelo.Compra modelocompra)
        {
            try{
                SqlCommand comando = CriarComando("DesativarCompra", System.Data.CommandType.StoredProcedure);
                comando.Parameters.AddWithValue("@CodCompra", modelocompra.Codigo);
                comando.ExecuteNonQuery();
                comando.Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public List<Modelo.Compra> TodasCompras(int id =0)
        {
            try
            {
                List<Modelo.Compra> compras = new List<Modelo.Compra>();
                SqlCommand comando = CriarComando("LerTodasCompra", CommandType.StoredProcedure);
                if (id != 0)
                {
                    comando.Parameters.AddWithValue("@IdCompra", id);
                }
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Modelo.Compra c = new Modelo.Compra();
                        c.Codigo = (int)reader["Codigo"];
                        c.NomeCliente = (string)reader["Nome"];
                        c.CodCliente = (int)reader["CodCliente"];
                        c.DataCompra = reader["DataCompra"].ToString() != string.Empty ? (DateTime)reader["DataCompra"] : c.DataCompra;
                        c.ValorTotal = reader["ValorTotal"].ToString() != string.Empty ? (decimal)reader["ValorTotal"] : c.ValorTotal;
                        compras.Add(c);
                    }
                    if (reader.NextResult())
                    {
                        List<Modelo.ItemCompra> compraitem = new List<Modelo.ItemCompra>();
                        while (reader.Read())
                        {
                            Modelo.ItemCompra m = new Modelo.ItemCompra();
                            m.codCompra = (int)reader["CodCompra"];
                            m.guid = Guid.NewGuid().ToString();
                            m.Codigo = (int)reader["Codigo"];  //codigo do item    
                            m.codProduto = (int)reader["codProduto"];
                            m.auxDescricao = (string)reader["Descricao"];
                            m.auxValor = (decimal)reader["ValorUnitario"];
                            m.Quantidade = (int)reader["Quantidade"];

                            //m.ValorTotal = reader["ValorTotal"].ToString() != string.Empty ? (decimal)reader["ValorTotal"] : c.ValorTotal;
                            compraitem.Add(m);
                        }
                        foreach (Modelo.Compra compramodelo in compras)
                        {
                            compramodelo.AuxItems = compraitem.Where(x => x.codCompra == compramodelo.Codigo).ToList();
                        }
                    }
                }
                comando.Connection.Close();
                return compras;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int Salvar(Modelo.Compra modelocompra)
        {
            try
            {
                SqlCommand comando = CriarComando("Insert into Compra (CodCliente, ValorTotal) values (@CodCliente, @ValorTotal); select SCOPE_IDENTITY();", System.Data.CommandType.Text);
                //Insert into Cliente (Nome, DataNascimento, CPF, Endereco) values (@nome, @datanascimento, @cpf, @endereco)
                comando.Parameters.AddWithValue("@CodCliente", modelocompra.CodCliente);
                comando.Parameters.AddWithValue("@ValorTotal", modelocompra.ValorTotal);
                int codigocompra = int.Parse(comando.ExecuteScalar().ToString());
                comando.Connection.Close();
                return codigocompra;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SalvarTudo(Modelo.Compra modelocompra)
        {
            try
            {
                //Insert into Compra (CodCliente, ValorTotal) values (@CodCliente, @ValorTotal); 
                SqlCommand comando = CriarComando("SalvarCompra", System.Data.CommandType.StoredProcedure);
                comando.Parameters.AddWithValue("@CodCliente", modelocompra.CodCliente);
                comando.Parameters.AddWithValue("@ValorTotal", modelocompra.ValorTotal);
                DataTable tabela = PreencherDataTable(modelocompra.AuxItems);
                SqlParameter parametro = new SqlParameter();
                parametro.Value = tabela;
                parametro.SqlDbType = SqlDbType.Structured;
                parametro.ParameterName = "items";
                comando.Parameters.Add(parametro);

                comando.ExecuteNonQuery();
                comando.Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void AtualizarCompra(Compra modelocompra)
        {
            try
            {
                SqlCommand comando = CriarComando("AtualizarCompra", System.Data.CommandType.StoredProcedure);
                comando.Parameters.AddWithValue("@CodCompra", modelocompra.Codigo);
                comando.Parameters.AddWithValue("@CodCliente", modelocompra.CodCliente);
                DataTable tabela = PreencherDataTableUpdate(modelocompra.AuxItems);
                SqlParameter parametro = new SqlParameter();
                parametro.Value = tabela;
                parametro.SqlDbType = SqlDbType.Structured;
                parametro.ParameterName = "meusitensdacompra";
                comando.Parameters.Add(parametro);

                comando.ExecuteNonQuery();
                comando.Connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable PreencherDataTableUpdate(List<Modelo.ItemCompra> auxItems) {
            try
            {

                DataTable tabela = new DataTable();
                tabela.Columns.Add("codItem");
                tabela.Columns.Add("codCompra");
                tabela.Columns.Add("codProduto");
                tabela.Columns.Add("Quantidade");
                foreach (Modelo.ItemCompra item in auxItems)
                {
                    DataRow linha = tabela.NewRow();
                    linha["codItem"] = item.Codigo;
                    linha["codCompra"] = item.codCompra;
                    linha["codProduto"] = item.codProduto;
                    linha["Quantidade"] = item.Quantidade;
                    tabela.Rows.Add(linha);
                }
                return tabela;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable PreencherDataTable(List<Modelo.ItemCompra> auxItems)
        {
            try
            {
                DataTable tabela = new DataTable();
                tabela.Columns.Add("codProduto");
                tabela.Columns.Add("Quantidade");
                foreach (Modelo.ItemCompra item in auxItems)
                {
                    DataRow linha = tabela.NewRow();
                    linha["codProduto"] = item.codProduto;
                    linha["Quantidade"] = item.Quantidade;
                    tabela.Rows.Add(linha);
                }
                return tabela;
                //codProduto
                //Quantidade
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}



//using (SqlConnection conn = new SqlConnection(connection))
//{
//    DataSet dataset = new DataSet();
//    SqlDataAdapter adapter = new SqlDataAdapter();
//    adapter.SelectCommand = new SqlCommand("MyProcedure", conn);
//    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
//    adapter.Fill(dataset);
//    return dataset;
//}