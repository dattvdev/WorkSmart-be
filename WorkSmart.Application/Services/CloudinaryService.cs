﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImage(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.AbsoluteUri; // Trả về link ảnh
        }

        public async Task<string> UploadFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            await using var stream = file.OpenReadStream();

            // Kiểm tra xem file có phải là PDF không
            bool isPdf = file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
                         file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

            if (isPdf)
            {
                // Nếu là PDF, sử dụng ImageUploadParams thay vì RawUploadParams
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName,
                    Format = "pdf" // Giữ nguyên định dạng PDF
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult.SecureUrl.AbsoluteUri;
            }
            else
            {
                // Nếu không phải PDF, tiếp tục sử dụng RawUploadParams cho các loại file khác
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderName
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult.SecureUrl.AbsoluteUri;
            }
        }

        public async Task<bool> DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                // Lấy PublicId từ URL
                var publicId = ExtractPublicId(imageUrl);
                if (string.IsNullOrEmpty(publicId))
                    return false;

                // Gọi API Cloudinary để xóa ảnh
                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
                return false;
            }
        }

        // Hàm trích xuất public_id từ URL của Cloudinary
        private string ExtractPublicId(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var pathSegments = uri.Segments;
                var fileNameWithExtension = pathSegments[^1]; // Lấy tên file có phần mở rộng
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);

                return $"profile_pictures/{fileNameWithoutExtension}";
            }
            catch
            {
                return null;
            }
        }
    }
}
