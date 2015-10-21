﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exceptionless.Core.Queues.Models;
using Foundatio.Logging;
using Foundatio.Storage;

namespace Exceptionless.Core.Extensions {
    public static class StorageExtensions {
        public static async Task<EventPostInfo> GetEventPostAndSetActiveAsync(this IFileStorage storage, string path, CancellationToken cancellationToken = default(CancellationToken)) {
            if (String.IsNullOrEmpty(path))
                return null;

            EventPostInfo eventPostInfo;
            try {
                eventPostInfo = await storage.GetObjectAsync<EventPostInfo>(path, cancellationToken).AnyContext();
                if (eventPostInfo == null)
                    return null;

                if (cancellationToken.IsCancellationRequested)
                    return null;

                if (!await storage.ExistsAsync(path + ".x").AnyContext() && !await storage.SaveFileAsync(path + ".x", new MemoryStream(Encoding.UTF8.GetBytes(String.Empty))).AnyContext())
                    return null;
            } catch (Exception ex) {
                Logger.Error().Exception(ex).Message("Error retrieving event post data \"{0}\".", path).Write();
                return null;
            }

            return eventPostInfo;
        }

        public static async Task<bool> SetNotActiveAsync(this IFileStorage storage, string path) {
            if (String.IsNullOrEmpty(path))
                return false;

            try {
                return await storage.DeleteFileAsync(path + ".x").AnyContext();
            } catch (Exception ex) {
                Logger.Error().Exception(ex).Message("Error deleting work marker \"{0}\".", path + ".x").Write();
            }

            return false;
        }

        public static async Task<bool> CompleteEventPostAsync(this IFileStorage storage, string path, string projectId, DateTime created, bool shouldArchive = true) {
            if (String.IsNullOrEmpty(path))
                return false;

            // don't move files that are already in the archive
            if (path.StartsWith("archive"))
                return true;

            string archivePath = $"archive\\{projectId}\\{created.ToString("yy\\\\MM\\\\dd")}\\{Path.GetFileName(path)}";
            
            try {
                if (shouldArchive) {
                    if (!await storage.RenameFileAsync(path, archivePath).AnyContext())
                        return false;
                } else {
                    if (!await storage.DeleteFileAsync(path).AnyContext())
                        return false;
                }
            } catch (Exception ex) {
                Logger.Error().Exception(ex).Message("Error archiving event post data \"{0}\".", path).Write();
                return false;
            }

            await storage.SetNotActiveAsync(path).AnyContext();
            return true;
        }
    }
}
