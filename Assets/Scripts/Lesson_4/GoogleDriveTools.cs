using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityGoogleDrive;
using UnityGoogleDrive.Data;

public static class GoogleDriveTools
{
    // Название твоей папки на Google Диске
    private const string TARGET_FOLDER_NAME = "SkillBox_ECS";

    // Ищем скрытый ID папки по её имени
    private static Task<string> GetFolderIdAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        var request = GoogleDriveFiles.List();

        // Указываем, что ищем именно папку
        request.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{TARGET_FOLDER_NAME}' and trashed = false";
        request.Fields = new List<string> { "files(id)" };

        request.Send().OnDone += fileList =>
        {
            if (fileList != null && fileList.Files != null && fileList.Files.Count > 0)
                tcs.SetResult(fileList.Files[0].Id);
            else
                tcs.SetResult(null);
        };

        return tcs.Task;
    }

    // Ищем файл СТРОГО внутри найденной папки
    private static async Task<File> FindFileByNameAsync(string fileName, string folderId)
    {
        if (string.IsNullOrEmpty(folderId)) return null;

        var tcs = new TaskCompletionSource<File>();
        var request = GoogleDriveFiles.List();

        // Добавляем параметр '{folderId}' in parents, чтобы отсечь файлы из корня диска
        request.Q = $"name = '{fileName}' and '{folderId}' in parents and trashed = false";
        request.Fields = new List<string> { "files(id, name)" };

        request.Send().OnDone += fileList =>
        {
            if (fileList != null && fileList.Files != null && fileList.Files.Count > 0)
                tcs.SetResult(fileList.Files[0]);
            else
                tcs.SetResult(null);
        };

        return await tcs.Task;
    }

    // Скачивание контента файла по ID
    private static Task<File> DownloadAsync(string fileId)
    {
        var tcs = new TaskCompletionSource<File>();

        GoogleDriveFiles.Download(fileId).Send().OnDone += downloadedFile =>
        {
            if (downloadedFile == null) Debug.LogError($"[GoogleDrive] Ошибка скачивания файла с ID: {fileId}");
            tcs.SetResult(downloadedFile);
        };

        return tcs.Task;
    }

    // Главный метод скачивания данных для менеджера
    public static async Task<string> DownloadFileAsync(string fileName)
    {
        string folderId = await GetFolderIdAsync();

        if (string.IsNullOrEmpty(folderId))
        {
            Debug.LogError($"[GoogleDrive] Папка {TARGET_FOLDER_NAME} не найдена на диске!");
            return null;
        }

        File targetFile = await FindFileByNameAsync(fileName, folderId);

        if (targetFile != null)
        {
            File downloadedFile = await DownloadAsync(targetFile.Id);
            if (downloadedFile != null && downloadedFile.Content != null)
            {
                return Encoding.ASCII.GetString(downloadedFile.Content);
            }
        }

        return null;
    }

    // Главный метод сохранения (перезаписи/создания) данных
    public static async Task<File> UploadAsync(string fileName, string jsonData)
    {
        string folderId = await GetFolderIdAsync();

        if (string.IsNullOrEmpty(folderId))
        {
            Debug.LogError($"[GoogleDrive] Ошибка сохранения: Папка {TARGET_FOLDER_NAME} не найдена!");
            return null;
        }

        File existingFile = await FindFileByNameAsync(fileName, folderId);

        File fileData = new File
        {
            Name = fileName,
            Content = Encoding.ASCII.GetBytes(jsonData)
        };

        var tcs = new TaskCompletionSource<File>();

        if (existingFile != null)
        {
            // Файл найден ВНУТРИ папки - перезаписываем его
            GoogleDriveFiles.Update(existingFile.Id, fileData).Send().OnDone += uploadedFile =>
            {
                if (uploadedFile != null) Debug.Log($"[GoogleDrive] Файл {fileName} ОБНОВЛЕН в папке {TARGET_FOLDER_NAME}!");
                tcs.SetResult(uploadedFile);
            };
        }
        else
        {
            // Файл не найден - создаем новый и жестко привязываем его к нашей папке
            fileData.Parents = new List<string> { folderId };

            GoogleDriveFiles.Create(fileData).Send().OnDone += uploadedFile =>
            {
                if (uploadedFile != null) Debug.Log($"[GoogleDrive] Новый файл {fileName} СОЗДАН в папке {TARGET_FOLDER_NAME}!");
                tcs.SetResult(uploadedFile);
            };
        }

        return await tcs.Task;
    }
}