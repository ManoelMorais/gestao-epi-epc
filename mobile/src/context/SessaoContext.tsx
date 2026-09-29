import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { api } from '@/services/api';
import type { Sessao } from '@/types/dominio';

interface SessaoContexto {
  sessao: Sessao | null;
  entrar(matricula: string, senha: string): Promise<void>;
  sair(): void;
}

const Contexto = createContext<SessaoContexto | null>(null);

export function SessaoProvider({ children }: { children: ReactNode }) {
  const [sessao, setSessao] = useState<Sessao | null>(null);

  const entrar = useCallback(async (matricula: string, senha: string) => {
    setSessao(await api.login(matricula, senha));
  }, []);

  const sair = useCallback(() => setSessao(null), []);

  const valor = useMemo(() => ({ sessao, entrar, sair }), [sessao, entrar, sair]);
  return <Contexto.Provider value={valor}>{children}</Contexto.Provider>;
}

export function useSessao() {
  const contexto = useContext(Contexto);
  if (!contexto) throw new Error('useSessao precisa estar dentro de <SessaoProvider>.');
  return contexto;
}

/** Para telas que só existem com usuário logado (protegidas no layout raiz). */
export function useSessaoAtiva(): Sessao {
  const { sessao } = useSessao();
  if (!sessao) throw new Error('Nenhuma sessão ativa.');
  return sessao;
}
