using Microsoft.AspNet.SignalR; // Importing SignalR functionality
using Microsoft.AspNetCore.Mvc; // Importing MVC controller classes
using System.Threading.Tasks; // Importing types for asynchronous programming
using System.Diagnostics; // Importing classes for working with system processes
using YoutubeToMp3.Models; // Importing custom models

namespace YoutubeToMp3.Controllers
{
    // This class represents a controller for handling video-related actions
    public class VideoController : Controller
    {
        // Fields for SignalR communication and thread safety
        private IHubContext progressHubContext;
        private readonly object hubContextLock = new object();
        private readonly IConfiguration _configuration;

        // Constructor for the VideoController class
        public VideoController(IConfiguration configuration)
        {
            _configuration = configuration;
            // The progressHubContext field is not initialized here
        }

        // Property for lazy initialization of progressHubContext
        private IHubContext ProgressHubContext
        {
            get
            {
                // Lazy initialization using double-checked locking pattern
                if (progressHubContext == null)
                {
                    lock (hubContextLock)
                    {
                        if (progressHubContext == null)
                        {
                            // Getting the SignalR hub context
                            progressHubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
                        }
                    }
                }
                return progressHubContext;
            }
        }

        // Action method for rendering the index view
        public ActionResult Index()
        {
            return View();
        }

        // Action method for handling video download requests
        [HttpPost]
        [Route("Video/Download")]
        public async Task<IActionResult> Download(VideoModel model)
        {
            // Check if the YouTube URL is provided
            if (string.IsNullOrWhiteSpace(model.YouTubeUrl))
            {
                // Set an error message if the URL is missing
                ViewBag.Message = "Please enter a valid YouTube URL.";
                return View("Index");
            }

            try
            {
                // Download the video asynchronously
                string videoPath = await DownloadVideoAsync(model.YouTubeUrl);
                // Set success message and output path
                ViewBag.Message = "Video downloaded successfully.";
                ViewBag.OutputPath = videoPath;
            }
            catch (Exception ex)
            {
                // Set error message if an exception occurs during download
                ViewBag.Message = $"An error occurred: {ex.Message}";
            }

            return View("Index");
        }

        // Action method for handling video conversion requests
        [HttpPost]
        [Route("Video/Convert")]
        public async Task<IActionResult> Convert(VideoModel model)
        {
            // Check if the video output path is provided and exists
            if (string.IsNullOrWhiteSpace(model.OutputPath) || !System.IO.File.Exists(model.OutputPath))
            {
                // Set an error message if the path is missing or invalid
                ViewBag.Message = "Invalid video path. Please download the video first.";
                return View("Index");
            }

            try
            {
                // Convert the video to MP3 asynchronously
                string mp3Path = Path.ChangeExtension(model.OutputPath, ".mp3");
                await ConvertToMp3Async(model.OutputPath, mp3Path);
                // Set success message and MP3 path
                ViewBag.Message = "Video converted to MP3 successfully.";
                ViewBag.Mp3Path = mp3Path;
            }
            catch (Exception ex)
            {
                // Set error message if an exception occurs during conversion
                ViewBag.Message = $"An error occurred: {ex.Message}";
            }

            return View("Index");
        }

        // Asynchronously download a video from a given URL
        private async Task<string> DownloadVideoAsync(string url)
        {
            // Ensure the SignalR hub context is initialized
            var hubContext = ProgressHubContext;
            var ytDlpPath = _configuration["Paths:YtDlpPath"];

            // Set the output directory and file name for the downloaded video
            string outputDirectory = _configuration["Paths:DownloadFolder"]; 
            string outputFile = Path.Combine(outputDirectory, $"downloaded_{DateTime.Now:yyyyMMddHHmmss}.mp4");

            // Path to the yt-dlp executable            
            var startInfo = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                Arguments = $"-o \"{outputFile}\" {url}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the yt-dlp process to download the video
            using (var process = new Process { StartInfo = startInfo })
            {
                // Send progress update to clients before starting the process
                await hubContext.Clients.All.SendAsync("ReceiveProgress", "Starting download...");

                // Event handlers for capturing process output
                process.OutputDataReceived += async (sender, args) => await hubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);
                process.ErrorDataReceived += async (sender, args) => await hubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);

                // Start the process and wait for it to exit
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                // Check if yt-dlp process exited with a non-zero code
                if (process.ExitCode != 0)
                {
                    throw new Exception("yt-dlp failed to download the video.");
                }
            }

            // Check if the downloaded video file exists
            if (!System.IO.File.Exists(outputFile))
            {
                // Check if a WebM version of the file exists
                string webmFile = outputFile + ".webm";
                if (System.IO.File.Exists(webmFile))
                {
                    outputFile = webmFile;
                }
                else
                {
                    // Throw exception if neither the MP4 nor WebM file is found
                    throw new Exception("Downloaded video file not found.");
                }
            }

            return outputFile;
        }

        // Asynchronously convert a video to MP3 format
        private async Task ConvertToMp3Async(string inputFile, string outputFile)
        {
            // Path to the ffmpeg executable
            var ffmpegPath = _configuration["Paths:FfmpegPath"];
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputFile}\" -q:a 0 -map a \"{outputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the ffmpeg process to convert the video
            using (var process = new Process { StartInfo = startInfo })
            {
                // Check if progressHubContext is initialized before subscribing to events
                if (progressHubContext != null)
                {
                    // Event handlers for capturing process output
                    process.OutputDataReceived += async (sender, args) => await progressHubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);
                    process.ErrorDataReceived += async (sender, args) => await progressHubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);
                }

                // Start the process and wait for it to exit
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                // Check if ffmpeg process exited with a non-zero code
                if (process.ExitCode != 0)
                {
                    throw new Exception("FFmpeg failed to convert the video to MP3.");
                }
            }
        }
    }
}
