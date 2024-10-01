using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Storage.Files.Shares;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System.Net;

/*
 ================================================================
// Code Attribution

Author: Mick Gouweloos
Link: https://github.com/mickymouse777/Cloud_Storage
Date Accessed: 20 August 2024

Author: Mick Gouweloos
Link: https://github.com/mickymouse777/SimpleSample.git
Date Accessed: 20 September 2024

Author: W3schools
Link: https://www.w3schools.com/colors/colors_picker.asp
Date Accessed: 21 August 2024

Author: W3schools
Link: https://www.w3schools.com/css/css_font.asp 
Date Accessed: 21 August 2024

 *********All Images used throughout project are adapted from https://bangtanpictures.net/index.php and https://shop.weverse.io/en/home*************

 ================================================================
!--All PAGES are edited but layout depicted from Tooplate Template-
(https://www.tooplate.com/) 

 */

namespace CLDV6212_FileShareFunction
{
    public class CLDV6212_FileShareFunction
    {
        // This is a logger object that will be used to log information about the function's execution
        private readonly ILogger<CLDV6212_FileShareFunction> _logger;

        // Constructor: This function is called when the CLDV6212_FileShareFunction class is created.
        // It takes a logger as an argument and assigns it to the private _logger field.
        public CLDV6212_FileShareFunction(ILogger<CLDV6212_FileShareFunction> logger)
        {
            _logger = logger; // Assign the logger provided by the system
        }

        // This is the main function that gets triggered by an HTTP request (GET or POST).
        // The Function attribute specifies the name of the Azure function.
        [Function("CLDV6212_FileShareFunction")]
        public async Task<IActionResult> Run(
            // The HttpTrigger attribute listens for an HTTP request (either GET or POST).
            // The incoming request is passed as an argument.
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            // Log that the function has started processing the request.
            _logger.LogInformation("FileShareFunction processing a request for a file.");

            // Check if any files were uploaded in the request (the request contains a form with files).
            if (req.Form.Files.Count == 0)
            {
                // If no file was uploaded, return a "Bad Request" response with a message.
                return new BadRequestObjectResult("No file uploaded.");
            }

            // If a file was uploaded, get the first file in the form.
            var fileUpload = req.Form.Files[0];

            try
            {
                // Get the Azure Storage connection string from the environment variables.
                string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                // Create a ShareClient object to interact with Azure File Share. 
                // This connects to a file share called "contractsshare".
                ShareClient share = new ShareClient(connectionString, "contractsshare");

                // Check if the file share exists. If not, return a 500 Internal Server Error.
                if (!await share.ExistsAsync())
                {
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Get a directory client for the "uploads" folder within the file share.
                ShareDirectoryClient directory = share.GetDirectoryClient("uploads");

                // Get a file client for the file being uploaded, using the uploaded file's name.
                ShareFileClient fileClient = directory.GetFileClient(fileUpload.FileName);

                // Open the uploaded file as a stream so it can be read.
                using (var stream = fileUpload.OpenReadStream())
                {
                    // Create a new file in Azure File Share with the same size as the uploaded file.
                    await fileClient.CreateAsync(fileUpload.Length);

                    // Upload the file stream to the newly created file in the Azure File Share.
                    await fileClient.UploadRangeAsync(new HttpRange(0, fileUpload.Length), stream);
                }

                // If everything went well, return a success message.
                return new OkObjectResult("File uploaded successfully.");
            }
            catch (Exception ex)
            {
                // Log any errors that occurred during the file upload process.
                _logger.LogError(ex, "An error occurred during file upload.");

                // If an error occurred, return a 500 Internal Server Error.
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}