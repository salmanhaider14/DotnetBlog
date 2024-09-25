using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Components.Forms;

namespace Blog.Services;

public class ImageUploadService(Cloudinary _cloudinary)
{
    public async Task<List<string>> UploadImagesAsync(IReadOnlyList<IBrowserFile> files)
    {
        var maxSizeInBytes = 5 * 1024 * 1024; // 5 MB
        var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };
        var uploadedImageUrls = new List<string>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.Name).ToLower();
            if (!allowedExtensions.Contains(extension) || file.Size > maxSizeInBytes)
            {
                // Skip invalid files
                continue;
            }

            // Process the valid files
            try
            {
                using var stream = file.OpenReadStream(maxSizeInBytes);  // <-- Set the max allowed size here
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.Name, stream)
                };
                // Upload the image to Cloudinary (or any other service)
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    uploadedImageUrls.Add(uploadResult.SecureUrl.AbsoluteUri);
                }
            }
            catch (IOException ioEx)
            {
                // Handle the error when the file size exceeds the limit
                Console.WriteLine(ioEx.Message);
                continue;
            }
        }

        return uploadedImageUrls;
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Extract public ID from the URL
            var publicId = ExtractPublicId(imageUrl);

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            return deletionResult.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            // Handle exceptions if needed
            Console.WriteLine($"Error deleting image: {ex.Message}");
            return false;
        }
    }

    private string ExtractPublicId(string imageUrl)
    {
        // Assuming the image URL follows Cloudinary's URL format
        var uri = new Uri(imageUrl);
        var segments = uri.Segments;

        // Example URL: https://res.cloudinary.com/demo/image/upload/v1234567890/sample.jpg
        // The public ID is the part after "/upload/" and before the extension
        var publicId = segments[segments.Length - 1]; // last segment
        publicId = Path.GetFileNameWithoutExtension(publicId); // remove extension

        // You might need to remove the version if present
        if (publicId.Contains("/"))
        {
            publicId = publicId.Split('/').Last();
        }

        return publicId;
    }

}
