using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TodoSynchronizer.Core.Services
{
    internal sealed class DidaSourceIndex
    {
        private const int IvSize = 16;
        private const int TagSize = 32;
        private readonly string path;
        private readonly byte[] encryptionKey;
        private readonly byte[] authenticationKey;
        private readonly Dictionary<(string ProjectId, string TaskId), Entry> entries = new();
        private bool changed;

        public DidaSourceIndex(string path, byte[] key)
        {
            this.path = Path.GetFullPath(path);
            encryptionKey = key[..32];
            authenticationKey = key[32..];
            Load();
        }

        public string FindSource(string projectId, string taskId)
        {
            return entries.TryGetValue((projectId, taskId), out var entry) ? entry.Source : null;
        }

        public bool NeedsComment(string projectId, string taskId)
        {
            return entries.TryGetValue((projectId, taskId), out var entry) && entry.NeedsComment;
        }

        public void Remember(string projectId, string taskId, string source, bool needsComment = false)
        {
            if (string.IsNullOrWhiteSpace(source)
                || (FindSource(projectId, taskId) == source && NeedsComment(projectId, taskId) == needsComment))
                return;

            entries[(projectId, taskId)] = new Entry
            {
                ProjectId = projectId, TaskId = taskId, Source = source, NeedsComment = needsComment
            };
            changed = true;
        }

        public void RemoveMissing(string projectId, HashSet<string> liveIds)
        {
            var missing = entries.Keys.Where(id => id.ProjectId == projectId && !liveIds.Contains(id.TaskId)).ToArray();
            foreach (var id in missing)
                changed |= entries.Remove(id);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var file = File.ReadAllBytes(path).AsSpan();
                if (file.Length <= TagSize + IvSize)
                    return;

                // 校验 IV 和密文的 HMAC 后再解密。
                var tag = HMACSHA256.HashData(authenticationKey, file[TagSize..]);
                if (!CryptographicOperations.FixedTimeEquals(tag, file[..TagSize]))
                    return;

                using var cipher = Aes.Create();
                cipher.Key = encryptionKey;
                var plaintext = cipher.DecryptCbc(file[(TagSize + IvSize)..], file.Slice(TagSize, IvSize));
                var restored = JsonConvert.DeserializeObject<List<Entry>>(Encoding.UTF8.GetString(plaintext));
                if (restored == null)
                    return;

                foreach (var entry in restored)
                {
                    if (!string.IsNullOrWhiteSpace(entry?.ProjectId)
                        && !string.IsNullOrWhiteSpace(entry.TaskId) && !string.IsNullOrWhiteSpace(entry.Source))
                        entries[(entry.ProjectId, entry.TaskId)] = entry;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException or JsonException)
            {
                // 缓存损坏、过期或口令更换后，重新通过官方 API 识别任务。
            }
        }

        public void Save()
        {
            if (!changed)
                return;

            var plaintext = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entries.Values));
            using var cipher = Aes.Create();
            cipher.Key = encryptionKey;
            var iv = RandomNumberGenerator.GetBytes(IvSize);
            var ciphertext = cipher.EncryptCbc(plaintext, iv);
            var file = new byte[TagSize + IvSize + ciphertext.Length];
            iv.CopyTo(file, TagSize);
            ciphertext.CopyTo(file, TagSize + IvSize);
            HMACSHA256.HashData(authenticationKey, file.AsSpan(TagSize), file.AsSpan(0, TagSize));

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllBytes(temporaryPath, file);
                File.Move(temporaryPath, path, overwrite: true);
                changed = false;
            }
            finally
            {
                File.Delete(temporaryPath);
            }
        }

        private sealed class Entry
        {
            public string ProjectId { get; set; }
            public string TaskId { get; set; }
            public string Source { get; set; }
            public bool NeedsComment { get; set; }
        }
    }
}
