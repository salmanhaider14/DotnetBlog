using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Components.Forms;

namespace Blog.Services;

public class ImageUploadService(Cloudinary _cloudinary)
{
    public async Task<List<string>> UploadImagesAsync(IReadOnlyList<IBrowserFile> files)
    {
        var imageUrls = new List<string>();

        foreach (var file in files)
        {
            // Convert the file to a stream
            await using var stream = file.OpenReadStream();

            // Create a Cloudinary upload parameter
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.Name, stream)
            };

            // Upload to Cloudinary and get the result
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                imageUrls.Add(uploadResult.SecureUrl.ToString()); // Store the secure URL of the uploaded image
            }
        }

        return imageUrls;
    }
}
