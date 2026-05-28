# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WebPagesDownloaderSystem is a .NET 10 console application designed to download web pages from a list of URLs specified in `urls.json`. It uses HttpClient with Polly for retry policies, processes downloads in chunks, saves pages locally as HTML files, and reports success/failure metrics.

## Development Commands

### Building the Project
```bash
dotnet build
```

### Running the Application
```bash
dotnet run
```
The application reads `urls.json` from the working directory and begins downloading pages in chunks of 10 (configurable in Program.cs).

### Cleaning Build Artifacts
```bash
dotnet clean
```

### Restoring Dependencies
```bash
dotnet restore
```

## Code Structure

### Entry Point
- `Program.cs`: Sets up dependency injection, configures HttpClient with Polly retry policy, reads `urls.json`, and initiates the download process.

### Core Logic
- `Services/WebPageDownloaderService.cs`: 
  - Contains the main download logic with chunked processing (`DownloadAllInChunksAsync`)
  - Implements single URL download with retry handling (`DownloadSingleAsync`)
  - Uses dependency injection for HttpClient and IPageSaver
  - Saves downloaded pages locally as HTML files

### Models
- `Models/UrlConfig.cs`: Deserializes the `urls.json` file (expects a JSON object with a "urls" array)
- `Models/DownloadResult.cs`: Represents the result of a single download attempt (URL, success status, content length, duration, error message)

### Extensions
- `Extensions/HttpClientPollyExtensions.cs`: Defines a Polly retry policy for HTTP 5xx errors, request timeouts, and HttpRequestExceptions (3 retries with exponential backoff)
- `Extensions/StreamReaderExtensions.cs`: Provides `ReadLimitedContentAsync` to read only the first 100,000 characters of a response (to limit memory usage)

### Services
- `Services/IPageSaver.cs`: Interface for saving web page content
- `Services/PageSaverService.cs`: Implementation of IPageSaver that saves pages as HTML files locally using MD5-hashed filenames

### Configuration
- `urls.json`: Contains the list of URLs to download (checked for existence at startup)

## Key Features
- Chunked processing: Downloads URLs in batches (default size 10) to limit concurrent connections
- Retry mechanism: Automatic retries for transient HTTP failures
- Limited content download: Only reads first 100k characters of each response to avoid excessive memory usage
- Local page saving: Saves downloaded pages as HTML files in a local directory
- Console output: Color-coded success (green) and failure (red) messages with timing information

## Notes
- The application targets .NET 10.0
- Requires `urls.json` to be present in the working directory
- Saved pages are stored in the "saved_pages" directory by default (configurable via PageSaverService constructor)
- No unit tests are currently implemented
- Build outputs are placed in `bin/Debug/net10.0` by default