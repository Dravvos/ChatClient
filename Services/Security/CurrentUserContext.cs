using ChatClient.Services.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient.Services.Security
{
    public class CurrentUserContext(ITokenStore tokenStore) : ICurrentUserContext
    {
        private Guid? _cachedUserId;
        public async Task<Guid?> GetUserIdAsync()
        {
            if(_cachedUserId.HasValue)
                return _cachedUserId;

            var accessToken = await tokenStore.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken)) return null;

            _cachedUserId = ExtracUserId(accessToken);
            return _cachedUserId;
        }

        public void Reset()=> _cachedUserId = null;

        private Guid? ExtracUserId(string accessToken)
        {

            // Leitura só pra fins de UI — a validação de assinatura/expiração já é
            // feita pelo servidor a cada request; aqui só precisamos saber quem é "eu".
            var parts = accessToken.Split('.');
            if (parts.Length != 3) return null;

            try
            {
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                using var doc = JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("sub", out var sub) &&
                    Guid.TryParse(sub.GetString(), out var id))
                    return id;
            }
            catch (Exception ex)
            {
                // token malformado — trata como não autenticado, não derruba a UI
                Console.WriteLine($"Erro ao extrair o id do token: {ex}");
            }
            return null;
        }

        private string ExtractUserName(string accessToken)
        {
            var parts = accessToken.Split('.');
            if (parts.Length != 3) return null;
            try
            {
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                using var doc = JsonDocument.Parse(payloadJson);
                if (doc.RootElement.TryGetProperty("username", out var username))
                    return username.GetString();
            }
            catch (Exception ex)
            {
                // token malformado — trata como não autenticado, não derruba a UI
                Console.WriteLine($"Erro ao extrair o username do token: {ex}");
            }
            return string.Empty;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var padded = input.Replace('-', '+').Replace('_', '/');
            padded += (padded.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
            return Convert.FromBase64String(padded);
        }

        public async Task<string> GetUserNameAsync()
        {
            var accessToken = await tokenStore.GetAccessTokenAsync();
            
            if (string.IsNullOrEmpty(accessToken)) return string.Empty;

            return ExtractUserName(accessToken);
        }
    }
}
