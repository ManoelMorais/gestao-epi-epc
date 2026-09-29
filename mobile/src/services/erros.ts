/** Erro de regra de negócio, com mensagem pronta para exibir ao usuário. */
export class ErroNegocio extends Error {}

/** Texto amigável para qualquer erro vindo da API. */
export function mensagemDeErro(e: unknown) {
  return e instanceof ErroNegocio ? e.message : 'Algo deu errado ao falar com o servidor. Tente de novo em instantes.';
}
