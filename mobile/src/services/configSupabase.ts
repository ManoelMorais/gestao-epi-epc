// Endereço do backend (Supabase) usado pelo app.
//
// A chave "publishable" é pública por definição — fica embutida no APK e só permite chamar as
// funções RPC da API do app (as tabelas têm RLS e nenhuma policy). Por isso pode ficar aqui como
// valor padrão: o build do EAS funciona sem precisar de arquivo .env.
// Para apontar para outro projeto, defina EXPO_PUBLIC_SUPABASE_URL / EXPO_PUBLIC_SUPABASE_KEY.

export const SUPABASE_URL = process.env.EXPO_PUBLIC_SUPABASE_URL ?? 'https://pboqynadvyfqtmeqxbqq.supabase.co';
export const SUPABASE_KEY = process.env.EXPO_PUBLIC_SUPABASE_KEY ?? 'sb_publishable_PadHzY1MPHVwHl_U9TUCyg_W1LmZfio';
