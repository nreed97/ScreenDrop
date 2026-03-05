# Publishing ScreenDrop to GitHub

Follow these steps to publish your ScreenDrop project to GitHub.

## Prerequisites

- Git installed on your Windows PC
- GitHub account
- GitHub CLI (`gh`) installed (optional, for easier workflow)

## Method 1: Using GitHub CLI (Recommended)

### Step 1: Initialize Git Repository

Open PowerShell or Command Prompt in the ScreenDrop directory:

```bash
cd path\to\ScreenDrop
git init
git add .
git commit -m "Initial commit: ScreenDrop v1.0"
```

### Step 2: Create GitHub Repository

```bash
# Login to GitHub (if not already logged in)
gh auth login

# Create repository and push
gh repo create ScreenDrop --public --source=. --remote=origin --push
```

### Step 3: Create a Release

After building your executable:

```bash
# Tag the current version
git tag -a v1.0.0 -m "ScreenDrop v1.0.0"
git push origin v1.0.0

# Create a release with the executable
gh release create v1.0.0 bin\Release\net8.0\win-x64\publish\ScreenDrop.exe --title "ScreenDrop v1.0.0" --notes "Initial release of ScreenDrop - screenshot capture and upload tool for DigitalOcean Spaces"
```

## Method 2: Using GitHub Web Interface

### Step 1: Initialize Git Repository

```bash
cd path\to\ScreenDrop
git init
git add .
git commit -m "Initial commit: ScreenDrop v1.0"
```

### Step 2: Create Repository on GitHub

1. Go to [GitHub](https://github.com)
2. Click the "+" icon → "New repository"
3. Name it "ScreenDrop"
4. Choose "Public" visibility
5. Do NOT initialize with README (we already have one)
6. Click "Create repository"

### Step 3: Push Your Code

GitHub will show you commands. Use these:

```bash
git remote add origin https://github.com/yourusername/ScreenDrop.git
git branch -M main
git push -u origin main
```

### Step 4: Create a Release

1. Go to your repository on GitHub
2. Click "Releases" → "Create a new release"
3. Tag: `v1.0.0`
4. Title: "ScreenDrop v1.0.0"
5. Description: Add release notes (features, fixes, etc.)
6. Attach the built executable: `bin\Release\net8.0\win-x64\publish\ScreenDrop.exe`
7. Click "Publish release"

## Post-Publishing Steps

### 1. Add Topics to Repository

Add relevant topics to help people discover your project:
- `screenshot`
- `windows`
- `digitalocean`
- `wpf`
- `dotnet`
- `system-tray`
- `file-upload`

### 2. Enable Issues

Make sure Issues are enabled in your repository settings for bug reports and feature requests.

### 3. Add Repository Description

Add a short description in the repository settings:
> "Lightweight Windows screenshot tool with automatic DigitalOcean Spaces upload"

### 4. Update README with Repository URL

Update the clone command in README.md:
```bash
git clone https://github.com/yourusername/ScreenDrop.git
```

Commit and push:
```bash
git add README.md
git commit -m "Update repository URL in README"
git push
```

## Optional Enhancements

### Add Screenshots to README

1. Take screenshots of the application
2. Create a `screenshots/` directory
3. Add images to the directory
4. Update README.md to include them:
   ```markdown
   ## Screenshots
   
   ![Settings Window](screenshots/settings.png)
   ![Upload History](screenshots/history.png)
   ```

### Create GitHub Actions Workflow

Add `.github/workflows/build.yml` for automated builds:

```yaml
name: Build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --configuration Release --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

### Add Contributing Guidelines

Create `CONTRIBUTING.md`:

```markdown
# Contributing to ScreenDrop

We love your input! We want to make contributing as easy as possible.

## Development Process

1. Fork the repo
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Code Style

- Follow standard C# conventions
- Use meaningful variable names
- Add comments for complex logic

## Reporting Bugs

Use GitHub Issues to report bugs. Include:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots (if applicable)
```

## Future Releases

For subsequent releases:

```bash
# Update version in code
# Commit changes
git add .
git commit -m "Bump version to v1.1.0"

# Create and push tag
git tag -a v1.1.0 -m "Version 1.1.0"
git push origin v1.1.0

# Build the new version
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Create release
gh release create v1.1.0 bin\Release\net8.0\win-x64\publish\ScreenDrop.exe --title "ScreenDrop v1.1.0" --notes "Release notes here"
```

## Troubleshooting

### Git not found
Install Git from https://git-scm.com/download/win

### GitHub CLI not found
Install from https://cli.github.com/

### Authentication issues
```bash
gh auth login
# Follow the prompts
```

## Need Help?

- [GitHub Docs](https://docs.github.com)
- [Git Documentation](https://git-scm.com/doc)
- [GitHub CLI Manual](https://cli.github.com/manual/)
