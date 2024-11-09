# JustWipeIt

## Overview

**JustWipeIt** is a command-line utility that securely wipes data on a specified drive by overwriting it with a variety
of data patterns. This tool is designed to help users permanently and irreversibly delete sensitive data, ensuring that
it cannot be recovered using standard methods. The application offers multiple wiping methods, giving users flexibility
to choose the security level appropriate for their needs.

> **Disclaimer:** This tool is highly experimental and could potentially cause irreversible damage to your drive. It is
> intended for users who understand the risks involved in low-level data manipulation. No warranty is included, and the
> developers do not assume responsibility for any damage resulting from the use of this tool. Use at your own risk.

---

## Features

- **Multiple Wiping Methods**: Choose from a variety of wiping patterns including zeroes, ones, specific binary
  patterns, random data, and encryption-based overwriting.
- **Command-line Interface**: Designed for efficiency and ease of use in a terminal environment.
- **Confirmation Prompt**: Optional `-y` flag to skip confirmation and speed up the process.
- **Progress Display**: Uses the Spectre.Console library to provide real-time feedback on the wipe progress.

---

## Usage Instructions

### Prerequisites

- **.NET 6.0 SDK** or higher is required to run this application.
- Ensure that the Spectre.Console NuGet package is installed, as it is used for terminal UI elements.

### Getting Started

1. **Clone the Repository**
    ```bash
    git clone https://github.com/YourUsername/JustWipeIt.git
    cd JustWipeIt
    ```

2. **Build the Application**
    ```bash
    dotnet build
    ```

3. **Run the Application**
    ```bash
    dotnet run -- -d <drive-letter> [-y]
    ```

### Command-line Arguments

| Argument            | Description                                                     |
|---------------------|-----------------------------------------------------------------|
| `-d <drive-letter>` | Specifies the drive letter of the target drive (e.g., `-d E:`). |
| `-y`                | Skips confirmation prompts for automated workflows.             |

### Usage Examples

1. **Run JustWipeIt with Prompted Confirmation**
    ```bash
    dotnet run -- -d E:
    ```
   This command will prompt the user to confirm the data wipe and will list available wiping methods for selection.

2. **Run JustWipeIt Without Confirmation**
    ```bash
    dotnet run -- -d E: -y
    ```
   The `-y` flag skips the confirmation, starting the wipe process immediately after choosing the wiping methods.

---

## Wiping Methods

The application provides multiple secure data-wiping methods:

1. **Overwrite with Zeros**: Fills the drive with `0x00` bytes.
2. **Overwrite with Ones**: Fills the drive with `0xFF` bytes.
3. **Overwrite with Pattern 10101010**: Fills the drive with `0xAA` bytes.
4. **Overwrite with Pattern 01010101**: Fills the drive with `0x55` bytes.
5. **Overwrite with Random Data**: Writes random bytes across the drive.
6. **Overwrite with SHA-256**: Uses SHA-256 hashing to overwrite data, providing additional security.
7. **Overwrite with AES Encryption**: Encrypts data on the drive with AES, further preventing data recovery.

> **Note:** Multiple wiping patterns can be selected for a layered data destruction approach.

