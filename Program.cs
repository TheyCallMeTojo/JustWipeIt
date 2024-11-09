using System.Security.Cryptography;
using Spectre.Console;

namespace JustWipeIt
{
    internal abstract class Program
    {
        private const int BufferSize = 4096; // Buffer size for file write operations

        private static void Main(string[] args)
        {
            // Check for the "-y" argument to skip confirmation prompts
            var skipConfirmation = args.Contains("-y");
            string driveLetter;

            // Display application banner with information and instructions
            AnsiConsole.Write(
                new FigletText("JustWipeIt")
                    .Color(Color.Yellow2));

            // Application details and usage instructions
            AnsiConsole.MarkupLine("[bold yellow]Author:[/] TheyCallMeTojo");
            AnsiConsole.MarkupLine("[bold yellow]Version:[/] 1.0");
            AnsiConsole.MarkupLine(
                "[bold yellow]Description:[/] This utility wipes the specified drive with selected data patterns, ensuring secure deletion.");
            AnsiConsole.MarkupLine("[bold yellow]Usage Instructions:[/]");
            AnsiConsole.MarkupLine(" - Run the application and specify a drive letter when prompted.");
            AnsiConsole.MarkupLine(" - Use the [green]-y[/] flag to skip confirmation prompts.");
            AnsiConsole.MarkupLine(" - Choose from multiple wiping patterns for enhanced security.");
            AnsiConsole.WriteLine();

            // Warning message about irreversible data destruction
            AnsiConsole.Markup("[bold red]WARNING: This will permanently destroy all data on the specified drive![/]");
            AnsiConsole.WriteLine();

            // Check if drive letter is passed as an argument; if not, prompt the user
            if (args.Length > 1 && args[0] == "-d")
            {
                driveLetter = args[1];
            }
            else
            {
                driveLetter = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter the drive letter to wipe (e.g., [green]E:\\[/]):")
                        .PromptStyle("green")
                        .Validate(drive =>
                            Directory.Exists(drive)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]Drive not found or inaccessible.[/]")));
            }

            // Define the available wiping methods and their corresponding actions
            var availableTasks = new (string Name, Action<string, long, ProgressTask> Action)[]
            {
                ("Overwrite with Zeros", (drive, size, task) => OverwriteWithPattern(drive, size, task, 0x00)),
                ("Overwrite with Ones", (drive, size, task) => OverwriteWithPattern(drive, size, task, 0xFF)),
                ("Overwrite with Pattern 10101010",
                    (drive, size, task) => OverwriteWithPattern(drive, size, task, 0xAA)),
                ("Overwrite with Pattern 01010101",
                    (drive, size, task) => OverwriteWithPattern(drive, size, task, 0x55)),
                ("Overwrite with Random Data", OverwriteWithRandomData),
                ("Overwrite with SHA-256", OverwriteWithSha256),
                ("Overwrite with AES Encryption", OverwriteWithAes)
            };

            // Prompt user to select one or more wiping methods from the list
            var selectedTasks = AnsiConsole.Prompt(
                new MultiSelectionPrompt<(string Name, Action<string, long, ProgressTask> Action)>()
                    .Title("Select the [green]wiping methods[/] to use:")
                    .PageSize(10)
                    .AddChoices(availableTasks)
                    .UseConverter(task => task.Name)
            );

            // Confirm data wipe if confirmation is not skipped
            if (!skipConfirmation)
            {
                AnsiConsole.MarkupLine("[bold yellow]Are you sure you want to proceed?[/]");
                if (!AnsiConsole.Confirm("[red]Confirm data wipe?[/]", false))
                {
                    AnsiConsole.MarkupLine("[bold yellow]Operation canceled by the user.[/]");
                    return;
                }
            }

            // Start wiping process with selected tasks
            try
            {
                ScrubDrive(driveLetter, selectedTasks.ToArray());
                AnsiConsole.MarkupLine("[bold green]Drive wiped successfully![/]");
            }
            catch (Exception ex)
            {
                // Display error message if wiping process fails
                AnsiConsole.MarkupLine($"[red]Error during wipe:[/] {ex.Message}");
            }
        }

        // Method to perform the drive wiping using selected tasks
        private static void ScrubDrive(string driveLetter,
            (string Name, Action<string, long, ProgressTask> Action)[] selectedTasks)
        {
            var drive = new DriveInfo(driveLetter); // Get drive information
            var driveSize = drive.TotalSize; // Get total drive size

            // Execute each selected wiping task on the drive
            foreach (var task in selectedTasks)
            {
                AnsiConsole.MarkupLine($"[blue]{task.Name}...[/]");

                // Display progress for each wiping task
                AnsiConsole.Progress()
                    .Start(ctx =>
                    {
                        var progressTask = ctx.AddTask($"[green]{task.Name}[/]");
                        progressTask.MaxValue = driveSize;

                        task.Action(driveLetter, driveSize, progressTask); // Execute wiping action
                    });
            }
        }

        // Method to overwrite drive with a specified byte pattern
        private static void OverwriteWithPattern(string driveLetter, long size, ProgressTask task, byte pattern)
        {
            var buffer = new byte[BufferSize];
            Array.Fill(buffer, pattern); // Fill buffer with specified pattern

            using var fs = new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate);
            while (fs.Position < size)
            {
                fs.Write(buffer, 0, buffer.Length);
                task.Increment(BufferSize); // Update progress
            }
        }

        // Method to overwrite drive with random data
        private static void OverwriteWithRandomData(string driveLetter, long size, ProgressTask task)
        {
            var random = new Random();
            var buffer = new byte[BufferSize];

            using var fs = new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate);
            while (fs.Position < size)
            {
                random.NextBytes(buffer); // Fill buffer with random bytes
                fs.Write(buffer, 0, buffer.Length);
                task.Increment(BufferSize); // Update progress
            }
        }

        // Method to overwrite drive with SHA-256 hash of an empty buffer
        private static void OverwriteWithSha256(string driveLetter, long size, ProgressTask task)
        {
            var buffer = new byte[BufferSize];
            using var fs = new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate);
            while (fs.Position < size)
            {
                var hash = SHA256.HashData(buffer); // Generate SHA-256 hash
                fs.Write(hash, 0, hash.Length);
                task.Increment(hash.Length); // Update progress
            }
        }

        // Method to overwrite drive using AES encryption
        private static void OverwriteWithAes(string driveLetter, long size, ProgressTask task)
        {
            using var aes = Aes.Create();
            aes.Key = new byte[32]; // Set AES key
            aes.IV = new byte[16]; // Set AES initialization vector

            var buffer = new byte[BufferSize];
            using var cryptoStream = new CryptoStream(
                new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate),
                aes.CreateEncryptor(),
                CryptoStreamMode.Write);
            while (cryptoStream.Position < size)
            {
                cryptoStream.Write(buffer, 0, buffer.Length);
                task.Increment(BufferSize); // Update progress
            }
        }
    }
}