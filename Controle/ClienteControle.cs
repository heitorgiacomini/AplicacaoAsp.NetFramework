using Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controle
{
    public class ClienteControle
    {
        public void Salvar(Cliente modelocliente)
        {
            try
            {
                if(modelocliente.DataNascimento > DateTime.Now){
                    throw new Exception("Data Impossivel");
                }
                DAL.ClienteDAO daocliente = new DAL.ClienteDAO();
                daocliente.Inserir(modelocliente);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Cliente> BuscarTodosClientes()
        {
            try
            {
                DAL.ClienteDAO cliente = new DAL.ClienteDAO();
                return cliente.BuscarTodos();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
