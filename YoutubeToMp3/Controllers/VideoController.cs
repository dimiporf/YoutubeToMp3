using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Diagnostics;
using YoutubeToMp3.Models;

namespace YoutubeToMp3.Controllers
{
    public class VideoController : Controller
    {
        private IHubContext progressHubContext;
        private readonly object hubContextLock = new object(); // Ensure thread safety

        public VideoController()
        {
            // Do not initialize progressHubContext here
        }

        private IHubContext ProgressHubContext
        {
            get
            {
                // Lazy initialization of progressHubContext
                if (progressHubContext == null)
                {
                    lock (hubContextLock)
                    {
                        if (progressHubContext == null)
                        {
                            progressHubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
                        }
                    }
                }
                return progressHubContext;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Video/Download")]
        public async Task<IActionResult> Download(VideoModel model)
        {
            if (string.IsNullOrWhiteSpace(model.YouTubeUrl))
            {
                ViewBag.Message = "Please enter a valid YouTube URL.";
                return View("Index");
            }

            try
            {
                string videoPath = await DownloadVideoAsync(model.YouTubeUrl);
                model.OutputPath = videoPath;
                ViewBag.Message = "Video downloaded successfully.";
                ViewBag.OutputPath = model.OutputPath;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
            }

            return View("Index");
        }

        [HttpPost]
        [Route("Video/Convert")]
        public async Task<IActionResult> Convert(VideoModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OutputPath) || !System.IO.File.Exists(model.OutputPath))
            {
                ViewBag.Message = "Invalid video path. Please download the video first.";
                return View("Index");
            }

            try
            {
                string mp3Path = Path.ChangeExtension(model.OutputPath, ".mp3");
                await ConvertToMp3Async(model.OutputPath, mp3Path);
                ViewBag.Message = "Video converted to MP3 successfully.";
                ViewBag.Mp3Path = mp3Path;
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"An error occurred: {ex.Message}";
            }

            return View("Index");
        }

        private async Task<string> DownloadVideoAsync(string url)
        {
            // Ensure progressHubContext is initialized
            var hubContext = ProgressHubContext;

            string outputDirectory = @"C:\Users\dimip\Desktop\TestFolder\";
            string outputFile = Path.Combine(outputDirectory, $"downloaded_{DateTime.Now:yyyyMMddHHmmss}.mp4");

            var ytDlpPath = @"C:\Users\dimip\Downloads\ffmpeg-2024-06-09-git-94f2274a8b-essentials_build\ffmpeg-2024-06-09-git-94f2274a8b-essentials_build\bin\yt-dlp.exe";
            var startInfo = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                Arguments = $"-o \"{outputFile}\" {url}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                // Ensure progressHubContext is initialized
                await hubContext.Clients.All.SendAsync("ReceiveProgress", "Starting download...");

                process.OutputDataReceived += async (sender, args) => await hubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);
                process.ErrorDataReceived += async (sender, args) => await hubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception("yt-dlp failed to download the video.");
                }
            }

            if (!System.IO.File.Exists(outputFile))
            {
                string webmFile = outputFile + ".webm";
                if (System.IO.File.Exists(webmFile))
                {
                    outputFile = webmFile;
                }
                else
                {
                    throw new Exception("Downloaded video file not found.");
                }
            }

            return outputFile;
        }

        private async Task ConvertToMp3Async(string inputFile, string outputFile)
        {
            var ffmpegPath = @"C:\Users\dimip\Downloads\ffmpeg-2024-06-09-git-94f2274a8b-essentials_build\ffmpeg-2024-06-09-git-94f2274a8b-essentials_build\bin\ffmpeg.exe";
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputFile}\" -q:a 0 -map a \"{outputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += async (sender, args) => await progressHubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);
                process.ErrorDataReceived += async (sender, args) => await progressHubContext.Clients.All.SendAsync("ReceiveProgress", args.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception("FFmpeg failed to convert the video to MP3.");
                }
            }
        }
    }
}
