using ChatClient.Services.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Services.Security
{
    public class DpapiTokenStore : ITokenStore
    {
        private const string AppFolderName = "ChatApp";
        private const string SessionFileName = "session.dat";

        // Entropia adicional: NÃO é um segredo (fica compilada no binário, qualquer um com
        // acesso ao código vê). Ela só separa este blob de qualquer outro dado que o mesmo
        // usuário Windows proteja com DPAPI — não é essa entropia que garante a segurança,
        // é o vínculo com a conta do usuário feito pelo próprio DPAPI.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ChatApp.Client.RefreshToken.v1");

        private readonly string _filePath;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private string? _accessToken;       // nunca toca o disco
        private string? _refreshTokenCache; // cache em memória do que está salvo, evita reler o arquivo a cada chamada

        public DpapiTokenStore()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName);
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, SessionFileName);
        }
        
        public Task<string?> GetAccessTokenAsync() => Task.FromResult(_accessToken);

        public async Task<string?> GetRefreshTokenAsync()
        {
            if (_refreshTokenCache != null)
                return _refreshTokenCache;

            await _lock.WaitAsync();

            try
            {
                if (_refreshTokenCache != null)
                    return _refreshTokenCache;

                if (!File.Exists(_filePath))
                    return null;

                var protectedBytes = await File.ReadAllBytesAsync(_filePath);
                var plainBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                _refreshTokenCache = Encoding.UTF8.GetString(plainBytes);
                return _refreshTokenCache;
            }
            catch (CryptographicException)
            {
                // Arquivo corrompido, ou criado por outro usuário/máquina (chave mestra diferente).
                // Trata como "sem sessão salva" em vez de derrubar o app na inicialização.
                TryDeleteFile();
                return null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(string accessToken, string refreshToken, bool persistRefreshToken = true)
        {
            _accessToken = accessToken;
            _refreshTokenCache = refreshToken;

            await _lock.WaitAsync();

            try
            {
                if (!persistRefreshToken)
                {
                    TryDeleteFile();
                    return;
                }

                var plainBytes = Encoding.UTF8.GetBytes(refreshToken);
                var protectedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                await File.WriteAllBytesAsync(_filePath, protectedBytes);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task ClearAsync()
        {
            _accessToken = null;
            _refreshTokenCache = null;
            await _lock.WaitAsync();
            try
            {
                TryDeleteFile();
            }
            finally
            {
                _lock.Release();
            }
        }

        private void TryDeleteFile()
        {
            try
            {
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
            }
            catch (IOException)
            {
                // Arquivo momentaneamente em uso (ex.: antivírus escaneando). Não é crítico —
                // a próxima chamada a ClearAsync/SaveAsync resolve.
            }
        }
    }
}
