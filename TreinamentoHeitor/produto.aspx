<%@ Page Title="Produto" Language="C#" MasterPageFile="~/Template/ContainerPrincipal.Master" AutoEventWireup="true" CodeBehind="produto.aspx.cs" Inherits="TreinamentoHeitor.WebForm2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label Text="Descricao: " runat="server" />
    <asp:TextBox runat="server" ID="txtdescricao" />
    <br>
    <asp:Label Text="Valor: " runat="server" />
    <asp:TextBox runat="server" ID="txtvalor" />
    <br>
    <asp:Button Text="Enviar" runat="server" ID="cadastrarProduto" OnClick="cadastrarProduto_Click" />
    <asp:Label ID="txterro" runat="server" />
    <hr>
        <table border="1">
        <thead>
            <tr>
                <th>Codigo</th>
                <th>DataCriacao</th>
                <th>Descricao</th>
                <th>ValorUnitario</th>
            </tr>
        </thead>
        <tbody>
            <asp:Repeater runat="server" ID="repetidor">
                <ItemTemplate>
                    <tr>    
                        <td><%# Eval("Codigo")  %></td>
                        <td><%# Eval("DataCriacao")  %></td>
                        <td><%# Eval("Descricao")  %></td>
                        <td><%# Eval("ValorUnitario")  %></td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </tbody>
    </table>

</asp:Content>
