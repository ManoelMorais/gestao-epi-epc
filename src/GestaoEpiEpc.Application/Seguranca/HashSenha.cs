using System.Security.Cryptography;

namespace GestaoEpiEpc.Application.Seguranca;

/// <summary>
/// Hash de senha com bcrypt (custo 10, prefixo $2a$). É o formato que o Postgres também sabe verificar
/// (<c>crypt()</c> do pgcrypto) — assim o login do app mobile, feito por uma função no banco, e o
/// desktop validam a mesma senha. Hashes PBKDF2 antigos ainda são aceitos na verificação.
/// </summary>
public static class HashSenha
{
    private const int Custo = 10;

    public static string Gerar(string senha) =>
        BCrypt.Net.BCrypt.HashPassword(senha, BCrypt.Net.BCrypt.GenerateSalt(Custo, 'a'));

    public static bool Verificar(string senha, string? hashGravado)
    {
        if (string.IsNullOrEmpty(hashGravado)) return false;
        if (EhLegado(hashGravado)) return VerificarPbkdf2(senha, hashGravado);

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hashGravado);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }

    /// <summary>Hash no formato PBKDF2 usado antes do login do app ir para o banco — deve ser regravado em bcrypt.</summary>
    public static bool EhLegado(string? hashGravado) => hashGravado?.StartsWith("pbkdf2$", StringComparison.Ordinal) == true;

    private static bool VerificarPbkdf2(string senha, string hashGravado)
    {
        var partes = hashGravado.Split('$');
        if (partes is not ["pbkdf2", var iteracoesTexto, var saltTexto, var hashTexto] || !int.TryParse(iteracoesTexto, out var iteracoes))
            return false;

        var esperado = Convert.FromBase64String(hashTexto);
        var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, Convert.FromBase64String(saltTexto), iteracoes, HashAlgorithmName.SHA256, esperado.Length);
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
