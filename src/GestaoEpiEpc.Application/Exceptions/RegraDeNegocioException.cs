namespace GestaoEpiEpc.Application.Exceptions;

/// <summary>Violação de uma regra de negócio, com mensagem pronta para exibir ao usuário.</summary>
public class RegraDeNegocioException(string mensagem) : Exception(mensagem)
{
}
