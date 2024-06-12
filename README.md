# YoutubeToMp3 - Video Downloader and Converter

## Table of Contents
- [Introduction](#introduction)
- [Installation](#installation)
- [Usage](#usage)
- [Features](#features)
- [Contributing](#contributing)

## Introduction
YoutubeToMp3 Video Downloader and Converter is a web application crafted to seamlessly download YouTube videos and convert them into MP3 format. Built using C#, ASP.NET Core, and SignalR, it offers real-time progress updates during both the video download and conversion processes.

The inspiration for this project stems from my experience as a DJ, where I frequently received song requests that I didn't have in my library or were newly released on YouTube. To fulfill these requests, I needed a reliable way to extract MP3 audio from YouTube videos.

While there were online tools available for this task, they often came with drawbacks such as high costs or limited functionality in their demo versions. Dissatisfied with these options, I set out to create my own solution that would be both affordable and fully functional. Thus, YoutubeToMp3 Video Downloader and Converter was born, aiming to provide a straightforward and efficient tool for converting YouTube videos into MP3 files.

## Installation
To run the YoutubeToMp3 Video Downloader and Converter application locally, follow these steps:

1. **Prerequisites:**
   - Install .NET Core SDK. You can download it from the [official .NET website](https://dotnet.microsoft.com/download).
   - Ensure you have a compatible web browser for testing.

2. **Clone the Repository:**
   ```bash
   git clone https://github.com/dimiporf/YoutubeToMp3.git
   cd YoutubeToMp3

## Usage
YoutubeToMp3 Video Downloader and Converter provides an intuitive web interface for downloading and converting YouTube videos:

### Downloading a Video:
1. Enter a valid YouTube URL in the input field on the home page.
2. Click the "Download Video" button.
3. Monitor progress updates in real-time via the progress log displayed on the page.

### Converting to MP3:
1. After downloading the video, click the "Convert to MP3" button.
2. Wait for the conversion process to complete.
3. View conversion progress and receive updates via the progress log.

### Downloading MP3:
1. Once the conversion is finished, a "Download MP3" button will appear.
2. Click "Download MP3" to save the converted MP3 file to your local machine.

### Notes:
- Ensure that the paths for yt-dlp and ffmpeg executables are correctly configured in `appsettings.json`.
- The application provides real-time progress updates using SignalR, enhancing user experience during lengthy operations.

### Features
YoutubeToMp3 Video Downloader and Converter offers the following features:
- **YouTube Video Download:** Easily download videos from YouTube by providing the URL.
- **MP3 Conversion:** Convert downloaded videos to MP3 format.
- **Real-Time Progress Updates:** Utilizes SignalR for live updates on download and conversion progress.
- **Error Handling:** Provides informative error messages for user guidance.
- **File Management:** Supports management of downloaded and converted files directly from the web interface.

### Contributing
Contributions are welcome! If you find any bugs, have feature requests, or want to contribute code, please follow these steps:

1. Fork the repository and clone it locally.
2. Create a new branch for your feature or bug fix: `git checkout -b feature/your-feature` or `git checkout -b bugfix/issue-description`.
3. Make your changes and test thoroughly.
4. Commit your changes: `git commit -am 'Add new feature'` or `git commit -am 'Fix issue #123'`.
5. Push to the branch: `git push origin feature/your-feature`.
6. Submit a pull request to the `main` branch of the repository.

Please provide detailed information in your pull request about the changes made and the problem solved. This will help us review your contribution effectively.

Thank you for contributing to YoutubeToMp3 Video Downloader and Converter!


![Screenshot 2024-06-12 220745](https://github.com/dimiporf/YoutubeToMp3/assets/74142959/8fc0bd9c-09de-47bc-8b37-e3eca707f4eb)
