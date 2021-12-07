using CDK.Entidade;
using Cetec.Notificacao.Exception;
using Controle;
using Controle.App;
using Controle.Comum;
using Controle.Exportar;
using Controle.Imobiliario;
using Controle.Importar;
using Controle.ProcessoQualidade;
using CTGIS.Uteis;
using CTGIS.Visao.GeoServer;
using Entidade;
using Entidade.App;
using Entidade.Comum;
using Entidade.Ferramenta;
using Entidade.Imobiliario;
using Entidade.ProcessoQualidade;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.NetworkInformation;
using System.Web.Http;

namespace CTGIS.Visao.Comum.Imobiliario.FRM
{
    public partial class FRM010001 : System.Web.UI.Page
    {
        private static string systemPath = ConfigurationManager.AppSettings["systemPath"];
        private LayerConfiguracaoCTR layerConfigCtr = new LayerConfiguracaoCTR();
        private LayerCtr layerCTR = new LayerCtr();
        private static int processorCounter = (int)Math.Floor(Environment.ProcessorCount * 0.8);
        private ClienteConfiguracaoCtr clienteConfiguracaoCtr = new ClienteConfiguracaoCtr();


        public object GetSession(ESession session)
        {
            object state = ControleSession.GetSession(session);

            if (state == null)
            {
                Response.Redirect("~/Visao/Comum/FRM/FRM000101.aspx?redirect=sessaoexpirada&return=" + HttpContext.Current.Request.Url.AbsolutePath);
            }

            return state;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);

                    FuncionarioControle funcionarioControle = new FuncionarioControle();

                    if (funcionarioCliente != null)
                    {
                        CarregarLayers();
                        ListarArquivos();
                        ListaRelatorios();
                        VerificarPrivilegios();
                        ListarOcorrencias();
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);

                if (ex.GetType().ToString() == "Cetec.Notificacao.Exception.CetecErroException")
                {
                    CetecErroException erro = (CetecErroException)ex;

                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "erroLogged", "console.log('" + erro.Configuracao.Mensagem + "');", true);
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "erroLogged", "console.log('" + ex.Message + "');", true);
                }
            }
        }

        [WebMethod]
        public static string[] BuscarFeicaoPorCodigo(string Codigo)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            long result;

            if (long.TryParse(Codigo, out result))
            {
                return ferramentaCTR.BuscarFeicaoPorCodigo(long.Parse(Codigo));
            }
            else
                return null;
        }

        [WebMethod]
        public static string ExportarFeicao(string Coordenadas, int EPSG)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<FeicaoExportacao> vetorCoordenadas = serializer.Deserialize<List<FeicaoExportacao>>(Coordenadas);

            foreach (FeicaoExportacao vetor in vetorCoordenadas)
            {
                vetor.Coordenadas = ferramentaCTR.ExportarFeicao(vetor.Coordenadas, EPSG);
            }

            string retorno = serializer.Serialize(vetorCoordenadas.ToArray());

            return retorno;
        }

        [WebMethod]
        public static string GetWMS()
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            LayerCtr layerCTR = new LayerCtr();

            return serializer.Serialize(layerCTR.GetWMS(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario).ToArray());
        }

        [WebMethod]
        public static string GetDadosMapa()
        {
            ProjetoLayerConfiguracaoCTR projetoLayerConfiguracaoCTR = new ProjetoLayerConfiguracaoCTR();
            ClienteConfiguracaoCtr clienteConfiguracaoCtr = new ClienteConfiguracaoCtr();
            LayerCtr layerCTR = new LayerCtr();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];

            DadosMapa dadosMapa = new DadosMapa();
            dadosMapa.Tematicos = projetoLayerConfiguracaoCTR.getTematicos(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);
            dadosMapa.WMS = layerCTR.GetWMS(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);

            ClienteConfiguracao clienteConfiguracao = clienteConfiguracaoCtr.BuscarPorCliente(funcionarioCliente.CodCliente);
            dadosMapa.ZoomInicial = clienteConfiguracao != null && clienteConfiguracao.ZoomPadrao > 0 ? clienteConfiguracao.ZoomPadrao : 7;

            return serializer.Serialize(dadosMapa);
        }

        [WebMethod]
        public static void SetModal(TipoModal tipoModal)
        {
            HttpContext.Current.Session["tipoModal"] = tipoModal;
        }

        [WebMethod]
        public static void RemoverFeicao(string camada, string codigos)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            JavaScriptSerializer JSserializer = new JavaScriptSerializer();
            FerramentaCTR ferramentaCTR = new FerramentaCTR();

            List<long> codigosSelecionados = JSserializer.Deserialize<List<long>>(codigos);

            ferramentaCTR.RemoverFeicao(codigosSelecionados.ToArray(), camada, funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);
        }

        [WebMethod]
        public static string GetCentroid(string geo)
        {
            string[] vetorRetorno;
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            string retorno = string.Empty;
            retorno = ferramentaCTR.GetCentroid(geo);
            vetorRetorno = retorno.Split('/');

            return retorno;
        }

        [WebMethod]
        public static string BuscarCentroideZoom(string Codigo, string Camada)
        {
            string[] vetorRetorno;
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            string retorno = string.Empty;
            retorno = ferramentaCTR.BuscarCentroideZoom(long.Parse(Codigo), Camada);
            vetorRetorno = retorno.Split('/');
            return retorno;
        }

        [WebMethod]
        public static string Desvincular(string Codigo, string Camada, string Atributo)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            return ferramentaCTR.Desvincular(Codigo, Camada, Atributo);
        }

        [WebMethod]
        public static void Alerta(string Titulo, string Mensagem, string Tipo)
        {
            Page page = (Page)HttpContext.Current.CurrentHandler;

            if (ScriptManager.GetCurrent(page) != null)
            {
                ScriptManager.RegisterStartupScript(page, typeof(Page), "ApprovalHistory", "console.log('Entrou');", true);
            }
            else
            {
                page.ClientScript.RegisterStartupScript(typeof(Page), "ApprovalHistory", "console.log('Entrou');", true);
            }
        }

        [WebMethod]
        public static bool PossuiFotoArquivo(string CodigosGeograficos)
        {
            JavaScriptSerializer s = new JavaScriptSerializer();
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            FotoCtr fotoCTR = new FotoCtr(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);
            return fotoCTR.PossuiFotoArquivo(s.Deserialize<string[]>(CodigosGeograficos));
        }

        [WebMethod]
        public static string GetDadosFotoArquivo(string CodigosGeograficos, string Camada)
        {
            FuncionarioCliente funcionarioCliente = new FuncionarioCliente();
            JavaScriptSerializer s = new JavaScriptSerializer();

            FotoCtr fotoCTR = new FotoCtr(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);
            return s.Serialize(fotoCTR.GetDadosFotoArquivo(s.Deserialize<string[]>(CodigosGeograficos), Camada));
        }

        [WebMethod]
        public static bool ValidarPopup()
        {
            bool popupValidado = false;

            string usuario = HttpContext.Current.Session["login"].ToString(); ;

            FuncionarioControle funcionarioControle = new FuncionarioControle();
            popupValidado = funcionarioControle.ValidarPopup(usuario);

            return popupValidado;
        }

        [WebMethod]
        public static string InformacoesGetData(string camada, string geografico)
        {
            try
            {
                FerramentaCTR ferramentaCTR = new FerramentaCTR();
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                List<Informacoes> informacoes = ferramentaCTR.informacoesGetData(camada, serializer.Deserialize<string[]>(geografico).ToList());

                return serializer.Serialize(informacoes);
            }
            catch (Exception erro)
            {
                new Logger().LogErro(erro);

                throw new Exception(erro.Message);
            }
        }

        [WebMethod(EnableSession = true)]
        public static string[] GerarTopologia(string camadaOrigem, string atributoOrigem, string camadaDestino, string atributoDestino, string relacaoEspacial, string feicoesSelecionadas, string codCliente, string tipoDiferente, string atualizar)
        {
            List<string> codigos = new List<string>();
            List<Int64> codigosSelecionados = new List<Int64>();
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            DateTime tempo_inicial = DateTime.Now;
            DateTime tempo_final = DateTime.Now;
            HttpContext context = HttpContext.Current;

            int interval = 5;

            if (feicoesSelecionadas != "null")
            {
                JavaScriptSerializer JSserializer = new JavaScriptSerializer();
                codigosSelecionados = JSserializer.Deserialize<List<long>>(feicoesSelecionadas);
                codigos = ferramentaCTR.GerarTopologia(camadaOrigem, atributoOrigem, camadaDestino, atributoDestino, relacaoEspacial, codigosSelecionados, long.Parse(codCliente), tipoDiferente, bool.Parse(atualizar));
            }
            else
            {
                /*
                 * Somente funciona para a camada de pontos
                 * o postgre vai retornar um array contendo todos os pontos na camada de pontos,
                 * depois o sistema vai pegar a quantidade de elementos que foi colocado na variavel jump e enviar para a proc_u_topologia
                 */
                if (camadaOrigem == "imobiliario.pontos")
                {
                    int index = 0;

                    List<string> pontos_origem = new List<string>(ferramentaCTR.ListPontos(camadaOrigem, codCliente));

                    //Serve somente para pegar o total que será inserido na camada de destino
                    List<string> pontos_destino = new List<string>(ferramentaCTR.ListPontos(camadaDestino, codCliente));

                    context.Session["progress_total"] = pontos_destino.Count();
                    context.Session["importados"] = 0;
                    context.Session["progress"] = 0;

                    while (index < pontos_origem.Count)
                    {
                        Int32 limit = 0;
                        Thread[] AvaiablesThreads = new Thread[processorCounter];

                        var response = HttpContext.Current.Response;

                        if (response.Cookies["theme"] != null)
                            response.Cookies["theme"].Value = "roger";

                        //var ecookie = new HttpCookie("ecookie");
                        //ecookie["name"] = "roger";
                        //HttpContext.Current.Response.Cookies.Add(ecookie);

                        for (int t = 0; t < AvaiablesThreads.Length; t++)
                        {
                            limit = (pontos_origem.Count - index >= interval ? interval : pontos_origem.Count - index);

                            if (limit > 0)
                            {
                                AvaiablesThreads[t] = new Thread(() =>
                                {
                                    tempo_inicial = DateTime.Now;

                                    var codSelecionados = pontos_origem.GetRange(index, limit).ToArray();

                                    int quantidade = ferramentaCTR.GerarTopologiaPontos(camadaOrigem, atributoOrigem, camadaDestino, atributoDestino, relacaoEspacial, codSelecionados, long.Parse(codCliente), tipoDiferente, bool.Parse(atualizar));

                                    tempo_final = DateTime.Now;

                                    if (context != null)
                                    {
                                        //int registroRestantes = pontos_origem.Count - HttpContext.Current.Session["progress"] / interval;
                                        context.Session["progress"] = quantidade;

                                        if (context.Session["progress"] != null)
                                        {
                                            int registroRestantes = int.Parse(context.Session["progress"].ToString());
                                            double tempo_estimado = Math.Ceiling((((tempo_final - tempo_inicial).TotalSeconds) * registroRestantes) / 60);

                                            //HttpContext.Current.Session["tempo_estimado"] = tempo_estimado;
                                            context.Session["tempo_estimado[" + Thread.CurrentThread.Name + "]"] = tempo_estimado;

                                            context.Session["importados"] = int.Parse(context.Session["progress"].ToString() ?? "0") + 1;
                                        }
                                    }
                                });

                                AvaiablesThreads[t].Name = "Importacao_" + t.ToString();
                                //Iniciando a thread passando os codigos como parametro da função definida ao criar a thread
                                AvaiablesThreads[t].Start();

                                context.Session["tempo_estimado[" + AvaiablesThreads[t].Name + "]"] = 0;

                                index += limit;
                            }
                            else
                            {
                                break;
                            }
                        }

                        System.Threading.Thread.Sleep(500);

                        //Esperando todas as threads terminaram para depois atribuir mais lotes
                        for (int t = 0; t < AvaiablesThreads.Length; t++)
                        {
                            if (AvaiablesThreads[t] != null)
                            {
                                AvaiablesThreads[t].Join();

                                Console.WriteLine("Thread_{0} finished.",
                                AvaiablesThreads[t].ManagedThreadId);
                            }
                        }
                    }


                }
                else
                {
                    codigos = ferramentaCTR.GerarTopologia(camadaOrigem, atributoOrigem, camadaDestino, atributoDestino, relacaoEspacial, null, long.Parse(codCliente), tipoDiferente, bool.Parse(atualizar));
                }

            }

            List<string> retorno = codigos;
            return retorno.ToArray();
        }

        [WebMethod]
        public static string GetTotalDeFeicoes(string camada)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            string retorno = string.Empty;

            retorno = ferramentaCTR.GetTotalDeFeicoes(camada, funcionarioCliente.CodCliente);

            return retorno;
        }

        [WebMethod]
        public static void RemoverCentroides(string camada)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];

            ferramentaCTR.RemoverCentroides(camada, funcionarioCliente.CodFuncionario, funcionarioCliente.CodCliente);
        }

        [WebMethod]
        public static string getBoundingBox(string camada)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            FerramentaCTR ferramentaCTR = new FerramentaCTR();

            return ferramentaCTR.getBoundingBox(camada, funcionarioCliente.CodCliente);
        }

        [WebMethod]
        public static string getTematicos()
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];

            ProjetoLayerConfiguracaoCTR projetoLayerConfiguracaoCTR = new ProjetoLayerConfiguracaoCTR();

            return new JavaScriptSerializer().Serialize(projetoLayerConfiguracaoCTR.getTematicos(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario));
        }

        [WebMethod]
        public static string ZoomLayersCliente()
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];

            LayerConfiguracaoCTR layerConfiguracaoCTR = new LayerConfiguracaoCTR();

            return new JavaScriptSerializer().Serialize(layerConfiguracaoCTR.ZoomLayersCliente(funcionarioCliente.CodCliente));
        }

        [WebMethod]
        public static string BuscarCoordenadasLote(string CodigosTributarios, string CodCliente)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            List<string> coordenadas = ferramentaCTR.BuscarCoordenadasLote(serializer.Deserialize<List<string>>(CodigosTributarios), long.Parse(CodCliente));

            return serializer.Serialize(coordenadas);
        }

        [WebMethod]
        public static string GetCodCaracteristica()
        {
            string codCaracteristica = (string)HttpContext.Current.Session["codcaracteristica"];
            return codCaracteristica;
        }

        [WebMethod]
        public static bool VerificarImovel(string codLote)
        {
            long codigoLote;
            codigoLote = long.TryParse(codLote, out codigoLote) ? codigoLote : 0;

            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            LoteCtr loteCtr = new LoteCtr(funcionarioCliente.CodCliente);

            return loteCtr.VerificarImovel(codigoLote, funcionarioCliente.CodCliente.ToString());
        }

        [WebMethod]
        public static void VincularImovel(string codLote, string valoresPrimaryKey, string principal)
        {
            string[] teste = valoresPrimaryKey.Split('.');
            string cod = teste[1].ToLower();

            long codigo, codigoLote;
            bool ePrincipal;
            codigo = long.TryParse(cod, out codigo) ? codigo : 0;
            codigoLote = long.TryParse(codLote, out codigoLote) ? codigoLote : 0;
            ePrincipal = bool.TryParse(principal, out ePrincipal) ? ePrincipal : false;

            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            LoteCtr loteCtr = new LoteCtr(funcionarioCliente.CodCliente);

            loteCtr.VincularImovel(codigo, codigoLote, ePrincipal);
        }

        [WebMethod]
        public static bool VincularInscricaoLote(string CodLote, string ValoresPrimaryKey)
        {
            try
            {
                FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
                ImovelCTR imovelCTR = new ImovelCTR();

                long codLote, codImovel;

                if (long.TryParse(CodLote, out codLote) && long.TryParse(ValoresPrimaryKey.Split('.')[1], out codImovel))
                    return imovelCTR.VincularImovelInscricao(codImovel, funcionarioCliente.CodFuncionario, funcionarioCliente.CodCliente, codLote);

                return false;
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);

                throw ex;
            }
        }

        [WebMethod]
        public static bool GetPopupContext()
        {
            FuncionarioControle funcionarioCTR = new FuncionarioControle();

            string usuario = (string)HttpContext.Current.Session["login"];

            return funcionarioCTR.IsUserInRole(usuario, EPrivilegio.TABPANEL_CADASTRO);
        }

        [WebMethod]
        public static string GetCoordenadasWGS84(string StringCodGeografico)
        {
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            long codGeografico;

            if (long.TryParse(StringCodGeografico, out codGeografico))
            {
                return ferramentaCTR.GetCoordenadasWGS84(codGeografico);
            }
            else
            {
                return string.Empty;
            }
        }

        protected void btnAbrirModal_Click(object sender, EventArgs e)
        {
            //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "dgasdgsagagasdgasdg", "console.log('Entrou no btnAbrirModal');", true);
            int paginaSelecionada = int.Parse(hfPage.Value);
            //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "paginaSelecionada", "console.log('Carregou btnAbrirModal com paginaSelecionada = ' + " + paginaSelecionada + ");", true); 
            Modal.Abrir(this, (TipoModal)paginaSelecionada);
            upModal.Update();
        }

        private void CarregarLayers()
        {
            try
            {
                FuncionarioControle funcionarioCTR = new FuncionarioControle();
                FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                ProjetoLayerConfiguracaoCTR projetoLayerConfiguracaoCTR = new ProjetoLayerConfiguracaoCTR();
                GrupoTrabalhoCtr grupoTrabalhoCtr = new GrupoTrabalhoCtr(funcionarioCliente.CodCliente);
                List<LayerConfiguracao> layersCarregadas = new List<LayerConfiguracao>();
                List<CamadaGeoServer> listaCamada = new List<CamadaGeoServer>();

                GrupoTrabalho grupoTrabalho = grupoTrabalhoCtr.Buscar();
                List<LayerConfiguracao> layers = grupoTrabalho.ProjetoGrupoTrabalho[0].Projeto.LayerConfiguracao;
                List<LayerConfiguracao> layersVisiveis = funcionarioCTR.GetLayersVisiveis(funcionarioCliente.CodFuncionario, funcionarioCliente.CodCliente);
                List<Entidade.Comum.Tematico> temTematico = projetoLayerConfiguracaoCTR.getTematicos(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);

                foreach (LayerConfiguracao layer in layers)
                {
                    if (layersVisiveis.Select(x => x.Codigo).Contains(layer.Codigo))
                    {
                        layersCarregadas.Add(layer);
                    }
                }

                foreach (var layer in layersCarregadas)
                {
                    List<ProjetoLayerConfiguracao> projLayerConfiguracao = new List<ProjetoLayerConfiguracao>();

                    if (temTematico.Exists(x => x.NomeCamada == layer.Nome))
                    {
                        List<ProjetoLayerConfiguracao> tematicosBanco = new List<ProjetoLayerConfiguracao>();
                        foreach (Entidade.Comum.Tematico tematico in temTematico)
                        {
                            if (tematico.NomeCamada == layer.Nome)
                            {
                                ProjetoLayerConfiguracao novoTematico = new ProjetoLayerConfiguracao();
                                novoTematico.Codigo = layer.ProjetoLayerConfiguracao.First().Codigo;
                                novoTematico.CodLayerConfiguracao = layer.ProjetoLayerConfiguracao.First().CodLayerConfiguracao;
                                novoTematico.Nome = layer.ProjetoLayerConfiguracao.First().Nome;

                                novoTematico.CorBorda = tematico.CorBorda;
                                novoTematico.CorFundo = tematico.CorFundo;
                                novoTematico.Opacidade = tematico.Opacidade;
                                novoTematico.Condicao = tematico.Condicao;
                                novoTematico.NomeFiltro = tematico.NomeFiltro;

                                tematicosBanco.Add(novoTematico);
                            }
                        }
                        projLayerConfiguracao = tematicosBanco;
                    }
                    else
                    {
                        projLayerConfiguracao = layer.ProjetoLayerConfiguracao;
                    }

                    List<CDK.Entidade.Tematico> tematicos = new List<CDK.Entidade.Tematico>();

                    if (projLayerConfiguracao != null)
                    {
                        foreach (var tematico in projLayerConfiguracao)
                        {

                            Etiqueta etiqueta = null;
                            if (((tematico.Filtro != "") && (tematico.Filtro != null)) || ((layer.Filtro != "") && (layer.Filtro != null)))
                            {
                                etiqueta = new Etiqueta();

                                etiqueta.Text = ((tematico.Filtro != null && tematico.Filtro.Length > 0) ? tematico.Filtro : layer.Filtro);
                            }

                            CDK.Entidade.Tematico t = new CDK.Entidade.Tematico();
                            t.Condicao = tematico.Condicao;
                            t.Cor = tematico.CorFundo;
                            t.CorBorda = tematico.CorBorda;
                            t.Opacidade = tematico.Opacidade;
                            t.Nome = tematico.NomeFiltro;

                            if (etiqueta != null)
                            {
                                t.Etiqueta = etiqueta;
                                t.Etiqueta.Cor = "#ffffff";
                                t.Etiqueta.CorBorda = tematico.CorBorda ?? "";
                                t.Etiqueta.Opacidade = 1;
                                t.Etiqueta.TipoLayer = layer.TipoLayer;
                                t.Etiqueta.TamanhoOutlineLegenda = (tematico.TamanhoOutlineLegenda > 0 ? tematico.TamanhoOutlineLegenda : (layer.TamanhoOutlineLegenda > 0 ? layer.TamanhoOutlineLegenda : 0));
                                t.Etiqueta.Filtro = ((funcionarioCliente.Cliente.NomeView).Replace("vw", "").Replace("_", "")) + "_" + layer.Filtro;
                            }

                            if (t.Condicao != null)
                                tematicos.Add(t);
                        }
                    }

                    CamadaGeoServer camada = new CamadaGeoServer();
                    camada.Nome = layer.Nome;
                    camada.EPSG = layer.ClienteConfiguracao.ProjecaoInicial;
                    camada.Visivel = layer.Visivel;
                    camada.Host = layer.ClienteConfiguracao.GeoServerHost;
                    camada.Porta = layer.ClienteConfiguracao.GeoServerPorta;
                    camada.EspacoTrabalho = layer.ClienteConfiguracao.Espacotrabalho;
                    camada.Bbox = layer.ClienteConfiguracao.Extende;
                    camada.Filtro = layer.Filtro;
                    camada.Service = layer.Service;
                    camada.ZoomMinimo = layer.ZoomMinimo;
                    camada.ZoomMaximo = layer.ZoomMaximo;
                    camada.TipoLayer = layer.TipoLayer;
                    camada.Descricao = layer.Descricao;

                    if (tematicos.Count > 0)
                        camada.Tematico = tematicos;

                    listaCamada.Add(camada);
                }

                vwMapa.DataSource = listaCamada;
                vwMapa.DataBind();

                legendaMapa.Viewer = vwMapa;
                legendaMapa.AddViewer();

            }
            catch (Exception er)
            {
                if (er.Message.Contains("Cetec.Notificacao.Exception.CetecErroException"))
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroCetec", "swal('Erro', '" + ((CetecErroException)er).Configuracao.Mensagem + "' , 'error');", true);
                //throw new Exception(((CetecErroException)er).Configuracao.Mensagem);
                else
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroGenerico", "swal('Erro', '" + er.Message + "', 'error');", true);
            }
        }

        private Entidade.Imobiliario.ConfiguracaoGIS.Camada AddGeoServer(string url, string path, string workspace, string datastores)
        {
            ServidorGeografico sg = new ServidorGeografico(url);
            Entidade.Imobiliario.ConfiguracaoGIS.Camada camada = new Entidade.Imobiliario.ConfiguracaoGIS.Camada();
            camada.Workspace = workspace;
            camada.DataStores = datastores;

            sg.ImportarShape(path, camada);

            return camada;
        }

        private void AddShapeViewer(string url, ClienteConfiguracao clienteConfiguracao, Entidade.Imobiliario.ConfiguracaoGIS.Camada camada)
        {
            ServidorGeografico sg = new ServidorGeografico(url);

            if (camada.Nome != "")
            {
                CamadaGeoServer camadaGs = new CamadaGeoServer();
                camadaGs.Nome = camada.Nome;
                camadaGs.EPSG = int.Parse(sg.GetEpsg(camada));
                camadaGs.Bbox = sg.GetBbox(camada);
                camadaGs.Visivel = true;
                camadaGs.Host = clienteConfiguracao.GeoServerHost;
                camadaGs.Porta = clienteConfiguracao.GeoServerPorta;
                camadaGs.EspacoTrabalho = camada.Workspace;
                camadaGs.FonteDeDados = camada.DataStores;
                camadaGs.Filtro = "";
                camadaGs.Service = "WFS";
                camadaGs.ZoomMinimo = 0;
                camadaGs.ZoomMaximo = 10;

                List<CamadaGeoServer> camadas = new List<CamadaGeoServer>();
                camadas.Add(camadaGs);

                vwMapa.AddShape(camadas);
            }
        }

        private void ListarArquivos()
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            FerramentaCTR ferramentaCTR = new FerramentaCTR();
            List<LayerConfiguracao> layers = ferramentaCTR.centroideGetLayers(funcionarioCliente.CodCliente);
            LayerConfiguracao selecione = new LayerConfiguracao();
            selecione.Descricao = "Selecione...";
            layers.Insert(0, selecione);

            ddlCamadasImportar.DataSource = layers;
            ddlCamadasImportar.DataValueField = "Nome";
            ddlCamadasImportar.DataTextField = "Descricao";
            ddlCamadasImportar.DataBind();
        }

        private Entidade.Imobiliario.ConfiguracaoGIS.Camada GetCamada()
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);

            ClienteConfiguracaoCtr clienteConfigCtr = new ClienteConfiguracaoCtr();
            ClienteConfiguracao clienteConfiguracao = clienteConfigCtr.BuscarPorCliente(funcionarioCliente.CodCliente);

            Entidade.Imobiliario.ConfiguracaoGIS.Camada camada = new Entidade.Imobiliario.ConfiguracaoGIS.Camada();
            camada.Workspace = clienteConfiguracao.Espacotrabalho;
            camada.DataStores = funcionarioCliente.CodFuncionario.ToString();

            return camada;
        }

        private string GetUrl()
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);

            ClienteConfiguracaoCtr clienteConfigCtr = new ClienteConfiguracaoCtr();
            ClienteConfiguracao clienteConfiguracao = clienteConfigCtr.BuscarPorCliente(funcionarioCliente.CodCliente);

            string url = clienteConfiguracao.GeoServerHost + ":" + clienteConfiguracao.GeoServerPorta;

            return url;
        }

        private ClienteConfiguracao GetClienteConfiguracao(Int64 codCliente)
        {
            ClienteConfiguracaoCtr clienteConfigCtr = new ClienteConfiguracaoCtr();
            ClienteConfiguracao clienteConfiguracao = clienteConfigCtr.BuscarPorCliente(codCliente);

            return clienteConfiguracao;
        }

        protected void Visualizar_Click(object sender, EventArgs e)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);

            string url = GetUrl();

            Entidade.Imobiliario.ConfiguracaoGIS.Camada camada = GetCamada();
            LinkButton lb = (LinkButton)sender;
            camada.Nome = lb.CommandArgument;

            ClienteConfiguracao clienteConfiguracao = GetClienteConfiguracao(funcionarioCliente.CodCliente);

            AddShapeViewer(url, clienteConfiguracao, camada);
        }

        protected void lbCarregar_Click(object sender, EventArgs e)
        {
            ListarArquivos();
            upArquivos.Update();
        }

        protected void Importar_Click(object sender, EventArgs e)
        {
            try
            {
                ScriptManager.RegisterClientScriptBlock(this, GetType(), "carregando", "Carregando('Importando')", true);

                FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                ClienteConfiguracao clienteConfiguracao = GetClienteConfiguracao(funcionarioCliente.CodCliente);

                LinkButton lb = (LinkButton)sender;
                string[] valor = lb.CommandArgument.Split(' ');
                string tabela = valor[0];
                string url = GetUrl();

                Entidade.Imobiliario.ConfiguracaoGIS.Camada camada = new Entidade.Imobiliario.ConfiguracaoGIS.Camada();
                camada.Workspace = clienteConfiguracao.Espacotrabalho;
                camada.DataStores = funcionarioCliente.CodFuncionario.ToString();
                camada.Nome = valor[1];

                ServidorGeografico sg = new ServidorGeografico(url);
                string sql = sg.GetSqlImportarBanco(camada, (int)funcionarioCliente.CodFuncionario, (int)funcionarioCliente.CodCliente, "imobiliario", tabela);
                //sql = sql.Replace("\"", "\\\"");

                ImportarBancoCtr importarCtr = new ImportarBancoCtr();
                importarCtr.Importar(sql);

                ScriptManager.RegisterClientScriptBlock(this, GetType(), "SucessoAoCarregar", "SucessoCarregando('Importado com sucesso')", true);

                //ScriptManager.RegisterClientScriptBlock(this, GetType(), "msg_alert", "alert(\"" + sql + "\")", true);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);

                ScriptManager.RegisterClientScriptBlock(this, GetType(), "ErroAoCarregar", "ErroCarregando('Erro ao importar')", true);
            }

        }
        protected void ListaRelatorios()
        {
            TipoAcuraciaCtr tipoAcuraciaCtr = new TipoAcuraciaCtr();

            ddlEscolherRelatorio.DataTextField = "Descricao";
            ddlEscolherRelatorio.DataValueField = "Codigo";
            ddlEscolherRelatorio.DataSource = tipoAcuraciaCtr.Buscar();
            ddlEscolherRelatorio.DataBind();

            //ddlListarFiltros.DataTextField = "Descricao";
            //ddlListarFiltros.DataValueField = "Codigo";
            //ddlListarFiltros.DataSource = tipoAcuraciaCtr.Buscar();
            //ddlListarFiltros.DataBind();

            ListarEmDuvida();
        }

        protected void ListarEmDuvida()
        {
            //FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            //VerificarAcuraciaCtr verificarAcuraciaCtr = new VerificarAcuraciaCtr();
            //rpEmDuvida.DataSource = verificarAcuraciaCtr.BuscarEmDuvida(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);
            //rpEmDuvida.DataBind();
        }

        protected void btnBuscarRelatorio_Click(object sender, EventArgs e)
        {
            try
            {
                if (rblTipoRelatorio.SelectedValue == "Pendente")
                {
                    FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                    VerificarAcuraciaCtr verificarAcuraciaCtr = new VerificarAcuraciaCtr();

                    long statusCorrecao = verificarAcuraciaCtr.VerificarStatusCorrecao(funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue), funcionarioCliente.CodCliente);
                    if (statusCorrecao == 0)
                    {
                        verificarAcuraciaCtr.VincularFuncionarioCorrecao(funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue), funcionarioCliente.CodCliente);
                        rpListarRelatorios.DataSource = verificarAcuraciaCtr.BuscarPorTipo(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue));
                        rpListarRelatorios.DataBind();
                    }
                    else if (statusCorrecao == 1)
                    {
                        rpListarRelatorios.DataSource = verificarAcuraciaCtr.BuscarPorTipo(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue));
                        rpListarRelatorios.DataBind();
                    }

                    pnlResultadosQualidade.Visible = true;
                }
                else if (rblTipoRelatorio.SelectedValue == "Duvida")
                {
                    FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                    VerificarAcuraciaCtr verificarAcuraciaCtr = new VerificarAcuraciaCtr();

                    rpListarRelatorios.DataSource = verificarAcuraciaCtr.BuscarEmDuvidaPorTipo(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue));
                    rpListarRelatorios.DataBind();

                    pnlResultadosQualidade.Visible = true;
                }


                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "FitQualidade", "FitQualidade();", true);
            }
            catch (Exception er)
            {
                if (er.Message.Contains("Cetec.Notificacao.Exception.CetecErroException"))
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroCetec", "swal('Erro', '" + ((CetecErroException)er).Configuracao.Mensagem + "' , 'error');", true);
                //throw new Exception(((CetecErroException)er).Configuracao.Mensagem);
                else
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroGenerico", "swal('Erro', '" + er.Message + "', 'error');", true);
            }
        }

        protected void btnFiltrarRelatorios_Click(object sender, EventArgs e)
        {
            //FuncionarioCliente funcionarioCliente = (FuncionarioCliente)HttpContext.Current.Session["funcionario_cliente"];
            //VerificarAcuraciaCtr verificarAcuraciaCtr = new VerificarAcuraciaCtr();
            //rpEmDuvida.DataSource = verificarAcuraciaCtr.BuscarEmDuvidaPorTipo(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, long.Parse(ddlListarFiltros.SelectedValue));
            //rpEmDuvida.DataBind();
        }

        protected void rpEmDuvida_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            VerificarAcuraciaCtr verificarAcuraciaCtr = new VerificarAcuraciaCtr();

            long codigo = long.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "Finalizado")
            {
                //verificarAcuraciaCtr.AtualizarStatus(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, 3, codigo);
                //rpEmDuvida.DataSource = verificarAcuraciaCtr.BuscarEmDuvida(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario);
                //rpEmDuvida.DataBind();
            }
            else if (e.CommandName == "Observacao")
            {
                modalObservacao.atribuiViewState(codigo);
                this.abrirModalObservacao("abrirModalObservacao");
            }
        }

        protected void rpListarRelatorios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            VerificarAcuraciaCtr verificarAcuraciaCtr = new VerificarAcuraciaCtr();

            long codigo = long.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "Finalizado")
            {
                verificarAcuraciaCtr.AtualizarStatus(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, 3, codigo);
                rpListarRelatorios.DataSource = verificarAcuraciaCtr.BuscarPorTipo(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue));
                rpListarRelatorios.DataBind();
            }
            else if (e.CommandName == "Duvida")
            {
                modalObservacao.atribuiViewState(codigo);
                this.abrirModalObservacao("abrirModalObservacao");

                verificarAcuraciaCtr.AtualizarStatus(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, 2, codigo);
                rpListarRelatorios.DataSource = verificarAcuraciaCtr.BuscarPorTipo(funcionarioCliente.CodCliente, funcionarioCliente.CodFuncionario, long.Parse(ddlEscolherRelatorio.SelectedValue));
                rpListarRelatorios.DataBind();
            }
            else if (e.CommandName == "Arquivos")
            {

            }
            else if (e.CommandName == "Observacao")
            {
                modalObservacao.atribuiViewState(codigo);
                this.abrirModalObservacao("abrirModalObservacao");
            }

            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "FitQualidade", "FitQualidade()", true);
        }

        public void VerificarPrivilegios()
        {
            try
            {
                FuncionarioControle funcionarioControle = new FuncionarioControle();
                string usuario = (string)GetSession(ESession.LOGIN);

                List<Privilegio> privilegios = funcionarioControle.GetPrivilegiosFornecidos(usuario);

                lkBuscar.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.TABPANEL_BUSCAR);
                lkExportar.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.TABPANEL_EXPORTAR);
                lkImportar.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.TABPANEL_IMPORTAR);
                lkOcorrencia.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.TABPANEL_OCORRENCIA);
                lkQualidade.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.TABPANEL_QUALIDADE);

                lbPersonalizarFeicoes.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.PAGINA_CONFIGURACOES_PERSONALIZARFEICOES);

                btnExcluirMapa.Visible = privilegios.Exists(x => x.Codigo == (long)EPrivilegio.BOTAO_EXCLUIR_MAPA);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }

        protected void lbPersonalizarLegenda_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Visao/Comum/FRM/FRM000501.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        public string GerarGeoJSON(bool Camada, long CodigoGeografico)
        {
            return string.Empty;
        }

        private List<DadosExportacao> TratarDadosExportacao(List<string> DadosExportacao)
        {
            List<DadosExportacao> retorno = new List<DadosExportacao>();

            string stringCabecalho = DadosExportacao.Last();
            List<string> Cabecalho = stringCabecalho.Split(new[] { "@@@@@" }, StringSplitOptions.None).ToList();

            DadosExportacao.RemoveAt(DadosExportacao.Count - 1);

            foreach (string dadoExportacao in DadosExportacao)
            {
                List<AtributoLayer> atributosExportacao = new List<AtributoLayer>();
                DadosExportacao dadosExportacao = new DadosExportacao();

                List<string> Valores = dadoExportacao.Split(new[] { "@@@@@" }, StringSplitOptions.None).ToList();

                if (Cabecalho.Count == Valores.Count)
                {
                    for (int i = 0; i < Cabecalho.Count; i++)
                    {
                        AtributoLayer atributo = new AtributoLayer();
                        atributo.Atributo = Cabecalho[i];
                        atributo.Descricao = Valores[i];
                        atributosExportacao.Add(atributo);
                    }

                    dadosExportacao.Atributos = atributosExportacao;
                    retorno.Add(dadosExportacao);
                }
            }

            return retorno;
        }

        static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }

        protected void ddlFeicaoBusca_SelectedIndexChanged(object sender, EventArgs e)
        {
            ControleJavaScript.resetarBotao2(this, "ResetarBotaoBuscar");
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            List<ListItem> ListaAtributos = new List<ListItem>();
            ListaAtributos.Add(new ListItem { Text = "Selecione...", Value = "selecione" });

            cbDestacarResultados.Checked = false;

            //List<layer> layers = new List<layer>();
            //foreach (Layer layer in layers)
            //{
            //foreach (Column column in layers.GetColumns)
            //{
            //    column.
            //      ListaAtributos.Add(new ListItem { Text = "Inscrição", Value = "inscricao" });
            //}
            //}aasearcadassdasdh_columns

            //SELECT * FROM aasearch_columns('360164', '{codcliente}','{}','{imobiliario}')




            switch (ddlFeicaoBusca.SelectedValue.ToString())
            {
                case "LOTE":
                    ListaAtributos.Add(new ListItem { Text = "Código do Cadastro", Value = "codigocadastro" });
                    ListaAtributos.Add(new ListItem { Text = "Inscrição", Value = "inscricao" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Proprietário", Value = "nomeproprietario" });
                    ListaAtributos.Add(new ListItem { Text = "Código do Logradouro", Value = "codigologradouro" });
                    ListaAtributos.Add(new ListItem { Text = "Prefixo do Logradouro", Value = "prefixologradouro" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Logradouro", Value = "nomelogradouro" });
                    ListaAtributos.Add(new ListItem { Text = "Número do Imóvel", Value = "numeroimovel" });
                    ListaAtributos.Add(new ListItem { Text = "CEP", Value = "cep" });
                    ListaAtributos.Add(new ListItem { Text = "Código do Lote", Value = "codigolote" });
                    ListaAtributos.Add(new ListItem { Text = "Código da Quadra", Value = "codigoquadra" });
                    ListaAtributos.Add(new ListItem { Text = "Código do Bairro", Value = "codigobairro" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Bairro", Value = "nomebairro" });
                    ListaAtributos.Add(new ListItem { Text = "Código do Setor", Value = "codigosetor" });
                    ListaAtributos.Add(new ListItem { Text = "Área Construída", Value = "areaconstruida" });
                    ListaAtributos.Add(new ListItem { Text = "Área do Lote", Value = "arealote" });
                    ListaAtributos.Add(new ListItem { Text = "Código CTGEO", Value = "codigoctgeo" });

                    if (funcionarioCliente.CodCliente == 360067)
                    {
                        ListaAtributos.Add(new ListItem { Text = "Tipo da Edificação", Value = "tipoedificacao" });
                    }

                    if (funcionarioCliente.CodCliente == 360070)
                    {
                        ListaAtributos.Add(new ListItem { Text = "Matrícula", Value = "matricula" });
                    }

                    cbDestacarResultados.Visible = true;

                    break;
                case "LOGRADOURO":
                    ListaAtributos.Add(new ListItem { Text = "Código do Logradouro", Value = "codigologradouro" });
                    ListaAtributos.Add(new ListItem { Text = "Prefixo do Logradouro", Value = "prefixologradouro" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Logradouro", Value = "nomelogradouro" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Bairro", Value = "nomebairro" });
                    ListaAtributos.Add(new ListItem { Text = "Código CTGEO", Value = "codigoctgeo" });

                    cbDestacarResultados.Visible = false;

                    break;
                case "QUADRA":
                    ListaAtributos.Add(new ListItem { Text = "Código da Quadra", Value = "codigoquadra" });
                    ListaAtributos.Add(new ListItem { Text = "Código do Bairro", Value = "codigobairro" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Bairro", Value = "nomebairro" });
                    ListaAtributos.Add(new ListItem { Text = "Código CTGEO", Value = "codigoctgeo" });

                    cbDestacarResultados.Visible = false;

                    break;
                case "BAIRRO":
                    ListaAtributos.Add(new ListItem { Text = "Código do Bairro", Value = "codbairrotributario" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Bairro", Value = "nomebairro" });
                    ListaAtributos.Add(new ListItem { Text = "Código CTGEO", Value = "codigoctgeo" });
                    cbDestacarResultados.Visible = false;
                    break;
                case "PROPRIEDADERURAL":
                    ListaAtributos.Add(new ListItem { Text = "Código do PROPRIEDADERURAL", Value = "codbairrotributario" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Bairro", Value = "nomebairro" });
                    ListaAtributos.Add(new ListItem { Text = "Código CTGEO", Value = "codigoctgeo" });
                    cbDestacarResultados.Visible = false;
                    break;
                case "ESTRADA":
                    ListaAtributos.Add(new ListItem { Text = "Código do ESTRADA", Value = "codbairrotributario" });
                    ListaAtributos.Add(new ListItem { Text = "Nome do Bairro", Value = "nomebairro" });
                    ListaAtributos.Add(new ListItem { Text = "Código CTGEO", Value = "codigoctgeo" });
                    cbDestacarResultados.Visible = false;
                    break;
                default:

                    cbDestacarResultados.Visible = false;

                    break;
            }

            ListaAtributos = ListaAtributos.OrderBy(x => x.Value != "selecione").ThenBy(x => x.Text).ToList();

            ddlParametroBusca.DataSource = ListaAtributos;
            ddlParametroBusca.DataTextField = "Text";
            ddlParametroBusca.DataValueField = "Value";
            ddlParametroBusca.DataBind();

            rpBusca.DataSource = null;
            rpBusca.DataBind();

            DivExportarBusca.Visible = false;

            //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "resetarCaixaBusca", "FitLegenda()", true);
            //upPesquisa.Update();
        }


        protected void btnBuscarFeicoesImportadas_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlCamadasImportar.SelectedIndex != 0 && ddlPeriodoImportar.SelectedIndex != 0)
                {
                    FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                    FerramentaCTR ferramentaCTR = new FerramentaCTR();

                    List<Feicao> feicoesImportadas = ferramentaCTR.BuscarFeicoesImportadas(funcionarioCliente.CodFuncionario, funcionarioCliente.CodCliente, ddlCamadasImportar.SelectedValue.ToString(), int.Parse(ddlPeriodoImportar.SelectedValue.ToString()));

                    if (feicoesImportadas.Count > 0)
                    {
                        foreach (Feicao feicao in feicoesImportadas)
                        {
                            feicao.Camada = ddlCamadasImportar.SelectedValue.ToString();
                        }

                        rpArquivos.DataSource = feicoesImportadas;
                        rpArquivos.DataBind();

                        DivExportarBusca.Visible = false;
                        rpBusca.DataSource = null;
                        rpBusca.DataBind();
                        upPesquisa.Update();

                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "FitFeicoesImportadas", "FitLegenda();", true);
                        pnlFeicoesImportadas.Visible = true;
                    }
                    else
                    {
                        rpArquivos.DataSource = null;
                        rpArquivos.DataBind();
                    }
                }
                else if (ddlCamadasImportar.SelectedIndex == 0)
                {
                    //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "Importar_CamadaVazia", "alert('Selecione uma camada antes de filtrar!');", true);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "Importar_CamadaVazia", "swal('Aviso','Selecione uma camada antes de filtrar','warning');", true);
                }
                else
                {
                    //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "Importar_PeriodoVazio", "alert('Selecione um período antes de filtrar!');", true);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "Importar_PeriodoVazio", "swal('Aviso','Selecione um período antes de filtrar','warning');", true);
                }
            }
            catch (Exception er)
            {
                if (er.Message.Contains("Cetec.Notificacao.Exception.CetecErroException"))
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroCetec", "swal('Erro', '" + ((CetecErroException)er).Configuracao.Mensagem + "' , 'error');", true);
                //throw new Exception(((CetecErroException)er).Configuracao.Mensagem);
                else
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroGenerico", "swal('Erro', '" + er.Message + "', 'error');", true);
            }

        }

        #region EXPORTAR
        public void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                Session.Add("CodCliente", funcionarioCliente.CodCliente);
                Session.Add("CodFuncionario", funcionarioCliente.CodFuncionario);
                loadingProgressBar.Visible = true;

                Thread th = new Thread(InitExport);

                th.Start();
                Thread.Sleep(500);
                th.Join();
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }

        protected void InitExport()
        {
            try
            {
                loadingProgressBar.Visible = true;
                long CodCliente = long.Parse(Session["CodCliente"].ToString());
                long CodFuncionario = long.Parse(Session["CodFuncionario"].ToString());

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                DadosExportacaoMapa dadosExportacaoMapa = serializer.Deserialize<DadosExportacaoMapa>(hfDadosExportacao.Value);

                hfLoadingGeral.Value = "5";

                LayerConfiguracao layerConfig = layerConfigCtr.BuscarLayerConfiguracaoPerNome(CodCliente, dadosExportacaoMapa.NomeCamada);
                dadosExportacaoMapa.NomeCamada = layerConfig.LayerName;

                string nomeArquivo = CodCliente.ToString() + "_" + CodFuncionario + "_" + layerConfig.LayerName.ToString().Replace("vw_", "");

                Session.Add("NomeArquivoSys", nomeArquivo);
                Session.Add("NomeArquivo", layerConfig.LayerName.ToString().Replace("vw_", ""));

                hfLoadingGeral.Value = "15";

                if (dadosExportacaoMapa.IsGeoFormat == false)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ExibirExtensao", "exportarMapa();", true);
                }
                else if (!String.IsNullOrEmpty(dadosExportacaoMapa.FormatoExportacao) && ValidarNomeCamada(layerConfig.Nome))
                {
                    switch (dadosExportacaoMapa.FormatoExportacao)
                    {
                        case "Shapefile":
                            string caminhoComplementarSHP = "Arquivos\\Exportacao\\SHP\\";

                            ExportarSHP(dadosExportacaoMapa, caminhoComplementarSHP, nomeArquivo);

                            hfLoadingGeral.Value = "40";

                            CompactarArquivosSHP(caminhoComplementarSHP, nomeArquivo);

                            hfLoadingGeral.Value = "60";

                            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), Guid.NewGuid().ToString(), "document.getElementById('btnBaixarArquivo').click();", true);

                            break;

                        case "GeoJson":
                            caminhoComplementarSHP = "Arquivos\\Exportacao\\SHP\\";
                            string caminhoComplementarGeoJSON = "Arquivos\\Exportacao\\GeoJSON\\";

                            ExportarSHP(dadosExportacaoMapa, caminhoComplementarSHP, nomeArquivo);

                            hfLoadingGeral.Value = "40";

                            if (ValidarArquivo(caminhoComplementarSHP, nomeArquivo, ".shp"))
                            {
                                ExportarGeoJSON(dadosExportacaoMapa, caminhoComplementarSHP, caminhoComplementarGeoJSON, nomeArquivo);

                                hfLoadingGeral.Value = "60";

                                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), Guid.NewGuid().ToString(), "document.getElementById('btnBaixarArquivo').click();", true);
                            }

                            break;

                        case string a when a.Contains("KML"):
                            caminhoComplementarSHP = "Arquivos\\Exportacao\\SHP\\";
                            string caminhoComplementarKML = "Arquivos\\Exportacao\\KML\\";

                            hfLoadingGeral.Value = "40";

                            if (layerConfig.LayerName == "vw_aracatuba_lotesemfoto" || dadosExportacaoMapa.FormatoExportacao == "KML(MapsME)")
                            {
                                string kml_maps = (new LoteCtr()).GerarKmlToMapsMe(CodCliente, layerConfig.LayerName);

                                hfLoadingGeral.Value = "60";

                                System.IO.File.WriteAllText(systemPath + caminhoComplementarKML + nomeArquivo + ".kml", kml_maps);
                            }
                            else
                            {

                                hfLoadingGeral.Value = "40";

                                ExportarSHP(dadosExportacaoMapa, caminhoComplementarSHP, nomeArquivo);

                                if (ValidarArquivo(caminhoComplementarSHP, nomeArquivo, ".shp"))
                                {
                                    ExportarKML(dadosExportacaoMapa, caminhoComplementarSHP, caminhoComplementarKML, nomeArquivo);

                                    hfLoadingGeral.Value = "60";

                                }
                            }

                            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), Guid.NewGuid().ToString(), "document.getElementById('btnBaixarArquivo').click();", true);

                            break;

                        case "DXF":
                            caminhoComplementarSHP = "Arquivos\\Exportacao\\SHP\\";
                            string caminhoComplementarDXF = "Arquivos\\Exportacao\\DXF\\";

                            hfLoadingGeral.Value = "40";

                            ExportarSHP(dadosExportacaoMapa, caminhoComplementarSHP, nomeArquivo);

                            if (ValidarArquivo(caminhoComplementarSHP, nomeArquivo, ".shp"))
                            {
                                ExportarDXF(dadosExportacaoMapa, caminhoComplementarSHP, caminhoComplementarDXF, nomeArquivo);

                                hfLoadingGeral.Value = "60";

                                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), Guid.NewGuid().ToString(), "document.getElementById('btnBaixarArquivo').click();", true);
                            }

                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "Erro", "swal('Aviso', 'Selecione um formato de exportação', 'warning');", true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void ExportarSHP(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementar, string nomeArquivo)
        {
            try
            {
                CriarPasta(caminhoComplementar);

                string comandoBAT = MontarSHPBAT(dadosExportacaoMapa, caminhoComplementar, nomeArquivo);
                CriarArquivoBAT(comandoBAT, caminhoComplementar, nomeArquivo);

                ExecutarBAT(caminhoComplementar, nomeArquivo);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        protected void ExportarGeoJSON(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementarSHP, string caminhoComplementarGeoJSON, string nomeArquivo)
        {
            try
            {
                CriarPasta(caminhoComplementarGeoJSON);

                string comandoBAT = MontarGeoJSONBAT(dadosExportacaoMapa, caminhoComplementarSHP, caminhoComplementarGeoJSON, nomeArquivo);
                CriarArquivoBAT(comandoBAT, caminhoComplementarGeoJSON, nomeArquivo);

                ExecutarBAT(caminhoComplementarGeoJSON, nomeArquivo);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        protected void ExportarKML(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementarSHP, string caminhoComplementarKML, string nomeArquivo)
        {
            try
            {
                CriarPasta(caminhoComplementarKML);

                string comandoBAT = MontarKMLBAT(dadosExportacaoMapa, caminhoComplementarSHP, caminhoComplementarKML, nomeArquivo);
                CriarArquivoBAT(comandoBAT, caminhoComplementarKML, nomeArquivo);

                ExecutarBAT(caminhoComplementarKML, nomeArquivo);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        protected void ExportarDXF(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementarSHP, string caminhoComplementarDXF, string nomeArquivo)
        {
            try
            {
                CriarPasta(caminhoComplementarDXF);

                string comandoBAT = MontarDXFBAT(dadosExportacaoMapa, caminhoComplementarSHP, caminhoComplementarDXF, nomeArquivo);
                CriarArquivoBAT(comandoBAT, caminhoComplementarDXF, nomeArquivo);

                ExecutarBAT(caminhoComplementarDXF, nomeArquivo);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        private void CriarPasta(string caminhoComplementar)
        {
            try
            {

                if (!Directory.Exists(systemPath + caminhoComplementar))
                    Directory.CreateDirectory(systemPath + caminhoComplementar);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        private void CriarArquivoBAT(string comando, string caminhoComplementar, string nomeArquivo)
        {
            try
            {
                if (File.Exists(systemPath + caminhoComplementar + nomeArquivo + ".bat"))
                {
                    File.Delete(systemPath + caminhoComplementar + nomeArquivo + ".bat");
                }

                StreamWriter writer = new StreamWriter(systemPath + caminhoComplementar + nomeArquivo + ".bat");
                writer.WriteLine(comando);
                writer.Close();
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        private string MontarSHPBAT(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementar, string nomeArquivo)
        {
            try
            {
                long CodCliente = long.Parse(Session["CodCliente"].ToString());

                ClienteConfiguracao clienteConfiguracao = clienteConfiguracaoCtr.BuscarPorCliente(CodCliente);

                string conexao = "pgsql2shp";
                conexao += " -f " + "\"" + systemPath + caminhoComplementar + nomeArquivo + "\"";
                conexao += " -u " + clienteConfiguracao.DataBaseUsuario;
                conexao += " -h " + clienteConfiguracao.DatabaseIp;
                conexao += " -p " + clienteConfiguracao.DatabasePorta;
                conexao += " -P " + clienteConfiguracao.DataBaseSenha;
                conexao += " -d " + clienteConfiguracao.DatabaseNome;

                string sql = "SELECT * FROM imobiliario." + dadosExportacaoMapa.NomeCamada.ToLower() + " WHERE codcliente = " + CodCliente.ToString() + " AND excluido IS FALSE";

                switch (dadosExportacaoMapa.TipoExportacao)
                {
                    case "0":
                        if (ValidarCodigosSelecionados(dadosExportacaoMapa.CodigoFeicoesSelecionadas))
                        {
                            sql += " AND codigo IN (" + string.Join(",", dadosExportacaoMapa.CodigoFeicoesSelecionadas) + ")";
                        }
                        break;
                    case "1":
                        if (dadosExportacaoMapa.Bbox.Length == 4)
                        {
                            decimal xMin = Math.Round(dadosExportacaoMapa.Bbox[0]);
                            decimal yMin = Math.Round(dadosExportacaoMapa.Bbox[1]);
                            decimal xMax = Math.Round(dadosExportacaoMapa.Bbox[2]);
                            decimal yMax = Math.Round(dadosExportacaoMapa.Bbox[3]);

                            string pontoxy = xMin + " " + yMin;
                            string pontoxY = xMin + " " + yMax;
                            string pontoXy = xMax + " " + yMin;
                            string pontoXY = xMax + " " + yMax;

                            sql += " AND ST_Intersects(geo, ST_GeomFromText('POLYGON((" + pontoxy + "," + pontoxY + "," + pontoXY + "," + pontoXy + "," + pontoxy + "))', 31982)) IS TRUE";
                        }
                        break;
                    case "Camada":
                        break;
                    default:
                        break;
                }

                return conexao + " \"" + sql + "\"";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private string MontarGeoJSONBAT(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementarSHP, string caminhoComplementarGeoJSON, string nomeArquivo)
        {
            try
            {

                string comando = "ogr2ogr";
                string driver = "-f GeoJSON";
                string geoJSONPath = "\"" + systemPath + caminhoComplementarGeoJSON + nomeArquivo + ".geojson" + "\"";
                string shpPath = "\"" + systemPath + caminhoComplementarSHP + nomeArquivo + ".shp" + "\"";

                return comando + " " + driver + " " + geoJSONPath + " " + shpPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private string MontarKMLBAT(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementarSHP, string caminhoComplementarKML, string nomeArquivo)
        {
            try
            {

                string comando = "ogr2ogr";
                string driver = "-f KML";
                string kmlPath = "\"" + systemPath + caminhoComplementarKML + nomeArquivo + ".kml" + "\"";
                string shpPath = "\"" + systemPath + caminhoComplementarSHP + nomeArquivo + ".shp" + "\"";

                return comando + " " + driver + " " + kmlPath + " " + shpPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private string MontarDXFBAT(DadosExportacaoMapa dadosExportacaoMapa, string caminhoComplementarSHP, string caminhoComplementarDXF, string nomeArquivo)
        {
            try
            {
                string comando = "ogr2ogr";
                string driver = "-f DXF";
                string shpPath = "\"" + systemPath + caminhoComplementarSHP + nomeArquivo + ".shp" + "\"";
                string dxfPath = "\"" + systemPath + caminhoComplementarDXF + nomeArquivo + ".dxf" + "\"";

                return comando + " " + driver + " " + dxfPath + " " + shpPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void ExecutarBAT(string caminhoComplementar, string nomeArquivo)
        {
            try
            {

                if (File.Exists(systemPath + caminhoComplementar + nomeArquivo + ".bat"))
                {
                    Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(systemPath + caminhoComplementar + nomeArquivo);
                    proc.StartInfo.FileName = systemPath + caminhoComplementar + nomeArquivo + ".bat";

                    proc.Start();
                    proc.WaitForExit();
                    proc.Close();
                }
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        private void CompactarArquivosSHP(string caminhoComplementar, string nomeArquivo)
        {
            try
            {
                List<string> formatosArquivosComprimidos = new List<string>() { ".cpg", ".dbf", ".prj", ".shp", ".shx" };

                if (File.Exists(systemPath + caminhoComplementar + nomeArquivo + ".zip"))
                {
                    File.Delete(systemPath + caminhoComplementar + nomeArquivo + ".zip");
                }

                using (ZipArchive zip = ZipFile.Open(systemPath + caminhoComplementar + nomeArquivo + ".zip", ZipArchiveMode.Create))
                {
                    foreach (string formato in formatosArquivosComprimidos)
                    {
                        if (File.Exists(systemPath + caminhoComplementar + nomeArquivo + formato))
                        {
                            //zip.CreateEntryFromFile(systemPath + caminhoComplementar + nomeArquivo + ".shp", nomeArquivo + ".shp");
                            zip.CreateEntryFromFile(systemPath + caminhoComplementar + nomeArquivo + formato, nomeArquivo + formato);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        private void DeletarArquivo(string caminhoComplementar, string nomeArquivo, string formato)
        {
            try
            {

                if (File.Exists(systemPath + caminhoComplementar + nomeArquivo + formato))
                    File.Delete(systemPath + caminhoComplementar + nomeArquivo + formato);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }


        private bool ValidarNomeCamada(string nomeCamada)
        {
            try
            {
                return layerCTR.ValidarNomeCamada(nomeCamada);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool ValidarCodigosSelecionados(string[] codigosSelecionados)
        {
            long codigoSelecionado;

            for (int index = 0; index < codigosSelecionados.Length; index++)
            {
                if (!long.TryParse(codigosSelecionados[index], out codigoSelecionado))
                {
                    return false;
                }
            }

            return true;
        }


        private bool ValidarArquivo(string caminhoComplementar, string nomeArquivo, string formato)
        {
            try
            {
                return File.Exists(systemPath + caminhoComplementar + nomeArquivo + formato);
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);

                return false;
            }
        }


        protected void btnBaixarArquivo_Click(object sender, EventArgs e)
        {
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                DadosExportacaoMapa dadosExportacaoMapa = serializer.Deserialize<DadosExportacaoMapa>(hfDadosExportacao.Value);


                string nomeArquivo = Session["NomeArquivoSys"].ToString();

                string caminhoComplementar = string.Empty;
                string formato = string.Empty;

                if (dadosExportacaoMapa != null && !String.IsNullOrEmpty(dadosExportacaoMapa.FormatoExportacao))
                {
                    switch (dadosExportacaoMapa.FormatoExportacao)
                    {
                        case string a when a.Contains("Shapefile"):
                            caminhoComplementar = "Arquivos\\Exportacao\\SHP\\";
                            formato = ".zip";
                            break;

                        case string a when a.Contains("GeoJson"):
                            caminhoComplementar = "Arquivos\\Exportacao\\GeoJSON\\";
                            formato = ".geojson";
                            break;

                        case string a when a.Contains("KML"):
                            caminhoComplementar = "Arquivos\\Exportacao\\KML\\";
                            formato = ".kml";
                            break;

                        case string a when a.Contains("DXF"):
                            caminhoComplementar = "Arquivos\\Exportacao\\DXF\\";
                            formato = ".dxf";
                            break;

                        default:
                            break;
                    }
                }

                string pathArquivo = systemPath + caminhoComplementar + nomeArquivo + formato;

                hfLoadingGeral.Value = "90";

                if (
                     Directory.Exists(systemPath + caminhoComplementar) && File.Exists(pathArquivo)
                    )
                {

                    if ((new FileInfo(pathArquivo)).Length > 10000000 && formato != ".zip")
                    {
                        var zipFile = systemPath + caminhoComplementar + Session["NomeArquivo"].ToString() + ".zip";
                        var file = pathArquivo;

                        if (File.Exists(zipFile))
                        {
                            File.Delete(zipFile);
                        }

                        using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
                        {
                            if (formato == ".shp")
                            {
                                archive.CreateEntryFromFile(file, nomeArquivo + ".cpg");
                                archive.CreateEntryFromFile(file, nomeArquivo + ".shp");
                                archive.CreateEntryFromFile(file, nomeArquivo + ".prj");
                                archive.CreateEntryFromFile(file, nomeArquivo + ".shx");
                                archive.CreateEntryFromFile(file, nomeArquivo + ".dbf");

                            }
                            else
                            {
                                archive.CreateEntryFromFile(file, nomeArquivo + formato);
                            }
                        }

                        formato = ".zip";

                        pathArquivo = systemPath + caminhoComplementar + Session["NomeArquivo"].ToString() + formato;
                    }


                    FileInfo arquivo = new FileInfo(pathArquivo);
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.ClearContent();
                    Response.AddHeader("Content-Disposition", "attachment; filename=\"" + Session["NomeArquivo"] + formato + "\"");
                    Response.AddHeader("Content-Length", arquivo.Length.ToString());
                    Response.Charset = "utf8";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.ContentType = "application/octet-stream";
                    Response.Flush();
                    Response.WriteFile(arquivo.FullName);
                    Response.Flush();
                    Response.Close();

                    hfLoadingGeral.Value = "100";
                    hfLoadingGeral.Value = "0";
                }
            }
            catch (Exception ex)
            {
                new Logger().LogErro(ex);
            }
        }
        #endregion


        protected void ddlTesteExportacao_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlFormatoExportacao.SelectedValue == "GeoJson")
                pnlProjecao.Visible = true;
            else
                pnlProjecao.Visible = false;
        }


        private void ListarOcorrencias()
        {
            OcorrenciaCTR ocorrenciaCTR = new OcorrenciaCTR();
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            List<OcorrenciaCliente> ocorrencias = new List<OcorrenciaCliente>() { new OcorrenciaCliente { Codigo = 0, Descricao = "Selecione...", Tabela = string.Empty } };
            ocorrencias.AddRange(ocorrenciaCTR.OcorrenciaGet(funcionarioCliente.CodCliente));

            ddlTipoOcorrencia.DataSource = ocorrencias;
            ddlTipoOcorrencia.DataTextField = "Descricao";
            ddlTipoOcorrencia.DataValueField = "Codigo";
            ddlTipoOcorrencia.DataBind();
        }


        protected void ddlTipoOcorrencia_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OcorrenciaCTR ocorrenciaCTR = new OcorrenciaCTR();
                List<AtributoLayer> atributosOcorrencia = new List<AtributoLayer>() { new AtributoLayer { Descricao = "Selecione...", Atributo = string.Empty, Tabela = string.Empty, Tipo = string.Empty } };

                if (ddlTipoOcorrencia.SelectedValue != string.Empty)
                {
                    atributosOcorrencia.AddRange(ocorrenciaCTR.AtributoOcorrenciaGet(ddlTipoOcorrencia.SelectedValue));
                }

                rpOcorrencia.DataSource = null;
                rpOcorrencia.DataBind();

                ddlFiltroOcorrencia.DataSource = atributosOcorrencia;
                ddlFiltroOcorrencia.DataTextField = "Descricao";
                ddlFiltroOcorrencia.DataValueField = "Atributo";
                ddlFiltroOcorrencia.DataBind();
            }
            catch (Exception er)
            {
                new Logger().LogErro(er);

                throw new Exception(er.Message);
            }
        }


        protected void btnBuscarOcorrencia_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlTipoOcorrencia.SelectedIndex != 0)
                {
                    OcorrenciaCTR ocorrenciaCTR = new OcorrenciaCTR();
                    List<FeicaoBusca> ocorrencias = ocorrenciaCTR.BuscarOcorrencias(ddlTipoOcorrencia.SelectedValue, ddlFiltroOcorrencia.SelectedValue, txtValorOcorrencia.Text);

                    if (ocorrencias.Count > 0)
                    {
                        rpOcorrencia.DataSource = ocorrencias;
                        rpOcorrencia.DataBind();

                        pnlOcorrencia.Visible = true;

                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "FitOcorrencias", "FitLegenda();", true);
                    }
                    else
                    {
                        pnlOcorrencia.Visible = false;

                        rpOcorrencia.DataSource = null;
                        rpOcorrencia.DataBind();
                    }
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroTipoOcorrencia", "swal('Aviso', 'Selecione um Tipo de Ocorrência para Continuar', 'warning');", true);
                }

                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ResetarBtnBuscarOcorrencia", "$(document.body).css('cursor', 'auto');", true);
            }
            catch (Exception er)
            {
                if (er.Message.Contains("Cetec.Notificacao.Exception.CetecErroException"))
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroCetec", "swal('Erro', '" + ((CetecErroException)er).Configuracao.Mensagem + "' , 'error');", true);
                //throw new Exception(((CetecErroException)er).Configuracao.Mensagem);
                else
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroGenerico", "swal('Erro', '" + er.Message + "', 'error');", true);
            }

        }


        protected void BtnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlFeicaoBusca.SelectedIndex != 0 && ddlParametroBusca.SelectedIndex != 0)
                {
                    FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
                    FerramentaCTR ferramentaCTR = new FerramentaCTR();
                    LoteCtr loteCtr = new LoteCtr(funcionarioCliente.CodCliente);

                    //List<Entidade.Imobiliario.API.PesquisaLote> resultados = loteJoin.BuscarLotesCadastrados(funcionarioCliente.CodCliente, new string[] { ddlParametroBusca.SelectedValue.ToString() }, new string[] { txbBusca.Text.ToString().ToLower() }, 500);
                    List<FeicaoBusca> resultados = ferramentaCTR.Buscar(
                        funcionarioCliente.CodTributacao.ToString(), funcionarioCliente.CodCliente,
                        ddlFeicaoBusca.SelectedItem.ToString(),
                        new List<string> { ddlParametroBusca.SelectedValue.ToString() },
                        new List<string> { txbBusca.Text.ToString().Trim().ToLower() },
                        150);

                    if (resultados.Count > 0)
                    {
                        rpBusca.DataSource = resultados;
                        rpBusca.DataBind();

                        rpArquivos.DataSource = null;
                        rpArquivos.DataBind();
                        upArquivos.Update();

                        DivExportarBusca.Visible = true;
                        pnlFeicoesImportadas.Visible = false;

                        List<string> codigos = new List<string>();

                        foreach (FeicaoBusca pesquisa in resultados)
                        {
                            codigos.Add(pesquisa.CodigoTributario);
                        }

                        string parametro = string.Join(",", codigos.ToArray());

                        ControleSession.PutSession(ESession.CAMADA_BUSCA, ddlFeicaoBusca.SelectedValue.ToString());
                        ControleSession.PutSession(ESession.CODIGOS_TRIBUTARIOS_BUSCA, codigos);

                        if (cbDestacarResultados.Checked == true)
                        {
                            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "destacarBusca", "popup.getCdk().DestacarBusca('" + parametro + "','" + funcionarioCliente.CodCliente.ToString() + "');", true);
                        }
                        else
                        {
                            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "destacarBusca", "popup.getCdk().RemoverDestaqueBusca();", true);
                        }
                    }
                    else
                    {
                        rpBusca.DataSource = null;
                        rpBusca.DataBind();

                        rpArquivos.DataSource = null;
                        rpArquivos.DataBind();

                        DivExportarBusca.Visible = false;


                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ResultadoVazio", "swal('Aviso','Nenhum resultado encontrado','info');", true);

                        ControleJavaScript.resetarBotao2(this, "ResetarBotaoBuscar");
                    }
                }
                else
                {
                    ControleJavaScript.resetarBotao2(this, "ResetarBotaoBuscar");
                    if (ddlFeicaoBusca.SelectedIndex == 0)
                        //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "BuscaVazia", "alert('Selecione uma camada para buscar!')", true);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "BuscaVazia", "swal('Aviso','Selecione uma camada para buscar','warning');", true);
                    else
                        //ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "BuscaVazia", "alert('Selecione um campo para buscar!')", true);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "BuscaVazia", "swal('Aviso','Selecione um campo para buscar','warning');", true);
                }

                ControleJavaScript.resetarBotao2(this, "ResetarBotaoBuscar");
            }
            catch (Exception er)
            {
                ControleJavaScript.resetarBotao2(this, "ResetarBotaoBuscar");
                if (er.Message.Contains("Cetec.Notificacao.Exception.CetecErroException"))
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroCetec", "swal('Erro', '" + ((CetecErroException)er).Configuracao.Mensagem + "' , 'error');", true);
                //throw new Exception(((CetecErroException)er).Configuracao.Mensagem);
                else
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "ErroGenerico", "swal('Erro', '" + er.Message + "', 'error');", true);
            }
        }


        protected void ddlTabelaBanco_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                Tabela tabela = new Tabela();
                tabela.NomeTabela = ddlTabelaBanco.SelectedItem.ToString();
                tabela.ApelidoTabela = ddlTabelaBanco.SelectedValue.ToString();
                tabela = TabelaCTR.PesquisarColunas(tabela);

                List<ListItem> listadropdown = new List<ListItem>();
                foreach (Coluna coluna in tabela.Coluna)
                {
                    listadropdown.Add(new ListItem { Text = coluna.Nome, Value = coluna.Nome });
                }

                listadropdown = TiraItensIndesejados(listadropdown);

                listadropdown = listadropdown.OrderBy(x => x.Value != "selecione").ThenBy(x => x.Text).ToList();

                ddlColunasDaTabela.DataSource = listadropdown;
                ddlColunasDaTabela.DataTextField = "Text";
                ddlColunasDaTabela.DataValueField = "Value";
                ddlColunasDaTabela.DataBind();

                rpBusca.DataSource = null;
                rpBusca.DataBind();

                DivExportarBusca.Visible = false;
            }
            catch (Exception )
            {
                throw ;
            }
        }
        List<ListItem> TiraItensIndesejados(List<ListItem> colunasnototal)
        {
            try
            {
                List<String> colunasexcluidas = new List<String> {
                    "codcliente",
                    "codfuncionariocriacao",
                    "codfuncionarioexclusao",
                    "codfuncionarioultimaalteracao",
                    "datacriacao",
                    "dataexclusao",
                    "dataultimaalteracao",
                    "excluido",
                    "geo",
                    "geom",
                    "ipcriacao",
                    "ipexclusao",
                    "ipultimaalteracao"};

                List < ListItem > colunasaceitas = new List<ListItem>();
                foreach (ListItem item in colunasnototal)
                {
                    if (!colunasexcluidas.Contains(item.Value.ToString()))
                    {
                        //colunasaceitas.Remove(item);
                        colunasaceitas.Add(item);
                    }
                }
                return colunasaceitas;
            }
            catch (Exception er)
            {

                throw er;
            }
        }

        List<String> TiraColunasIndesejadas(List<String> colunasnototal)
        {
            try
            {
                List<String> colunasexcluidas = new List<String> { "codcliente",
                    "codfucionariocriacao",
                    "codfuncionarioexclusao",
                    "codfuncionarioultimaalteracao",
                    "datacriacao",
                    "dataexclusao",
                    "dataultimaalteracao",
                    "excluido",
                    "geo",
                    "geom",
                    "ipcriacao",
                    "ipexclusao",
                    "ipultimaalteracao"};

                //var result = colunasnototal.Where(p => colunasexcluidas.All(p2 => p2 != p));
                //colunasaceitas = colunasaceitas.AddRange(colunasnototal.Where(p => colunasexcluidas.All(p2 => p2 != p)));
                IEnumerable<String> colunasaceitasienumerable = colunasnototal.Except(colunasexcluidas);

                return new List<string>(colunasaceitasienumerable);
                //colunasnototal = colunasnototal.all;
                //var colunasaceitas = colunasnototal.Except(colunasexcluidas);
            }
            catch (Exception er)
            {

                throw er;
            }
        }

        protected void btnPesquisar_Click(object sender, EventArgs e)
        {
            try
            {
                Tabela tabela = PegaTabelaEColunaSelecionadas();
                List<String> colunasnototal = new List<String>();
                foreach (ListItem item in ddlColunasDaTabela.Items)
                {
                    colunasnototal.Add(item.ToString());
                }
                List<String> colunasaceitas = TiraColunasIndesejadas(colunasnototal);

                //List<Dictionary<string, string>> resultadoselect =
                //TabelaCTR.PesquisaCamposCustomizados(colunasaceitas, tabela, txttermodapesquisa.Text.Trim());

                List<LinhaRegistro> resultadoselect =
                TabelaCTR.PesquisaCamposCustomizadosPelaTabela(colunasaceitas, tabela, txttermodapesquisa.Text.Trim());

                if (resultadoselect.Count > 0)
                {
                    DivExportarBuscaNovo.Visible = true;
                    rpBuscaNovo.DataSource = resultadoselect;
                    rpBuscaNovo.DataBind();
                }
                else
                {
                    //rpBuscaNovo.DataSource = null;
                    //rpBuscaNovo.DataBind();
                }
                // upPesquisa.Update();
            }
            catch (Exception er)
            {
                throw er;
            }
        }

        Tabela PegaTabelaEColunaSelecionadas()
        {
            Tabela tabela = new Tabela();
            tabela.ApelidoTabela = ddlTabelaBanco.SelectedItem.ToString();
            tabela.NomeTabela = ddlTabelaBanco.SelectedValue.ToString();
            Coluna coluna = new Coluna();
            coluna.Nome = ddlColunasDaTabela.SelectedItem.ToString();
            coluna.ApelidoNome = ddlColunasDaTabela.SelectedValue.ToString();
            tabela.Coluna.Add(coluna);
            return tabela;
        }
        protected void ddlTipoPesquisa_SelectedIndexChanged(object sender, EventArgs e)
        {
            FuncionarioCliente funcionarioCliente = (FuncionarioCliente)GetSession(ESession.FUNCIONARIO_CLIENTE);
            List<Tabela> tabelasdapesquisa = new List<Tabela>();
            switch (ddlTipoPesquisa.SelectedValue.ToString())
            {
                case "view":
                    tabelasdapesquisa = TabelaCTR.PesquisarTabelasPelaView(
                        int.Parse(funcionarioCliente.CodCliente.ToString()));
                    break;
                case "layer":
                    tabelasdapesquisa = TabelaCTR.PesquisarTabelasPeloLayerName(
                        int.Parse(funcionarioCliente.CodCliente.ToString()));
                    break;
                case "banco":
                    tabelasdapesquisa = TabelaCTR.PesquisarTabelasPeloBancoTodo(
                        int.Parse(funcionarioCliente.CodCliente.ToString()));
                    break;
            }

            List<ListItem> listadropdown = new List<ListItem>();
            foreach (Tabela itemtabela in tabelasdapesquisa)
            {
                listadropdown.Add(new ListItem { Text = itemtabela.NomeTabela, Value = itemtabela.ApelidoTabela });
            }

            listadropdown = listadropdown.OrderBy(x => x.Value != "selecione").ThenBy(x => x.Text).ToList();

            ddlTabelaBanco.DataSource = listadropdown;
            ddlTabelaBanco.DataTextField = "Text";
            ddlTabelaBanco.DataValueField = "Value";
            ddlTabelaBanco.DataBind();
            rpBusca.DataSource = null;
            rpBusca.DataBind();

            DivExportarBusca.Visible = false;
        }

        protected void rpBuscaNovo_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            Repeater r = (Repeater)e.Item.FindControl("RepetidorItens");
            
            //List<Modelo.ItemCompra> listaitems = new List<Modelo.ItemCompra>();
            //Modelo.Compra compramodelo = (Modelo.Compra)e.Item.DataItem;
            //listaitems = compramodelo.AuxItems;

            List<ChaveValor> listachavevalor = new List<ChaveValor>();
            LinhaRegistro modeloregistrolinha = (LinhaRegistro)e.Item.DataItem;
            listachavevalor = modeloregistrolinha.Registro;

            r.DataSource = listachavevalor;
            r.DataBind();
        }
    }





}
