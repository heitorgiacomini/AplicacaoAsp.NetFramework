<%@ Page Title="compra" Language="C#" MasterPageFile="~/Template/ContainerPrincipal.Master" AutoEventWireup="true" CodeBehind="itemcompra.aspx.cs" Inherits="TreinamentoHeitor.compra" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label Text="Cliente:" runat="server" />
    <asp:DropDownList runat="server" ID="ddlTest" DataTextField="Nome" DataValueField="Codigo">
    </asp:DropDownList>
    <br>
    <asp:Label Text="Produto:" runat="server" />
    <asp:DropDownList runat="server" ID="listProduto" DataTextField="Descricao" DataValueField="Codigo">
    </asp:DropDownList>
    <asp:Label Text="Quantidade:" runat="server" />
    <asp:TextBox runat="server" ID="txtqtdproduto" />
    <asp:Button Text="Adicionar" runat="server" ID="btnAddProduto" OnClick="BtnAddProduto_Click" /> 
    <hr>
    <asp:Repeater runat="server" ID="RepetidorCentral" OnItemDataBound="RepetidorTabela_ItemDataBound">
        <ItemTemplate>
            <hr>
            <asp:Label ID="lblCodigo" runat="server" Text='<%# Eval("Codigo")  %>'></asp:Label>
            <asp:Label ID="lblNomeCliente" runat="server" Text='<%# Eval("NomeCliente")  %>'></asp:Label>
            <asp:Label ID="lblDataCompra" runat="server" Text='<%# Eval("DataCompra")  %>'></asp:Label>

            <table border="1">
                <thead>
                    <tr>
                        <td>Codigo do Item</td>
                        <td>guid</td>
                        <td>codProduto</td>
                        <td>auxDescricao</td>
                        <td>auxValor</td>
                        <td>Quantidade</td>
                        <td>subTotal</td>
                        <td>Excluir</td>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="RepetidorItems" OnItemCommand="RepetidorItems_ItemCommand">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <asp:Label ID="lbCodigo" runat="server" Text='<%# Eval("Codigo")  %>'></asp:Label>
                                 </td>
                                <td>
                                    <asp:Label ID="lbguid" runat="server" Text='<%# Eval("guid")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lbcodProduto" runat="server" Text='<%# Eval("codProduto")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblauxDescricao" runat="server" Text='<%# Eval("auxDescricao")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblauxValor" runat="server" Text='<%# Eval("auxValor")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:TextBox ID="lblQuantidade" runat="server" Text='<%# Eval("Quantidade")  %>'></asp:TextBox>
                                </td>
                                <td>
                                    <asp:Label ID="lblsubTotal" runat="server" Text='<%# Eval("subTotal")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:LinkButton ID="Editar" runat="server" CommandName="Editar" CommandArgument='<%#Eval("guid")  %>' >X</asp:LinkButton>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan="5">Valor Total</td>
                        <td>
                            <asp:Label ID="lblValorTotal" runat="server" Text='<%# Eval("ValorTotal")  %>'></asp:Label>
                        </td>
                    </tr>
                </tfoot>
            </table>
        </ItemTemplate>
    </asp:Repeater>
    <asp:Button Text="Salvar" runat="server" ID="BtnSalvar" OnClick="BtnSalvar_Click" />



    <hr>
    <asp:Repeater runat="server" ID="RepetidorTabela" OnItemDataBound="RepetidorTabela_ItemDataBound" OnItemCommand="RepetidorTabela_ItemCommand">
        <ItemTemplate>
            <hr>
            <asp:Label ID="lblCodigo" runat="server" Text='<%# Eval("Codigo")  %>'></asp:Label>
            <asp:Label ID="lblNomeCliente" runat="server" Text='<%# Eval("NomeCliente")  %>'></asp:Label>
            <asp:Label ID="lblDataCompra" runat="server" Text='<%# Eval("DataCompra")  %>'></asp:Label>
            <asp:LinkButton ID="Editar" runat="server" CommandName="Editar" CommandArgument='<%# Eval("Codigo")  %>'>Editar</asp:LinkButton>
            <asp:LinkButton ID="Desabilitar" runat="server" CommandName="Desabilitar" CommandArgument='<%# Eval("Codigo")  %>'>Desabilitar</asp:LinkButton>
            <table border="1">
                <thead>
                    <tr>
                        <td>Codigo do Item</td>
                        <td>codProduto</td>
                        <td>auxDescricao</td>
                        <td>auxValor</td>
                        <td>Quantidade</td>
                        <td>SubTotal</td>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="RepetidorItems">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <asp:Label ID="lbCodigo" runat="server" Text='<%# Eval("Codigo")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lbcodProduto" runat="server" Text='<%# Eval("codProduto")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblauxDescricao" runat="server" Text='<%# Eval("auxDescricao")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="auxValor" runat="server" Text='<%# Eval("auxValor")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblQuantidade" runat="server" Text='<%# Eval("Quantidade")  %>'></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblauxValorTotal" runat="server" Text='<%# Eval("SubTotal")  %>'></asp:Label>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan="5">Valor Total</td>
                        <td>
                            <asp:Label ID="lblValorTotal" runat="server" Text='<%# Eval("ValorTotal")  %>'></asp:Label>
                        </td>
                    </tr>
                </tfoot>
            </table>
        </ItemTemplate>
    </asp:Repeater>


</asp:Content>
